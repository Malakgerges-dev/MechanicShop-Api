using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
    public string Name { get; private set; }
    public decimal LaborCost { get; private set; }
    public RepairDurationInMinutes EstimatedDurationInMins { get; private set; }
    private readonly List<Part> _parts = [];
    public IEnumerable<Part> Parts => _parts.AsReadOnly();
    public decimal TotalCost => LaborCost + Parts.Sum(p => p.Cost * p.Quantity);


    #pragma warning disable CS8618

    private RepairTask()
    { }

    #pragma warning restore CS8618

    private RepairTask(Guid id, string name, decimal laborCost,
                       RepairDurationInMinutes estimatedDurationInMinutes, List<Part> parts)
                       : base(id)
    {
        Name = name;
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMinutes;
        _parts = parts;
    }

    public static Result<RepairTask> Create(Guid id, string name, decimal laborCost,
            RepairDurationInMinutes estimatedDurationInMins, List<Part> parts)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0)
        {
            return RepairTaskErrors.LaborCostInvalid;
        }

        if (!Enum.IsDefined(typeof(RepairDurationInMinutes), estimatedDurationInMins))
        {
            return RepairTaskErrors.DurationInvalid;
        }

        return new RepairTask(id, name.Trim(), laborCost, estimatedDurationInMins, parts);
    }

    public Result<Updated> Update(string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0)
        {
            return RepairTaskErrors.LaborCostInvalid;
        }

        if (!Enum.IsDefined(typeof(RepairDurationInMinutes), estimatedDurationInMins))
        {
            return RepairTaskErrors.DurationInvalid;
        }

        Name = name.Trim();
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;

        return Result.updated;
    }

    public Result<Updated> UpSertParts(List<Part> inComingParts)
    {
        _parts.RemoveAll(Existing => inComingParts.All(p => p.Id != Existing.Id));

        foreach (var incoming in inComingParts)
        {
             var existing = _parts.FirstOrDefault(p => p.Id == incoming.Id);
             if(existing is null)
            {
                _parts.Add(incoming);
            }
            else
            {
                var updateResult = existing.Update(incoming.Name, incoming.Cost, incoming.Quantity);
                if (updateResult.IsError)
                {
                    return updateResult.Errors;
                }
            }
        }

        return Result.updated;
    }
}