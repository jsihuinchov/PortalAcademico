using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ¡ESTAS LÍNEAS SON CRÍTICAS!
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configurar restricciones
            builder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();
                
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin");
                
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Creditos", "Creditos > 0");
            
            // Un usuario no puede estar matriculado más de una vez en el mismo curso
            builder.Entity<Matricula>()
                .HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            // Datos semilla
            builder.Entity<Curso>().HasData(
                new Curso { 
                    Id = 1, 
                    Codigo = "MAT101", 
                    Nombre = "Matemáticas Básicas", 
                    Creditos = 4, 
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso { 
                    Id = 2, 
                    Codigo = "PROG201", 
                    Nombre = "Programación I", 
                    Creditos = 5, 
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 0, 0),
                    HorarioFin = new TimeSpan(12, 0, 0),
                    Activo = true
                },
                new Curso { 
                    Id = 3, 
                    Codigo = "BD301", 
                    Nombre = "Bases de Datos", 
                    Creditos = 4, 
                    CupoMaximo = 20,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            );
        }
    }
}