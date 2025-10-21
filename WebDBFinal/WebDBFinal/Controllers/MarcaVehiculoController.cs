using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
    public class MarcaVehiculoController : BaseController<MarcaVehiculo>
    {
        protected override string EntityName => "MarcaVehiculo";
        protected override string SpCreate => "EXEC InsertarMarcaVehiculo @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarMarcaVehiculo @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarMarcaVehiculo @p0";
        public MarcaVehiculoController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    }
}
