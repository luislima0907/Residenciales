using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoIntegranteController : BaseController<TipoIntegrante>
    {
        protected override string EntityName => "TipoIntegrante";
        protected override string SpCreate => "EXEC InsertarTipoIntegrante @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoIntegrante @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoIntegrante @p0";
        public TipoIntegranteController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
