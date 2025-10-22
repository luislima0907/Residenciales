using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WebDBFinal.Controllers;

public class MunicipioController : BaseController<Municipio>
{
    protected override string EntityName => "Municipio";
    protected override string SpCreate => "EXEC InsertarMunicipio @p0, @p1, @p2";
    protected override string SpUpdate => "EXEC sp_ActualizarMunicipio @p0, @p1, @p2";
    protected override string SpDelete => "EXEC sp_EliminarMunicipio @p0, @p1";
    
    public MunicipioController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<Municipio>()
            .Include(m => m.CodigoDepartamentoNavigation)
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
        ViewBag.ForeignKeyData = await GetMunicipioForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new Municipio());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // El id viene como "CodigoMunicipio,CodigoDepartamento" (separado por comas desde GenericIndex)
            var parts = id.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = $"ID inválido para Municipio. Se esperaban 2 valores pero se recibieron {parts.Length}. ID recibido: '{id}'";
                return RedirectToAction(nameof(Index));
            }

            var codigoMunicipio = short.Parse(parts[0].Trim());
            var codigoDepartamento = short.Parse(parts[1].Trim());

            var entity = await _context.Set<Municipio>()
                .Include(m => m.CodigoDepartamentoNavigation)
                .FirstOrDefaultAsync(m => m.CodigoMunicipio == codigoMunicipio && m.CodigoDepartamento == codigoDepartamento);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado con los códigos: Municipio={codigoMunicipio}, Departamento={codigoDepartamento}";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetMunicipioForeignKeyDataAsync();
            
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
    public override async Task<IActionResult> Create(Municipio entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoDepartamentoNavigation");
        ModelState.Remove("DireccionSucursals");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarMunicipio: @CodigoMunicipio, @CodigoDepartamento, @Descripcion
                var parameters = new object[]
                {
                    entity.CodigoMunicipio,
                    entity.CodigoDepartamento,
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
                
                Console.WriteLine($"Error al crear Municipio: {ex}");
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
        ViewBag.ForeignKeyData = await GetMunicipioForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(Municipio entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoDepartamentoNavigation");
        ModelState.Remove("DireccionSucursals");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Tu procedimiento sp_ActualizarMunicipio solo acepta 3 parámetros:
                // @CodigoMunicipio, @CodigoDepartamento, @Descripcion
                // Los valores de CodigoMunicipio y CodigoDepartamento NO se pueden modificar (son la llave primaria)
                // Solo se actualiza la Descripcion
                var parameters = new object[]
                {
                    entity.CodigoMunicipio,       // Código de municipio (identifica el registro, no se modifica)
                    entity.CodigoDepartamento,    // Código de departamento (identifica el registro, no se modifica)
                    entity.Descripcion            // Nuevo valor de descripción
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar Municipio: {ex}");
                Console.WriteLine($"Parámetros: CodigoMunicipio={entity.CodigoMunicipio}, CodigoDepartamento={entity.CodigoDepartamento}, Descripcion={entity.Descripcion}");
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
        ViewBag.ForeignKeyData = await GetMunicipioForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete - Override para manejar la llave compuesta
    [HttpPost]
    public override async Task<IActionResult> Delete(string keys)
    {
        try
        {
            // El keys viene como "CodigoMunicipio,CodigoDepartamento"
            var parts = keys.Split(',');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = "Identificador inválido para eliminar";
                return RedirectToAction(nameof(Index));
            }

            var codigoMunicipio = short.Parse(parts[0].Trim());
            var codigoDepartamento = short.Parse(parts[1].Trim());
            
            var parameters = new object[] { codigoMunicipio, codigoDepartamento };
            await _context.Database.ExecuteSqlRawAsync(SpDelete, parameters);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            Console.WriteLine($"Error al eliminar Municipio: {ex}");
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Método específico para cargar datos de llaves foráneas de Municipio
    private async Task<Dictionary<string, List<DropdownItem>>> GetMunicipioForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        
        try
        {
            // Cargar datos de Departamento
            var departamentos = await _context.Departamentos
                .OrderBy(d => d.Descripcion)
                .Select(d => new DropdownItem
                {
                    Value = d.CodigoDepartamento.ToString(),
                    Text = $"{d.CodigoDepartamento} - {d.Descripcion}"
                })
                .ToListAsync();
            
            foreignKeyData["CodigoDepartamento"] = departamentos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando datos de llaves foráneas: {ex.Message}");
        }

        return foreignKeyData;
    }

    protected new List<PropertyInfo> GetEditableProperties()
    {
        return typeof(Municipio).GetProperties()
            .Where(p => p.Name != "CodigoDepartamentoNavigation" &&
                       p.Name != "DireccionSucursals")
            .ToList();
    }

    protected new List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(Municipio).GetProperties()
            .Where(p => p.Name == "CodigoMunicipio" ||
                       p.Name == "CodigoDepartamento" ||
                       p.Name == "Descripcion" ||
                       p.Name == "CodigoDepartamentoNavigation")
            .ToList();
    }
}

