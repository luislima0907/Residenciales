using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class CensoController : BaseController<Censo>
    {
        protected override string EntityName => "Censo";
        protected override string SpCreate => "EXEC InsertarCenso @p0, @p1, @p2, @p3";
        protected override string SpUpdate => "EXEC sp_ActualizarCenso @p0, @p1, @p2, @p3";
        protected override string SpDelete => "EXEC sp_EliminarCenso @p0";

        public CensoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context))
        {
        }
    }
}
