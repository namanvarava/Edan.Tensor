using System.Text;
using EDAN.Tensor.DataSet.Application.Validation;
using Xunit;

namespace EDAN.Tensor.DataSet.Application.Tests;

public sealed class DataSetValidationTests
{
    [Fact]
    public void ProfileParser_SupportsApprovedTopLevelRulesAndLimits()
    {
        var profile = DataSetValidationProfileParser.Parse("""
            {
              "fields": [
                { "name": "id", "type": "integer", "required": true, "min": 1 },
                { "name": "name", "type": "string", "maxLength": 20 },
                { "name": "active", "type": "boolean" }
              ],
              "duplicateSets": [{ "fields": ["id"], "severity": "warning" }],
              "limits": { "maxRecords": 10, "maxFieldLength": 50, "maxDetailItems": 2 }
            }
            """);

        Assert.Equal(3, profile.Fields.Count);
        Assert.Single(profile.DuplicateSets);
        Assert.Equal(10, profile.MaxRecords);
        Assert.Equal(DataSetValidationSeverity.Warning, profile.DuplicateSets[0].Severity);
    }

    [Fact]
    public void ProfileParser_RejectsNestedFieldPaths()
    {
        var exception = Assert.Throws<ArgumentException>(() => DataSetValidationProfileParser.Parse(
            "{\"fields\":[{\"name\":\"customer.id\",\"type\":\"string\"}]}"));

        Assert.Contains("최상위", exception.Message);
    }

    [Fact]
    public async Task ContentReader_ReadsCsvQuotesAndJsonObjectArrays()
    {
        await using var csv = new MemoryStream(Encoding.UTF8.GetBytes("id,name\r\n1,\"A, B\"\r\n2,C\r\n"));
        var csvRows = await DataSetValidationContentReader.ReadAsync(csv, "csv", 10, 50);
        Assert.Equal("A, B", csvRows[0].Values["name"].Text);

        await using var json = new MemoryStream(Encoding.UTF8.GetBytes("[{\"id\":1,\"active\":true},{\"id\":2,\"active\":false}]"));
        var jsonRows = await DataSetValidationContentReader.ReadAsync(json, "json-array", 10, 50);
        Assert.Equal(DataSetValidationCellKind.Number, jsonRows[0].Values["id"].Kind);
        Assert.Equal("true", jsonRows[0].Values["active"].Text);
    }

    [Fact]
    public void EngineReportsRequiredTypeRangeAndDuplicateWithoutRawValues()
    {
        var profile = DataSetValidationProfileParser.Parse("""
            {
              "fields": [
                { "name": "id", "type": "integer", "required": true, "min": 1 },
                { "name": "name", "type": "string", "required": true }
              ],
              "duplicateSets": [{ "fields": ["id"] }]
            }
            """);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var first = DataSetValidationEngine.ValidateRow(
            new DataSetValidationRecord(1, new Dictionary<string, DataSetValidationCell>
            {
                ["id"] = new(DataSetValidationCellKind.Number, "0"),
                ["name"] = new(DataSetValidationCellKind.String, "alpha")
            }), profile, seen);
        var second = DataSetValidationEngine.ValidateRow(
            new DataSetValidationRecord(2, new Dictionary<string, DataSetValidationCell>
            {
                ["id"] = new(DataSetValidationCellKind.Number, "0"),
                ["name"] = DataSetValidationCell.Missing
            }), profile, seen);

        Assert.Contains(first.Findings, x => x.Code.EndsWith(".min", StringComparison.Ordinal));
        Assert.Contains(second.Findings, x => x.Code.EndsWith(".required", StringComparison.Ordinal));
        Assert.Contains(second.Findings, x => x.Code.StartsWith("duplicate.", StringComparison.Ordinal));
        Assert.DoesNotContain(first.Findings.Concat(second.Findings), x => x.Message.Contains("alpha", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ContentReader_RejectsInvalidUtf8AndRecordLimit()
    {
        await using var invalid = new MemoryStream(new byte[] { 0xC3, 0x28 });
        var invalidException = await Assert.ThrowsAsync<DataSetValidationInputException>(() =>
            DataSetValidationContentReader.ReadAsync(invalid, "json-array", 10, 50));
        Assert.Equal("dataset.validation.utf8_invalid", invalidException.Code);

        await using var tooMany = new MemoryStream(Encoding.UTF8.GetBytes("[{\"id\":1},{\"id\":2}]"));
        var exception = await Assert.ThrowsAsync<DataSetValidationInputException>(() =>
            DataSetValidationContentReader.ReadAsync(tooMany, "json-array", 1, 50));
        Assert.Equal("dataset.validation.max_records_exceeded", exception.Code);

        await using var invalidJson = new MemoryStream(Encoding.UTF8.GetBytes("[{"));
        var jsonException = await Assert.ThrowsAsync<DataSetValidationInputException>(() =>
            DataSetValidationContentReader.ReadAsync(invalidJson, "json-array", 10, 50));
        Assert.Equal("dataset.validation.json_invalid", jsonException.Code);
    }
}
