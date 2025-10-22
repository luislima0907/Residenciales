using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class DireccionSucursalController : BaseController<DireccionSucursal>
{
    protected override string EntityName => "DireccionSucursal";
    protected override string SpCreate => "EXEC InsertarDireccionSucursal @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpUpdate => "EXEC sp_ActualizarDireccionSucursal @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpDelete => "EXEC sp_EliminarDireccionSucursal @p0, @p1";
    
    public DireccionSucursalController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<DireccionSucursal>()
            .Include(ds => ds.CodigoSucursalNavigation)
            .Include(ds => ds.Municipio)
                .ThenInclude(m => m.CodigoDepartamentoNavigation)
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
        ViewBag.ForeignKeyData = await GetDireccionSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new DireccionSucursal());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // El id viene como "CodigoDireccionSucursal,CodigoSucursal"
            var parts = id.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = $"ID inválido para DireccionSucursal. Se esperaban 2 valores pero se recibieron {parts.Length}.";
                return RedirectToAction(nameof(Index));
            }

            var codigoDireccionSucursal = int.Parse(parts[0].Trim());
            var codigoSucursal = int.Parse(parts[1].Trim());

            var entity = await _context.Set<DireccionSucursal>()
                .Include(ds => ds.CodigoSucursalNavigation)
                .Include(ds => ds.Municipio)
                    .ThenInclude(m => m.CodigoDepartamentoNavigation)
                .FirstOrDefaultAsync(ds => ds.CodigoDireccionSucursal == codigoDireccionSucursal && ds.CodigoSucursal == codigoSucursal);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetDireccionSucursalForeignKeyDataAsync();
            
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
    public override async Task<IActionResult> Create(DireccionSucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("Municipio");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarDireccionSucursal: @CodigoDireccionSucursal, @CodigoSucursal, @Calle, @Avenida, @Zona, @Ciudad, @CodigoMunicipio, @CodigoDepartamento
                var parameters = new object[]
                {
                    entity.CodigoDireccionSucursal,
                    entity.CodigoSucursal,
                    entity.Calle,
                    entity.Avenida,
                    entity.Zona,
                    entity.Ciudad,
                    entity.CodigoMunicipio,
                    entity.CodigoDepartamento
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear DireccionSucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetDireccionSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(DireccionSucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("Municipio");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarDireccionSucursal: @CodigoDireccionSucursal, @CodigoSucursal, @Calle, @Avenida, @Zona, @Ciudad, @CodigoMunicipio, @CodigoDepartamento
                var parameters = new object[]
                {
                    entity.CodigoDireccionSucursal,
                    entity.CodigoSucursal,
                    entity.Calle,
                    entity.Avenida,
                    entity.Zona,
                    entity.Ciudad,
                    entity.CodigoMunicipio,
                    entity.CodigoDepartamento
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar DireccionSucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetDireccionSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete - Override para manejar la llave compuesta
    [HttpPost]
    public override async Task<IActionResult> Delete(string keys)
    {
        try
        {
            // El keys viene como "CodigoDireccionSucursal,CodigoSucursal"
            var parts = keys.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = "Identificador inválido para eliminar";
                return RedirectToAction(nameof(Index));
            }

            var codigoDireccionSucursal = int.Parse(parts[0].Trim());
            var codigoSucursal = int.Parse(parts[1].Trim());
            
            var parameters = new object[] { codigoDireccionSucursal, codigoSucursal };
            await _context.Database.ExecuteSqlRawAsync(SpDelete, parameters);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            Console.WriteLine($"Error al eliminar DireccionSucursal: {ex}");
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Método específico para cargar datos de llaves foráneas de DireccionSucursal
    private async Task<Dictionary<string, List<DropdownItem>>> GetDireccionSucursalForeignKeyDataAsync()
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
            
            // Cargar datos de Municipio agrupados por Departamento
            // El Value será "CodigoMunicipio,CodigoDepartamento" para manejar la llave compuesta
            var municipios = await _context.Municipios
                .Include(m => m.CodigoDepartamentoNavigation)
                .OrderBy(m => m.CodigoDepartamentoNavigation.Descripcion)
                .ThenBy(m => m.Descripcion)
                .Select(m => new DropdownItem
                {
                    Value = $"{m.CodigoMunicipio},{m.CodigoDepartamento}",
                    Text = $"{m.Descripcion} - {m.CodigoDepartamentoNavigation.Descripcion}"
                })
                .ToListAsync();
            
            // Usar el mismo nombre para CodigoMunicipio y CodigoDepartamento
            // El frontend manejará la separación de valores
            foreignKeyData["CodigoMunicipio"] = municipios;
            foreignKeyData["CodigoDepartamento"] = municipios;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos de llaves foráneas: {ex.Message}");
        }
        
        return foreignKeyData;
    }
}
