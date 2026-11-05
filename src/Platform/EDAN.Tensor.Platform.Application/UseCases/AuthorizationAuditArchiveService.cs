using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>권한 감사 기록을 검증 가능한 독립 아카이브로 생성하고 확인합니다.</summary>
public sealed class AuthorizationAuditArchiveService(
    IAuthorizationAuditRepository audits,
    IAuthorizationAuditArchiveStore archives)
{
    private const int MaximumRecordCount = 100_000;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>기간 내 감사 기록을 Tenant 경계로 추출하고 SHA-256 아카이브를 저장합니다.</summary>
    public async Task<AuthorizationAuditArchiveResult> CreateAsync(TenantId tenantId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        ValidateWindow(fromUtc, toUtc);
        var records = await audits.GetForIntegrityAsync(tenantId, fromUtc, toUtc, cancellationToken);
        if (records.Count > MaximumRecordCount)
            throw new ArgumentException("아카이브 대상은 최대 100,000건까지 허용합니다.");

        var payload = new ArchivePayload(
            1,
            tenantId.Value,
            fromUtc.ToUniversalTime(),
            toUtc.ToUniversalTime(),
            records.Select(ToRecord).ToArray());
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
        var checksum = Convert.ToHexString(SHA256.HashData(payloadBytes));
        var archiveId = Guid.NewGuid();
        var document = new ArchiveDocument(archiveId, checksum, payload);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);

        await archives.WriteAsync(archiveId, bytes, cancellationToken);
        return new AuthorizationAuditArchiveResult(archiveId, tenantId.Value,
            payload.FromUtc, payload.ToUtc, records.Count, checksum, bytes.Length);
    }

    /// <summary>아카이브 내부 체크섬과 선택적 외부 기준 체크섬을 비교합니다.</summary>
    public async Task<AuthorizationAuditArchiveVerification> VerifyAsync(Guid archiveId,
        string? expectedSha256 = null, CancellationToken cancellationToken = default)
    {
        if (archiveId == Guid.Empty)
            throw new ArgumentException("아카이브 식별자는 비어 있을 수 없습니다.");
        if (expectedSha256 is not null && (expectedSha256.Length != 64
            || expectedSha256.Any(x => !Uri.IsHexDigit(x))))
            throw new ArgumentException("기준 체크섬은 64자리 16진수여야 합니다.");

        var bytes = await archives.ReadAsync(archiveId, cancellationToken);
        if (bytes is null)
            return new AuthorizationAuditArchiveVerification(archiveId, false, "archive.not_found", null);

        try
        {
            var document = JsonSerializer.Deserialize<ArchiveDocument>(bytes, JsonOptions);
            if (document is null || document.ArchiveId != archiveId || document.Payload.SchemaVersion != 1)
                return new AuthorizationAuditArchiveVerification(archiveId, false, "archive.invalid_format", null);

            var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(document.Payload, JsonOptions);
            var actual = SHA256.HashData(payloadBytes);
            var stored = Convert.FromHexString(document.Sha256);
            var internalValid = stored.Length == actual.Length
                && CryptographicOperations.FixedTimeEquals(stored, actual);
            var externalValid = expectedSha256 is null
                || CryptographicOperations.FixedTimeEquals(actual, Convert.FromHexString(expectedSha256));
            return new AuthorizationAuditArchiveVerification(archiveId, internalValid && externalValid,
                internalValid ? (externalValid ? "archive.verified" : "archive.external_checksum_mismatch")
                    : "archive.checksum_mismatch", document.Sha256);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or ArgumentException)
        {
            return new AuthorizationAuditArchiveVerification(archiveId, false, "archive.invalid_format", null);
        }
    }

    private static void ValidateWindow(DateTimeOffset fromUtc, DateTimeOffset toUtc)
    {
        if (fromUtc > toUtc)
            throw new ArgumentException("시작 시각은 종료 시각보다 늦을 수 없습니다.");
        if (toUtc - fromUtc > TimeSpan.FromDays(31))
            throw new ArgumentException("아카이브 기간은 최대 31일입니다.");
    }

    private static ArchiveRecord ToRecord(AuthorizationAuditEntry entry) => new(
        entry.Id.Value, entry.TenantId.Value, entry.SubjectId, entry.Action, entry.ResourceType,
        entry.Scope, entry.IsAllowed, entry.ReasonCode, entry.MatchedPolicyId, entry.OccurredAtUtc);

    private sealed record ArchivePayload(int SchemaVersion, Guid TenantId, DateTimeOffset FromUtc,
        DateTimeOffset ToUtc, IReadOnlyList<ArchiveRecord> Records);
    private sealed record ArchiveDocument(Guid ArchiveId, string Sha256, ArchivePayload Payload);
    private sealed record ArchiveRecord(Guid Id, Guid TenantId, string SubjectId, string Action,
        string ResourceType, string Scope, bool IsAllowed, string ReasonCode,
        string? MatchedPolicyId, DateTimeOffset OccurredAtUtc);
}

/// <summary>감사 아카이브 생성 결과와 외부 보관에 필요한 기준 정보를 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveResult(Guid ArchiveId, Guid TenantId,
    DateTimeOffset FromUtc, DateTimeOffset ToUtc, int RecordCount, string Sha256, int SizeBytes);

/// <summary>감사 아카이브의 내부 및 외부 체크섬 검증 결과를 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveVerification(Guid ArchiveId, bool IsValid,
    string ReasonCode, string? Sha256);
