using System.Text;
using System.Text.Json;

namespace EDAN.Tensor.DataSet.Application.Validation;

/// <summary>승인된 CSV와 JSON 객체 배열 콘텐츠를 제한된 메모리 범위에서 읽습니다.</summary>
public static class DataSetValidationContentReader
{
    /// <summary>콘텐츠 형식에 맞는 최상위 레코드 목록을 읽습니다.</summary>
    public static async Task<IReadOnlyList<DataSetValidationRecord>> ReadAsync(
        Stream content, string contentFormat, int maxRecords, int maxFieldLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentFormat);
        if (maxRecords is < 1 or > 1_000_000) throw new ArgumentOutOfRangeException(nameof(maxRecords));
        if (maxFieldLength is < 1 or > 65_536) throw new ArgumentOutOfRangeException(nameof(maxFieldLength));

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        using var reader = new StreamReader(content, encoding, detectEncodingFromByteOrderMarks: true);
        string text;
        try
        {
            text = await reader.ReadToEndAsync(cancellationToken);
        }
        catch (DecoderFallbackException exception)
        {
            throw new DataSetValidationInputException("dataset.validation.utf8_invalid",
                "검증 콘텐츠가 유효한 UTF-8 문서가 아닙니다.", exception);
        }

        try
        {
            return contentFormat.Trim().ToLowerInvariant() switch
            {
                "csv" => ReadCsv(text, maxRecords, maxFieldLength),
                "json-array" => ReadJsonArray(text, maxRecords, maxFieldLength),
                _ => throw new DataSetValidationInputException(
                    "dataset.validation.format_unsupported", "지원하지 않는 DataSet 콘텐츠 형식입니다.")
            };
        }
        catch (JsonException exception)
        {
            throw new DataSetValidationInputException("dataset.validation.json_invalid",
                "검증 콘텐츠가 유효한 JSON 문서가 아닙니다.", exception);
        }
    }

    private static IReadOnlyList<DataSetValidationRecord> ReadCsv(
        string text, int maxRecords, int maxFieldLength)
    {
        var rows = ParseCsvRows(text, maxFieldLength);
        if (rows.Count == 0 || rows[0].Count == 0)
            throw new DataSetValidationInputException("dataset.validation.csv_header_required",
                "CSV는 하나 이상의 헤더 필드를 포함해야 합니다.");
        var headers = rows[0].Select(x => x.Trim()).ToArray();
        if (headers.Any(string.IsNullOrWhiteSpace)
            || headers.Distinct(StringComparer.Ordinal).Count() != headers.Length)
            throw new DataSetValidationInputException("dataset.validation.csv_header_invalid",
                "CSV 헤더는 비어 있지 않은 고유 필드명이어야 합니다.");
        if (rows.Count - 1 > maxRecords)
            throw new DataSetValidationInputException("dataset.validation.max_records_exceeded",
                "검증 프로필의 최대 레코드 수를 초과했습니다.");

        var records = new List<DataSetValidationRecord>(rows.Count - 1);
        for (var index = 1; index < rows.Count; index++)
        {
            var row = rows[index];
            if (row.Count > headers.Length)
                throw new DataSetValidationInputException("dataset.validation.csv_column_count",
                    "CSV 데이터 행의 필드 수가 헤더와 다릅니다.");
            var values = new Dictionary<string, DataSetValidationCell>(StringComparer.Ordinal);
            for (var column = 0; column < headers.Length; column++)
            {
                var value = column < row.Count ? row[column] : null;
                values[headers[column]] = value is null
                    ? DataSetValidationCell.Missing
                    : new DataSetValidationCell(DataSetValidationCellKind.String, value);
            }
            records.Add(new DataSetValidationRecord(index, values));
        }
        return records;
    }

    private static IReadOnlyList<DataSetValidationRecord> ReadJsonArray(
        string text, int maxRecords, int maxFieldLength)
    {
        using var document = JsonDocument.Parse(text);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new DataSetValidationInputException("dataset.validation.json_array_required",
                "JSON 콘텐츠의 최상위 값은 객체 배열이어야 합니다.");
        var elements = document.RootElement.EnumerateArray().ToArray();
        if (elements.Length > maxRecords)
            throw new DataSetValidationInputException("dataset.validation.max_records_exceeded",
                "검증 프로필의 최대 레코드 수를 초과했습니다.");

        var records = new List<DataSetValidationRecord>(elements.Length);
        for (var index = 0; index < elements.Length; index++)
        {
            var element = elements[index];
            if (element.ValueKind != JsonValueKind.Object)
                throw new DataSetValidationInputException("dataset.validation.json_object_required",
                    "JSON 배열의 각 항목은 객체여야 합니다.");
            var values = new Dictionary<string, DataSetValidationCell>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                var value = property.Value;
                var cell = value.ValueKind switch
                {
                    JsonValueKind.Null => DataSetValidationCell.Null,
                    JsonValueKind.String => CreateStringCell(value.GetString() ?? string.Empty, maxFieldLength),
                    JsonValueKind.Number => CreateCell(DataSetValidationCellKind.Number, value.GetRawText(), maxFieldLength),
                    JsonValueKind.True or JsonValueKind.False =>
                        CreateCell(DataSetValidationCellKind.Boolean, value.GetRawText(), maxFieldLength),
                    _ => DataSetValidationCell.Unsupported
                };
                values[property.Name] = cell;
            }
            records.Add(new DataSetValidationRecord(index + 1, values));
        }
        return records;
    }

    private static DataSetValidationCell CreateStringCell(string value, int maxFieldLength) =>
        CreateCell(DataSetValidationCellKind.String, value, maxFieldLength);

    private static DataSetValidationCell CreateCell(
        DataSetValidationCellKind kind, string value, int maxFieldLength)
    {
        if (value.Length > maxFieldLength)
            throw new DataSetValidationInputException("dataset.validation.field_length_exceeded",
                "검증 프로필의 필드 길이 제한을 초과했습니다.");
        return new DataSetValidationCell(kind, value);
    }

    private static List<List<string>> ParseCsvRows(string text, int maxFieldLength)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var fieldClosed = false;

        void AddField()
        {
            if (field.Length > maxFieldLength)
                throw new DataSetValidationInputException("dataset.validation.field_length_exceeded",
                    "검증 프로필의 필드 길이 제한을 초과했습니다.");
            row.Add(field.ToString());
            field.Clear();
            fieldClosed = false;
        }

        void AddRow()
        {
            if (row.Count > 0 || field.Length > 0)
            {
                AddField();
                rows.Add(row);
                row = [];
            }
        }

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (quoted)
            {
                if (character == '\"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '\"')
                    {
                        field.Append('\"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                        fieldClosed = true;
                    }
                }
                else
                    field.Append(character);
                continue;
            }

            if (fieldClosed)
            {
                if (character == ',') { AddField(); continue; }
                if (character is '\r' or '\n')
                {
                    if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n') index++;
                    AddRow();
                    continue;
                }
                if (!char.IsWhiteSpace(character))
                    throw new DataSetValidationInputException("dataset.validation.csv_quote",
                        "CSV 인용 필드 뒤에 허용되지 않은 문자가 있습니다.");
                continue;
            }

            if (character == ',' ) { AddField(); continue; }
            if (character is '\r' or '\n')
            {
                if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n') index++;
                AddRow();
                continue;
            }
            if (character == '\"' && field.Length == 0)
            {
                quoted = true;
                continue;
            }
            field.Append(character);
        }

        if (quoted)
            throw new DataSetValidationInputException("dataset.validation.csv_quote",
                "CSV 인용 필드가 닫히지 않았습니다.");
        if (field.Length > 0 || row.Count > 0)
            AddRow();
        return rows;
    }
}

