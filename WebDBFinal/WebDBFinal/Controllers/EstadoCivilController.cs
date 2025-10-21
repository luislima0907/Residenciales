using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;
 public class EstadoCivilController : BaseController<EstadoCivil>
 {
        protected override string EntityName => "EstadoCivil";
        protected override string SpCreate => "EXEC InsertarEstadoCivil @p0, @p1";
        protected override string SpUpdate => "EXEC sp_ActualizarEstadoCivil @p0, @p1";
        protected override string SpDelete => "EXEC sp_EliminarEstadoCivil @p0";
        public EstadoCivilController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }
 }
    