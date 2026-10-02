
namespace MechanicShop.Domain.Common.Results.Abstractions;

public interface IResult
{
    List<Error> Errors{get;}
    bool IsSuccess {get;}
}

public interface IResult<out TValue> : IResult
{
    public TValue Value {get;}
}