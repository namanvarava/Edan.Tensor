namespace EDAN.Tensor.DataSet.Domain.Model;

/// <summary>물리 경로와 접속 정보를 노출하지 않는 저장소 내 불투명 참조입니다.</summary>
public sealed record StorageReference
{
    public StorageReference(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        if (normalized.Length > 256 ||
            normalized.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or ':')))
            throw new ArgumentException("저장소 참조는 256자 이하의 불투명 식별자여야 합니다.", nameof(value));
        Value = normalized;
    }

    public string Value { get; }
}
