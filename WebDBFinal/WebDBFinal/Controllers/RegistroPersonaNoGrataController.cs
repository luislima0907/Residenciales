using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class RegistroPersonaNoGrataController : BaseController<RegistroPersonaNoGrata>
{
    protected override string EntityName => "RegistroPersonaNoGrata";
    protected override string SpCreate => "EXEC InsertarRegistroPersonaNoGrata @p0, @p1, @p2, @p3, @p4, @p5, @p6";
    protected override string SpUpdate => "EXEC sp_ActualizarPersonaNoGrata @p0, @p1, @p2, @p3, @p4, @p5, @p6";
    protected override string SpDelete => "EXEC sp_EliminarRegistroPersonaNoGrata @p0";
    
    public RegistroPersonaNoGrataController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(RegistroPersonaNoGrata entity)
    {
        // Limpiar el estado del modelo para la propiedad PersonaRol que causa problemas
        ModelState.Remove("PersonaRol");
        
        // Manejar el checkbox de Estado
        if (Request.Form.ContainsKey("Estado"))
        {
            var estadoValue = Request.Form["Estado"].ToString();
            entity.Estado = estadoValue.Contains("true") || estadoValue.Contains("on");
            ModelState.Remove("Estado");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoRegistroPersonaNoGrata,
                    entity.CodigoPersonaRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoRol,
                    entity.FechaDeclaracion,
                    entity.MotivoDeclaracion,
                    entity.Estado.HasValue ? (object)entity.Estado.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear RegistroPersonaNoGrata: {ex}");
            }
        }
        else
        {
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
            
            TempData["ErrorMessage"] = "Errores de validación: " + 
                string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetPersonaNoGrataForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(RegistroPersonaNoGrata entity)
    {
        ModelState.Remove("PersonaRol");
        
        if (Request.Form.ContainsKey("Estado"))
        {
            var estadoValue = Request.Form["Estado"].ToString();
            entity.Estado = estadoValue.Contains("true") || estadoValue.Contains("on");
            ModelState.Remove("Estado");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                var parameters = new object[]
                {
                    entity.CodigoRegistroPersonaNoGrata,
                    entity.CodigoPersonaRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoRol,
                    entity.FechaDeclaracion,
                    entity.MotivoDeclaracion,
                    entity.Estado.HasValue ? (object)entity.Estado.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar RegistroPersonaNoGrata: {ex}");
            }
        }
        else
        {
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
            
            TempData["ErrorMessage"] = "Errores de validación: " + 
                string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.KeyProperties = GetKeyProperties();
        ViewBag.ForeignKeyData = await GetPersonaNoGrataForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetPersonaNoGrataForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new RegistroPersonaNoGrata());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<RegistroPersonaNoGrata>()
                .Include(r => r.PersonaRol)
                    .ThenInclude(pr => pr.CodigoPersonaNavigation)
                .Include(r => r.PersonaRol)
                    .ThenInclude(pr => pr.CodigoTipoRolNavigation)
                .FirstOrDefaultAsync(r => r.CodigoRegistroPersonaNoGrata == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetPersonaNoGrataForeignKeyDataAsync();
            
            // Agregar información para preseleccionar el valor en el dropdown
            ViewBag.CurrentPersonaRolValue = $"{entity.CodigoPersonaRol},{entity.CodigoPersona},{entity.CodigoTipoRol}";
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<RegistroPersonaNoGrata>()
            .Include(r => r.PersonaRol)
                .ThenInclude(pr => pr.CodigoPersonaNavigation)
            .Include(r => r.PersonaRol)
                .ThenInclude(pr => pr.CodigoTipoRolNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // Método específico para cargar datos de llaves foráneas de RegistroPersonaNoGrata
    private async Task<Dictionary<string, List<DropdownItem>>> GetPersonaNoGrataForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para PersonaRol (llave compuesta)
            var personaRoles = await _context.PersonaRols
                .Include(pr => pr.CodigoPersonaNavigation)
                .Include(pr => pr.CodigoTipoRolNavigation)
                .ToListAsync();
            
            var personaRolItems = personaRoles.Select(pr => new DropdownItem
            {
                Value = $"{pr.CodigoPersonaRol},{pr.CodigoPersona},{pr.CodigoTipoRol}",
                Text = $"Persona: {pr.CodigoPersonaNavigation.PrimerNombre} {pr.CodigoPersonaNavigation.PrimerApellido} - Rol: {pr.CodigoTipoRolNavigation.Descripcion} - Código: {pr.CodigoPersonaRol}"
            }).ToList();

            // Asignar los mismos datos a todas las partes de la llave compuesta
            foreignKeyData["CodigoPersonaRol"] = personaRolItems;
            foreignKeyData["CodigoPersona"] = personaRolItems;
            foreignKeyData["CodigoTipoRol"] = personaRolItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
