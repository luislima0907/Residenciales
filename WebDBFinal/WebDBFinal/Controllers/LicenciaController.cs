using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class LicenciaController : BaseController<Licencia>
{
    protected override string EntityName => "Licencia";
    protected override string SpCreate => "EXEC InsertarLicencia @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpUpdate => "EXEC sp_ActualizarLicencia @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpDelete => "EXEC sp_EliminarLicencia @p0";
    
    public LicenciaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Licencia entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoLicenciaNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoLicencia,
                    entity.CodigoPersona,
                    entity.NumeroLicencia,
                    entity.CodigoTipoLicencia,
                    entity.FechaEmision,
                    entity.FechaVencimiento
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear Licencia: {ex}");
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
        ViewBag.ForeignKeyData = await GetLicenciaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Licencia entity)
    {
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoLicenciaNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                var parameters = new object[]
                {
                    entity.CodigoLicencia,
                    entity.CodigoPersona,
                    entity.NumeroLicencia,
                    entity.CodigoTipoLicencia,
                    entity.FechaEmision,
                    entity.FechaVencimiento
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar Licencia: {ex}");
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
        ViewBag.ForeignKeyData = await GetLicenciaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetLicenciaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Licencia());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<Licencia>()
                .Include(l => l.CodigoPersonaNavigation)
                .Include(l => l.CodigoTipoLicenciaNavigation)
                .FirstOrDefaultAsync(l => l.CodigoLicencia == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrada";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetLicenciaForeignKeyDataAsync();
            
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
        var entities = await _context.Set<Licencia>()
            .Include(l => l.CodigoPersonaNavigation)
            .Include(l => l.CodigoTipoLicenciaNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // Método específico para cargar datos de llaves foráneas de Licencia
    private async Task<Dictionary<string, List<DropdownItem>>> GetLicenciaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Persona
            var personas = await _context.Personas.ToListAsync();
            var personaItems = personas.Select(p => new DropdownItem
            {
                Value = p.CodigoPersona.ToString(),
                Text = $"{p.PrimerNombre} {p.SegundoNombre ?? ""} {p.PrimerApellido} {p.SegundoApellido ?? ""} - CUI: {p.CUI}".Replace("  ", " ").Trim()
            }).ToList();
            foreignKeyData["CodigoPersona"] = personaItems;

            // Cargar datos para TipoLicencia
            var tiposLicencia = await _context.TipoLicencia.ToListAsync();
            var tipoLicenciaItems = tiposLicencia.Select(tl => new DropdownItem
            {
                Value = tl.CodigoTipoLicencia.ToString(),
                Text = $"{tl.Descripcion} - Código: {tl.CodigoTipoLicencia}"
            }).ToList();
            foreignKeyData["CodigoTipoLicencia"] = tipoLicenciaItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
