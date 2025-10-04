using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string searchNombre, int? creditosMin, int? creditosMax)
        {
            var cursos = _context.Cursos.Where(c => c.Activo);

            // Filtro por nombre
            if (!string.IsNullOrEmpty(searchNombre))
            {
                cursos = cursos.Where(c => c.Nombre.Contains(searchNombre) || c.Codigo.Contains(searchNombre));
            }

            // Filtro por rango de créditos
            if (creditosMin.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos >= creditosMin.Value);
            }

            if (creditosMax.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos <= creditosMax.Value);
            }

            // Pasar los filtros a la vista para mantenerlos
            ViewData["SearchNombre"] = searchNombre;
            ViewData["CreditosMin"] = creditosMin;
            ViewData["CreditosMax"] = creditosMax;

            return View(await cursos.ToListAsync());
        }

        // GET: Courses/Details/5
        // GET: Courses/Details/5
public async Task<IActionResult> Details(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var curso = await _context.Cursos
        .FirstOrDefaultAsync(m => m.Id == id && m.Activo);
        
    if (curso == null)
    {
        return NotFound();
    }

    // ↓↓↓↓ ESTE CÓDIGO DEBE ESTAR AQUÍ ↓↓↓↓
    HttpContext.Session.SetString("UltimoCursoVisitado", $"{curso.Codigo} - {curso.Nombre}");
    HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);
    // ↑↑↑↑ ESTE CÓDIGO DEBE ESTAR AQUÍ ↑↑↑↑

    return View(curso);
}
    }
}