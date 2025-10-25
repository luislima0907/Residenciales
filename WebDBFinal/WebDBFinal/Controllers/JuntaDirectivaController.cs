using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class JuntaDirectivaController : BaseController<JuntaDirectiva>
{
    protected override string EntityName => "JuntaDirectiva";
    protected override string SpCreate => "EXEC InsertarJuntaDirectiva @p0, @p1, @p2, @p3, @p4, @p5, @p6";
    protected override string SpUpdate => "EXEC sp_ActualizarJuntaDirectiva @p0, @p1, @p2, @p3, @p4, @p5, @p6";
    protected override string SpDelete => "EXEC sp_EliminarJuntaDirectiva @p0, @p1, @p2, @p3";
    
    public JuntaDirectivaController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new JuntaDirectiva());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // La llave compuesta viene como: "CodigoJunta,CodigoCluster,CodigoSucursal,CodigoSector"
            var parts = id.Split(',');
            if (parts.Length != 4)
            {
                TempData["ErrorMessage"] = "ID de junta directiva inválido";
                return RedirectToAction(nameof(Index));
            }

            var codigoJunta = int.Parse(parts[0]);
            var codigoCluster = int.Parse(parts[1]);
            var codigoSucursal = int.Parse(parts[2]);
            var codigoSector = int.Parse(parts[3]);

            var entity = await _context.Set<JuntaDirectiva>()
                .Include(j => j.Cluster)
                    .ThenInclude(c => c.CodigoSucursalNavigation)
                .Include(j => j.Cluster)
                    .ThenInclude(c => c.CodigoSectorNavigation)
                .FirstOrDefaultAsync(j => j.CodigoJunta == codigoJunta 
                                       && j.CodigoCluster == codigoCluster
                                       && j.CodigoSucursal == codigoSucursal
                                       && j.CodigoSector == codigoSector);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrada";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
            
            // Preseleccionar valor de Cluster (llave compuesta)
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
        var entities = await _context.Set<JuntaDirectiva>()
            .Include(j => j.Cluster)
                .ThenInclude(c => c.CodigoSucursalNavigation)
            .Include(j => j.Cluster)
                .ThenInclude(c => c.CodigoSectorNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(JuntaDirectiva entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("Cluster");
        ModelState.Remove("IntegranteJunta");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la descripción no esté vacía
                if (string.IsNullOrWhiteSpace(entity.Descripcion))
                {
                    ModelState.AddModelError("Descripcion", "La descripción es requerida");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que la fecha fin sea posterior a la fecha inicio
                if (entity.FechaFin <= entity.FechaInicio)
                {
                    ModelState.AddModelError("FechaFin", "La fecha fin debe ser posterior a la fecha de inicio");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoJunta,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.FechaInicio,
                    entity.FechaFin,
                    entity.Descripcion
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear JuntaDirectiva: {ex}");
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
        ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(JuntaDirectiva entity)
    {
        ModelState.Remove("Cluster");
        ModelState.Remove("IntegranteJunta");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la descripción no esté vacía
                if (string.IsNullOrWhiteSpace(entity.Descripcion))
                {
                    ModelState.AddModelError("Descripcion", "La descripción es requerida");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que la fecha fin sea posterior a la fecha inicio
                if (entity.FechaFin <= entity.FechaInicio)
                {
                    ModelState.AddModelError("FechaFin", "La fecha fin debe ser posterior a la fecha de inicio");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoJunta,
                    entity.CodigoCluster,
                    entity.CodigoSucursal,
                    entity.CodigoSector,
                    entity.FechaInicio,
                    entity.FechaFin,
                    entity.Descripcion
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar JuntaDirectiva: {ex}");
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
        ViewBag.ForeignKeyData = await GetJuntaDirectivaForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de JuntaDirectiva
    private async Task<Dictionary<string, List<DropdownItem>>> GetJuntaDirectivaForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Cluster (llave compuesta)
            var clusters = await _context.Clusters
                .Include(c => c.CodigoSucursalNavigation)
                .Include(c => c.CodigoSectorNavigation)
                .ToListAsync();
            
            var clusterItems = clusters.Select(c => new DropdownItem
            {
                Value = $"{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Cluster: {c.CodigoCluster} - Sucursal: {c.CodigoSucursalNavigation.Descripcion} - Sector: {c.CodigoSectorNavigation.Nombre}"
            }).ToList();
            
            // Asignar los mismos datos a todas las partes de la llave compuesta Cluster
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

