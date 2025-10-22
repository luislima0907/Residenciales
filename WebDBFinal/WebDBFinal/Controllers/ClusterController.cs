using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Controllers;

public class ClusterController : BaseController<Cluster>
{
    protected override string EntityName => "Cluster";
    protected override string SpCreate => "EXEC InsertarCluster @p0, @p1, @p2";
    protected override string SpUpdate => "EXEC sp_ActualizarCluster @p0, @p1, @p2, @p3, @p4";
    protected override string SpDelete => "EXEC sp_EliminarCluster @p0, @p1, @p2";
    
    public ClusterController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<Cluster>()
            .Include(c => c.CodigoSectorNavigation)
            .Include(c => c.CodigoSucursalNavigation)
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
        ViewBag.ForeignKeyData = await GetClusterForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Cluster());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // Convertir el string de IDs en un array de objetos
            var keyValues = id.Split(',').Select(k => (object)int.Parse(k.Trim())).ToArray();
            var entity = await _context.Set<Cluster>()
                .Include(c => c.CodigoSectorNavigation)
                .Include(c => c.CodigoSucursalNavigation)
                .FirstOrDefaultAsync(c => c.CodigoCluster == (int)keyValues[0] && 
                                         c.CodigoSucursal == (int)keyValues[1] && 
                                         c.CodigoSector == (int)keyValues[2]);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Guardar los valores originales para el procedimiento almacenado
            ViewBag.OriginalCodigoSucursal = entity.CodigoSucursal;
            ViewBag.OriginalCodigoSector = entity.CodigoSector;
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetClusterForeignKeyDataAsync();
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity); // Usar vista genérica
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Cluster entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación que causan problemas
        ModelState.Remove("CodigoSectorNavigation");
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("Casas");
        ModelState.Remove("GaritaSeguridads");
        ModelState.Remove("JuntaDirectivas");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                // Log para debugging
                Console.WriteLine($"Error al crear Cluster: {ex}");
                Console.WriteLine($"Parámetros: CodigoCluster={entity.CodigoCluster}, CodigoSucursal={entity.CodigoSucursal}, CodigoSector={entity.CodigoSector}");
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
        ViewBag.ForeignKeyData = await GetClusterForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Cluster entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación que causan problemas
        ModelState.Remove("CodigoSectorNavigation");
        ModelState.Remove("CodigoSucursalNavigation");
        ModelState.Remove("Casas");
        ModelState.Remove("GaritaSeguridads");
        ModelState.Remove("JuntaDirectivas");
        
        // Obtener los valores originales del formulario (campos ocultos)
        var originalCodigoSucursal = int.Parse(Request.Form["OriginalCodigoSucursal"].ToString());
        var originalCodigoSector = int.Parse(Request.Form["OriginalCodigoSector"].ToString());
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // @CodigoCluster, @CodigoSucursal (original), @CodigoSector (original), @NuevoCodigoSucursal, @NuevoCodigoSector
                var parameters = new object[]
                {
                    entity.CodigoCluster,
                    originalCodigoSucursal,  // Sucursal original para identificar
                    originalCodigoSector,     // Sector original para identificar
                    entity.CodigoSucursal,    // Nueva sucursal
                    entity.CodigoSector       // Nuevo sector
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                // Log para debugging
                Console.WriteLine($"Error al actualizar Cluster: {ex}");
                Console.WriteLine($"Parámetros: CodigoCluster={entity.CodigoCluster}, OriginalSucursal={originalCodigoSucursal}, OriginalSector={originalCodigoSector}, NuevaSucursal={entity.CodigoSucursal}, NuevoSector={entity.CodigoSector}");
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
        ViewBag.ForeignKeyData = await GetClusterForeignKeyDataAsync();
        ViewBag.OriginalCodigoSucursal = originalCodigoSucursal;
        ViewBag.OriginalCodigoSector = originalCodigoSector;
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de Cluster
    private async Task<Dictionary<string, List<DropdownItem>>> GetClusterForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Sector
            var sectores = await _context.Sectors.ToListAsync();
            var sectorItems = sectores.Select(s => new DropdownItem
            {
                Value = s.CodigoSector.ToString(),
                Text = $"Sector {s.CodigoSector} - {s.Nombre}"
            }).ToList();

            foreignKeyData["CodigoSector"] = sectorItems;

            // Cargar datos para Sucursal
            var sucursales = await _context.Sucursals.ToListAsync();
            var sucursalItems = sucursales.Select(s => new DropdownItem
            {
                Value = s.CodigoSucursal.ToString(),
                Text = $"Sucursal {s.CodigoSucursal} - {s.Descripcion}"
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