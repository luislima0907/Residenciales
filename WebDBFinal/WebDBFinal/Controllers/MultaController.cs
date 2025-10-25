using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class MultaController : BaseController<Multa>
{
    protected override string EntityName => "Multa";
    protected override string SpCreate => "EXEC InsertarMulta @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpUpdate => "EXEC sp_ActualizarMulta @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpDelete => "EXEC sp_EliminarMulta @p0";
    
    public MultaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Multa());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<Multa>()
                .Include(m => m.CodigoTipoMultaNavigation)
                .Include(m => m.Casa)
                    .ThenInclude(c => c.Cluster)
                        .ThenInclude(cl => cl.CodigoSucursalNavigation)
                .Include(m => m.Casa)
                    .ThenInclude(c => c.Cluster)
                        .ThenInclude(cl => cl.CodigoSectorNavigation)
                .FirstOrDefaultAsync(m => m.CodigoMulta == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrada";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
            
            // Preseleccionar valor de Casa (llave compuesta)
            ViewBag.CurrentCasaValue = $"{entity.NumeroCasa},{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
            
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
        var entities = await _context.Set<Multa>()
            .Include(m => m.CodigoTipoMultaNavigation)
            .Include(m => m.Casa)
                .ThenInclude(c => c.Cluster)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Multa entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoTipoMultaNavigation");
        ModelState.Remove("Casa");
        ModelState.Remove("AplicacionDocumentos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha no sea futura
                if (entity.Fecha > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("Fecha", "La fecha de la multa no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoMulta,
                    entity.CodigoTipoMulta,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.Fecha,
                    entity.Pagada.HasValue ? (object)entity.Pagada.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear Multa: {ex}");
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
        ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Multa entity)
    {
        ModelState.Remove("CodigoTipoMultaNavigation");
        ModelState.Remove("Casa");
        ModelState.Remove("AplicacionDocumentos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha no sea futura
                if (entity.Fecha > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("Fecha", "La fecha de la multa no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoMulta,
                    entity.CodigoTipoMulta,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.Fecha,
                    entity.Pagada.HasValue ? (object)entity.Pagada.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar Multa: {ex}");
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
        ViewBag.ForeignKeyData = await GetMultaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de Multa
    private async Task<Dictionary<string, List<DropdownItem>>> GetMultaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para TipoMulta
            var tiposMulta = await _context.TipoMulta.ToListAsync();
            
            var tipoMultaItems = tiposMulta.Select(tm => new DropdownItem
            {
                Value = tm.CodigoTipoMulta.ToString(),
                Text = $"{tm.Descripcion}"
            }).ToList();
            
            foreignKeyData["CodigoTipoMulta"] = tipoMultaItems;

            // Cargar datos para Casa (llave compuesta)
            var casas = await _context.Casas
                .Include(c => c.Cluster)
                    .ThenInclude(cl => cl.CodigoSucursalNavigation)
                .Include(c => c.Cluster)
                    .ThenInclude(cl => cl.CodigoSectorNavigation)
                .ToListAsync();
            
            var casaItems = casas.Select(c => new DropdownItem
            {
                Value = $"{c.NumeroCasa},{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Casa #{c.NumeroCasa} - Cluster: {c.CodigoCluster} - Sucursal: {c.Cluster.CodigoSucursalNavigation.Descripcion} - Sector: {c.Cluster.CodigoSectorNavigation.Nombre}"
            }).ToList();
            
            // Asignar los mismos datos a todas las partes de la llave compuesta Casa
            foreignKeyData["NumeroCasa"] = casaItems;
            foreignKeyData["CodigoCluster"] = casaItems;
            foreignKeyData["CodigoSucursal"] = casaItems;
            foreignKeyData["CodigoSector"] = casaItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}