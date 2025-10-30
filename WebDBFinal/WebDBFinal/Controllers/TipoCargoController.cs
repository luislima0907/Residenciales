using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoCargoController : BaseController<TipoCargo>
    {
        protected override string EntityName => "TipoCargo";
        protected override string SpCreate => "EXEC InsertarTipoCargo @p0, @p1, @p2";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoCargo @p0, @p1, @p2";
        protected override string SpDelete => "EXEC sp_EliminarTipoCargo @p0";
        public TipoCargoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
