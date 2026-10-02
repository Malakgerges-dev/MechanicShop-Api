
namespace MechanicShop.Application.Common.Models;

public class PaginatedList<T>
{
    public int PageNumber{get;init;}

    public int PageSize{get;init;}

    public int TotalPage{get;init;}

    public int TotalCount{get;init;}
    
    public IReadOnlyCollection<T>? Items { get; init; }

}