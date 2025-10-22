using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Controllers;

public class PersonaController : BaseController<Persona>
{
    protected override string EntityName => "Persona";
    protected override string SpCreate => "EXEC InsertarPersona @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10";
    protected override string SpUpdate => "EXEC sp_ActualizarPersona @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10";
    protected override string SpDelete => "EXEC sp_EliminarPersona @p0";
    
    public PersonaController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<Persona>()
            .Include(p => p.CodigoEstadoCivilNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetPersonaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Persona());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var codigoPersona = int.Parse(id);
            var entity = await _context.Set<Persona>()
                .Include(p => p.CodigoEstadoCivilNavigation)
                .FirstOrDefaultAsync(p => p.CodigoPersona == codigoPersona);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetPersonaForeignKeyDataAsync();
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Persona entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación que causan problemas
        ModelState.Remove("CodigoEstadoCivilNavigation");
        ModelState.Remove("Licencia");
        ModelState.Remove("PersonaRols");
        ModelState.Remove("PersonaTelefonos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarPersona: @CodigoPersona, @CUI, @PrimerNombre, @SegundoNombre, @TercerNombre, @PrimerApellido, @SegundoApellido, @TercerApellido, @Genero, @FechaDeNacimiento, @CodigoEstadoCivil
                
                // Para campos opcionales, usar SqlParameter para enviar NULL correctamente
                var sqlParams = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.CodigoPersona),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CUI),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.PrimerNombre),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", (object?)entity.SegundoNombre ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", (object?)entity.TercerNombre ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.PrimerApellido),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", (object?)entity.SegundoApellido ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p7", (object?)entity.TercerApellido ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.Genero),
                    new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.FechaDeNacimiento),
                    new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.CodigoEstadoCivil)
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, sqlParams);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                // Log para debugging
                Console.WriteLine($"Error al crear Persona: {ex}");
                Console.WriteLine($"Parámetros: CodigoPersona={entity.CodigoPersona}, CUI={entity.CUI}, Nombre={entity.PrimerNombre} {entity.PrimerApellido}, Genero={entity.Genero}, FechaNac={entity.FechaDeNacimiento}, EstadoCivil={entity.CodigoEstadoCivil}");
            }
        }
        else
        {
            // Mostrar errores de validación
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
            
            TempData["ErrorMessage"] = "Errores de validación: " + 
                string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetPersonaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Persona entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación que causan problemas
        ModelState.Remove("CodigoEstadoCivilNavigation");
        ModelState.Remove("Licencia");
        ModelState.Remove("PersonaRols");
        ModelState.Remove("PersonaTelefonos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarPersona: CodigoPersona, CUI, PrimerNombre, SegundoNombre, TercerNombre, PrimerApellido, SegundoApellido, TercerApellido, Genero, FechaDeNacimiento, CodigoEstadoCivil
                
                // Para campos opcionales, usar SqlParameter para enviar NULL correctamente
                var sqlParams = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.CodigoPersona),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CUI),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.PrimerNombre),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", (object?)entity.SegundoNombre ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", (object?)entity.TercerNombre ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.PrimerApellido),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", (object?)entity.SegundoApellido ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p7", (object?)entity.TercerApellido ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.Genero),
                    new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.FechaDeNacimiento),
                    new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.CodigoEstadoCivil)
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, sqlParams);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                // Log para debugging
                Console.WriteLine($"Error al actualizar Persona: {ex}");
                Console.WriteLine($"Parámetros: CodigoPersona={entity.CodigoPersona}, CUI={entity.CUI}");
            }
        }
        else
        {
            // Mostrar errores de validación
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
            
            TempData["ErrorMessage"] = "Errores de validación: " + 
                string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.KeyProperties = GetKeyProperties();
        ViewBag.ForeignKeyData = await GetPersonaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de Persona
    private async Task<Dictionary<string, List<DropdownItem>>> GetPersonaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para EstadoCivil
            var estadosCiviles = await _context.EstadoCivils.ToListAsync();
            var estadoCivilItems = estadosCiviles.Select(ec => new DropdownItem
            {
                Value = ec.CodigoEstadoCivil.ToString(),
                Text = $"{ec.Descripcion}"
            }).ToList();

            foreignKeyData["CodigoEstadoCivil"] = estadoCivilItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
