using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class MarcajeLaboralController : BaseController<MarcajeLaboral>
{
    protected override string EntityName => "MarcajeLaboral";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public MarcajeLaboralController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

