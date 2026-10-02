
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;

public static class PartErrors
{
    public static readonly Error NameRequired =
        Error.Validation("part.name.Required","Part Name Is Required.");

    public static readonly Error CostInvalid =
        Error.Validation("Part.Cost.InValid","Part Cost Must Between 1 and 10,000.");

    public static readonly Error QuantityInvalid = 
        Error.Validation("Part.Quantity.Invalid","Part Quantity Must Between1 and 10");
};