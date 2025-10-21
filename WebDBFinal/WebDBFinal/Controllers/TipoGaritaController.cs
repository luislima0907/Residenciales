using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoGaritaController : BaseController<TipoGaritum>
    {
        protected override string EntityName => "TipoGarita";
        protected override string SpCreate => "EXEC InsertarTipoGarita @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoGarita @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoGarita @p0";
        public TipoGaritaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
