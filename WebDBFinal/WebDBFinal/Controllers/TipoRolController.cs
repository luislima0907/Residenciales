using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class TipoRolController : BaseController<TipoRol>
    {
        protected override string EntityName => "TipoRol";
        protected override string SpCreate => "EXEC InsertarTipoRol @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarTipoRol @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarTipoRol @p0";
        public TipoRolController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
        
    }
}
