using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EDAN.Tensor.Platform.Application.Abstractions;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseConsistencyChecker(ITenantDatabaseRoutineExecutor executor) : ITenantDatabaseConsistencyChecker
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<TenantDatabaseConsistencyResult> VerifyAsync(
        IReadOnlyList<TenantDatabaseConsistencyCheck> checks,
        CancellationToken cancellationToken = default)
    {
        if (checks.Count < 2)
            throw new ArgumentException("정합성 검증에는 두 개 이상의 Provider 결과가 필요합니다.", nameof(checks));

        var results = new List<TenantDatabaseConsistencyProviderResult>(checks.Count);
        foreach (var check in checks)
        {
            try
            {
                var execution = check.BindingId is { } bindingId
                    ? await executor.ExecuteAsync(bindingId, check.TenantId, check.Kind, check.SchemaName, check.RoutineName, check.Parameters, cancellationToken)
                    : await executor.ExecuteAsync(check.TenantId, check.Kind, check.SchemaName, check.RoutineName, check.Parameters, cancellationToken);
                var normalized = Normalize(execution.Rows);
                results.Add(new(check.TenantId, true, Fingerprint(normalized), execution.AffectedRows,
                    execution.Rows, null, check.BindingId));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                results.Add(new(check.TenantId, false, string.Empty, 0,
                    Array.Empty<IReadOnlyDictionary<string, object?>>(), ex.Message, check.BindingId));
            }
        }

        var successful = results.Where(x => x.Succeeded).ToArray();
        var status = successful.Length != results.Count
            ? "Unknown"
            : successful.Select(x => x.Fingerprint).Distinct(StringComparer.Ordinal).Count() == 1
                ? "Consistent"
                : "Inconsistent";
        var fingerprint = successful.Length == 0 ? string.Empty : Fingerprint(
            string.Join("|", successful.Select(x => $"{x.TenantId.Value:D}:{x.Fingerprint}").OrderBy(x => x, StringComparer.Ordinal)));
        return new(status, fingerprint, results);
    }

    private static string Normalize(IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var normalizedRows = rows.Select(row => row
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => NormalizeValue(x.Value), StringComparer.OrdinalIgnoreCase))
            .OrderBy(row => JsonSerializer.Serialize(row, JsonOptions), StringComparer.Ordinal)
            .ToArray();
        return JsonSerializer.Serialize(normalizedRows, JsonOptions);
    }

    private static object? NormalizeValue(object? value) => value switch
    {
        null => null,
        DateTime dateTime => dateTime.ToUniversalTime().ToString("O"),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToUniversalTime().ToString("O"),
        byte[] bytes => Convert.ToHexString(bytes),
        _ => value
    };

    private static string Fingerprint(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
