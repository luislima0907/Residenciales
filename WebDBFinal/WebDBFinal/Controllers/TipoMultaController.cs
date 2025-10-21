using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoMultaController : BaseController<TipoMultum>
    {
        protected override string EntityName => "TipoMulta";
        protected override string SpCreate => "EXEC InsertarTipoMulta @p0, @p1, @p2";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoMulta @p0, @p1, @p2";
        protected override string SpDelete => "EXEC sp_EliminarTipoMulta @p0";
        public TipoMultaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
