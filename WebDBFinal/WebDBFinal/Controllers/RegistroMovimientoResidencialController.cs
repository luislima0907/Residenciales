using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class RegistroMovimientoResidencialController : BaseController<RegistroMovimientoResidencial>
{
    protected override string EntityName => "RegistroMovimientoResidencial";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public RegistroMovimientoResidencialController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

