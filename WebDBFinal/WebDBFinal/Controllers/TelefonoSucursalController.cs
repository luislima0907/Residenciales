using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class TelefonoSucursalController : BaseController<TelefonoSucursal>
{
    protected override string EntityName => "TelefonoSucursal";
    protected override string SpCreate => "EXEC InsertarTelefonoSucursal @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarTelefonoSucursal @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarTelefonoSucursal @p0, @p1";
    
    public TelefonoSucursalController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<TelefonoSucursal>()
            .Include(ts => ts.CodigoSucursalNavigation)
            .Include(ts => ts.CodigoTipoTelefonoNavigation)
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
        ViewBag.ForeignKeyData = await GetTelefonoSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new TelefonoSucursal());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // El id viene como "CodigoTelefonoSucursal,CodigoSucursal"
            var parts = id.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = $"ID inválido para TelefonoSucursal. Se esperaban 2 valores pero se recibieron {parts.Length}.";
                return RedirectToAction(nameof(Index));
            }

            var codigoTelefonoSucursal = int.Parse(parts[0].Trim());
            var codigoSucursal = int.Parse(parts[1].Trim());

            var entity = await _context.Set<TelefonoSucursal>()
                .Include(ts => ts.CodigoSucursalNavigation)
                .Include(ts => ts.CodigoTipoTelefonoNavigation)
                .FirstOrDefaultAsync(ts => ts.CodigoTelefonoSucursal == codigoTelefonoSucursal && ts.CodigoSucursal == codigoSucursal);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetTelefonoSucursalForeignKeyDataAsync();
            
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
    public override async Task<IActionResult> Create(TelefonoSucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("CodigoTipoTelefonoNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarTelefonoSucursal: @CodigoTelefonoSucursal, @CodigoSucursal, @CodigoTipoTelefono, @Telefono
                var parameters = new object[]
                {
                    entity.CodigoTelefonoSucursal,
                    entity.CodigoSucursal,
                    entity.CodigoTipoTelefono,
                    entity.Telefono
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear TelefonoSucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetTelefonoSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(TelefonoSucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("CodigoTipoTelefonoNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarTelefonoSucursal: @CodigoTelefonoSucursal, @CodigoSucursal, @CodigoTipoTelefono, @Telefono
                var parameters = new object[]
                {
                    entity.CodigoTelefonoSucursal,
                    entity.CodigoSucursal,
                    entity.CodigoTipoTelefono,
                    entity.Telefono
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar TelefonoSucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetTelefonoSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete - Override para manejar la llave compuesta
    [HttpPost]
    public override async Task<IActionResult> Delete(string keys)
    {
        try
        {
            // El keys viene como "CodigoTelefonoSucursal,CodigoSucursal"
            var parts = keys.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = "Identificador inválido para eliminar";
                return RedirectToAction(nameof(Index));
            }

            var codigoTelefonoSucursal = int.Parse(parts[0].Trim());
            var codigoSucursal = int.Parse(parts[1].Trim());
            
            var parameters = new object[] { codigoTelefonoSucursal, codigoSucursal };
            await _context.Database.ExecuteSqlRawAsync(SpDelete, parameters);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            Console.WriteLine($"Error al eliminar TelefonoSucursal: {ex}");
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Método específico para cargar datos de llaves foráneas de TelefonoSucursal
    private async Task<Dictionary<string, List<DropdownItem>>> GetTelefonoSucursalForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de Sucursal
            var sucursales = await _context.Sucursals
                .OrderBy(s => s.Descripcion)
                .Select(s => new DropdownItem
                {
                    Value = s.CodigoSucursal.ToString(),
                    Text = $"{s.CodigoSucursal} - {s.Descripcion}"
                })
                .ToListAsync();
            
            foreignKeyData["CodigoSucursal"] = sucursales;
            
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
        return typeof(TelefonoSucursal).GetProperties()
            .Where(p => p.Name != "CodigoSucursalNavigation" &&
                       p.Name != "CodigoTipoTelefonoNavigation")
            .ToList();
    }

    protected new List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(TelefonoSucursal).GetProperties()
            .Where(p => p.Name == "CodigoTelefonoSucursal" ||
                       p.Name == "CodigoSucursal" ||
                       p.Name == "Telefono" ||
                       p.Name == "CodigoSucursalNavigation" ||
                       p.Name == "CodigoTipoTelefonoNavigation")
            .ToList();
    }
}
