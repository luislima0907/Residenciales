using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class ResidencialController : BaseController<Residencial>
{
    protected override string EntityName => "Residencial";
    protected override string SpCreate => "EXEC sp_CrearResidencial @p0, @p1, @p2";
    protected override string SpUpdate => "EXEC sp_ActualizarResidencial @p0, @p1, @p2";
    protected override string SpDelete => "EXEC sp_EliminarResidencial @p0";

    public ResidencialController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context))
    {
    }
}