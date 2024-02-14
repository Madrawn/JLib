using System.Collections;
using JLib.Helper;

namespace JLib.Exceptions;

public sealed class InvalidTypeException : InvalidSetupException
{
    public Type TvtType { get; }
    public Type Value { get; }

    public InvalidTypeException(Type tvtType, Type value, string message) : base(
        message)
    {
        TvtType = tvtType;
        Value = value;
        Data[nameof(TvtType)] = tvtType;
        Data[nameof(value)] = value;
    }
}