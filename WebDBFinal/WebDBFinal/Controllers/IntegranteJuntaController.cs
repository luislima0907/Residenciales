using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class IntegranteJuntaController : BaseController<IntegranteJunta>
{
    protected override string EntityName => "IntegranteJunta";
    protected override string SpCreate => "EXEC InsertarIntegranteJunta @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9";
    protected override string SpUpdate => "EXEC sp_ActualizarIntegranteJunta @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9";
    protected override string SpDelete => "EXEC sp_EliminarIntegranteJunta @p0";
    
    public IntegranteJuntaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new IntegranteJunta());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var entityId = int.Parse(id);
            var entity = await _context.Set<IntegranteJunta>()
                .Include(i => i.JuntaDirectiva)
                    .ThenInclude(j => j.Cluster)
                        .ThenInclude(c => c.CodigoSucursalNavigation)
                .Include(i => i.JuntaDirectiva)
                    .ThenInclude(j => j.Cluster)
                        .ThenInclude(c => c.CodigoSectorNavigation)
                .Include(i => i.PersonaRol)
                    .ThenInclude(pr => pr.CodigoPersonaNavigation)
                .Include(i => i.PersonaRol)
                    .ThenInclude(pr => pr.CodigoTipoRolNavigation)
                .Include(i => i.CodigoTipoIntegranteNavigation)
                .FirstOrDefaultAsync(i => i.CodigoIntegranteJunta == entityId);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
            
            // Preseleccionar valores de llaves compuestas
            ViewBag.CurrentJuntaDirectivaValue = $"{entity.CodigoJunta},{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
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
        var entities = await _context.Set<IntegranteJunta>()
            .Include(i => i.JuntaDirectiva)
            .Include(i => i.PersonaRol)
                .ThenInclude(pr => pr.CodigoPersonaNavigation)
            .Include(i => i.CodigoTipoIntegranteNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(IntegranteJunta entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("JuntaDirectiva");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("CodigoTipoIntegranteNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha de designación no sea futura
                if (entity.FechaDesignacion > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("FechaDesignacion", "La fecha de designación no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que sea propietario (esto lo validará también el SP, pero lo verificamos aquí primero)
                if (entity.CodigoTipoRol != 1)
                {
                    ModelState.AddModelError("CodigoTipoRol", "Solo los propietarios pueden formar parte de la junta directiva");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoIntegranteJunta,
                    entity.CodigoJunta,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.CodigoPersonaRol,
                    entity.CodigoTipoRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoIntegrante,
                    entity.FechaDesignacion
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear IntegranteJunta: {ex}");
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
        ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(IntegranteJunta entity)
    {
        ModelState.Remove("JuntaDirectiva");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("CodigoTipoIntegranteNavigation");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la fecha de designación no sea futura
                if (entity.FechaDesignacion > DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("FechaDesignacion", "La fecha de designación no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que sea propietario
                if (entity.CodigoTipoRol != 1)
                {
                    ModelState.AddModelError("CodigoTipoRol", "Solo los propietarios pueden formar parte de la junta directiva");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoIntegranteJunta,
                    entity.CodigoJunta,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.CodigoPersonaRol,
                    entity.CodigoTipoRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoIntegrante,
                    entity.FechaDesignacion
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar IntegranteJunta: {ex}");
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
        ViewBag.ForeignKeyData = await GetIntegranteJuntaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de IntegranteJunta
    private async Task<Dictionary<string, List<DropdownItem>>> GetIntegranteJuntaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para TipoIntegrante
            var tiposIntegrante = await _context.TipoIntegrantes.ToListAsync();
            
            var tipoIntegranteItems = tiposIntegrante.Select(ti => new DropdownItem
            {
                Value = ti.CodigoTipoIntegrante.ToString(),
                Text = $"{ti.Descripcion}"
            }).ToList();
            
            foreignKeyData["CodigoTipoIntegrante"] = tipoIntegranteItems;

            // Cargar datos para JuntaDirectiva (llave compuesta)
            var juntas = await _context.JuntaDirectivas
                .Include(j => j.Cluster)
                    .ThenInclude(c => c.CodigoSucursalNavigation)
                .Include(j => j.Cluster)
                    .ThenInclude(c => c.CodigoSectorNavigation)
                .ToListAsync();
            
            var juntaItems = juntas.Select(j => new DropdownItem
            {
                Value = $"{j.CodigoJunta},{j.CodigoCluster},{j.CodigoSucursal},{j.CodigoSector}",
                Text = $"Junta {j.CodigoJunta}: {j.Descripcion} - Cluster: {j.CodigoCluster} - Sucursal: {j.Cluster.CodigoSucursalNavigation.Descripcion} - Sector: {j.Cluster.CodigoSectorNavigation.Nombre} ({j.FechaInicio:dd/MM/yyyy} - {j.FechaFin:dd/MM/yyyy})"
            }).ToList();
            
            // Asignar los mismos datos a todas las partes de la llave compuesta JuntaDirectiva
            foreignKeyData["CodigoJunta"] = juntaItems;
            foreignKeyData["CodigoCluster"] = juntaItems;
            foreignKeyData["CodigoSucursal"] = juntaItems;
            foreignKeyData["CodigoSector"] = juntaItems;

            // Cargar datos para PersonaRol (llave compuesta) - SOLO PROPIETARIOS
            var personasRol = await _context.PersonaRols
                .Include(pr => pr.CodigoPersonaNavigation)
                .Include(pr => pr.CodigoTipoRolNavigation)
                .Where(pr => pr.CodigoTipoRol == 1) // Solo propietarios
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

