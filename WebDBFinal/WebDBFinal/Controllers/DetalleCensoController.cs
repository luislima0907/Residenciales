using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DetalleCensoController : BaseController<DetalleCenso>
{
    protected override string EntityName => "DetalleCenso";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public DetalleCensoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

