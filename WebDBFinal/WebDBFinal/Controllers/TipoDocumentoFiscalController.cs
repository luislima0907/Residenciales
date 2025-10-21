using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoDocumentoFiscalController : BaseController<TipoDocumentoFiscal>
    {
        protected override string EntityName => "TipoDocumentoFiscal";
        protected override string SpCreate => "EXEC InsertarTipoDocumentoFiscal @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoDocumentoFiscal @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoDocumentoFiscal @p0";
        public TipoDocumentoFiscalController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
