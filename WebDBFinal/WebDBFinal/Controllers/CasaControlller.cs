using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Controllers;

public class CasaController : BaseController<Casa>
{
    protected override string EntityName => "Casa";
    protected override string SpCreate => "EXEC sp_InsertarCasa @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpUpdate => "EXEC sp_ActualizarCasa @p0, @p1, @p2, @p3, @p4, @p5";
    protected override string SpDelete => "EXEC sp_EliminarCasa @p0, @p1, @p2, @p3";

    public CasaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context))
    {
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Casa entity)
    {
        // Limpiar el estado del modelo para la propiedad Cluster que causa problemas
        ModelState.Remove("Cluster");
        
        // Manejar los valores de checkbox manualmente
        if (Request.Form.ContainsKey("EsAlquilada"))
        {
            var esAlquiladaValue = Request.Form["EsAlquilada"].ToString();
            entity.EsAlquilada = esAlquiladaValue.Contains("true") || esAlquiladaValue.Contains("on");
            ModelState.Remove("EsAlquilada");
        }
        
        if (Request.Form.ContainsKey("EsOcupada"))
        {
            var esOcupadaValue = Request.Form["EsOcupada"].ToString();
            entity.EsOcupada = esOcupadaValue.Contains("true") || esOcupadaValue.Contains("on");
            ModelState.Remove("EsOcupada");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.EsAlquilada,
                    entity.EsOcupada
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
                Console.WriteLine($"Error al crear Casa: {ex}");
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
        ViewBag.ForeignKeyData = await GetCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Casa entity)
    {
        // Limpiar el estado del modelo para la propiedad Cluster que causa problemas
        ModelState.Remove("Cluster");
        
        // Manejar los valores de checkbox manualmente
        if (Request.Form.ContainsKey("EsAlquilada"))
        {
            var esAlquiladaValue = Request.Form["EsAlquilada"].ToString();
            entity.EsAlquilada = esAlquiladaValue.Contains("true") || esAlquiladaValue.Contains("on");
            ModelState.Remove("EsAlquilada");
        }
        
        if (Request.Form.ContainsKey("EsOcupada"))
        {
            var esOcupadaValue = Request.Form["EsOcupada"].ToString();
            entity.EsOcupada = esOcupadaValue.Contains("true") || esOcupadaValue.Contains("on");
            ModelState.Remove("EsOcupada");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.EsAlquilada,
                    entity.EsOcupada
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
                Console.WriteLine($"Error al actualizar Casa: {ex}");
                Console.WriteLine($"Parámetros: {entity.NumeroCasa}, {entity.CodigoCluster}, {entity.CodigoSucursal}, {entity.CodigoSector}, {entity.EsAlquilada}, {entity.EsOcupada}");
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
        ViewBag.ForeignKeyData = await GetCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Casa());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // Convertir el string de IDs en un array de objetos
            var keyValues = id.Split(',').Select(k => (object)int.Parse(k.Trim())).ToArray();
            var entity = await _context.Set<Casa>()
                .Include(c => c.Cluster)
                .FirstOrDefaultAsync(c => c.NumeroCasa == (int)keyValues[0] && 
                                         c.CodigoCluster == (int)keyValues[1] && 
                                         c.CodigoSucursal == (int)keyValues[2] && 
                                         c.CodigoSector == (int)keyValues[3]);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetCasaForeignKeyDataAsync();
            
            // Agregar información para preseleccionar el valor en el dropdown
            ViewBag.CurrentClusterValue = $"{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
            
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
        var entities = await _context.Set<Casa>()
            .Include(c => c.Cluster)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // Método específico para cargar datos de llaves foráneas de Casa
    private async Task<Dictionary<string, List<DropdownItem>>> GetCasaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Cluster (llave compuesta)
            var clusters = await _context.Clusters.ToListAsync();
            var clusterItems = clusters.Select(c => new DropdownItem
            {
                Value = $"{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Cluster: {c.CodigoCluster} - Sucursal: {c.CodigoSucursal} - Sector: {c.CodigoSector}"
            }).ToList();

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