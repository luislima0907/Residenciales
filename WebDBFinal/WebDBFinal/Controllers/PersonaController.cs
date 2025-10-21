using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers
{
        public class PersonaController : BaseController<Persona>
        {
            protected override string EntityName => "Persona";
            protected override string SpCreate => "EXEC InsertarPersona @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10";
            protected override string SpUpdate => "EXEC sp_ActualizarPersona @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10";
            protected override string SpDelete => "EXEC sp_EliminarPersona @p0";

            public PersonaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context))
            {
            }
        }
}
