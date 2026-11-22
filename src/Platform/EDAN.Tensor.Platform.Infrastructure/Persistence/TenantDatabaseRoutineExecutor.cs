using System.Data;
using System.Text.RegularExpressions;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseRoutineExecutor(PlatformDbContext platformDb, ITenantDatabaseResolver resolver) : ITenantDatabaseRoutineExecutor
{
    private static readonly Regex Identifier = new("^[A-Za-z_][A-Za-z0-9_$#]*(\\.[A-Za-z_][A-Za-z0-9_$#]*)?$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public async Task<TenantDatabaseRoutineExecutionResult> ExecuteAsync(TenantId tenantId, TenantDatabaseRoutineKind kind, string schemaName, string routineName, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default)
    {
        if (!Identifier.IsMatch(schemaName) || !Identifier.IsMatch(routineName))
            throw new ArgumentException("루틴 식별자는 안전한 영문 식별자여야 합니다.");
        var allowed = await platformDb.TenantDatabaseRoutineAllowlists.AnyAsync(x => x.TenantId == tenantId && x.Kind == kind && x.SchemaName == schemaName && x.RoutineName == routineName && x.IsEnabled, cancellationToken);
        if (!allowed) throw new UnauthorizedAccessException("허용 목록에 등록되지 않은 루틴입니다.");
        var resolution = await resolver.ResolveAsync(tenantId, cancellationToken);
        if (resolution.Provider == TenantDatabaseProvider.SQLite)
            throw new NotSupportedException("SQLite는 저장 프로시저/함수를 지원하지 않습니다.");
        await using var connection = await new TenantDatabaseConnectionFactory(new StaticResolver(resolution)).OpenAsync(tenantId, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        var names = parameters.Keys.Select((_, i) => "@p" + i).ToArray();
        command.CommandText = resolution.Provider == TenantDatabaseProvider.Oracle
            ? $"BEGIN {schemaName}.{routineName}({string.Join(", ", names)}); END;"
            : resolution.Provider == TenantDatabaseProvider.SqlServer
            ? $"EXEC [{schemaName}].[{routineName}] {string.Join(", ", names)}"
            : kind == TenantDatabaseRoutineKind.Function
                ? $"SELECT * FROM \"{schemaName}\".\"{routineName}\"({string.Join(", ", names)})"
                : $"CALL \"{schemaName}\".\"{routineName}\"({string.Join(", ", names)})";
        foreach (var value in parameters.Values)
        {
            var parameter = command.CreateParameter(); parameter.ParameterName = names[command.Parameters.Count]; parameter.Value = (object?)value ?? DBNull.Value; command.Parameters.Add(parameter);
        }
        var rows = new List<IReadOnlyDictionary<string, object?>>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
        }
        catch (InvalidOperationException) when (kind == TenantDatabaseRoutineKind.Procedure)
        {
            return new(0, rows);
        }
        return new(0, rows);
    }

    private sealed class StaticResolver(TenantDatabaseResolution value) : ITenantDatabaseResolver
    {
        public Task<TenantDatabaseResolution> ResolveAsync(TenantId _, CancellationToken __ = default) => Task.FromResult(value);
    }
}
