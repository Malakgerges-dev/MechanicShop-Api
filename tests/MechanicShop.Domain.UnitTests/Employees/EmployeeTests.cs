using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WithValidData()
    {
        var id = Guid.NewGuid();
        const string firstName = "Malak";
        const string lastName = "Gerges";
        const Role role = Role.Labor;

        var result = EmployeeFactory.CreateEmployee(id: id, firstName: firstName, lastName: lastName, role: role);

        Assert.True(result.IsSuccess);
        var employee = result.Value;
        Assert.Equal(id, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(role, employee.Role);
        Assert.Equal("Malak Gerges", employee.FullName);
    }

    [Fact]
    public void Create_ShouldFail_WithEmptyId()
    {
        var result = Employee.Create(Guid.Empty, "Malak", "Gerges", Role.Manager);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.IdRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_ShouldFail_WithEmptyFirstName()
    {
        var result = Employee.Create(Guid.NewGuid(), " ", "Gerges", Role.Manager);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_ShouldFail_WithEmptyLastName()
    {
        var result = Employee.Create(Guid.Empty, "Malak", " ", Role.Manager);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.IdRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_ShouldFail_WithInvalidRole()
    {
        var result = Employee.Create(Guid.NewGuid(), "Malak", "Gerges", (Role)999);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.RoleInvalid.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.RoleInvalid.Description, result.TopError.Description);
    }
}