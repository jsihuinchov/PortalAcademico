using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Security.Claims;

namespace PortalAcademico.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Matriculas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cursoId)
        {
            // 1. Validar que el usuario esté autenticado
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para inscribirte en un curso.";
                return RedirectToAction("Details", "Courses", new { id = cursoId });
            }

            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "El curso no existe o no está disponible.";
                return RedirectToAction("Index", "Courses");
            }

            // 2. Validar que no esté ya matriculado
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == userId);

            if (matriculaExistente != null)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Details", "Courses", new { id = cursoId });
            }

            // 3. Validar cupo máximo
            var matriculasActivas = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);

            if (matriculasActivas >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ha alcanzado su cupo máximo.";
                return RedirectToAction("Details", "Courses", new { id = cursoId });
            }

            // 4. Validar solapamiento de horarios
            var cursosMatriculados = await _context.Matriculas
                .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                .Include(m => m.Curso)
                .Select(m => m.Curso)
                .ToListAsync();

            var solapamiento = cursosMatriculados.Any(c => 
                (curso.HorarioInicio < c.HorarioFin && curso.HorarioFin > c.HorarioInicio));

            if (solapamiento)
            {
                TempData["Error"] = "El horario de este curso se solapa con otro curso en el que estás matriculado.";
                return RedirectToAction("Details", "Courses", new { id = cursoId });
            }

            // 5. Crear la matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Inscripción realizada con éxito! Estado: Pendiente";
            return RedirectToAction("Details", "Courses", new { id = cursoId });
        }
    }
}