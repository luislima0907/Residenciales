using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class LineaVehiculoController : BaseController<LineaVehiculo>
{
    protected override string EntityName => "LineaVehiculo";
    protected override string SpCreate => "EXEC InsertarLineaVehiculo @p0, @p1, @p2";
    protected override string SpUpdate => "EXEC sp_ActualizarLineaVehiculo @p0, @p1, @p2";
    protected override string SpDelete => "EXEC sp_EliminarLineaVehiculo @p0, @p1";
    
    public LineaVehiculoController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<LineaVehiculo>()
            .Include(l => l.CodigoMarcaNavigation)
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
        ViewBag.ForeignKeyData = await GetLineaVehiculoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new LineaVehiculo());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // El id viene como "CodigoLinea,CodigoMarca" (separado por comas desde GenericIndex)
            var parts = id.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = $"ID inválido para LineaVehiculo. Se esperaban 2 valores pero se recibieron {parts.Length}. ID recibido: '{id}'";
                return RedirectToAction(nameof(Index));
            }

            var codigoLinea = int.Parse(parts[0].Trim());
            var codigoMarca = int.Parse(parts[1].Trim());

            var entity = await _context.Set<LineaVehiculo>()
                .Include(l => l.CodigoMarcaNavigation)
                .FirstOrDefaultAsync(l => l.CodigoLinea == codigoLinea && l.CodigoMarca == codigoMarca);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado con los códigos: Línea={codigoLinea}, Marca={codigoMarca}";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetLineaVehiculoForeignKeyDataAsync();
            
            // Guardar valores originales para el SP
            ViewBag.OriginalCodigoLinea = entity.CodigoLinea;
            ViewBag.OriginalCodigoMarca = entity.CodigoMarca;
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}. ID recibido: '{id}'";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(LineaVehiculo entity)
    {
        // Limpiar el estado del modelo para la propiedad de navegación
        ModelState.Remove("CodigoMarcaNavigation");
        ModelState.Remove("Vehiculos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarLineaVehiculo: @CodigoLinea, @CodigoMarca, @Descripcion
                var parameters = new object[]
                {
                    entity.CodigoLinea,
                    entity.CodigoMarca,
                    entity.Descripcion
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear LineaVehiculo: {ex}");
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
        ViewBag.ForeignKeyData = await GetLineaVehiculoForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(LineaVehiculo entity)
    {
        // Limpiar el estado del modelo para la propiedad de navegación
        ModelState.Remove("CodigoMarcaNavigation");
        ModelState.Remove("Vehiculos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // sp_ActualizarLineaVehiculo solo acepta 3 parámetros:
                // @CodigoLinea, @CodigoMarca, @Descripcion
                // Los valores de CodigoLinea y CodigoMarca NO se pueden modificar (son la llave primaria)
                // Solo se actualiza la Descripcion
                var parameters = new object[]
                {
                    entity.CodigoLinea,       // Código de línea (identifica el registro, no se modifica)
                    entity.CodigoMarca,       // Código de marca (identifica el registro, no se modifica)
                    entity.Descripcion        // Nuevo valor de descripción
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar LineaVehiculo: {ex}");
                Console.WriteLine($"Parámetros: CodigoLinea={entity.CodigoLinea}, CodigoMarca={entity.CodigoMarca}, Descripcion={entity.Descripcion}");
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
        ViewBag.ForeignKeyData = await GetLineaVehiculoForeignKeyDataAsync();
        ViewBag.OriginalCodigoLinea = entity.CodigoLinea;
        ViewBag.OriginalCodigoMarca = entity.CodigoMarca;
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete - Override para manejar la llave compuesta
    [HttpPost]
    public override async Task<IActionResult> Delete(string keys)
    {
        try
        {
            // El keys viene como "CodigoLinea,CodigoMarca"
            var parts = keys.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = "Identificador inválido para eliminar";
                return RedirectToAction(nameof(Index));
            }

            var codigoLinea = int.Parse(parts[0].Trim());
            var codigoMarca = int.Parse(parts[1].Trim());
            
            var parameters = new object[] { codigoLinea, codigoMarca };
            await _context.Database.ExecuteSqlRawAsync(SpDelete, parameters);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminada exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            Console.WriteLine($"Error al eliminar LineaVehiculo: {ex}");
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Método específico para cargar datos de llaves foráneas de LineaVehiculo
    private async Task<Dictionary<string, List<DropdownItem>>> GetLineaVehiculoForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de MarcaVehiculo
            var marcas = await _context.MarcaVehiculos
                .OrderBy(m => m.Descripcion)
                .Select(m => new DropdownItem
                {
                    Value = m.CodigoMarca.ToString(),
                    Text = $"{m.CodigoMarca} - {m.Descripcion}"
                })
                .ToListAsync();
            
            foreignKeyData["CodigoMarca"] = marcas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }

    protected List<PropertyInfo> GetEditableProperties()
    {
        return typeof(LineaVehiculo).GetProperties()
            .Where(p => p.Name != "Vehiculos" && 
                       p.Name != "CodigoMarcaNavigation")
            .ToList();
    }

    protected List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(LineaVehiculo).GetProperties()
            .Where(p => p.Name == "CodigoLinea" ||
                       p.Name == "CodigoMarca" ||
                       p.Name == "Descripcion")
            .ToList();
    }
}