using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Controllers;

public class SucursalController : BaseController<Sucursal>
{
    protected override string EntityName => "Sucursal";
    protected override string SpCreate => "EXEC InsertarSucursal @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarSucursal @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarSucursal @p0";
    
    public SucursalController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<Sucursal>()
            .Include(s => s.CodigoEmpresaNavigation)
            .Include(s => s.CodigoResidencialNavigation)
            .Include(s => s.Clusters)
            .Include(s => s.DireccionSucursals)
            .Include(s => s.TelefonoSucursals)
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
        ViewBag.ForeignKeyData = await GetSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Sucursal());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            var codigoSucursal = int.Parse(id);

            var entity = await _context.Set<Sucursal>()
                .Include(s => s.CodigoEmpresaNavigation)
                .Include(s => s.CodigoResidencialNavigation)
                .FirstOrDefaultAsync(s => s.CodigoSucursal == codigoSucursal);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetSucursalForeignKeyDataAsync();
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(Sucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoEmpresaNavigation");
        ModelState.Remove("CodigoResidencialNavigation");
        ModelState.Remove("Clusters");
        ModelState.Remove("DireccionSucursals");
        ModelState.Remove("TelefonoSucursals");
        ModelState.Remove("PeriodoFacturacionSeguridads");
        
        // Validación personalizada: debe pertenecer a Empresa O Residencial (no ambos, no ninguno)
        if (!entity.CodigoEmpresa.HasValue && !entity.CodigoResidencial.HasValue)
        {
            ModelState.AddModelError("", "La sucursal debe pertenecer a una Empresa de Seguridad o a un Residencial.");
        }
        
        if (entity.CodigoEmpresa.HasValue && entity.CodigoResidencial.HasValue)
        {
            ModelState.AddModelError("", "La sucursal no puede pertenecer simultáneamente a una Empresa y a un Residencial.");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarSucursal: @CodigoSucursal, @CodigoEmpresa, @CodigoResidencial, @Descripcion
                // Para valores NULL, pasamos el objeto nullable directamente, no DBNull.Value
                var parameters = new object[]
                {
                    entity.CodigoSucursal,
                    (object?)entity.CodigoEmpresa ?? null!,
                    (object?)entity.CodigoResidencial ?? null!,
                    entity.Descripcion
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear Sucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Sucursal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoEmpresaNavigation");
        ModelState.Remove("CodigoResidencialNavigation");
        ModelState.Remove("Clusters");
        ModelState.Remove("DireccionSucursals");
        ModelState.Remove("TelefonoSucursals");
        ModelState.Remove("PeriodoFacturacionSeguridads");
        
        // Validación personalizada: debe pertenecer a Empresa O Residencial (no ambos, no ninguno)
        if (!entity.CodigoEmpresa.HasValue && !entity.CodigoResidencial.HasValue)
        {
            ModelState.AddModelError("", "La sucursal debe pertenecer a una Empresa de Seguridad o a un Residencial.");
        }
        
        if (entity.CodigoEmpresa.HasValue && entity.CodigoResidencial.HasValue)
        {
            ModelState.AddModelError("", "La sucursal no puede pertenecer simultáneamente a una Empresa y a un Residencial.");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarSucursal: @CodigoSucursal, @CodigoEmpresa, @CodigoResidencial, @Descripcion
                // Para valores NULL, pasamos el objeto nullable directamente, no DBNull.Value
                var parameters = new object[]
                {
                    entity.CodigoSucursal,
                    (object?)entity.CodigoEmpresa ?? null!,
                    (object?)entity.CodigoResidencial ?? null!,
                    entity.Descripcion
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar Sucursal: {ex}");
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
        ViewBag.ForeignKeyData = await GetSucursalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de Sucursal
    private async Task<Dictionary<string, List<DropdownItem>>> GetSucursalForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de EmpresaSeguridad con opción "Ninguno"
            var empresas = new List<DropdownItem>
            {
                new DropdownItem { Value = "", Text = "-- Ninguna Empresa (usar Residencial) --" }
            };
            
            var empresasDB = await _context.EmpresaSeguridads
                .OrderBy(e => e.RazonSocial)
                .Select(e => new DropdownItem
                {
                    Value = e.CodigoEmpresa.ToString(),
                    Text = $"{e.CodigoEmpresa} - {e.RazonSocial}"
                })
                .ToListAsync();
            
            empresas.AddRange(empresasDB);
            foreignKeyData["CodigoEmpresa"] = empresas;
            
            // Cargar datos de Residencial con opción "Ninguno"
            var residenciales = new List<DropdownItem>
            {
                new DropdownItem { Value = "", Text = "-- Ningún Residencial (usar Empresa) --" }
            };
            
            var residencialesDB = await _context.Residencials
                .OrderBy(r => r.Nombre)
                .Select(r => new DropdownItem
                {
                    Value = r.CodigoResidencial.ToString(),
                    Text = $"{r.CodigoResidencial} - {r.Nombre}"
                })
                .ToListAsync();
            
            residenciales.AddRange(residencialesDB);
            foreignKeyData["CodigoResidencial"] = residenciales;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos de llaves foráneas: {ex.Message}");
        }
        
        return foreignKeyData;
    }
}
