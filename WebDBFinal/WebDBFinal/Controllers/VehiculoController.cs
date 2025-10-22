using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class VehiculoController : BaseController<Vehiculo>
{
    protected override string EntityName => "Vehiculo";
    protected override string SpCreate => "EXEC InsertarVehiculo @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpUpdate => "EXEC sp_ActualizarVehiculo @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpDelete => "EXEC sp_EliminarVehiculo @p0";
    
    public VehiculoController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<Vehiculo>()
            .Include(v => v.LineaVehiculo)
                .ThenInclude(lv => lv.CodigoMarcaNavigation)
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
        ViewBag.ForeignKeyData = await GetVehiculoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Vehiculo());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var codigoVehiculo = int.Parse(id);

            var entity = await _context.Set<Vehiculo>()
                .Include(v => v.LineaVehiculo)
                    .ThenInclude(lv => lv.CodigoMarcaNavigation)
                .FirstOrDefaultAsync(v => v.CodigoVehiculo == codigoVehiculo);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetVehiculoForeignKeyDataAsync();
            
            // Pasar el valor actual de la FK compuesta LineaVehiculo para pre-selección
            ViewBag.CurrentLineaVehiculoValue = $"{entity.CodigoLinea},{entity.CodigoMarca}";
            
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
    public override async Task<IActionResult> Create(Vehiculo entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("LineaVehiculo");
        ModelState.Remove("RegistroMovimientoResidencials");
        ModelState.Remove("RegistroVehiculoNoPermitidos");
        ModelState.Remove("RegistroVehiculoResidentes");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarVehiculo: @CodigoVehiculo, @CodigoLinea, @CodigoMarca, @Anio, @Color, @Placa
                var parameters = new object[]
                {
                    entity.CodigoVehiculo,
                    entity.CodigoLinea,
                    entity.CodigoMarca,
                    entity.Anio,
                    entity.Color,
                    entity.Placa
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear Vehiculo: {ex}");
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
        ViewBag.ForeignKeyData = await GetVehiculoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Vehiculo entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("LineaVehiculo");
        ModelState.Remove("RegistroMovimientoResidencials");
        ModelState.Remove("RegistroVehiculoNoPermitidos");
        ModelState.Remove("RegistroVehiculoResidentes");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarVehiculo: @CodigoVehiculo, @CodigoLinea, @CodigoMarca, @Anio, @Color, @Placa
                var parameters = new object[]
                {
                    entity.CodigoVehiculo,
                    entity.CodigoLinea,
                    entity.CodigoMarca,
                    entity.Anio,
                    entity.Color,
                    entity.Placa
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar Vehiculo: {ex}");
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
        ViewBag.ForeignKeyData = await GetVehiculoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de Vehiculo
    private async Task<Dictionary<string, List<DropdownItem>>> GetVehiculoForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de LineaVehiculo (FK compuesta)
            var lineasVehiculo = await _context.LineaVehiculos
                .Include(lv => lv.CodigoMarcaNavigation)
                .OrderBy(lv => lv.CodigoMarcaNavigation.Descripcion)
                .ThenBy(lv => lv.Descripcion)
                .Select(lv => new DropdownItem
                {
                    Value = $"{lv.CodigoLinea},{lv.CodigoMarca}",
                    Text = $"{lv.CodigoMarcaNavigation.Descripcion} - {lv.Descripcion} (L:{lv.CodigoLinea}, M:{lv.CodigoMarca})"
                })
                .ToListAsync();
            
            // Agregar los mismos items para CodigoLinea y CodigoMarca (FK compuesta)
            foreignKeyData["CodigoLinea"] = lineasVehiculo;
            foreignKeyData["CodigoMarca"] = lineasVehiculo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }

    protected new List<PropertyInfo> GetEditableProperties()
    {
        return typeof(Vehiculo).GetProperties()
            .Where(p => p.Name != "LineaVehiculo" &&
                       p.Name != "RegistroMovimientoResidencials" &&
                       p.Name != "RegistroVehiculoNoPermitidos" &&
                       p.Name != "RegistroVehiculoResidentes")
            .ToList();
    }

    protected new List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(Vehiculo).GetProperties()
            .Where(p => p.Name == "CodigoVehiculo" ||
                       p.Name == "Placa" ||
                       p.Name == "Color" ||
                       p.Name == "Anio" ||
                       p.Name == "CodigoLinea" ||
                       p.Name == "CodigoMarca" ||
                       p.Name == "LineaVehiculo")
            .ToList();
    }
}
