using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class EstadoCuentumController : BaseController<EstadoCuentum>
{
    protected override string EntityName => "EstadoCuentum";
    protected override string SpCreate => "";
    protected override string SpUpdate => "";
    protected override string SpDelete => "";
    
    public EstadoCuentumController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
}

