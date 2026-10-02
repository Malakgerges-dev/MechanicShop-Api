
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Employees;

public static class EmployeeErrors
{
    public static readonly Error IdRequired =
        Error.Validation("Employee.Id.Required","Employee Id is required.");

    public static Error FirstNameRequired =>
        Error.Validation("Employee.FirstName.Required","First Name Is Required.");

    public static Error LastNameRequired =>
        Error.Validation("Employee.LastName.Required","Last Name Is Required.");

    public static Error RoleInvalid =>
        Error.Validation("Employee.Role.Required","Invalid Role Assigned To Employee.");
}