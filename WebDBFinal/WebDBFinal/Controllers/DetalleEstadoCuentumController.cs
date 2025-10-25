using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DetalleEstadoCuentumController : BaseController<DetalleEstadoCuentum>
{
    protected override string EntityName => "DetalleEstadoCuentum";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public DetalleEstadoCuentumController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

