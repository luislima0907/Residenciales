using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoLicenciaController : BaseController<TipoLicencium>
    {
        protected override string EntityName => "TipoLicencia";
        protected override string SpCreate => "EXEC InsertarTipoLicencia @p0, @p1, @p2";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoLicencia @p0, @p1, @p2";
        protected override string SpDelete => "EXEC sp_EliminarTipoLicencia @p0";
        public TipoLicenciaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