/// <summary>검증 콘텐츠에서 읽은 한 행입니다.</summary>
public sealed record DataSetValidationRecord(
    int RowNumber,
    IReadOnlyDictionary<string, DataSetValidationCell> Values);

/// <summary>검증 콘텐츠의 필드 값과 원천 형식입니다.</summary>
public sealed record DataSetValidationCell(DataSetValidationCellKind Kind, string? Text)
{
    public static DataSetValidationCell Missing { get; } = new(DataSetValidationCellKind.Missing, null);
    public static DataSetValidationCell Null { get; } = new(DataSetValidationCellKind.Null, null);
    public static DataSetValidationCell Unsupported { get; } = new(DataSetValidationCellKind.Unsupported, null);
}

/// <summary>검증 콘텐츠 필드의 원천 형식입니다.</summary>
public enum DataSetValidationCellKind { Missing, Null, String, Number, Boolean, Unsupported }

/// <summary>검증 입력이 계약 또는 자원 제한을 위반했습니다.</summary>
public sealed class DataSetValidationInputException : Exception
{
    public DataSetValidationInputException(string code, string message)
        : base(message) => Code = code;

    public DataSetValidationInputException(string code, string message, Exception innerException)
        : base(message, innerException) => Code = code;

    public string Code { get; }
}
