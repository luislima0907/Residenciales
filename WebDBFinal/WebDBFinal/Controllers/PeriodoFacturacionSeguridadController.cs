using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class PeriodoFacturacionSeguridadController : BaseController<PeriodoFacturacionSeguridad>
{
    protected override string EntityName => "PeriodoFacturacionSeguridad";
    protected override string SpCreate => "EXEC InsertarPeriodoFacturacionSeguridad @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarPeriodoFacturacionSeguridad @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarPeriodoFacturacionSeguridad @p0";
    
    public PeriodoFacturacionSeguridadController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new PeriodoFacturacionSeguridad());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<PeriodoFacturacionSeguridad>()
                .Include(p => p.CodigoSucursalNavigation)
                .FirstOrDefaultAsync(p => p.CodigoPeriodoFacturacion == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
            
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
        var entities = await _context.Set<PeriodoFacturacionSeguridad>()
            .Include(p => p.CodigoSucursalNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(PeriodoFacturacionSeguridad entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("DetalleFacturacionSeguridads");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha fin sea posterior a la fecha inicio
                if (entity.FechaFin <= entity.FechaInicio)
                {
                    ModelState.AddModelError("FechaFin", "La fecha fin debe ser posterior a la fecha de inicio");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoPeriodoFacturacion,
                    entity.CodigoSucursal,
                    entity.FechaInicio,
                    entity.FechaFin
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear PeriodoFacturacionSeguridad: {ex}");
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
        ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(PeriodoFacturacionSeguridad entity)
    {
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("DetalleFacturacionSeguridads");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha fin sea posterior a la fecha inicio
                if (entity.FechaFin <= entity.FechaInicio)
                {
                    ModelState.AddModelError("FechaFin", "La fecha fin debe ser posterior a la fecha de inicio");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoPeriodoFacturacion,
                    entity.CodigoSucursal,
                    entity.FechaInicio,
                    entity.FechaFin
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar PeriodoFacturacionSeguridad: {ex}");
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
        ViewBag.ForeignKeyData = await GetPeriodoFacturacionSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de PeriodoFacturacionSeguridad
    private async Task<Dictionary<string, List<DropdownItem>>> GetPeriodoFacturacionSeguridadForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Sucursal
            var sucursales = await _context.Sucursals
                .Include(s => s.CodigoEmpresaNavigation)
                .ToListAsync();
            
            var sucursalItems = sucursales.Select(s => new DropdownItem
            {
                Value = s.CodigoSucursal.ToString(),
                Text = s.CodigoEmpresaNavigation != null 
                    ? $"{s.Descripcion} - Empresa: {s.CodigoEmpresaNavigation.RazonSocial}"
                    : s.Descripcion
            }).ToList();
            
            foreignKeyData["CodigoSucursal"] = sucursalItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
