using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class GaritaSeguridadController : BaseController<GaritaSeguridad>
{
    protected override string EntityName => "GaritaSeguridad";
    protected override string SpCreate => "EXEC InsertarGaritaSeguridad @p0, @p1, @p2, @p3, @p4";
    protected override string SpUpdate => "EXEC sp_ActualizarGaritaSeguridad @p0, @p1, @p2, @p3, @p4";
    protected override string SpDelete => "EXEC sp_EliminarGaritaSeguridad @p0";
    
    public GaritaSeguridadController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(GaritaSeguridad entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoTipoGaritaNavigation");
        ModelState.Remove("Cluster");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoGaritaSeguridad,
                    entity.CodigoTipoGarita,
                    entity.CodigoCluster.HasValue ? (object)entity.CodigoCluster.Value : null!,
                    entity.CodigoSucursal.HasValue ? (object)entity.CodigoSucursal.Value : null!,
                    entity.CodigoSector.HasValue ? (object)entity.CodigoSector.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear GaritaSeguridad: {ex}");
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
        ViewBag.ForeignKeyData = await GetGaritaSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(GaritaSeguridad entity)
    {
        ModelState.Remove("CodigoTipoGaritaNavigation");
        ModelState.Remove("Cluster");
        
        if (ModelState.IsValid)
        {
            try
            {
                var parameters = new object[]
                {
                    entity.CodigoGaritaSeguridad,
                    entity.CodigoTipoGarita,
                    entity.CodigoCluster.HasValue ? (object)entity.CodigoCluster.Value : null!,
                    entity.CodigoSucursal.HasValue ? (object)entity.CodigoSucursal.Value : null!,
                    entity.CodigoSector.HasValue ? (object)entity.CodigoSector.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar GaritaSeguridad: {ex}");
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
        ViewBag.ForeignKeyData = await GetGaritaSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetGaritaSeguridadForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new GaritaSeguridad());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<GaritaSeguridad>()
                .Include(g => g.CodigoTipoGaritaNavigation)
                .Include(g => g.Cluster)
                    .ThenInclude(c => c.CodigoSucursalNavigation)
                .Include(g => g.Cluster)
                    .ThenInclude(c => c.CodigoSectorNavigation)
                .FirstOrDefaultAsync(g => g.CodigoGaritaSeguridad == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrada";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetGaritaSeguridadForeignKeyDataAsync();
            
            // Preseleccionar el valor del cluster si existe
            if (entity.CodigoCluster.HasValue && entity.CodigoSucursal.HasValue && entity.CodigoSector.HasValue)
            {
                ViewBag.CurrentClusterValue = $"{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
            }
            
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
        var entities = await _context.Set<GaritaSeguridad>()
            .Include(g => g.CodigoTipoGaritaNavigation)
            .Include(g => g.Cluster)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // Método específico para cargar datos de llaves foráneas de GaritaSeguridad
    private async Task<Dictionary<string, List<DropdownItem>>> GetGaritaSeguridadForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para TipoGarita
            var tiposGarita = await _context.TipoGarita.ToListAsync();
            var tipoGaritaItems = tiposGarita.Select(tg => new DropdownItem
            {
                Value = tg.CodigoTipoGarita.ToString(),
                Text = $"{tg.Descripcion} - Código: {tg.CodigoTipoGarita}"
            }).ToList();
            foreignKeyData["CodigoTipoGarita"] = tipoGaritaItems;

            // Cargar datos para Cluster (llave compuesta - opcional)
            var clusters = await _context.Clusters
                .Include(c => c.CodigoSucursalNavigation)
                .Include(c => c.CodigoSectorNavigation)
                .ToListAsync();
            
            var clusterItems = clusters.Select(c => new DropdownItem
            {
                Value = $"{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Cluster: {c.CodigoCluster} - Sucursal: {c.CodigoSucursalNavigation.Descripcion} - Sector: {c.CodigoSectorNavigation.Nombre}"
            }).ToList();

            // Agregar opción vacía para campos opcionales
            clusterItems.Insert(0, new DropdownItem { Value = "", Text = "(Sin asignar)" });

            // Asignar los mismos datos a todas las partes de la llave compuesta
            foreignKeyData["CodigoCluster"] = clusterItems;
            foreignKeyData["CodigoSucursal"] = clusterItems;
            foreignKeyData["CodigoSector"] = clusterItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}