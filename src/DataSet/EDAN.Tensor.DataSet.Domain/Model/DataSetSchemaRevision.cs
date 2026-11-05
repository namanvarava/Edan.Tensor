using System.Text.Json;

namespace EDAN.Tensor.DataSet.Domain.Model;

/// <summary>DataSet에 연결된 변경 불가능한 JSON Schema 버전입니다.</summary>
public sealed record DataSetSchemaRevision
{
    private DataSetSchemaRevision(int version, string schemaJson, DateTimeOffset createdAtUtc)
    {
        Version = version;
        SchemaJson = schemaJson;
        CreatedAtUtc = createdAtUtc;
    }

    public int Version { get; }
    public string SchemaJson { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>양의 버전과 JSON 객체 형태의 스키마 문서를 검증하여 생성합니다.</summary>
    public static DataSetSchemaRevision Create(int version, string schemaJson, DateTimeOffset createdAtUtc)
    {
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version), "스키마 버전은 1 이상이어야 합니다.");
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);
        if (schemaJson.Length > 1_048_576)
            throw new ArgumentException("스키마 문서는 1 MiB를 초과할 수 없습니다.", nameof(schemaJson));

        using var document = JsonDocument.Parse(schemaJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("JSON Schema 문서의 최상위 값은 객체여야 합니다.", nameof(schemaJson));
        return new DataSetSchemaRevision(version, schemaJson, createdAtUtc);
    }
}
