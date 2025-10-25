using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class RegistroVehiculoResidenteController : BaseController<RegistroVehiculoResidente>
{
    protected override string EntityName => "RegistroVehiculoResidente";
    protected override string SpCreate => "EXEC InsertarRegistroVehiculoResidente @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11";
    protected override string SpUpdate => "EXEC sp_ActualizarRegistroVehiculoResidente @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11";
    protected override string SpDelete => "EXEC sp_EliminarRegistroVehiculoResidente @p0";
    
    public RegistroVehiculoResidenteController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new RegistroVehiculoResidente());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<RegistroVehiculoResidente>()
                .Include(r => r.CodigoVehiculoNavigation)
                    .ThenInclude(v => v.LineaVehiculo)
                        .ThenInclude(l => l.CodigoMarcaNavigation)
                .Include(r => r.PersonaRol)
                    .ThenInclude(pr => pr.CodigoPersonaNavigation)
                .Include(r => r.PersonaRol)
                    .ThenInclude(pr => pr.CodigoTipoRolNavigation)
                .Include(r => r.Casa)
                    .ThenInclude(c => c.Cluster)
                        .ThenInclude(cl => cl.CodigoSucursalNavigation)
                .Include(r => r.Casa)
                    .ThenInclude(c => c.Cluster)
                        .ThenInclude(cl => cl.CodigoSectorNavigation)
                .FirstOrDefaultAsync(r => r.CodigoVehiculoResidente == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
            
            // Preseleccionar valores de llaves compuestas
            ViewBag.CurrentCasaValue = $"{entity.NumeroCasa},{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
            ViewBag.CurrentPersonaRolValue = $"{entity.CodigoPersonaRol},{entity.CodigoPersona},{entity.CodigoTipoRol}";
            
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
        var entities = await _context.Set<RegistroVehiculoResidente>()
            .Include(r => r.CodigoVehiculoNavigation)
                .ThenInclude(v => v.LineaVehiculo)
                    .ThenInclude(l => l.CodigoMarcaNavigation)
            .Include(r => r.PersonaRol)
                .ThenInclude(pr => pr.CodigoPersonaNavigation)
            .Include(r => r.Casa)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(RegistroVehiculoResidente entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoVehiculoNavigation");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("Casa");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que el número de tarjeta no esté vacío
                if (string.IsNullOrWhiteSpace(entity.NumeroTarjetaTalanquera))
                {
                    ModelState.AddModelError("NumeroTarjetaTalanquera", "El número de tarjeta de talanquera es requerido");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que la fecha de registro no sea futura (si se proporciona)
                if (entity.FechaRegistro.HasValue && entity.FechaRegistro.Value > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("FechaRegistro", "La fecha de registro no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoVehiculoResidente,
                    entity.CodigoVehiculo,
                    entity.CodigoPersonaRol,
                    entity.CodigoTipoRol,
                    entity.CodigoPersona,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.NumeroTarjetaTalanquera,
                    entity.FechaRegistro.HasValue ? (object)entity.FechaRegistro.Value : null!,
                    entity.Activo.HasValue ? (object)entity.Activo.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear RegistroVehiculoResidente: {ex}");
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
        ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(RegistroVehiculoResidente entity)
    {
        ModelState.Remove("CodigoVehiculoNavigation");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("Casa");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que el número de tarjeta no esté vacío
                if (string.IsNullOrWhiteSpace(entity.NumeroTarjetaTalanquera))
                {
                    ModelState.AddModelError("NumeroTarjetaTalanquera", "El número de tarjeta de talanquera es requerido");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que la fecha de registro no sea futura (si se proporciona)
                if (entity.FechaRegistro.HasValue && entity.FechaRegistro.Value > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("FechaRegistro", "La fecha de registro no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoVehiculoResidente,
                    entity.CodigoVehiculo,
                    entity.CodigoPersonaRol,
                    entity.CodigoTipoRol,
                    entity.CodigoPersona,
                    entity.NumeroCasa,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.NumeroTarjetaTalanquera,
                    entity.FechaRegistro.HasValue ? (object)entity.FechaRegistro.Value : null!,
                    entity.Activo.HasValue ? (object)entity.Activo.Value : null!
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar RegistroVehiculoResidente: {ex}");
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
        ViewBag.ForeignKeyData = await GetRegistroVehiculoResidenteForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de RegistroVehiculoResidente
    private async Task<Dictionary<string, List<DropdownItem>>> GetRegistroVehiculoResidenteForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Vehiculo
            var vehiculos = await _context.Vehiculos
                .Include(v => v.LineaVehiculo)
                    .ThenInclude(l => l.CodigoMarcaNavigation)
                .ToListAsync();
            
            var vehiculoItems = vehiculos.Select(v => new DropdownItem
            {
                Value = v.CodigoVehiculo.ToString(),
                Text = $"Placa: {v.Placa} - {v.LineaVehiculo.CodigoMarcaNavigation.Descripcion} {v.LineaVehiculo.Descripcion} - Color: {v.Color}"
            }).ToList();
            
            foreignKeyData["CodigoVehiculo"] = vehiculoItems;

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

            // Cargar datos para PersonaRol (llave compuesta)
            var personasRol = await _context.PersonaRols
                .Include(pr => pr.CodigoPersonaNavigation)
                .Include(pr => pr.CodigoTipoRolNavigation)
                .ToListAsync();
            
            var personaRolItems = personasRol.Select(pr => new DropdownItem
            {
                Value = $"{pr.CodigoPersonaRol},{pr.CodigoPersona},{pr.CodigoTipoRol}",
                Text = $"{pr.CodigoPersonaNavigation.PrimerNombre} {pr.CodigoPersonaNavigation.PrimerApellido} - {pr.CodigoTipoRolNavigation.Descripcion}"
            }).ToList();
            
            // Asignar los mismos datos a todas las partes de la llave compuesta PersonaRol
            foreignKeyData["CodigoPersonaRol"] = personaRolItems;
            foreignKeyData["CodigoPersona"] = personaRolItems;
            foreignKeyData["CodigoTipoRol"] = personaRolItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }
}
