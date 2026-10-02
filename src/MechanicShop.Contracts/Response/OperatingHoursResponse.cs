
namespace MechanicShop.Contracts.Response;

public sealed record OperatingHoursResponse(TimeOnly OpeningTime,TimeOnly ClosingTime);