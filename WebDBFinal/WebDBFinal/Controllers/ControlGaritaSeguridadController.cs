using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class ControlGaritaSeguridadController : BaseController<ControlGaritaSeguridad>
{
    protected override string EntityName => "ControlGaritaSeguridad";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public ControlGaritaSeguridadController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

