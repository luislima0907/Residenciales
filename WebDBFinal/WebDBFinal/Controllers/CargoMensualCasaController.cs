using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class CargoMensualCasaController : BaseController<CargoMensualCasa>
{
    protected override string EntityName => "CargoMensualCasa";
    protected override string SpCreate => "EXEC sp_InsertarCargoMensualCasa @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9";
    protected override string SpUpdate => "EXEC sp_ActualizarCargoMensualCasa @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9";
    protected override string SpDelete => "EXEC sp_EliminarCargoMensualCasa @p0";

    public CargoMensualCasaController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService)
    {
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(CargoMensualCasa entity)
    {
        // Limpiar el estado del modelo para la propiedad Casa que causa problemas
        ModelState.Remove("Casa");
        
        // Manejar los valores de checkbox manualmente
        if (Request.Form.ContainsKey("Pagado"))
        {
            var pagadoValue = Request.Form["Pagado"].ToString();
            entity.Pagado = pagadoValue.Contains("true") || pagadoValue.Contains("on");
            ModelState.Remove("Pagado");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoCargoMensual,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.MesCargo,
                    entity.AnioCargo,
                    entity.FechaVencimiento,
                    entity.MontoTotal,
                    entity.Pagado
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
                Console.WriteLine($"Error al crear CargoMensualCasa: {ex}");
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
        ViewBag.ForeignKeyData = await GetCargoMensualCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(CargoMensualCasa entity)
    {
        // Limpiar el estado del modelo para la propiedad Casa que causa problemas
        ModelState.Remove("Casa");
        
        // Manejar los valores de checkbox manualmente
        if (Request.Form.ContainsKey("Pagado"))
        {
            var pagadoValue = Request.Form["Pagado"].ToString();
            entity.Pagado = pagadoValue.Contains("true") || pagadoValue.Contains("on");
            ModelState.Remove("Pagado");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoCargoMensual,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.MesCargo,
                    entity.AnioCargo,
                    entity.FechaVencimiento,
                    entity.MontoTotal,
                    entity.Pagado
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
                Console.WriteLine($"Error al actualizar CargoMensualCasa: {ex}");
                Console.WriteLine($"Parámetros: {entity.CodigoCargoMensual}, {entity.NumeroCasa}, {entity.CodigoCluster}, {entity.CodigoSucursal}, {entity.CodigoSector}, {entity.MesCargo}, {entity.AnioCargo}, {entity.FechaVencimiento}, {entity.MontoTotal}, {entity.Pagado}");
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
        ViewBag.ForeignKeyData = await GetCargoMensualCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetCargoMensualCasaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new CargoMensualCasa());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // Para CargoMensualCasa, el ID es simple (CodigoCargoMensual)
            var entityId = int.Parse(id);
            var entity = await _context.Set<CargoMensualCasa>()
                .Include(c => c.Casa)
                .FirstOrDefaultAsync(c => c.CodigoCargoMensual == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetCargoMensualCasaForeignKeyDataAsync();
            
            // Agregar información para preseleccionar el valor en el dropdown
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
        var entities = await _context.Set<CargoMensualCasa>()
            .Include(c => c.Casa)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // Método específico para cargar datos de llaves foráneas de CargoMensualCasa
    private async Task<Dictionary<string, List<DropdownItem>>> GetCargoMensualCasaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Casa (llave compuesta)
            var casas = await _context.Casas.Include(c => c.Cluster).ToListAsync();
            var casaItems = casas.Select(c => new DropdownItem
            {
                Value = $"{c.NumeroCasa},{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Casa: {c.NumeroCasa} - Cluster: {c.CodigoCluster} - Sucursal: {c.CodigoSucursal} - Sector: {c.CodigoSector}"
            }).ToList();

            // Asignar los mismos datos a todas las partes de la llave compuesta
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
