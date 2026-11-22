using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.ApiHost.Services;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Application.Abstractions;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>플랫폼 관리 API는 인증된 주체의 Tenant 권한을 기준으로 업무를 수행합니다.</summary>
public static class PlatformEndpoints
{
    /// <summary>플랫폼 관리 API를 관리자 및 Tenant 구성원 경계로 나누어 등록합니다.</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/platform").WithTags("Platform");

        // 수락은 token 자체가 인증 수단이므로 로그인 전에도 가능하지만, token hash가 맞을 때만 사용자 활성화가 됩니다.
        group.MapPost("/invitations/accept", async (AcceptInvitationRequest request, InvitationService service, CancellationToken token) =>
        {
            var result = await service.AcceptAsync(request.Token, token);
            return result.IsSuccess ? Results.Ok(new { UserId = result.Value.Value, Status = "Active" }) : ToHttpResult(result);
        }).WithName("AcceptInvitation");

        // Tenant 생성과 최초 활성화는 플랫폼 관리자에게만 허용합니다.
        group.MapPost("/tenants", async (CreateTenantRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateTenantAsync(request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapPost("/tenants/{tenantId:guid}/memberships", async (Guid tenantId, GrantTenantMembershipRequest request, TenantMembershipService service, CancellationToken token) =>
        {
            if (!Enum.TryParse<TenantRole>(request.Role, true, out var role))
                return ValidationError("지원하지 않는 테넌트 역할입니다.");
            var result = await service.GrantAsync(new TenantId(tenantId), request.SubjectId, role, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapPost("/tenants/{tenantId:guid}/activate", async (Guid tenantId, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.ActivateTenantAsync(new TenantId(tenantId), token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapGet("/tenants/mine", async (HttpContext context, TenantMembershipService service, CancellationToken token) =>
        {
            var subjectId = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(subjectId))
                return Results.Unauthorized();
            var memberships = await service.GetMyMembershipsAsync(subjectId, token);
            return Results.Ok(memberships.Select(x => new { TenantId = x.TenantId.Value, Role = x.Role.ToString() }));
        }).RequireAuthorization("TenantMember");

        group.MapGet("/tenants", async (string? search, TenantStatus? status, int? page, int? pageSize,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetTenantsAsync(search, status, page ?? 1, pageSize ?? 50, token);
            return Results.Ok(result);
        }).RequireAuthorization("PlatformAdministrator");

        // 경로의 TenantId가 요청에서 선택한 Tenant와 일치하고 활성 소속이 확인된 경우에만 업무 API를 실행합니다.
        var tenantGroup = group.MapGroup("/tenants")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/database-binding", async (Guid tenantId, PlatformDbContext db, CancellationToken token) =>
        {
            var binding = await db.TenantDatabaseBindings.AsNoTracking()
                .SingleOrDefaultAsync(x => x.TenantId == new TenantId(tenantId)
                    && x.Status == TenantDatabaseBindingStatus.Active, token);
            return binding is null
                ? Results.NotFound(new ApiError("platform.database_binding.not_found", "활성 DB 바인딩을 찾을 수 없습니다."))
                : Results.Ok(new
                {
                    BindingId = binding.Id.Value,
                    TenantId = binding.TenantId.Value,
                    Provider = binding.Provider.ToString(),
                    binding.Username,
                    binding.ServerAddress,
                    binding.Port,
                    binding.DatabaseName,
                    binding.SchemaName,
                    binding.Status,
                    binding.MigrationVersion,
                    binding.LastHealthAtUtc
                });
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/database-binding/activate", async (Guid tenantId, PlatformDbContext db, TimeProvider clock, CancellationToken token) =>
        {
            var binding = await db.TenantDatabaseBindings.SingleOrDefaultAsync(x => x.TenantId == new TenantId(tenantId) && x.Status == TenantDatabaseBindingStatus.Provisioning, token);
            if (binding is null) return Results.NotFound(new ApiError("platform.database_binding.provisioning_not_found", "활성화할 Provisioning 바인딩을 찾을 수 없습니다."));
            binding.Activate(clock.GetUtcNow());
            await db.SaveChangesAsync(token);
            return Results.Ok(new { BindingId = binding.Id.Value, Status = binding.Status.ToString() });
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/database-binding/health", async (Guid tenantId, ITenantDatabaseConnectionFactory factory, PlatformDbContext db, TimeProvider clock, CancellationToken token) =>
        {
            try
            {
                await using var connection = await factory.OpenAsync(new TenantId(tenantId), token);
                var binding = await db.TenantDatabaseBindings.SingleAsync(x => x.TenantId == new TenantId(tenantId) && x.Status == TenantDatabaseBindingStatus.Active, token);
                binding.RecordHealth(clock.GetUtcNow());
                await db.SaveChangesAsync(token);
                return Results.Ok(new { Status = "Healthy", Provider = connection.GetType().Name, CheckedAtUtc = clock.GetUtcNow() });
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Data.Common.DbException or TimeoutException)
            {
                return Results.Problem("테넌트 DB 연결 확인에 실패했습니다.", statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/database-binding", async (
            Guid tenantId,
            CreateTenantDatabaseBindingRequest request,
            PlatformDbContext db,
            TimeProvider clock,
            CancellationToken token) =>
        {
            if (!Enum.TryParse<TenantDatabaseProvider>(request.Provider, true, out var provider))
                return ValidationError("지원하지 않는 DB Provider입니다.");

            if (!await db.Tenants.AnyAsync(x => x.Id == new TenantId(tenantId), token))
                return Results.NotFound(new ApiError("platform.tenant.not_found", "테넌트를 찾을 수 없습니다."));

            var tenant = new TenantId(tenantId);
            var active = await db.TenantDatabaseBindings.AnyAsync(
                x => x.TenantId == tenant && x.Status == TenantDatabaseBindingStatus.Active, token);
            if (active)
                return Results.Conflict(new ApiError("platform.database_binding.active_exists", "활성 DB 바인딩이 이미 존재합니다."));

            var binding = TenantDatabaseBinding.Create(
                new TenantDatabaseBindingId(Guid.NewGuid()), tenant, provider, request.Username,
                request.SecretKey, request.ServerAddress, request.Port, request.DatabaseName,
                request.SchemaName, clock.GetUtcNow());
            db.TenantDatabaseBindings.Add(binding);
            await db.SaveChangesAsync(token);
            return Results.Json(new { BindingId = binding.Id.Value, Status = binding.Status.ToString() }, statusCode: StatusCodes.Status201Created);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/database/metadata/tables", async (
            Guid tenantId, ITenantDatabaseMetadataReader reader, CancellationToken token) =>
        {
            var tables = await reader.ListTablesAsync(new TenantId(tenantId), token);
            return Results.Ok(tables);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/database/metadata/columns", async (
            Guid tenantId, string schema, string table, ITenantDatabaseMetadataReader reader, CancellationToken token) =>
        {
            var columns = await reader.ListColumnsAsync(new TenantId(tenantId), schema, table, token);
            return Results.Ok(columns);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/database/routines/allowlist", async (Guid tenantId, PlatformDbContext db, CancellationToken token) =>
        {
            var items = await db.TenantDatabaseRoutineAllowlists.AsNoTracking()
                .Where(x => x.TenantId == new TenantId(tenantId))
                .OrderBy(x => x.SchemaName).ThenBy(x => x.RoutineName)
                .Select(x => new { x.Id, x.Kind, x.SchemaName, x.RoutineName, x.IsEnabled, x.CreatedAtUtc, x.UpdatedAtUtc })
                .ToListAsync(token);
            return Results.Ok(items);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/database/routines/allowlist", async (Guid tenantId, CreateRoutineAllowlistRequest request, PlatformDbContext db, TimeProvider clock, CancellationToken token) =>
        {
            if (!Enum.TryParse<TenantDatabaseRoutineKind>(request.Kind, true, out var kind))
                return ValidationError("지원하지 않는 루틴 유형입니다.");
            var tenant = new TenantId(tenantId);
            var exists = await db.TenantDatabaseRoutineAllowlists.AnyAsync(x => x.TenantId == tenant && x.Kind == kind && x.SchemaName == request.SchemaName && x.RoutineName == request.RoutineName, token);
            if (exists) return Results.Conflict(new ApiError("platform.database_routine.duplicate", "동일한 루틴 허용 항목이 이미 존재합니다."));
            var item = TenantDatabaseRoutineAllowlist.Create(new TenantDatabaseRoutineAllowlistId(Guid.NewGuid()), tenant, kind, request.SchemaName, request.RoutineName, clock.GetUtcNow());
            db.TenantDatabaseRoutineAllowlists.Add(item);
            await db.SaveChangesAsync(token);
            return Results.Json(new { Id = item.Id.Value, item.Kind, item.SchemaName, item.RoutineName, item.IsEnabled }, statusCode: StatusCodes.Status201Created);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/database/routines/execute", async (Guid tenantId, ExecuteRoutineRequest request, ITenantDatabaseRoutineExecutor executor, CancellationToken token) =>
        {
            if (!Enum.TryParse<TenantDatabaseRoutineKind>(request.Kind, true, out var kind))
                return ValidationError("지원하지 않는 루틴 유형입니다.");
            var result = await executor.ExecuteAsync(new TenantId(tenantId), kind, request.SchemaName, request.RoutineName, request.Parameters ?? new Dictionary<string, string?>(), token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}", async (Guid tenantId, PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetTenantAsync(new TenantId(tenantId), token);
            return result is null ? Results.NotFound(new ApiError("platform.tenant.not_found", "테넌트를 찾을 수 없습니다.")) : Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/organizations", async (Guid tenantId, string? search, OrganizationStatus? status,
            int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetOrganizationsAsync(new TenantId(tenantId), search, status, page ?? 1, pageSize ?? 50, token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/organizations/{organizationId:guid}", async (Guid organizationId,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetOrganizationAsync(new OrganizationId(organizationId), token);
            return result is null ? Results.NotFound(new ApiError("platform.organization.not_found", "조직을 찾을 수 없습니다.")) : Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/projects", async (Guid tenantId, Guid? organizationId, string? search,
            ProjectStatus? status, int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetProjectsAsync(new TenantId(tenantId),
                organizationId is Guid org ? new OrganizationId(org) : null, search, status, page ?? 1, pageSize ?? 50, token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/projects/{projectId:guid}", async (Guid projectId,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetProjectAsync(new ProjectId(projectId), token);
            return result is null ? Results.NotFound(new ApiError("platform.project.not_found", "프로젝트를 찾을 수 없습니다.")) : Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/users", async (Guid tenantId, string? search, UserStatus? status,
            int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetUsersAsync(new TenantId(tenantId), search, status, page ?? 1, pageSize ?? 50, token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/users/{userId:guid}", async (Guid userId,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            var result = await service.GetUserAsync(new UserId(userId), token);
            return result is null ? Results.NotFound(new ApiError("platform.user.not_found", "사용자를 찾을 수 없습니다.")) : Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/memberships", async (Guid tenantId, string? subjectId,
            TenantMembershipStatus? status, int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            if (subjectId is not null || status.HasValue || page.HasValue || pageSize.HasValue)
            {
                var paged = await service.GetTenantMembershipsPageAsync(new TenantId(tenantId), subjectId, status,
                    page ?? 1, pageSize ?? 50, token);
                return Results.Ok(paged);
            }
            var result = await service.GetTenantMembershipsAsync(new TenantId(tenantId), token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/organizations/{organizationId:guid}/memberships", async (Guid tenantId, Guid organizationId,
            string? subjectId, ScopedMembershipStatus? status, int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            if (subjectId is not null || status.HasValue || page.HasValue || pageSize.HasValue)
            {
                var paged = await service.GetOrganizationMembershipsPageAsync(new TenantId(tenantId), new OrganizationId(organizationId),
                    subjectId, status, page ?? 1, pageSize ?? 50, token);
                return Results.Ok(paged);
            }
            var result = await service.GetOrganizationMembershipsAsync(new TenantId(tenantId), new OrganizationId(organizationId), token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/organizations/{organizationId:guid}/projects/{projectId:guid}/memberships", async (Guid tenantId, Guid organizationId, Guid projectId,
            string? subjectId, ScopedMembershipStatus? status, int? page, int? pageSize, PlatformDirectoryService service, CancellationToken token) =>
        {
            if (subjectId is not null || status.HasValue || page.HasValue || pageSize.HasValue)
            {
                var paged = await service.GetProjectMembershipsPageAsync(new TenantId(tenantId), new OrganizationId(organizationId),
                    new ProjectId(projectId), subjectId, status, page ?? 1, pageSize ?? 50, token);
                return Results.Ok(paged);
            }
            var result = await service.GetProjectMembershipsAsync(new TenantId(tenantId), new OrganizationId(organizationId), new ProjectId(projectId), token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}", async (Guid tenantId, UpdateDirectoryEntryRequest request,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            TenantStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse(request.Status, true, out TenantStatus parsed))
                    return ValidationError("지원하지 않는 테넌트 상태입니다.");
                status = parsed;
            }
            var result = await service.UpdateTenantAsync(new TenantId(tenantId), request.Name, status, token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/organizations/{organizationId:guid}", async (Guid organizationId, UpdateDirectoryEntryRequest request,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            OrganizationStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse(request.Status, true, out OrganizationStatus parsed))
                    return ValidationError("지원하지 않는 조직 상태입니다.");
                status = parsed;
            }
            var result = await service.UpdateOrganizationAsync(new OrganizationId(organizationId), request.Name, status, token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/organizations/{organizationId:guid}/projects/{projectId:guid}", async (Guid projectId, UpdateDirectoryEntryRequest request,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            ProjectStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse(request.Status, true, out ProjectStatus parsed))
                    return ValidationError("지원하지 않는 프로젝트 상태입니다.");
                status = parsed;
            }
            var result = await service.UpdateProjectAsync(new ProjectId(projectId), request.Name, status, token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/users/{userId:guid}", async (Guid userId, UpdateDirectoryEntryRequest request,
            PlatformDirectoryService service, CancellationToken token) =>
        {
            UserStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse(request.Status, true, out UserStatus parsed))
                    return ValidationError("지원하지 않는 사용자 상태입니다.");
                status = parsed;
            }
            var result = await service.UpdateUserAsync(new UserId(userId), request.Name, status, token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/users/{userId:guid}/status", async (Guid userId,
            SetUserStatusRequest request, PlatformDirectoryService service, CancellationToken token) =>
        {
            if (!Enum.TryParse(request.Status, true, out UserStatus status))
                return ValidationError("지원하지 않는 사용자 상태입니다.");
            var result = await service.UpdateUserAsync(new UserId(userId), null, status, token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("SetUserStatus");

        tenantGroup.MapPost("/{tenantId:guid}/authorization-policies",
            async (Guid tenantId, CreateAuthorizationPolicyRequest request, AuthorizationPolicyService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<AuthorizationPolicyEffect>(request.Effect, true, out var effect))
                    return ValidationError("지원하지 않는 접근 정책 효과입니다.");
                var result = await service.CreateAsync(new TenantId(tenantId),
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null,
                    request.SubjectId, request.Action, request.ResourceType, effect, request.Priority, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-policies",
            async (Guid tenantId, Guid? organizationId, Guid? projectId, string? subjectId, string? action,
                AuthorizationPolicyEffect? effect, bool? isEnabled, int? page, int? pageSize,
                AuthorizationPolicyService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.GetPageAsync(new TenantId(tenantId),
                        organizationId is Guid organization ? new OrganizationId(organization) : null,
                        projectId is Guid project ? new ProjectId(project) : null,
                        subjectId, action, effect, isEnabled, page ?? 1,
                        pageSize ?? 50, token);
                    return Results.Ok(new
                    {
                        result.TotalCount,
                        result.Page,
                        result.PageSize,
                        Items = result.Items.Select(x => new
                        {
                            Id = x.Id.Value,
                            OrganizationId = x.OrganizationId?.Value,
                            ProjectId = x.ProjectId?.Value,
                            x.SubjectId,
                            x.Action,
                            x.ResourceType,
                            Effect = x.Effect.ToString(),
                            x.Priority,
                            x.IsEnabled,
                            x.CreatedAtUtc
                        })
                    });
                }
                catch (ArgumentOutOfRangeException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-policies/evaluate",
            async (Guid tenantId, EvaluateAuthorizationPolicyRequest request, HttpContext context,
                PlatformAuthorizationService service, CancellationToken token) =>
            {
                var subjectId = string.IsNullOrWhiteSpace(request.SubjectId)
                    ? context.User.FindFirst("sub")?.Value
                    : request.SubjectId;
                if (string.IsNullOrWhiteSpace(subjectId)) return Results.Unauthorized();
                try
                {
                    var result = await service.EvaluateAsync(new TenantId(tenantId),
                        request.OrganizationId is Guid organization ? new OrganizationId(organization) : null,
                        request.ProjectId is Guid project ? new ProjectId(project) : null,
                        subjectId, request.Action, request.ResourceType, token);
                    return Results.Ok(new { result.IsAllowed, result.ReasonCode, result.MatchedPolicyId });
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 저장 용량 정책은 Tenant 관리자만 변경하며 조회도 제한된 페이지로 반환합니다.
        tenantGroup.MapGet("/{tenantId:guid}/storage-quota-policies",
            async (Guid tenantId, StorageQuotaPolicyManagementService service, CancellationToken token,
                StorageQuotaPolicyScope? scope = null, Guid? targetId = null, bool? isEnabled = null,
                int page = 1, int pageSize = 50) =>
            {
                try
                {
                    var result = await service.GetPoliciesAsync(new TenantId(tenantId), scope, targetId, isEnabled,
                        page, pageSize, token);
                    return Results.Ok(new
                    {
                        result.TotalCount,
                        result.Page,
                        result.PageSize,
                        Items = result.Items.Select(ToStorageQuotaPolicyResponse)
                    });
                }
                catch (ArgumentException exception)
                {
                    return ValidationError(exception.Message);
                }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("GetStorageQuotaPolicies");

        tenantGroup.MapGet("/{tenantId:guid}/storage-quota-policies/{policyId:guid}/history",
            async (Guid tenantId, Guid policyId, StorageQuotaPolicyManagementService service,
                CancellationToken token, int page = 1, int pageSize = 50) =>
            {
                try
                {
                    var result = await service.GetHistoryAsync(new TenantId(tenantId), policyId,
                        page, pageSize, token);
                    if (result is null)
                        return Results.NotFound(new ApiError("quota_policy.not_found",
                            "선택한 Tenant에서 저장 용량 정책을 찾을 수 없습니다."));
                    return Results.Ok(new
                    {
                        result.TotalCount,
                        result.Page,
                        result.PageSize,
                        Items = result.Items.Select(ToStorageQuotaPolicyHistoryResponse)
                    });
                }
                catch (ArgumentException exception)
                {
                    return ValidationError(exception.Message);
                }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("GetStorageQuotaPolicyHistory");

        tenantGroup.MapPost("/{tenantId:guid}/storage-quota-policies",
            async (Guid tenantId, CreateStorageQuotaPolicyRequest request, HttpContext context,
                StorageQuotaPolicyManagementService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<StorageQuotaPolicyScope>(request.Scope, true, out var scope))
                    return Results.BadRequest(new ApiError("quota_policy.invalid_scope",
                        "지원하지 않는 저장 용량 정책 범위입니다."));
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var policy = await service.CreateAsync(new TenantId(tenantId), scope, request.TargetId,
                        request.TenantLimitBytes, request.OrganizationLimitBytes, request.ProjectLimitBytes,
                        request.DataSetLimitBytes, request.WarningThresholdPercent, request.PlanCode ?? "Community",
                        actor, token);
                    return Results.Created($"/api/platform/tenants/{tenantId}/storage-quota-policies/{policy.Id}",
                        ToStorageQuotaPolicyResponse(policy));
                }
                catch (BusinessRuleViolationException exception) when (exception.Code == "quota_policy.conflict")
                {
                    return Results.Conflict(new ApiError(exception.Code, exception.Message));
                }
                catch (ArgumentException exception)
                {
                    return ValidationError(exception.Message);
                }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("CreateStorageQuotaPolicy");

        tenantGroup.MapPut("/{tenantId:guid}/storage-quota-policies/{policyId:guid}",
            async (Guid tenantId, Guid policyId, UpdateStorageQuotaPolicyRequest request, HttpContext context,
                StorageQuotaPolicyManagementService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var policy = await service.UpdateAsync(new TenantId(tenantId), policyId, request.ExpectedVersion,
                        request.TenantLimitBytes, request.OrganizationLimitBytes, request.ProjectLimitBytes,
                        request.DataSetLimitBytes, request.WarningThresholdPercent, request.PlanCode, actor, token);
                    return policy is null
                        ? Results.NotFound(new ApiError("quota_policy.not_found",
                            "선택한 Tenant에서 저장 용량 정책을 찾을 수 없습니다."))
                        : Results.Ok(ToStorageQuotaPolicyResponse(policy));
                }
                catch (BusinessRuleViolationException exception) when (exception.Code == "quota_policy.version_conflict")
                {
                    return Results.Conflict(new ApiError(exception.Code, exception.Message));
                }
                catch (ArgumentException exception)
                {
                    return ValidationError(exception.Message);
                }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("UpdateStorageQuotaPolicy");

        tenantGroup.MapPatch("/{tenantId:guid}/storage-quota-policies/{policyId:guid}/enabled",
            async (Guid tenantId, Guid policyId, SetStorageQuotaPolicyEnabledRequest request, HttpContext context,
                StorageQuotaPolicyManagementService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var policy = await service.SetEnabledAsync(new TenantId(tenantId), policyId,
                        request.IsEnabled, request.ExpectedVersion, actor, token);
                    return policy is null
                        ? Results.NotFound(new ApiError("quota_policy.not_found",
                            "선택한 Tenant에서 저장 용량 정책을 찾을 수 없습니다."))
                        : Results.Ok(ToStorageQuotaPolicyResponse(policy));
                }
                catch (BusinessRuleViolationException exception) when (exception.Code == "quota_policy.version_conflict")
                {
                    return Results.Conflict(new ApiError(exception.Code, exception.Message));
                }
                catch (ArgumentException exception)
                {
                    return ValidationError(exception.Message);
                }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("SetStorageQuotaPolicyEnabled");

        // 권한 감사 조회는 Tenant 관리자에게만 허용하며 감사 데이터는 읽기 전용으로 제공합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit",
            async (Guid tenantId, AuthorizationAuditQueryService service, CancellationToken token,
                string? subjectId, string? action, bool? isAllowed, DateTimeOffset? fromUtc,
                DateTimeOffset? toUtc, int page = 1, int pageSize = 50) =>
            {
                try
                {
                    var result = await service.GetPageAsync(new TenantId(tenantId), subjectId, action,
                        isAllowed, fromUtc, toUtc, page, pageSize, token);
                    return Results.Ok(new
                    {
                        result.TotalCount,
                        result.Page,
                        result.PageSize,
                        Items = result.Items.Select(x => new
                        {
                            Id = x.Id.Value,
                            x.SubjectId,
                            x.Action,
                            x.ResourceType,
                            x.Scope,
                            x.IsAllowed,
                            x.ReasonCode,
                            x.MatchedPolicyId,
                            x.OccurredAtUtc
                        })
                    });
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 무결성 체크섬은 별도 보관 후 검증 요청에 재사용합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/integrity",
            async (Guid tenantId, AuthorizationAuditIntegrityService service, CancellationToken token,
                DateTimeOffset fromUtc, DateTimeOffset toUtc) =>
            {
                try
                {
                    var result = await service.CalculateAsync(new TenantId(tenantId), fromUtc, toUtc, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/integrity/verify",
            async (Guid tenantId, VerifyAuthorizationAuditIntegrityRequest request,
                AuthorizationAuditIntegrityService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.VerifyAsync(new TenantId(tenantId), request.FromUtc,
                        request.ToUtc, request.ExpectedSha256, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 아카이브는 Tenant별로 생성하며 검증 전 원본 기록은 변경하지 않습니다.
        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archives",
            async (Guid tenantId, CreateAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.CreateAsync(new TenantId(tenantId),
                        request.FromUtc, request.ToUtc, token);
                    return Results.Json(result, statusCode: StatusCodes.Status201Created);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archives/{archiveId:guid}/verify",
            async (Guid tenantId, Guid archiveId, VerifyAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveService service, CancellationToken token) =>
            {
                var result = await service.VerifyAsync(new TenantId(tenantId), archiveId, request.ExpectedSha256, token);
                if (result.ReasonCode == "archive.not_found")
                    return Results.NotFound(new ApiError(
                        "authorization.audit.archive.not_found",
                        "Tenant 범위에서 감사 아카이브를 찾을 수 없습니다."));
                return Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archives/{archiveId:guid}",
            async (Guid tenantId, Guid archiveId, AuthorizationAuditArchiveService service,
                CancellationToken token) =>
            {
                var result = await service.DownloadAsync(new TenantId(tenantId), archiveId, token);
                return result is null
                    ? Results.NotFound(new ApiError("authorization.audit.archive.not_found",
                        "Tenant 범위에서 검증 가능한 감사 아카이브를 찾을 수 없습니다."))
                    : Results.File(result.Content, "application/json", $"{archiveId:N}.edan-audit.json");
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>()
            .WithName("DownloadAuthorizationAuditArchive");

        // 아카이브 작업의 상태와 재시도·검증 이력을 Tenant 관리자에게 제공합니다.
        // Tenant 관리자는 아카이브 예약 주기와 활성 상태를 관리합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archive-schedule",
            async (Guid tenantId, AuthorizationAuditArchiveScheduleService service, CancellationToken token) =>
                Results.Ok(await service.GetAsync(new TenantId(tenantId), token)))
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-audit/archive-schedule",
            async (Guid tenantId, UpdateAuthorizationAuditArchiveScheduleRequest request,
                AuthorizationAuditArchiveScheduleService service, CancellationToken token) =>
            {
                try
                {
                    return Results.Ok(await service.UpdateAsync(new TenantId(tenantId),
                        request.IntervalHours, request.IsEnabled, token));
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs",
            async (Guid tenantId, CreateAuthorizationAuditArchiveRequest request, HttpContext context,
                AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var result = await service.CreateAsync(new TenantId(tenantId), request.FromUtc,
                        request.ToUtc, actor, token);
                    return Results.Json(result, statusCode: StatusCodes.Status201Created);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}",
            async (Guid tenantId, Guid jobId, AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                var result = await service.GetAsync(new TenantId(tenantId), jobId, token);
                return result is null
                    ? Results.NotFound(new ApiError("authorization.audit.archive_job.not_found",
                        "Tenant 범위에서 감사 아카이브 작업을 찾을 수 없습니다."))
                    : Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archive-jobs",
            async (Guid tenantId, string? status, DateTimeOffset? fromUtc, DateTimeOffset? toUtc,
                int page, int pageSize, AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.GetPageAsync(new TenantId(tenantId), status, fromUtc, toUtc,
                        page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, token);
                    return Results.Ok(result);
                }
                catch (ArgumentOutOfRangeException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}/retry",
            async (Guid tenantId, Guid jobId, AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.RetryAsync(new TenantId(tenantId), jobId, token);
                    return result is null
                        ? Results.NotFound(new ApiError("authorization.audit.archive_job.not_found",
                            "Tenant 범위에서 감사 아카이브 작업을 찾을 수 없습니다."))
                        : Results.Ok(result);
                }
                catch (InvalidOperationException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}/verify",
            async (Guid tenantId, Guid jobId, VerifyAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.VerifyAsync(new TenantId(tenantId), jobId,
                        request.ExpectedSha256, token);
                    return result is null
                        ? Results.NotFound(new ApiError("authorization.audit.archive_job.not_found",
                            "Tenant 범위에서 감사 아카이브 작업을 찾을 수 없습니다."))
                        : Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
                catch (InvalidOperationException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 보존 정책은 Tenant 관리자만 변경하며 모든 변경은 별도 감사 이력으로 남깁니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/retention",
            async (Guid tenantId, AuthorizationAuditRetentionService service, CancellationToken token) =>
            {
                var result = await service.GetAsync(new TenantId(tenantId), token);
                return Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-audit/retention",
            async (Guid tenantId, UpdateAuthorizationAuditRetentionRequest request, HttpContext context,
                AuthorizationAuditRetentionService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var result = await service.UpdateAsync(new TenantId(tenantId), request.RetentionDays,
                        request.IsLegalHold, request.LegalHoldReason, actor, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-policies/{policyId:guid}",
            async (Guid tenantId, Guid policyId, UpdateAuthorizationPolicyRequest request, HttpContext context,
                AuthorizationPolicyService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<AuthorizationPolicyEffect>(request.Effect, true, out var effect))
                    return ValidationError("지원하지 않는 접근 정책 효과입니다.");
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                var result = await service.UpdateAsync(new TenantId(tenantId), new AuthorizationPolicyId(policyId),
                    effect, request.Priority, actor, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/authorization-policies/{policyId:guid}/enabled",
            async (Guid tenantId, Guid policyId, SetAuthorizationPolicyEnabledRequest request, HttpContext context,
                AuthorizationPolicyService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                var result = await service.SetEnabledAsync(new TenantId(tenantId), new AuthorizationPolicyId(policyId),
                    request.IsEnabled, actor, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/memberships",
            async (Guid tenantId, Guid organizationId, GrantOrganizationMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<OrganizationRole>(request.Role, true, out var role))
                    return ValidationError("지원하지 않는 조직 역할입니다.");
                var result = await service.GrantOrganizationAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.SubjectId, role, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/projects/{projectId:guid}/memberships",
            async (Guid tenantId, Guid organizationId, Guid projectId, GrantProjectMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
                    return ValidationError("지원하지 않는 프로젝트 역할입니다.");
                var result = await service.GrantProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), new ProjectId(projectId), request.SubjectId, role, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/memberships/{membershipId:guid}",
            async (Guid tenantId, Guid membershipId, UpdateMembershipRequest request, TenantMembershipService service, CancellationToken token) =>
            {
                TenantRole? role = null;
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    if (!Enum.TryParse(request.Role, true, out TenantRole parsed))
                        return ValidationError("지원하지 않는 테넌트 역할입니다.");
                    role = parsed;
                }
                var result = await service.UpdateAsync(new TenantId(tenantId), new TenantMembershipId(membershipId), role, request.Revoke, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/organizations/{organizationId:guid}/memberships/{membershipId:guid}",
            async (Guid tenantId, Guid organizationId, Guid membershipId, UpdateMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                OrganizationRole? role = null;
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    if (!Enum.TryParse(request.Role, true, out OrganizationRole parsed))
                        return ValidationError("지원하지 않는 조직 역할입니다.");
                    role = parsed;
                }
                var result = await service.UpdateOrganizationAsync(new TenantId(tenantId), new OrganizationId(organizationId),
                    new OrganizationMembershipId(membershipId), role, request.Revoke, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/organizations/{organizationId:guid}/projects/{projectId:guid}/memberships/{membershipId:guid}",
            async (Guid tenantId, Guid organizationId, Guid projectId, Guid membershipId, UpdateMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                ProjectRole? role = null;
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    if (!Enum.TryParse(request.Role, true, out ProjectRole parsed))
                        return ValidationError("지원하지 않는 프로젝트 역할입니다.");
                    role = parsed;
                }
                var result = await service.UpdateProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), new ProjectId(projectId),
                    new ProjectMembershipId(membershipId), role, request.Revoke, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 조직 생성은 Tenant 관리자만 수행하여 일반 구성원의 Tenant 구조 변경을 차단합니다.
        tenantGroup.MapPost("/{tenantId:guid}/organizations", async (Guid tenantId, CreateOrganizationRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateOrganizationAsync(new TenantId(tenantId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 프로젝트 생성은 Tenant 관리자만 수행하여 조직 하위 구조 변경을 제한합니다.
        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/projects", async (Guid tenantId, Guid organizationId, CreateProjectRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 사용자 초대는 Tenant 관리자만 수행하며 raw token은 Delivery Adapter로만 전달합니다.
        tenantGroup.MapGet("/{tenantId:guid}/invitations", async (Guid tenantId, string? search, InvitationStatus? status,
            int? page, int? pageSize, InvitationService service, CancellationToken token) =>
        {
            var result = await service.GetPageAsync(new TenantId(tenantId), search, status, page ?? 1, pageSize ?? 50, token);
            return Results.Ok(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("GetInvitations");

        tenantGroup.MapPost("/{tenantId:guid}/users/invitations", async (Guid tenantId, InviteUserRequest request, InvitationService service, CancellationToken token) =>
        {
            var lifetime = request.LifetimeDays.HasValue ? TimeSpan.FromDays(request.LifetimeDays.Value) : (TimeSpan?)null;
            var result = await service.CreateAsync(new TenantId(tenantId), request.Email, request.DisplayName, lifetime, token);
            return result.IsSuccess
                ? Results.Json(new InvitationCreatedResponse(result.Value.UserId, result.Value.Id, result.Value.UserId,
                    result.Value.TenantId, result.Value.Email, result.Value.DisplayName, result.Value.Status,
                    result.Value.CreatedAtUtc, result.Value.ExpiresAtUtc, result.Value.AcceptedAtUtc,
                    result.Value.RevokedAtUtc, result.Value.ResendCount), statusCode: StatusCodes.Status201Created)
                : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/invitations/{invitationId:guid}/resend", async (Guid tenantId, Guid invitationId,
            InvitationLifetimeRequest request, InvitationService service, CancellationToken token) =>
        {
            var lifetime = request.LifetimeDays.HasValue ? TimeSpan.FromDays(request.LifetimeDays.Value) : (TimeSpan?)null;
            var result = await service.ResendAsync(new TenantId(tenantId), new InvitationId(invitationId), lifetime, token);
            return result.IsSuccess ? Results.Ok(result.Value) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("ResendInvitation");

        tenantGroup.MapPost("/{tenantId:guid}/invitations/{invitationId:guid}/revoke", async (Guid tenantId, Guid invitationId,
            InvitationService service, CancellationToken token) =>
        {
            var result = await service.RevokeAsync(new TenantId(tenantId), new InvitationId(invitationId), token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("RevokeInvitation");

        // 자원과 정책의 Tenant 범위는 요청 본문이 아닌 선택된 Tenant 헤더와 데이터베이스 소속 검증으로 결정합니다.
        group.MapPost("/resources", async (CreateResourceRequest request, ITenantContext tenantContext, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(tenantContext.TenantId,
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreateResourceAsync(scope, request.ResourceType, request.Name, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>().AddEndpointFilter<ScopedResourceAuthorizationFilter>();

        group.MapPost("/policies", async (CreatePolicyRequest request, ITenantContext tenantContext, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(tenantContext.TenantId,
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreatePolicyAsync(scope, request.Key, request.Value, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>().AddEndpointFilter<ScopedResourceAuthorizationFilter>();

        return endpoints;
    }

    /// <summary>업무 오류 코드를 표준 HTTP 상태 코드로 변환합니다.</summary>
    private static IResult ToHttpResult(Result result)
    {
        var code = result.Error.Code;
        var status = code.EndsWith(".not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound
            : code.Contains("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict
            : code.Contains("unavailable", StringComparison.Ordinal) || code.Contains("scope_mismatch", StringComparison.Ordinal) || code.Contains("tenant_required", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return Results.Json(new ApiError(code, result.Error.Description), statusCode: status);
    }

    private static IResult ValidationError(string description) =>
        Results.Json(new ApiError("platform.validation", description), statusCode: StatusCodes.Status400BadRequest);

    private static object ToStorageQuotaPolicyResponse(StorageQuotaPolicy policy) => new
    {
        policy.Id,
        TenantId = policy.TenantId.Value,
        Scope = policy.Scope.ToString(),
        policy.TargetId,
        policy.TenantLimitBytes,
        policy.OrganizationLimitBytes,
        policy.ProjectLimitBytes,
        policy.DataSetLimitBytes,
        policy.WarningThresholdPercent,
        policy.PlanCode,
        policy.IsEnabled,
        policy.Version,
        policy.UpdatedBy,
        policy.UpdatedAtUtc
    };

    private static object ToStorageQuotaPolicyHistoryResponse(StorageQuotaPolicyChangeAuditEntry entry) => new
    {
        entry.Id,
        TenantId = entry.TenantId.Value,
        entry.PolicyId,
        entry.ActorSubjectId,
        entry.ChangeType,
        entry.BeforeJson,
        entry.AfterJson,
        entry.OccurredAtUtc
    };
}

public sealed record ApiError(string Code, string Description);
public sealed record CreateTenantRequest(string Code, string Name);
public sealed record CreateTenantDatabaseBindingRequest(string Provider, string Username, string SecretKey,
    string? ServerAddress, int? Port, string? DatabaseName, string? SchemaName);
public sealed record CreateRoutineAllowlistRequest(string Kind, string SchemaName, string RoutineName);
public sealed record ExecuteRoutineRequest(string Kind, string SchemaName, string RoutineName, Dictionary<string, string?>? Parameters);
public sealed record GrantTenantMembershipRequest(string SubjectId, string Role);
public sealed record GrantOrganizationMembershipRequest(string SubjectId, string Role);
public sealed record GrantProjectMembershipRequest(string SubjectId, string Role);
public sealed record UpdateMembershipRequest(string? Role, bool Revoke = false);
public sealed record CreateAuthorizationPolicyRequest(Guid? OrganizationId, Guid? ProjectId, string SubjectId, string Action, string ResourceType, string Effect, int Priority);
public sealed record UpdateAuthorizationPolicyRequest(string Effect, int Priority);
public sealed record SetAuthorizationPolicyEnabledRequest(bool IsEnabled);
public sealed record EvaluateAuthorizationPolicyRequest(Guid? OrganizationId, Guid? ProjectId,
    string? SubjectId, string Action, string ResourceType);
public sealed record VerifyAuthorizationAuditIntegrityRequest(DateTimeOffset FromUtc, DateTimeOffset ToUtc, string ExpectedSha256);
public sealed record CreateAuthorizationAuditArchiveRequest(DateTimeOffset FromUtc, DateTimeOffset ToUtc);
public sealed record VerifyAuthorizationAuditArchiveRequest(string? ExpectedSha256);
public sealed record UpdateAuthorizationAuditRetentionRequest(int RetentionDays, bool IsLegalHold, string? LegalHoldReason);
public sealed record UpdateAuthorizationAuditArchiveScheduleRequest(int IntervalHours, bool IsEnabled);
public sealed record CreateOrganizationRequest(string Code, string Name);
public sealed record CreateProjectRequest(string Code, string Name);
public sealed record InviteUserRequest(string Email, string DisplayName, int? LifetimeDays = null);
public sealed record InvitationLifetimeRequest(int? LifetimeDays = null);
public sealed record AcceptInvitationRequest(string Token);
public sealed record InvitationCreatedResponse(Guid Id, Guid InvitationId, Guid UserId, Guid TenantId, string Email,
    string DisplayName, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? AcceptedAtUtc, DateTimeOffset? RevokedAtUtc, int ResendCount);
public sealed record UpdateDirectoryEntryRequest(string? Name, string? Status);
public sealed record SetUserStatusRequest(string Status);
public sealed record CreateResourceRequest(Guid? OrganizationId, Guid? ProjectId, string ResourceType, string Name);
public sealed record CreatePolicyRequest(Guid? OrganizationId, Guid? ProjectId, string Key, string Value);

/// <summary>Tenant 관리자가 생성할 저장 용량 정책의 입력입니다.</summary>
public sealed record CreateStorageQuotaPolicyRequest(string Scope, Guid TargetId, long? TenantLimitBytes,
    long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes,
    int WarningThresholdPercent = 80, string? PlanCode = "Community");

/// <summary>Tenant 관리자가 변경할 저장 용량 정책의 입력입니다.</summary>
public sealed record UpdateStorageQuotaPolicyRequest(long ExpectedVersion, long? TenantLimitBytes,
    long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes,
    int WarningThresholdPercent, string PlanCode);

/// <summary>Tenant 관리자가 저장 용량 정책의 활성 상태를 변경할 입력입니다.</summary>
public sealed record SetStorageQuotaPolicyEnabledRequest(bool IsEnabled, long ExpectedVersion);
