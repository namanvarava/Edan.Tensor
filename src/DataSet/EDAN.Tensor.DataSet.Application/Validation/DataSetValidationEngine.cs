using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EDAN.Tensor.DataSet.Application.Validation;

/// <summary>최상위 필드의 형식·범위·필수값·중복 규칙을 행 단위로 평가합니다.</summary>
public static class DataSetValidationEngine
{
    /// <summary>한 행을 평가하고 중복 검사용 해시 관찰값을 반환합니다.</summary>
    public static DataSetValidationRowResult ValidateRow(
        DataSetValidationRecord record,
        DataSetValidationProfileDefinition profile,
        ISet<string> seenKeys)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(seenKeys);

        var findings = new List<DataSetValidationFinding>();
        var observations = new List<DataSetValidationDuplicateObservation>();
        foreach (var field in profile.Fields)
        {
            var cell = record.Values.TryGetValue(field.Name, out var value)
                ? value
                : DataSetValidationCell.Missing;
            if (cell.Kind is DataSetValidationCellKind.Missing or DataSetValidationCellKind.Null
                || string.IsNullOrWhiteSpace(cell.Text))
            {
                if (field.Required)
                    findings.Add(new DataSetValidationFinding(field.RequiredCode, field.Severity,
                        field.Name, "필수 필드 값이 없습니다."));
                continue;
            }

            if (!TryConvert(cell, field.Type, out var converted))
            {
                findings.Add(new DataSetValidationFinding(field.TypeCode, field.Severity,
                    field.Name, "필드 값의 형식이 프로필과 일치하지 않습니다."));
                continue;
            }

            if (field.Type == DataSetValidationFieldType.String)
            {
                var length = converted.Text!.Length;
                if (field.MinLength is int minLength && length < minLength)
                    findings.Add(new DataSetValidationFinding(field.TypeCode + ".minLength", field.Severity,
                        field.Name, "문자열 길이가 최소 제한보다 짧습니다."));
                if (field.MaxLength is int maxLength && length > maxLength)
                    findings.Add(new DataSetValidationFinding(field.TypeCode + ".maxLength", field.Severity,
                        field.Name, "문자열 길이가 최대 제한을 초과했습니다."));
            }
            else if (converted.Number is decimal number)
            {
                if (field.Min is decimal min && number < min)
                    findings.Add(new DataSetValidationFinding(field.TypeCode + ".min", field.Severity,
                        field.Name, "수치가 최소 범위보다 작습니다."));
                if (field.Max is decimal max && number > max)
                    findings.Add(new DataSetValidationFinding(field.TypeCode + ".max", field.Severity,
                        field.Name, "수치가 최대 범위를 초과했습니다."));
            }
        }

        foreach (var duplicate in profile.DuplicateSets)
        {
            var values = duplicate.Fields.Select(field =>
                record.Values.TryGetValue(field, out var cell) ? cell : DataSetValidationCell.Missing).ToArray();
            if (values.Any(cell => cell.Kind is DataSetValidationCellKind.Missing
                or DataSetValidationCellKind.Null or DataSetValidationCellKind.Unsupported
                || string.IsNullOrWhiteSpace(cell.Text)))
                continue;

            var canonical = string.Join("\u001f", values.Select(cell =>
                ((int)cell.Kind).ToString(CultureInfo.InvariantCulture) + ":" + cell.Text!.Trim()));
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
            var key = duplicate.Code + "\u001f" + hash;
            if (!seenKeys.Add(key))
            {
                findings.Add(new DataSetValidationFinding(duplicate.Code, duplicate.Severity, null,
                    "중복 규칙에 해당하는 행이 이미 존재합니다."));
            }
            else
            {
                observations.Add(new DataSetValidationDuplicateObservation(duplicate.Code, hash));
            }
        }

        return new DataSetValidationRowResult(findings, observations);
    }

    private static bool TryConvert(DataSetValidationCell cell, DataSetValidationFieldType type,
        out DataSetValidationConvertedValue converted)
    {
        converted = default;
        var text = cell.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return false;
        switch (type)
        {
            case DataSetValidationFieldType.String when cell.Kind == DataSetValidationCellKind.String:
                converted = new(text, null);
                return true;
            case DataSetValidationFieldType.Integer:
                if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var integer)
                    && decimal.Truncate(integer) == integer)
                {
                    converted = new(text, integer);
                    return true;
                }
                return false;
            case DataSetValidationFieldType.Decimal:
                if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    converted = new(text, number);
                    return true;
                }
                return false;
            case DataSetValidationFieldType.Boolean:
                if (bool.TryParse(text, out _))
                {
                    converted = new(text, null);
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private readonly record struct DataSetValidationConvertedValue(string? Text, decimal? Number);
}

/// <summary>행 검증 결과는 원문 값을 제외한 결과만 포함합니다.</summary>
public sealed record DataSetValidationRowResult(
    IReadOnlyList<DataSetValidationFinding> Findings,
    IReadOnlyList<DataSetValidationDuplicateObservation> DuplicateObservations);
