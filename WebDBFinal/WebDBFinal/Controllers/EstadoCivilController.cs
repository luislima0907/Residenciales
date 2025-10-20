using Microsoft.AspNetCore.Mvc;
using WebDBFinal.Context;

namespace WebDBFinal.Controllers;

public class EstadoCivilController : Controller
{
    private readonly ResidencialesDbContext _context;
    
    public EstadoCivilController(ResidencialesDbContext context)
    {
        _context = context;
    }
    
    

}