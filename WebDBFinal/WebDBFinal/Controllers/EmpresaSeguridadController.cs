using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Controllers;

public class EmpresaSeguridadController : BaseController<EmpresaSeguridad>
{
    protected override string EntityName => "EmpresaSeguridad";
    protected override string SpCreate => "EXEC InsertarEmpresaSeguridad @p0, @p1, @p2, @p3";
    protected override string SpUpdate => "EXEC sp_ActualizarEmpresaSeguridad @p0, @p1, @p2, @p3";
    protected override string SpDelete => "EXEC sp_EliminarEmpresaSeguridad @p0";
    
    public EmpresaSeguridadController(ResidencialesDbContext context, ForeignKeyService foreignKeyService) 
        : base(context, foreignKeyService) 
    { 
    }

    // GET: Index - Override para incluir las relaciones de navegación
    public override async Task<IActionResult> Index()
    {
        var entities = await _context.Set<EmpresaSeguridad>()
            .Include(e => e.Sucursals)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(EmpresaSeguridad entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("Sucursals");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // InsertarEmpresaSeguridad: @CodigoEmpresa, @RazonSocial, @NIT, @ContactoPrincipal
                var parameters = new object[]
                {
                    entity.CodigoEmpresa,
                    entity.RazonSocial,
                    entity.NIT ?? (object)DBNull.Value,
                    entity.ContactoPrincipal
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear EmpresaSeguridad: {ex}");
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
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(EmpresaSeguridad entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("Sucursals");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Crear parámetros en el orden correcto que espera el SP
                // sp_ActualizarEmpresaSeguridad: @CodigoEmpresa, @RazonSocial, @NIT, @ContactoPrincipal
                var parameters = new object[]
                {
                    entity.CodigoEmpresa,
                    entity.RazonSocial,
                    entity.NIT ?? (object)DBNull.Value,
                    entity.ContactoPrincipal
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar EmpresaSeguridad: {ex}");
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
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }
}

