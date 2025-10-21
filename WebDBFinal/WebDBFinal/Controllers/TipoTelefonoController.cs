using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoTelefonoController : BaseController<TipoTelefono>
    {
        protected override string EntityName => "TipoTelefono";
        protected override string SpCreate => "EXEC InsertarTipoTelefono @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoTelefono @p0, @p1";
        protected override string SpDelete => "EXEC EliminarTipoTelefono @p0";
        public TipoTelefonoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
