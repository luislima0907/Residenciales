using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class DepartamentoController : BaseController<Departamento>
    {
        protected override string EntityName => "Departamento";
        protected override string SpCreate => "EXEC InsertarDepartamento @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarDepartamento @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarDepartamento @p0";
        public DepartamentoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
