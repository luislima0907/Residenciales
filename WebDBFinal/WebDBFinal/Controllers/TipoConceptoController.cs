using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoConceptoController : BaseController<TipoConcepto>
    {
        protected override string EntityName => "TipoConcepto";
        protected override string SpCreate => "EXEC InsertarTipoConcepto @p0, @p1, @p2";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoConcepto @p0, @p1, @p2";
        protected override string SpDelete => "EXEC sp_EliminarTipoConcepto @p0";
        public TipoConceptoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
