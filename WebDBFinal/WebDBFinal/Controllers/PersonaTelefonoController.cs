using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class PersonaTelefonoController : BaseController<PersonaTelefono>
{
    protected override string EntityName => "PersonaTelefono";
    protected override string SpCreate => "EXEC InsertarPersonaTelefono @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarPersonaTelefono @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarPersonaTelefono @p0";
    
    public PersonaTelefonoController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<PersonaTelefono>()
            .Include(pt => pt.CodigoPersonaNavigation)
            .Include(pt => pt.CodigoTipoTelefonoNavigation)
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
        ViewBag.ForeignKeyData = await GetPersonaTelefonoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new PersonaTelefono());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var codigoPersonaTelefono = int.Parse(id);

            var entity = await _context.Set<PersonaTelefono>()
                .Include(pt => pt.CodigoPersonaNavigation)
                .Include(pt => pt.CodigoTipoTelefonoNavigation)
                .FirstOrDefaultAsync(pt => pt.CodigoPersonaTelefono == codigoPersonaTelefono);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetPersonaTelefonoForeignKeyDataAsync();
            
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
    public override async Task<IActionResult> Create(PersonaTelefono entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoTelefonoNavigation");
        ModelState.Remove("RegistroMovimientoResidencials");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarPersonaTelefono: @CodigoPersonaTelefono, @CodigoPersona, @CodigoTipoTelefono, @Numero
                var parameters = new object[]
                {
                    entity.CodigoPersonaTelefono,
                    entity.CodigoPersona,
                    entity.CodigoTipoTelefono,
                    entity.Numero
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear PersonaTelefono: {ex}");
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
        ViewBag.ForeignKeyData = await GetPersonaTelefonoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(PersonaTelefono entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoTelefonoNavigation");
        ModelState.Remove("RegistroMovimientoResidencials");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarPersonaTelefono: @CodigoPersonaTelefono, @CodigoPersona, @CodigoTipoTelefono, @Numero
                var parameters = new object[]
                {
                    entity.CodigoPersonaTelefono,
                    entity.CodigoPersona,
                    entity.CodigoTipoTelefono,
                    entity.Numero
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar PersonaTelefono: {ex}");
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
        ViewBag.ForeignKeyData = await GetPersonaTelefonoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de PersonaTelefono
    private async Task<Dictionary<string, List<DropdownItem>>> GetPersonaTelefonoForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de Persona
            var personas = await _context.Personas
                .OrderBy(p => p.PrimerNombre)
                .ThenBy(p => p.PrimerApellido)
                .Select(p => new DropdownItem
                {
                    Value = p.CodigoPersona.ToString(),
                    Text = $"{p.CodigoPersona} - {p.PrimerNombre} {p.SegundoNombre ?? ""} {p.PrimerApellido} {p.SegundoApellido ?? ""}".Trim()
                })
                .ToListAsync();
            
            foreignKeyData["CodigoPersona"] = personas;
            
            // Cargar datos de TipoTelefono
            var tiposTelefono = await _context.TipoTelefonos
                .OrderBy(tt => tt.Descripcion)
                .Select(tt => new DropdownItem
                {
                    Value = tt.CodigoTipoTelefono.ToString(),
                    Text = $"{tt.CodigoTipoTelefono} - {tt.Descripcion}"
                })
                .ToListAsync();
            
            foreignKeyData["CodigoTipoTelefono"] = tiposTelefono;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }

    protected new List<PropertyInfo> GetEditableProperties()
    {
        return typeof(PersonaTelefono).GetProperties()
            .Where(p => p.Name != "CodigoPersonaNavigation" &&
                       p.Name != "CodigoTipoTelefonoNavigation" &&
                       p.Name != "RegistroMovimientoResidencials")
            .ToList();
    }

    protected new List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(PersonaTelefono).GetProperties()
            .Where(p => p.Name == "CodigoPersonaTelefono" ||
                       p.Name == "Numero" ||
                       p.Name == "CodigoPersonaNavigation" ||
                       p.Name == "CodigoTipoTelefonoNavigation")
            .ToList();
    }
}
