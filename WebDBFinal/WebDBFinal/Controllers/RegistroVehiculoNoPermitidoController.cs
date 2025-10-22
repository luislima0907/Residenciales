using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class RegistroVehiculoNoPermitidoController : BaseController<RegistroVehiculoNoPermitido>
{
    protected override string EntityName => "RegistroVehiculoNoPermitido";
    protected override string SpCreate => "EXEC InsertarRegistroVehiculoNoPermitido @p0, @p1, @p2, @p3, @p4";
    protected override string SpUpdate => "EXEC sp_ActualizarRegistroVehiculoNoPermitido @p0, @p1, @p2, @p3, @p4";
    protected override string SpDelete => "EXEC sp_EliminarRegistroVehiculoNoPermitido @p0";
    
    public RegistroVehiculoNoPermitidoController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<RegistroVehiculoNoPermitido>()
            .Include(r => r.CodigoVehiculoNavigation)
                .ThenInclude(v => v.LineaVehiculo)
                    .ThenInclude(l => l.CodigoMarcaNavigation)
            .OrderByDescending(r => r.FechaDeclaracion)
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
        ViewBag.ForeignKeyData = await GetRegistroVehiculoNoPermitidoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new RegistroVehiculoNoPermitido());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var codigoVehiculoNoPermitido = int.Parse(id);

            var entity = await _context.Set<RegistroVehiculoNoPermitido>()
                .Include(r => r.CodigoVehiculoNavigation)
                .FirstOrDefaultAsync(r => r.CodigoVehiculoNoPermitido == codigoVehiculoNoPermitido);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetRegistroVehiculoNoPermitidoForeignKeyDataAsync();
            
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
    public override async Task<IActionResult> Create(RegistroVehiculoNoPermitido entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoVehiculoNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarRegistroVehiculoNoPermitido: @CodigoVehiculoNoPermitido, @CodigoVehiculo, @FechaDeclaracion, @Motivo, @Estado
                var parameters = new object[]
                {
                    entity.CodigoVehiculoNoPermitido,
                    entity.CodigoVehiculo,
                    entity.FechaDeclaracion,
                    entity.Motivo,
                    (object?)entity.Estado ?? null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear RegistroVehiculoNoPermitido: {ex}");
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
        ViewBag.ForeignKeyData = await GetRegistroVehiculoNoPermitidoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(RegistroVehiculoNoPermitido entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoVehiculoNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarRegistroVehiculoNoPermitido: @CodigoVehiculoNoPermitido, @CodigoVehiculo, @FechaDeclaracion, @Motivo, @Estado
                var parameters = new object[]
                {
                    entity.CodigoVehiculoNoPermitido,
                    entity.CodigoVehiculo,
                    entity.FechaDeclaracion,
                    entity.Motivo,
                    (object?)entity.Estado ?? null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar RegistroVehiculoNoPermitido: {ex}");
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
        ViewBag.ForeignKeyData = await GetRegistroVehiculoNoPermitidoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de RegistroVehiculoNoPermitido
    private async Task<Dictionary<string, List<DropdownItem>>> GetRegistroVehiculoNoPermitidoForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de Vehiculo con información de línea y marca
            var vehiculos = await _context.Vehiculos
                .Include(v => v.LineaVehiculo)
                    .ThenInclude(l => l.CodigoMarcaNavigation)
                .OrderBy(v => v.Placa)
                .Select(v => new DropdownItem
                {
                    Value = v.CodigoVehiculo.ToString(),
                    Text = $"Placa: {v.Placa} - {v.LineaVehiculo.Descripcion} ({v.LineaVehiculo.CodigoMarcaNavigation.Descripcion})"
                })
                .ToListAsync();
            
            foreignKeyData["CodigoVehiculo"] = vehiculos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos de llaves foráneas: {ex.Message}");
        }
        
        return foreignKeyData;
    }

    // Override para incluir la propiedad Estado en la visualización
    protected new List<PropertyInfo> GetDisplayProperties()
    {
        var properties = base.GetDisplayProperties();
        
        // Agregar manualmente la propiedad Estado
        var estadoProperty = typeof(RegistroVehiculoNoPermitido).GetProperty("Estado");
        if (estadoProperty != null && !properties.Contains(estadoProperty))
        {
            properties.Add(estadoProperty);
        }
        
        return properties;
    }
}
