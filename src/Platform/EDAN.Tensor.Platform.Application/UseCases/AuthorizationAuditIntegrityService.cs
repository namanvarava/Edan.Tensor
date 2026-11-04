using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>권한 감사 기록의 기간별 무결성 요약과 외부 보관용 체크섬을 생성합니다.</summary>
public sealed class AuthorizationAuditIntegrityService(AuthorizationAuditQueryService query)
{
    private const int PageSize = 100;
    private const int MaximumRecords = 100_000;
    private static readonly TimeSpan MaximumPeriod = TimeSpan.FromDays(31);

    /// <summary>Tenant 감사 기록의 정규화된 SHA-256 체크섬을 계산합니다.</summary>
    public async Task<AuthorizationAuditIntegrityResult> CalculateAsync(TenantId tenantId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        ValidateRange(fromUtc, toUtc);
        var records = new List<IntegrityRecord>();
        var firstPage = await query.GetPageAsync(tenantId, null, null, null,
            fromUtc, toUtc, 1, PageSize, cancellationToken);
        if (firstPage.TotalCount > MaximumRecords)
            throw new ArgumentException("한 번의 무결성 검증은 최대 100,000건까지 지원합니다.");

        records.AddRange(firstPage.Items.Select(IntegrityRecord.From));
        var pageCount = (int)Math.Ceiling(firstPage.TotalCount / (double)PageSize);
        for (var page = 2; page <= pageCount; page++)
        {
            var result = await query.GetPageAsync(tenantId, null, null, null,
                fromUtc, toUtc, page, PageSize, cancellationToken);
            records.AddRange(result.Items.Select(IntegrityRecord.From));
        }

        var canonicalJson = JsonSerializer.Serialize(records);
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson)));
        return new AuthorizationAuditIntegrityResult(tenantId.Value, fromUtc.ToUniversalTime(),
            toUtc.ToUniversalTime(), records.Count, digest);
    }

    /// <summary>새로 계산한 체크섬을 외부에 보관한 기준 체크섬과 상수 시간으로 비교합니다.</summary>
    public async Task<AuthorizationAuditIntegrityVerification> VerifyAsync(TenantId tenantId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        if (expectedSha256 is null || expectedSha256.Length != 64 ||
            !expectedSha256.All(Uri.IsHexDigit))
            throw new ArgumentException("기준 SHA-256 값은 64자리 16진수여야 합니다.");

        var actual = await CalculateAsync(tenantId, fromUtc, toUtc, cancellationToken);
        var expected = Convert.FromHexString(expectedSha256);
        var computed = Convert.FromHexString(actual.Sha256);
        return new AuthorizationAuditIntegrityVerification(actual,
            CryptographicOperations.FixedTimeEquals(expected, computed));
    }

    private static void ValidateRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
    {
        if (fromUtc > toUtc) throw new ArgumentException("조회 시작 시각은 종료 시각보다 늦을 수 없습니다.");
        if (toUtc - fromUtc > MaximumPeriod) throw new ArgumentException("무결성 검증 기간은 최대 31일입니다.");
    }

    private sealed record IntegrityRecord(Guid Id, string SubjectId, string Action, string ResourceType,
        string Scope, bool IsAllowed, string ReasonCode, string? MatchedPolicyId, DateTimeOffset OccurredAtUtc)
    {
        public static IntegrityRecord From(AuthorizationAuditEntry entry) => new(entry.Id.Value,
            entry.SubjectId, entry.Action, entry.ResourceType, entry.Scope, entry.IsAllowed,
            entry.ReasonCode, entry.MatchedPolicyId, entry.OccurredAtUtc.ToUniversalTime());
    }
}

/// <summary>기간별 감사 기록 건수와 외부 보관용 SHA-256 체크섬을 제공합니다.</summary>
public sealed record AuthorizationAuditIntegrityResult(Guid TenantId, DateTimeOffset FromUtc,
    DateTimeOffset ToUtc, int RecordCount, string Sha256);

/// <summary>기준 체크섬과 현재 감사 기록의 일치 여부를 제공합니다.</summary>
public sealed record AuthorizationAuditIntegrityVerification(
    AuthorizationAuditIntegrityResult Current, bool IsValid);
