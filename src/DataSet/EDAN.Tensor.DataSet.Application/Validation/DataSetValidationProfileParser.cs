using System.Text.Json;

namespace EDAN.Tensor.DataSet.Application.Validation;

/// <summary>명시적인 v1 검증 프로필 문서를 안전한 규칙 객체로 변환합니다.</summary>
public static class DataSetValidationProfileParser
{
    private const int DefaultMaxRecords = 100_000;
    private const int DefaultMaxFieldLength = 4_096;
    private const int DefaultMaxDetailItems = 1_000;

    /// <summary>프로필 JSON의 필드·중복 규칙과 자원 제한을 검증하여 반환합니다.</summary>
    public static DataSetValidationProfileDefinition Parse(string definitionJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionJson);
        try
        {
            using var document = JsonDocument.Parse(definitionJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("검증 프로필의 최상위 값은 객체여야 합니다.", nameof(definitionJson));

            var root = document.RootElement;
            var fieldsElement = RequiredProperty(root, "fields", JsonValueKind.Array);
            var fields = fieldsElement.EnumerateArray().Select(ParseField).ToArray();
            if (fields.Length == 0 || fields.Length > 256)
                throw new ArgumentException("검증 프로필은 1~256개의 필드를 포함해야 합니다.", nameof(definitionJson));
            if (fields.Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != fields.Length)
                throw new ArgumentException("검증 프로필의 필드명은 중복될 수 없습니다.", nameof(definitionJson));

            var names = fields.Select(x => x.Name).ToHashSet(StringComparer.Ordinal);
            var duplicateSets = root.TryGetProperty("duplicateSets", out var duplicateElement)
                ? ParseDuplicateSets(duplicateElement, names)
                : [];
            var limits = root.TryGetProperty("limits", out var limitsElement)
                ? ParseLimits(limitsElement)
                : (MaxRecords: DefaultMaxRecords, MaxFieldLength: DefaultMaxFieldLength,
                    MaxDetailItems: DefaultMaxDetailItems);

            return new DataSetValidationProfileDefinition(fields, duplicateSets,
                limits.MaxRecords, limits.MaxFieldLength, limits.MaxDetailItems);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("검증 프로필 JSON 형식이 올바르지 않습니다.", nameof(definitionJson), exception);
        }
    }

    private static DataSetValidationFieldRule ParseField(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("fields 항목은 객체여야 합니다.");
        var name = RequiredString(element, "name", 128);
        if (name.Contains('.', StringComparison.Ordinal) || name.Contains('/', StringComparison.Ordinal))
            throw new ArgumentException($"필드명은 최상위 필드명이어야 합니다: {name}");

        var typeText = RequiredString(element, "type", 16).ToLowerInvariant();
        var type = typeText switch
        {
            "string" => DataSetValidationFieldType.String,
            "integer" => DataSetValidationFieldType.Integer,
            "decimal" => DataSetValidationFieldType.Decimal,
            "boolean" => DataSetValidationFieldType.Boolean,
            _ => throw new ArgumentException($"지원하지 않는 필드 형식입니다: {typeText}")
        };
        var required = OptionalBoolean(element, "required") ?? false;
        var severity = ParseSeverity(element, "severity", DataSetValidationSeverity.Error);
        var minLength = OptionalInt(element, "minLength", 0, 65_536);
        var maxLength = OptionalInt(element, "maxLength", 0, 65_536);
        var min = OptionalDecimal(element, "min");
        var max = OptionalDecimal(element, "max");
        if (minLength.HasValue && maxLength.HasValue && minLength > maxLength)
            throw new ArgumentException($"필드의 minLength가 maxLength보다 클 수 없습니다: {name}");
        if (min.HasValue && max.HasValue && min > max)
            throw new ArgumentException($"필드의 min이 max보다 클 수 없습니다: {name}");
        if (type != DataSetValidationFieldType.String && (minLength.HasValue || maxLength.HasValue))
            throw new ArgumentException($"문자열이 아닌 필드에는 길이 제한을 지정할 수 없습니다: {name}");
        if (type is not (DataSetValidationFieldType.Integer or DataSetValidationFieldType.Decimal)
            && (min.HasValue || max.HasValue))
            throw new ArgumentException($"수치가 아닌 필드에는 수치 범위를 지정할 수 없습니다: {name}");
        if (type == DataSetValidationFieldType.Integer &&
            (min is decimal minInteger && minInteger != decimal.Truncate(minInteger)
                || max is decimal maxInteger && maxInteger != decimal.Truncate(maxInteger)))
            throw new ArgumentException($"정수 필드의 범위는 정수여야 합니다: {name}");

        var codePrefix = "field." + name;
        return new DataSetValidationFieldRule(name, type, required, severity,
            codePrefix + ".required", codePrefix + ".type", minLength, maxLength, min, max);
    }

