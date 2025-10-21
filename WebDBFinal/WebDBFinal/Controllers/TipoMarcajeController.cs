using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoMarcajeController : BaseController<TipoMarcaje>
    {
        protected override string EntityName => "TipoMarcaje";
        protected override string SpCreate => "EXEC InsertarTipoMarcaje @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoMarcaje @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoMarcaje @p0";
        public TipoMarcajeController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
