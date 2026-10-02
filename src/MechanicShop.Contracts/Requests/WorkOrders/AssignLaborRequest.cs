using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public class AssignLaborRequest
{
    [Required(ErrorMessage ="LaborId Is Required")]

    public string LaborId{get;set;} = string.Empty;
}