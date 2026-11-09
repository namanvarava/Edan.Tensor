using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>OpenAPI 문서에 보호 API를 호출하는 인증·Tenant·멱등 헤더 사용법을 기록합니다.</summary>
/// <remarks>
/// API는 토큰을 발급하지 않으며 외부 IdP가 발급한 Bearer 토큰을 검증합니다.
/// Development Bypass를 사용하는 경우에도 이 문서는 운영 인증 계약을 대체하지 않습니다.
/// 상태 확인과 루트 응답은 공개 경로로 남기고, 나머지 경로에는 Bearer 인증 요구사항을 표시합니다.
/// </remarks>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private static readonly HashSet<string> PublicPaths =
    [
        "/",
        "/health",
        "/health/database"
    ];

    /// <summary>OpenAPI 문서에 Bearer 인증과 업무별 필수 헤더를 추가합니다.</summary>
    /// <param name="document">변경할 OpenAPI 문서를 지정합니다.</param>
    /// <param name="context">현재 문서와 API 설명 그룹을 제공하는 변환 컨텍스트를 지정합니다.</param>
    /// <param name="cancellationToken">문서 변환을 취소할 토큰을 지정합니다.</param>
    /// <returns>문서 변환이 끝난 뒤 완료되는 작업을 반환합니다.</returns>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "외부 IdP가 발급한 JWT를 Bearer <token> 형식으로 입력합니다."
            }
        };

        foreach (var path in document.Paths)
        {
            var pathItem = path.Value;
            if (pathItem is null)
                continue;

            foreach (var operation in pathItem.Operations ?? [])
            {
                if (PublicPaths.Contains(path.Key))
                    continue;
                if (operation.Value is null)
                    continue;

                operation.Value.Parameters ??= [];
                if (!operation.Value.Parameters.Any(parameter =>
                    parameter.In == ParameterLocation.Header
                    && string.Equals(parameter.Name, "X-Tenant-Id", StringComparison.OrdinalIgnoreCase)))
                {
                    var tenantHeaderRequired = RequiresTenantHeader(path.Key);
                    operation.Value.Parameters.Add(new OpenApiParameter
                    {
                        Name = "X-Tenant-Id",
                        In = ParameterLocation.Header,
                        Required = tenantHeaderRequired,
                        Description = tenantHeaderRequired
                            ? "Tenant 범위를 지정합니다. Tenant 경로 API와 선택 Tenant 기반 API에서는 필수이며 소속과 요청 권한 검사는 서버에서 수행합니다."
                            : "Tenant 범위를 선택할 때 지정합니다. Tenant 소속과 요청 권한 검사는 서버에서 수행합니다."
                    });
                }

                if (string.Equals(operation.Key.ToString(), "Post", StringComparison.OrdinalIgnoreCase)
                    && TryGetRequiredHeader(path.Key, out var requiredHeaderName, out var requiredHeaderDescription)
                    && !operation.Value.Parameters.Any(parameter =>
                        parameter.In == ParameterLocation.Header
                        && string.Equals(parameter.Name, requiredHeaderName, StringComparison.OrdinalIgnoreCase)))
                {
                    operation.Value.Parameters.Add(new OpenApiParameter
                    {
                        Name = requiredHeaderName,
                        In = ParameterLocation.Header,
                        Required = true,
                        Description = requiredHeaderDescription
                    });
                }

                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>요청 경로가 Tenant 선택 헤더를 반드시 요구하는지 판단합니다.</summary>
    /// <param name="path">OpenAPI에 기록된 요청 경로를 지정합니다.</param>
    /// <returns>경로의 Tenant 범위가 헤더를 요구하면 <c>true</c>를 반환합니다.</returns>
    private static bool RequiresTenantHeader(string path)
    {
        if (string.Equals(path, "/api/platform/resources", StringComparison.Ordinal)
            || string.Equals(path, "/api/platform/policies", StringComparison.Ordinal))
            return true;

        if (!path.StartsWith("/api/platform/tenants/{tenantId}", StringComparison.Ordinal))
            return false;

        // 플랫폼 관리자는 Tenant 선택 없이 생성 직후의 활성화·최초 소속 배정을 수행합니다.
        return !string.Equals(path, "/api/platform/tenants/{tenantId}/activate", StringComparison.Ordinal)
            && !string.Equals(path, "/api/platform/tenants/{tenantId}/memberships", StringComparison.Ordinal);
    }

    private static bool TryGetRequiredHeader(string path, out string name, out string description)
    {
        if (path.EndsWith("/worker/tasks", StringComparison.Ordinal)
            || path.EndsWith("/worker/deliveries", StringComparison.Ordinal)
            || path.EndsWith("/datasets/{dataSetId}/validations", StringComparison.Ordinal))
        {
            name = "Idempotency-Key";
            description = "같은 Tenant의 동일 요청을 하나의 작업으로 수렴시키는 키입니다. 최대 200자이며 요청마다 고유하게 지정합니다.";
            return true;
        }

        if (path.EndsWith("/worker/dead-letters/{taskId}/replay", StringComparison.Ordinal))
        {
            name = "Replay-Request-Id";
            description = "DLQ 재처리 요청을 한 번만 생성하기 위한 GUID입니다.";
            return true;
        }

        name = string.Empty;
        description = string.Empty;
        return false;
    }
}
