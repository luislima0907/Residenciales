using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class RegistroIngresoOSalidaController : BaseController<RegistroIngresoOSalida>
{
    protected override string EntityName => "RegistroIngresoOSalida";
    protected override string SpCreate => "EXEC InsertarRegistroIngresoOSalida @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarRegistroIngresoOSalida @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarRegistroIngresoOSalida @p0";
    
    public RegistroIngresoOSalidaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new RegistroIngresoOSalida());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<RegistroIngresoOSalida>()
                .Include(r => r.CodigoMovimientoResidencialNavigation)
                    .ThenInclude(m => m.CodigoGaritaSeguridadNavigation)
                .Include(r => r.CodigoMovimientoResidencialNavigation)
                    .ThenInclude(m => m.CodigoPersonaTelefonoNavigation)
                        .ThenInclude(pt => pt.CodigoPersonaNavigation)
                .FirstOrDefaultAsync(r => r.CodigoEntradaOSalida == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
            
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
        var entities = await _context.Set<RegistroIngresoOSalida>()
            .Include(r => r.CodigoMovimientoResidencialNavigation)
                .ThenInclude(m => m.CodigoGaritaSeguridadNavigation)
            .Include(r => r.CodigoMovimientoResidencialNavigation)
                .ThenInclude(m => m.CodigoPersonaTelefonoNavigation)
                    .ThenInclude(pt => pt.CodigoPersonaNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(RegistroIngresoOSalida entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoMovimientoResidencialNavigation");
        
        // Log para debugging
        Console.WriteLine($"DEBUG - Creando RegistroIngresoOSalida:");
        Console.WriteLine($"  CodigoEntradaOSalida: {entity.CodigoEntradaOSalida}");
        Console.WriteLine($"  CodigoMovimientoResidencial: {entity.CodigoMovimientoResidencial}");
        Console.WriteLine($"  TipoMovimiento: '{entity.TipoMovimiento}'");
        Console.WriteLine($"  FechaHora: {entity.FechaHora}");
        Console.WriteLine($"  ModelState.IsValid: {ModelState.IsValid}");
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) });
            
            var errorDetails = string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
            Console.WriteLine($"DEBUG - Errores de validación: {errorDetails}");
            TempData["ErrorMessage"] = "Errores de validación: " + errorDetails;
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
            return View("~/Views/Shared/GenericCreate.cshtml", entity);
        }
        
        try
        {
            // Asegurar que TipoMovimiento solo tenga el primer carácter y esté en mayúscula
            if (!string.IsNullOrEmpty(entity.TipoMovimiento))
            {
                entity.TipoMovimiento = entity.TipoMovimiento.Trim().ToUpper().Substring(0, 1);
            }
            
            // Validar que el tipo de movimiento sea válido
            if (entity.TipoMovimiento != "I" && entity.TipoMovimiento != "S")
            {
                ModelState.AddModelError("TipoMovimiento", "El tipo de movimiento debe ser 'I' (Ingreso) o 'S' (Salida)");
                TempData["ErrorMessage"] = "El tipo de movimiento debe ser 'I' (Ingreso) o 'S' (Salida)";
                ViewBag.EntityName = EntityName;
                ViewBag.Properties = GetEditableProperties();
                ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
                return View("~/Views/Shared/GenericCreate.cshtml", entity);
            }

            // Validar que la fecha/hora no sea futura
            if (entity.FechaHora > DateTime.Now)
            {
                ModelState.AddModelError("FechaHora", "La fecha y hora no puede ser futura");
                TempData["ErrorMessage"] = "La fecha y hora no puede ser futura";
                ViewBag.EntityName = EntityName;
                ViewBag.Properties = GetEditableProperties();
                ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
                return View("~/Views/Shared/GenericCreate.cshtml", entity);
            }

            // Crear parámetros en el orden correcto que espera el SP
            var parameters = new object[]
            {
                entity.CodigoEntradaOSalida,
                entity.CodigoMovimientoResidencial,
                entity.TipoMovimiento,
                entity.FechaHora
            };

            Console.WriteLine($"DEBUG - Ejecutando SP con parámetros: {string.Join(", ", parameters)}");
            await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
            
            TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al guardar: {ex.Message}";
            var innerMsg = ex.InnerException?.Message ?? "";
            Console.WriteLine($"ERROR - {errorMsg}");
            Console.WriteLine($"ERROR - Inner: {innerMsg}");
            Console.WriteLine($"ERROR - StackTrace: {ex.StackTrace}");
            
            ModelState.AddModelError("", errorMsg);
            TempData["ErrorMessage"] = $"Error: {errorMsg}. Detalle: {innerMsg}";
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(RegistroIngresoOSalida entity)
    {
        ModelState.Remove("CodigoMovimientoResidencialNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que el tipo de movimiento sea válido
                if (entity.TipoMovimiento != "I" && entity.TipoMovimiento != "S")
                {
                    ModelState.AddModelError("TipoMovimiento", "El tipo de movimiento debe ser 'I' (Ingreso) o 'S' (Salida)");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que la fecha/hora no sea futura
                if (entity.FechaHora > DateTime.Now)
                {
                    ModelState.AddModelError("FechaHora", "La fecha y hora no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoEntradaOSalida,
                    entity.CodigoMovimientoResidencial,
                    entity.TipoMovimiento,
                    entity.FechaHora
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar RegistroIngresoOSalida: {ex}");
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
        ViewBag.ForeignKeyData = await GetRegistroIngresoOSalidaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de RegistroIngresoOSalida
    private async Task<Dictionary<string, List<DropdownItem>>> GetRegistroIngresoOSalidaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para RegistroMovimientoResidencial
            var movimientos = await _context.RegistroMovimientoResidencials
                .Include(m => m.CodigoGaritaSeguridadNavigation)
                .Include(m => m.CodigoPersonaTelefonoNavigation)
                    .ThenInclude(pt => pt.CodigoPersonaNavigation)
                .Include(m => m.Casa)
                .ToListAsync();
            
            var movimientoItems = movimientos.Select(m => new DropdownItem
            {
                Value = m.CodigoMovimientoResidencial.ToString(),
                Text = $"Mov. {m.CodigoMovimientoResidencial} - Persona: {m.CodigoPersonaTelefonoNavigation.CodigoPersonaNavigation.PrimerNombre} {m.CodigoPersonaTelefonoNavigation.CodigoPersonaNavigation.PrimerApellido} - Garita: {m.CodigoGaritaSeguridadNavigation.CodigoGaritaSeguridad}" +
                       (m.Casa != null ? $" - Casa: {m.Casa.NumeroCasa}" : "") +
                       (m.MotivoVisita != null ? $" - Motivo: {m.MotivoVisita}" : "")
            }).ToList();
            
            foreignKeyData["CodigoMovimientoResidencial"] = movimientoItems;

            // Cargar datos para TipoMovimiento (valores fijos)
            var tipoMovimientoItems = new List<DropdownItem>
            {
                new DropdownItem { Value = "I", Text = "I - Ingreso" },
                new DropdownItem { Value = "S", Text = "S - Salida" }
            };
            foreignKeyData["TipoMovimiento"] = tipoMovimientoItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
