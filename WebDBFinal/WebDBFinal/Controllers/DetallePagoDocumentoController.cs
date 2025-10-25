using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DetallePagoDocumentoController : BaseController<DetallePagoDocumento>
{
    protected override string EntityName => "DetallePagoDocumento";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public DetallePagoDocumentoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

