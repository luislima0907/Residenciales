using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class PersonaRolController : BaseController<PersonaRol>
{
    protected override string EntityName => "PersonaRol";
    protected override string SpCreate => "EXEC InsertarPersonaRol @p0, @p1, @p2, @p3, @p4, @p5, @p6";
    protected override string SpUpdate => "EXEC sp_ActualizarPersonaRol @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpDelete => "EXEC sp_EliminarPersonaRol @p0, @p1, @p2";
    
    public PersonaRolController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<PersonaRol>()
            .Include(pr => pr.CodigoPersonaNavigation)
            .Include(pr => pr.CodigoTipoRolNavigation)
            .Include(pr => pr.Casa)
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
        ViewBag.ForeignKeyData = await GetPersonaRolForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new PersonaRol());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // El id viene como "CodigoPersonaRol,CodigoPersona,CodigoTipoRol" (separado por comas desde GenericIndex)
            var parts = id.Split(',');
            if (parts.Length != 3)
            {
                TempData["ErrorMessage"] = $"ID inválido para PersonaRol. Se esperaban 3 valores pero se recibieron {parts.Length}. ID recibido: '{id}'";
                return RedirectToAction(nameof(Index));
            }

            var codigoPersonaRol = int.Parse(parts[0].Trim());
            var codigoPersona = int.Parse(parts[1].Trim());
            var codigoTipoRol = byte.Parse(parts[2].Trim());

            var entity = await _context.Set<PersonaRol>()
                .Include(pr => pr.CodigoPersonaNavigation)
                .Include(pr => pr.CodigoTipoRolNavigation)
                .Include(pr => pr.Casa)
                .FirstOrDefaultAsync(pr => pr.CodigoPersonaRol == codigoPersonaRol 
                                         && pr.CodigoPersona == codigoPersona 
                                         && pr.CodigoTipoRol == codigoTipoRol);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado con los códigos: PersonaRol={codigoPersonaRol}, Persona={codigoPersona}, TipoRol={codigoTipoRol}";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetPersonaRolForeignKeyDataAsync();

            // Agregar información para preseleccionar el valor de Casa en el dropdown
            // Si tiene casa asignada, construir el valor compuesto
            if (entity.NumeroCasa.HasValue && entity.CodigoCluster.HasValue && 
                entity.CodigoSucursal.HasValue && entity.CodigoSector.HasValue)
            {
                ViewBag.CurrentCasaValue = $"{entity.NumeroCasa},{entity.CodigoCluster},{entity.CodigoSucursal},{entity.CodigoSector}";
            }
            else
            {
                ViewBag.CurrentCasaValue = ""; // Sin casa asignada
            }
            
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}. ID recibido: '{id}'";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(PersonaRol entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoRolNavigation");
        ModelState.Remove("Casa");
        ModelState.Remove("ControlGaritaSeguridads");
        ModelState.Remove("DetalleCensos");
        ModelState.Remove("DocumentoFiscals");
        ModelState.Remove("EstadoCuenta");
        ModelState.Remove("IntegranteJunta");
        ModelState.Remove("MarcajeLaborals");
        ModelState.Remove("RegistroMovimientoResidencials");
        ModelState.Remove("RegistroPersonaNoGrata");
        ModelState.Remove("RegistroVehiculoResidentes");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_InsertarPersonaRol: @CodigoPersonaRol, @CodigoPersona, @CodigoTipoRol, @NumeroCasa, @CodigoCluster, @CodigoSucursal, @CodigoSector
                var sqlParams = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.CodigoPersonaRol),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CodigoPersona),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.CodigoTipoRol),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", (object?)entity.NumeroCasa ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", (object?)entity.CodigoCluster ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", (object?)entity.CodigoSucursal ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", (object?)entity.CodigoSector ?? DBNull.Value)
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, sqlParams);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear PersonaRol: {ex}");
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
        ViewBag.ForeignKeyData = await GetPersonaRolForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new PersonaRol());
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit([FromForm] PersonaRol entity)
    {
        // Convertir cadenas vacías a null para campos nullable de Casa
        // Esto es necesario porque HTML envía "" cuando un input está vacío
        if (Request.Form.ContainsKey("NumeroCasa"))
        {
            var numeroCasaValue = Request.Form["NumeroCasa"].ToString();
            entity.NumeroCasa = string.IsNullOrWhiteSpace(numeroCasaValue) ? null : int.Parse(numeroCasaValue);
        }
        
        if (Request.Form.ContainsKey("CodigoCluster"))
        {
            var codigoClusterValue = Request.Form["CodigoCluster"].ToString();
            entity.CodigoCluster = string.IsNullOrWhiteSpace(codigoClusterValue) ? null : int.Parse(codigoClusterValue);
        }
        
        if (Request.Form.ContainsKey("CodigoSucursal"))
        {
            var codigoSucursalValue = Request.Form["CodigoSucursal"].ToString();
            entity.CodigoSucursal = string.IsNullOrWhiteSpace(codigoSucursalValue) ? null : int.Parse(codigoSucursalValue);
        }
        
        if (Request.Form.ContainsKey("CodigoSector"))
        {
            var codigoSectorValue = Request.Form["CodigoSector"].ToString();
            entity.CodigoSector = string.IsNullOrWhiteSpace(codigoSectorValue) ? null : int.Parse(codigoSectorValue);
        }
        
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoPersonaNavigation");
        ModelState.Remove("CodigoTipoRolNavigation");
        ModelState.Remove("Casa");
        ModelState.Remove("ControlGaritaSeguridads");
        ModelState.Remove("DetalleCensos");
        ModelState.Remove("DocumentoFiscals");
        ModelState.Remove("EstadoCuenta");
        ModelState.Remove("IntegranteJunta");
        ModelState.Remove("MarcajeLaborals");
        ModelState.Remove("RegistroMovimientoResidencials");
        ModelState.Remove("RegistroPersonaNoGrata");
        ModelState.Remove("RegistroVehiculoResidentes");
        
        // Si NumeroCasa es 0 o no válido, asegurar que todos los campos de Casa sean null
        if (entity.NumeroCasa == null || entity.NumeroCasa == 0)
        {
            entity.NumeroCasa = null;
            entity.CodigoCluster = null;
            entity.CodigoSucursal = null;
            entity.CodigoSector = null;
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarPersonaRol: @CodigoPersonaRol, @CodigoPersona, @CodigoTipoRol, @NumeroCasa, @CodigoCluster, @CodigoSucursal, @CodigoSector
                var sqlParams = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.CodigoPersonaRol),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CodigoPersona),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.CodigoTipoRol),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.CodigoTipoRol), // NuevoCodigoTipoRol (mismo valor por defecto)
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", (object?)entity.NumeroCasa ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", (object?)entity.CodigoCluster ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", (object?)entity.CodigoSucursal ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p7", (object?)entity.CodigoSector ?? DBNull.Value)
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, sqlParams);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar PersonaRol: {ex}");
                Console.WriteLine($"Valores recibidos - NumeroCasa: {entity.NumeroCasa}, CodigoCluster: {entity.CodigoCluster}, CodigoSucursal: {entity.CodigoSucursal}, CodigoSector: {entity.CodigoSector}");
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
        ViewBag.ForeignKeyData = await GetPersonaRolForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete - Override para manejar la llave compuesta
    [HttpPost]
    public override async Task<IActionResult> Delete(string keys)
    {
        try
        {
            // El parámetro viene como "keys" desde el formulario GenericIndex
            // El valor viene como "CodigoPersonaRol,CodigoPersona,CodigoTipoRol" (separado por comas)
            var parts = keys.Split(',');
            if (parts.Length != 3)
            {
                TempData["ErrorMessage"] = $"ID inválido para PersonaRol. Se esperaban 3 valores pero se recibieron {parts.Length}";
                return RedirectToAction(nameof(Index));
            }

            var codigoPersonaRol = int.Parse(parts[0].Trim());
            var codigoPersona = int.Parse(parts[1].Trim());
            var codigoTipoRol = byte.Parse(parts[2].Trim());

            var sqlParams = new[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("@p0", codigoPersonaRol),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", codigoPersona),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", codigoTipoRol)
            };

            await _context.Database.ExecuteSqlRawAsync(SpDelete, sqlParams);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.InnerException?.Message ?? ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Método específico para cargar datos de llaves foráneas de PersonaRol
    private async Task<Dictionary<string, List<DropdownItem>>> GetPersonaRolForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para Persona
            var personas = await _context.Personas.ToListAsync();
            var personaItems = personas.Select(p => new DropdownItem
            {
                Value = p.CodigoPersona.ToString(),
                Text = $"{p.CodigoPersona} - {p.PrimerNombre} {p.PrimerApellido}"
            }).ToList();
            foreignKeyData["CodigoPersona"] = personaItems;

            // Cargar datos para TipoRol
            var tiposRol = await _context.TipoRols.ToListAsync();
            var tipoRolItems = tiposRol.Select(tr => new DropdownItem
            {
                Value = tr.CodigoTipoRol.ToString(),
                Text = $"{tr.Descripcion}"
            }).ToList();
            foreignKeyData["CodigoTipoRol"] = tipoRolItems;

            // Cargar datos para Casa (FK compuesta)
            var casas = await _context.Casas
                .Include(c => c.Cluster)
                    .ThenInclude(cl => cl.CodigoSucursalNavigation)
                .Include(c => c.Cluster)
                    .ThenInclude(cl => cl.CodigoSectorNavigation)
                .ToListAsync();

            var casaItems = casas.Select(c => new DropdownItem
            {
                Value = $"{c.NumeroCasa},{c.CodigoCluster},{c.CodigoSucursal},{c.CodigoSector}",
                Text = $"Casa {c.NumeroCasa} - Cluster {c.CodigoCluster} - {c.Cluster.CodigoSucursalNavigation.Descripcion} - {c.Cluster.CodigoSectorNavigation.Nombre}"
            }).ToList();

            // Agregar opción vacía para Casa (es opcional)
            casaItems.Insert(0, new DropdownItem { Value = "", Text = "-- Sin Casa Asignada --" });

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

    protected List<PropertyInfo> GetEditableProperties()
    {
        var properties = typeof(PersonaRol).GetProperties()
            .Where(p => {
                var propType = p.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propType);
                
                // Si es nullable, obtener el tipo base
                var checkType = underlyingType ?? propType;
                
                return (checkType.IsPrimitive || 
                        checkType == typeof(string) || 
                        checkType == typeof(decimal) || 
                        checkType == typeof(DateTime) ||
                        checkType == typeof(DateOnly) ||
                        checkType == typeof(bool));
            })
            .Where(p => p.GetCustomAttribute<InversePropertyAttribute>() == null)
            .ToList();
        
        return properties;
    }

    protected List<PropertyInfo> GetDisplayProperties()
    {
        var properties = typeof(PersonaRol).GetProperties()
            .Where(p => {
                // Incluir propiedades clave
                if (p.GetCustomAttribute<KeyAttribute>() != null)
                    return true;
                
                // Incluir propiedades simples y nullable
                var propType = p.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propType);
                var checkType = underlyingType ?? propType;
                
                if (checkType.IsPrimitive || checkType == typeof(string) || 
                    checkType == typeof(decimal) || checkType == typeof(DateTime) || 
                    checkType == typeof(DateOnly))
                    return true;
                
                // Incluir propiedades de navegación específicas
                if (p.Name == "CodigoPersonaNavigation" || 
                    p.Name == "CodigoTipoRolNavigation" || 
                    p.Name == "Casa")
                    return true;
                
                return false;
            })
            .Where(p => p.GetCustomAttribute<InversePropertyAttribute>() == null)
            .ToList();
        
        return properties;
    }

    // Override del método GetKeyValue para manejar la llave compuesta
    // protected override string GetKeyValue(PersonaRol entity)
    // {
    //     return $"{entity.CodigoPersonaRol}-{entity.CodigoPersona}-{entity.CodigoTipoRol}";
    // }
}
