using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoPagoController : BaseController<TipoPago>
    {
        protected override string EntityName => "TipoPago";
        protected override string SpCreate => "EXEC InsertarTipoPago @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoPago @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoPago @p0";
        public TipoPagoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
