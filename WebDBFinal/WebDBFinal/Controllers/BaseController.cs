using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public abstract class BaseController<T> : Controller where T : class, new()
{
    protected readonly ResidencialesDbContext _context;
    protected readonly ForeignKeyService _foreignKeyService;
    protected abstract string EntityName { get; }
    protected abstract string SpCreate { get; }
    protected abstract string SpUpdate { get; }
    protected abstract string SpDelete { get; }

    public BaseController(ResidencialesDbContext context, ForeignKeyService  foreignKeyService)
    {
        _context = context;
        _foreignKeyService = foreignKeyService;
    }

    // GET: Index - Lista todas las entidades
    public virtual async Task<IActionResult> Index()
    {
        var entities = await _context.Set<T>().ToListAsync();
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // GET: Create
    public virtual async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new T());
    }

    // POST: Create
    [HttpPost]
    public virtual async Task<IActionResult> Create(T entity)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var parameters = GetParametersForStoredProcedure(entity, false);
                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
            }
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // GET: Edit
    public virtual async Task<IActionResult> Edit(string id)
    {
        try
        {
            // Convertir el string de IDs en un array de objetos
            var keyValues = id.Split(',').Select(k => (object)int.Parse(k.Trim())).ToArray();
            var entity = await _context.Set<T>().FindAsync(keyValues);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetForeignKeyDataAsync();
            return View("~/Views/Shared/GenericEdit.cshtml", entity);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cargar {EntityName}: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Edit
    [HttpPost]
    public virtual async Task<IActionResult> Edit(T entity)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var parameters = GetParametersForStoredProcedure(entity, true);
                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
            }
        }
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.KeyProperties = GetKeyProperties();
        ViewBag.ForeignKeyData = await GetForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // POST: Delete
    [HttpPost]
    public virtual async Task<IActionResult> Delete(string keys)
    {
        try
        {
            var keyValues = keys.Split(',').Select(k => (object)int.Parse(k)).ToArray();
            await _context.Database.ExecuteSqlRawAsync(
                SpDelete, 
                keyValues);
            
            TempData["SuccessMessage"] = $"{EntityName} eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    // Métodos auxiliares
    protected List<PropertyInfo> GetDisplayProperties()
    {
        return typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<KeyAttribute>() != null || 
                       (!p.PropertyType.IsGenericType && 
                        !p.PropertyType.IsClass || 
                        p.PropertyType == typeof(string)))
            .Where(p => p.GetCustomAttribute<InversePropertyAttribute>() == null)
            .ToList();
    }

    protected List<PropertyInfo> GetEditableProperties()
    {
        return typeof(T).GetProperties()
            .Where(p => !p.PropertyType.IsGenericType && 
                       (p.PropertyType.IsPrimitive || 
                        p.PropertyType == typeof(string) || 
                        p.PropertyType == typeof(decimal) || 
                        p.PropertyType == typeof(DateTime) ||
                        p.PropertyType == typeof(bool)))
            .Where(p => p.GetCustomAttribute<InversePropertyAttribute>() == null)
            .ToList();
    }

    protected List<PropertyInfo> GetKeyProperties()
    {
        return typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<KeyAttribute>() != null)
            .ToList();
    }

    protected object[] GetParametersForStoredProcedure(T entity, bool includeKeys)
    {
        var properties = GetEditableProperties();
        var parameters = new List<object>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);
            parameters.Add(value ?? DBNull.Value);
        }

        return parameters.ToArray();
    }

    protected string GetKeyValuesAsString(T entity)
    {
        var keyProps = GetKeyProperties();
        return string.Join(",", keyProps.Select(p => p.GetValue(entity)?.ToString() ?? ""));
    }

    protected async Task<Dictionary<string, List<DropdownItem>>> GetForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();
        var properties = GetEditableProperties();
        var processedEntities = new HashSet<string>();

        foreach (var property in properties)
        {
            var fkInfo = _foreignKeyService.GetForeignKeyInfo(property, typeof(T));
            
            if (fkInfo != null)
            {
                var entityKey = fkInfo.RelatedEntityType.FullName ?? fkInfo.RelatedEntityType.Name;
                
                // Si es una FK compuesta, solo cargar los datos una vez para todas las partes
                if (fkInfo.IsComposite)
                {
                    if (!processedEntities.Contains(entityKey))
                    {
                        var items = await _foreignKeyService.GetRelatedRecordsAsync(fkInfo.RelatedEntityType);
                        
                        // Asignar los mismos datos a todas las partes de la FK compuesta
                        foreach (var keyPart in fkInfo.CompositeKeyParts)
                        {
                            foreignKeyData[keyPart] = items;
                        }
                        
                        processedEntities.Add(entityKey);
                    }
                }
                else
                {
                    // FK simple
                    if (!foreignKeyData.ContainsKey(property.Name))
                    {
                        var items = await _foreignKeyService.GetRelatedRecordsAsync(fkInfo.RelatedEntityType);
                        foreignKeyData[property.Name] = items;
                    }
                }
            }
        }

        return foreignKeyData;
    }
}
