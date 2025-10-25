using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DetalleFacturacionSeguridadController : BaseController<DetalleFacturacionSeguridad>
{
    protected override string EntityName => "DetalleFacturacionSeguridad";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public DetalleFacturacionSeguridadController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

