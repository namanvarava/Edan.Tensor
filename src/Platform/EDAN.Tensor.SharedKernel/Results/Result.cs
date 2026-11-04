namespace EDAN.Tensor.SharedKernel.Results;

/// <summary>Outcome of an operation where expected failures are represented as data.</summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && !error.IsNone) throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
        if (!isSuccess && error.IsNone) throw new ArgumentException("A failed result must contain an error.", nameof(error));
        IsSuccess = isSuccess; Error = error;
    }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) { ArgumentNullException.ThrowIfNull(error); return new(false, error); }
}

/// <summary>Outcome of an operation that may return a value.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;
    private Result(TValue value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) { }
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result does not contain a value.");
    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(Error error) { ArgumentNullException.ThrowIfNull(error); return new(error); }
}
