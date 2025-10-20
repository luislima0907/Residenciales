using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Services;
using WebDBFinal.Models;

namespace WebDBFinal.Controllers;

public class ConsultasController : Controller
{
    private readonly StoredProcedureService _procedureService;

    public ConsultasController(StoredProcedureService procedureService)
    {
        _procedureService = procedureService;
    }

    // GET: Consultas/Index - Lista todas las consultas disponibles
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index()
    {
        var procedures = await _procedureService.GetStoredProceduresAsync();
        
        // Debug: verificar si hay duplicados
        var duplicates = procedures.GroupBy(p => p.Name)
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key)
                                   .ToList();
        
        if (duplicates.Any())
        {
            ViewBag.DebugMessage = $"DUPLICADOS DETECTADOS: {string.Join(", ", duplicates)}";
        }
        
        ViewBag.TotalProcedures = procedures.Count;
        ViewBag.UniqueProcedures = procedures.Select(p => p.Name).Distinct().Count();
        
        return View(procedures);
    }

    // GET: Consultas/Execute/{procedureName}
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Execute(string procedureName)
    {
        var procedures = await _procedureService.GetStoredProceduresAsync();
        var procedure = procedures.FirstOrDefault(p => p.Name == procedureName);

        if (procedure == null)
        {
            TempData["ErrorMessage"] = "Procedimiento no encontrado";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ProcedureName = procedure.Name;
        ViewBag.Description = procedure.Description;
        
        return View(procedure);
    }

    // POST: Consultas/ExecuteProcedure
    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ExecuteProcedure(ProcedureExecutionRequest request)
    {
        try
        {
            var result = await _procedureService.ExecuteProcedureAsync(request.ProcedureName, request.Parameters);
            
            ViewBag.ProcedureName = request.ProcedureName;
            ViewBag.Parameters = request.Parameters;
            
            return View("Results", result);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al ejecutar el procedimiento: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
