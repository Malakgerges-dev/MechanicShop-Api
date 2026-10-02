using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Employees;

namespace MechanicShop.Application.Features.Labors.Mappers;


public static class LaborMapper
{
    public static LaborDto ToDto(this Employee entity)
    {
   
        return new LaborDto
        {
            LaborId = entity.Id,
            Name = entity.FullName
        };
    }

    
    public static List<LaborDto> ToDtos(this IEnumerable<Employee> entity)
    {
        return [.. entity.Select(e=>e.ToDto())];
    }


}