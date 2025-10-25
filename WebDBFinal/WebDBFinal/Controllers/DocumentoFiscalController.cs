using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Entities;
using WebDBFinal.Services;

namespace WebDBFinal.Controllers;

public class DocumentoFiscalController : BaseController<DocumentoFiscal>
{
    protected override string EntityName => "DocumentoFiscal";
    protected override string SpCreate => "EXEC InsertarDocumentoFiscal @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpUpdate => "EXEC sp_ActualizarDocumentoFiscal @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7";
    protected override string SpDelete => "EXEC sp_EliminarDocumentoFiscal @p0, @p1, @p2";
    
    public DocumentoFiscalController(ResidencialesDbContext context) : base(context, new ForeignKeyService(context)) { }

    // GET: Create - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Create()
    {
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetEditableProperties();
        ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", new DocumentoFiscal());
    }

    // GET: Edit - Override para manejar las llaves foráneas manualmente
    public override async Task<IActionResult> Edit(string id)
    {
        try
        {
            // La llave compuesta viene como: "CodigoTipoDocumento,Serie,Numero"
            var parts = id.Split(',');
            if (parts.Length != 3)
            {
                TempData["ErrorMessage"] = "ID de documento fiscal inválido";
                return RedirectToAction(nameof(Index));
            }

            var codigoTipoDocumento = byte.Parse(parts[0]);
            var serie = parts[1];
            var numero = int.Parse(parts[2]);

            var entity = await _context.Set<DocumentoFiscal>()
                .Include(d => d.CodigoTipoDocumentoNavigation)
                .Include(d => d.PersonaRol)
                    .ThenInclude(pr => pr.CodigoPersonaNavigation)
                .Include(d => d.PersonaRol)
                    .ThenInclude(pr => pr.CodigoTipoRolNavigation)
                .FirstOrDefaultAsync(d => d.CodigoTipoDocumento == codigoTipoDocumento 
                                       && d.Serie == serie 
                                       && d.Numero == numero);
            
            if (entity == null)
            {
                TempData["ErrorMessage"] = $"{EntityName} no encontrado";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EntityName = EntityName;
            ViewBag.Properties = GetEditableProperties();
            ViewBag.KeyProperties = GetKeyProperties();
            ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
            
            // Preseleccionar valor de PersonaRol (llave compuesta)
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
        var entities = await _context.Set<DocumentoFiscal>()
            .Include(d => d.CodigoTipoDocumentoNavigation)
            .Include(d => d.PersonaRol)
                .ThenInclude(pr => pr.CodigoPersonaNavigation)
            .ToListAsync();
        
        ViewBag.EntityName = EntityName;
        ViewBag.Properties = GetDisplayProperties();
        return View("~/Views/Shared/GenericIndex.cshtml", entities);
    }

    // POST: Create - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Create(DocumentoFiscal entity)
    {
        // Limpiar el estado del modelo para las propiedades de navegación
        ModelState.Remove("CodigoTipoDocumentoNavigation");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("AplicacionDocumentos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la serie no esté vacía
                if (string.IsNullOrWhiteSpace(entity.Serie))
                {
                    ModelState.AddModelError("Serie", "La serie del documento es requerida");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que el número sea positivo
                if (entity.Numero <= 0)
                {
                    ModelState.AddModelError("Numero", "El número del documento debe ser mayor a cero");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que el NIT no esté vacío
                if (string.IsNullOrWhiteSpace(entity.NIT))
                {
                    ModelState.AddModelError("NIT", "El NIT es requerido");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Validar que la fecha de emisión no sea futura
                if (entity.FechaEmision > DateTime.Now)
                {
                    ModelState.AddModelError("FechaEmision", "La fecha de emisión no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericCreate.cshtml", entity);
                }

                // Crear parámetros en el orden correcto que espera el SP
                var parameters = new object[]
                {
                    entity.CodigoTipoDocumento,
                    entity.Serie,
                    entity.Numero,
                    entity.FechaEmision,
                    entity.CodigoPersonaRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoRol,
                    entity.NIT
                };

                await _context.Database.ExecuteSqlRawAsync(SpCreate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al crear DocumentoFiscal: {ex}");
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
        ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericCreate.cshtml", entity);
    }

    // POST: Edit - Override para manejar correctamente los parámetros
    [HttpPost]
    public override async Task<IActionResult> Edit(DocumentoFiscal entity)
    {
        ModelState.Remove("CodigoTipoDocumentoNavigation");
        ModelState.Remove("PersonaRol");
        ModelState.Remove("AplicacionDocumentos");
        
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que la serie no esté vacía
                if (string.IsNullOrWhiteSpace(entity.Serie))
                {
                    ModelState.AddModelError("Serie", "La serie del documento es requerida");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que el número sea positivo
                if (entity.Numero <= 0)
                {
                    ModelState.AddModelError("Numero", "El número del documento debe ser mayor a cero");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que el NIT no esté vacío
                if (string.IsNullOrWhiteSpace(entity.NIT))
                {
                    ModelState.AddModelError("NIT", "El NIT es requerido");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                // Validar que la fecha de emisión no sea futura
                if (entity.FechaEmision > DateTime.Now)
                {
                    ModelState.AddModelError("FechaEmision", "La fecha de emisión no puede ser futura");
                    ViewBag.EntityName = EntityName;
                    ViewBag.Properties = GetEditableProperties();
                    ViewBag.KeyProperties = GetKeyProperties();
                    ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
                    return View("~/Views/Shared/GenericEdit.cshtml", entity);
                }

                var parameters = new object[]
                {
                    entity.CodigoTipoDocumento,
                    entity.Serie,
                    entity.Numero,
                    entity.FechaEmision,
                    entity.CodigoPersonaRol,
                    entity.CodigoPersona,
                    entity.CodigoTipoRol,
                    entity.NIT
                };

                await _context.Database.ExecuteSqlRawAsync(SpUpdate, parameters);
                
                TempData["SuccessMessage"] = $"{EntityName} actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                TempData["ErrorMessage"] = $"Error detallado: {ex.InnerException?.Message ?? ex.Message}";
                
                Console.WriteLine($"Error al actualizar DocumentoFiscal: {ex}");
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
        ViewBag.ForeignKeyData = await GetDocumentoFiscalForeignKeyDataAsync();
        return View("~/Views/Shared/GenericEdit.cshtml", entity);
    }

    // Método específico para cargar datos de llaves foráneas de DocumentoFiscal
    private async Task<Dictionary<string, List<DropdownItem>>> GetDocumentoFiscalForeignKeyDataAsync()
    {
        var foreignKeyData = new Dictionary<string, List<DropdownItem>>();

        try
        {
            // Cargar datos para TipoDocumentoFiscal
            var tiposDocumento = await _context.TipoDocumentoFiscals.ToListAsync();
            
            var tipoDocumentoItems = tiposDocumento.Select(td => new DropdownItem
            {
                Value = td.CodigoTipoDocumento.ToString(),
                Text = $"{td.Descripcion}"
            }).ToList();
            
            foreignKeyData["CodigoTipoDocumento"] = tipoDocumentoItems;

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

