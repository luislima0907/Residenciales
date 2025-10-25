using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class AplicacionDocumentoController : BaseController<AplicacionDocumento>
{
    protected override string EntityName => "AplicacionDocumento";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public AplicacionDocumentoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}