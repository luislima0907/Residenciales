using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DetalleCargoMensualController : BaseController<DetalleCargoMensual>
{
    protected override string EntityName => "DetalleCargoMensual";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public DetalleCargoMensualController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