    private static IReadOnlyList<DataSetValidationDuplicateRule> ParseDuplicateSets(
        JsonElement element, IReadOnlySet<string> fieldNames)
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("duplicateSets는 배열이어야 합니다.");
        var rules = new List<DataSetValidationDuplicateRule>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("duplicateSets 항목은 객체여야 합니다.");
            if (!item.TryGetProperty("fields", out var fieldsElement) || fieldsElement.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("중복 규칙은 fields 배열을 포함해야 합니다.");
            var fields = fieldsElement.EnumerateArray().Select(x =>
            {
                if (x.ValueKind != JsonValueKind.String)
                    throw new ArgumentException("중복 규칙의 필드명은 문자열이어야 합니다.");
                var field = x.GetString()!.Trim();
                if (field.Length is 0 or > 128 || !fieldNames.Contains(field))
                    throw new ArgumentException($"중복 규칙의 필드가 프로필에 없습니다: {field}");
                return field;
            }).ToArray();
            if (fields.Length is < 1 or > 8 || fields.Distinct(StringComparer.Ordinal).Count() != fields.Length)
                throw new ArgumentException("중복 규칙은 1~8개의 서로 다른 필드를 포함해야 합니다.");
            var severity = ParseSeverity(item, "severity", DataSetValidationSeverity.Error);
            var code = item.TryGetProperty("code", out _)
                ? RequiredString(item, "code", 200)
                : "duplicate." + string.Join(".", fields);
            rules.Add(new DataSetValidationDuplicateRule(fields, severity, code));
        }
        if (rules.Select(x => x.Code).Distinct(StringComparer.Ordinal).Count() != rules.Count)
            throw new ArgumentException("중복 규칙 코드는 중복될 수 없습니다.");
        return rules;
    }

    private static (int MaxRecords, int MaxFieldLength, int MaxDetailItems) ParseLimits(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("limits는 객체여야 합니다.");
        return (
            OptionalInt(element, "maxRecords", 1, 1_000_000) ?? DefaultMaxRecords,
            OptionalInt(element, "maxFieldLength", 1, 65_536) ?? DefaultMaxFieldLength,
            OptionalInt(element, "maxDetailItems", 0, 10_000) ?? DefaultMaxDetailItems);
    }

    private static JsonElement RequiredProperty(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != kind)
            throw new ArgumentException($"{name} 속성이 필요하며 {kind} 형식이어야 합니다.");
        return value;
    }

    private static string RequiredString(JsonElement element, string name, int maxLength)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"{name} 속성은 문자열이어야 합니다.");
        var result = value.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new ArgumentException($"{name} 속성은 1~{maxLength}자여야 합니다.");
        return result;
    }

    private static bool? OptionalBoolean(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new ArgumentException($"{name} 속성은 불리언이어야 합니다.");
        return value.GetBoolean();
    }

    private static int? OptionalInt(JsonElement element, string name, int min, int max)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var result) || result < min || result > max)
            throw new ArgumentException($"{name} 속성은 {min}~{max} 범위의 정수여야 합니다.");
        return result;
    }

    private static decimal? OptionalDecimal(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var result)
            || decimal.Round(result, 12) != result)
            throw new ArgumentException($"{name} 속성은 소수점 12자리 이내의 수치여야 합니다.");
        return result;
    }

    private static DataSetValidationSeverity ParseSeverity(JsonElement element, string name,
        DataSetValidationSeverity defaultValue)
    {
        if (!element.TryGetProperty(name, out var value)) return defaultValue;
        if (value.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"{name} 속성은 문자열이어야 합니다.");
        return value.GetString()?.Trim().ToLowerInvariant() switch
        {
            "error" => DataSetValidationSeverity.Error,
            "warning" => DataSetValidationSeverity.Warning,
            _ => throw new ArgumentException($"{name} 속성은 Error 또는 Warning이어야 합니다.")
        };
    }
}
