using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Security.Claims;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CoordinadorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Coordinador
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        // GET: Coordinador/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coordinador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // POST: Coordinador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Matriculas/5
        // GET: Coordinador/Matriculas/5
public async Task<IActionResult> Matriculas(int? id)
{
    if (id == null) return NotFound();

    var curso = await _context.Cursos
        .Include(c => c.Matriculas)
            .ThenInclude(m => m.Usuario)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (curso == null) return NotFound();

    // ↓↓↓↓ DEBUG TEMPORAL - VER QUÉ DATOS SE ENVÍAN A LA VISTA ↓↓↓↓
    Console.WriteLine($"\n=== DEBUG VISTA MATRÍCULAS ===");
    Console.WriteLine($"Curso: {curso.Nombre} (ID: {curso.Id})");
    Console.WriteLine($"Matrículas count: {curso.Matriculas?.Count ?? 0}");
    
    if (curso.Matriculas != null)
    {
        foreach (var matricula in curso.Matriculas)
        {
            Console.WriteLine($"  - Usuario: {matricula.Usuario?.Email}, Estado: {matricula.Estado}");
        }
    }
    Console.WriteLine($"==============================\n");
    // ↑↑↑↑ DEBUG TEMPORAL ↑↑↑↑

    return View(curso);
}

        // POST: Coordinador/ConfirmarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = EstadoMatricula.Confirmada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            return RedirectToAction("Matriculas", new { id = matricula.CursoId });
        }

        // POST: Coordinador/CancelarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = EstadoMatricula.Cancelada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            return RedirectToAction("Matriculas", new { id = matricula.CursoId });
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
    }
}