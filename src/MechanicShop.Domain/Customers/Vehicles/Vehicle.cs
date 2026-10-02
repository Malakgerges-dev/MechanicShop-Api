
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles.VehicleErrors;

namespace MechanicShop.Domain.Customers.Vehicles;


public class Vehicle : AuditableEntity
{
     public Guid CustomerId { get; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string LicensePlate { get; private set; }
    public Customer? Customer { get; set; }

    public string VehicleInfo => $"{Make} | {Model} | {Year}";

#pragma warning disable CS8618 

    private Vehicle()
    { }

#pragma warning restore CS8618 

    private Vehicle(Guid id, string make, string model, int year, string licensePlate)
        : base(id)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Result<Vehicle> Create(Guid id, string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.VehicleErrors.MakeRequired;
        }
        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.VehicleErrors.ModelRequired;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.VehicleErrors.LicensePlateRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.VehicleErrors.YearInvalid;
        }

        return new Vehicle(id, make, model, year, licensePlate);
    }

    public Result<Updated> Update(string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.VehicleErrors.MakeRequired;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.VehicleErrors.ModelRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.VehicleErrors.YearInvalid;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.VehicleErrors.LicensePlateRequired;
        }

        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;

        return Result.updated;
    }
}
    
