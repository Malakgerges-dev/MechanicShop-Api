using System.ComponentModel;
using System.Data.Common;
using System.Text.Json.Serialization;
using MechanicShop.Domain.Common.Results.Abstractions;

namespace MechanicShop.Domain.Common.Results;


public static class Result
{
    public static Success success = default;
    public static Created created = default;
    public static Deleted deleted = default;
    public static Updated updated = default;

 
}

public sealed class Result<TValue> : IResult<TValue>
{
    private readonly TValue? _Value = default;
    private readonly List<Error>? _errors = default;

    public bool IsSuccess {get;}

    public bool IsError => !IsSuccess;

    public List<Error> Errors => IsError? _errors! : [];
    public TValue Value => IsSuccess ? _Value! : default!;

    public Error TopError => (_errors?.Count > 0)? _errors[0] : default;

    [JsonConstructor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("For Serializer Only.",true)]
    public Result(TValue? value,List<Error>? errors,bool IsSuccess)
    {
        if (IsSuccess)
        {
            _Value = value ?? throw new ArgumentException(nameof(value));
            _errors = [];
            IsSuccess = true;
        }
        else
        {
            if(errors is null || errors.Count == 0)
            {
                throw new ArgumentException("Provide at Least One Error.",nameof(errors));
            }

            _errors = errors;
            _Value = default;
            IsSuccess = false;
            
        }
    }

    private Result(Error error)
    {
        _errors = [error];
    }

    private Result(List<Error> errors)
    {
        if(errors is null || errors.Count == 0)
           { throw new ArgumentException
            ("Cannot create an ErrorOr<TValue> from an empty collection of errors. Provide at least one error.", nameof(errors));
           }
        _errors = errors;
        IsSuccess = false;
    }

    private Result(TValue value)
    {
        if(value is null)
        {
            throw new ArgumentException(nameof(value));
        }
        _Value = value;
        IsSuccess = true;
    }

    public TNextValue Match<TNextValue>(Func<TValue,TNextValue>OnValue, Func<List<Error>,TNextValue> OnError)
        =>IsSuccess?OnValue(Value) : OnError(Errors);

    public static implicit operator Result<TValue>(TValue value)
        =>new (value);

    public static implicit operator Result<TValue>(Error error) 
        =>new (error);

    public static implicit operator Result<TValue>(List<Error>errors)
        =>new (errors);
}

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Deleted;
public readonly record struct Updated;
