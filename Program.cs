using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// ↓↓↓↓ INICIALIZACIÓN GARANTIZADA ↓↓↓↓
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    Console.WriteLine("\n\n=== INICIALIZANDO BASE DE DATOS ===");

    // 1. Asegurar que la base de datos esté creada
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Base de datos verificada");

    // 2. Crear rol Coordinador
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
        Console.WriteLine("✅ Rol 'Coordinador' creado");
    }

    // 3. Crear usuario coordinador
    var coordinadorEmail = "coordinador@universidad.edu";
    var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
    if (coordinador == null)
    {
        coordinador = new IdentityUser { UserName = coordinadorEmail, Email = coordinadorEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(coordinador, "Coordinador123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(coordinador, "Coordinador");
            Console.WriteLine("✅ Usuario coordinador creado: coordinador@universidad.edu / Coordinador123!");
        }
        else
        {
            Console.WriteLine("❌ Error creando coordinador: " + string.Join(", ", result.Errors));
        }
    }

    // 4. Crear usuario estudiante
    var estudianteEmail = "estudiante@universidad.edu";
    var estudiante = await userManager.FindByEmailAsync(estudianteEmail);
    if (estudiante == null)
    {
        estudiante = new IdentityUser { UserName = estudianteEmail, Email = estudianteEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(estudiante, "Estudiante123!");
        if (result.Succeeded)
        {
            Console.WriteLine("✅ Usuario estudiante creado: estudiante@universidad.edu / Estudiante123!");
        }
        else
        {
            Console.WriteLine("❌ Error creando estudiante: " + string.Join(", ", result.Errors));
        }
    }

    // 5. Verificar que hay cursos (deberían estar en los datos semilla)
    var cursos = await db.Cursos.ToListAsync();
    Console.WriteLine($"✅ Cursos en la base de datos: {cursos.Count}");

    // 6. CREAR MATRÍCULAS DE PRUEBA GARANTIZADAS
    if (estudiante != null && cursos.Count >= 2)
    {
        // Eliminar matrículas existentes primero
        var matriculasExistentes = await db.Matriculas.ToListAsync();
        if (matriculasExistentes.Any())
        {
            db.Matriculas.RemoveRange(matriculasExistentes);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Matrículas existentes eliminadas");
        }

        // Crear nuevas matrículas
        var nuevasMatriculas = new List<Matricula>
        {
            new Matricula { 
                CursoId = cursos[0].Id, 
                UsuarioId = estudiante.Id, 
                Estado = EstadoMatricula.Pendiente,
                FechaRegistro = DateTime.Now
            },
            new Matricula { 
                CursoId = cursos[1].Id, 
                UsuarioId = estudiante.Id, 
                Estado = EstadoMatricula.Confirmada,
                FechaRegistro = DateTime.Now.AddDays(-1)
            }
        };
        
        await db.Matriculas.AddRangeAsync(nuevasMatriculas);
        await db.SaveChangesAsync();
        Console.WriteLine("✅ MATRÍCULAS DE PRUEBA CREADAS EXITOSAMENTE");

        // Verificar que se crearon
        var matriculasVerificadas = await db.Matriculas
            .Include(m => m.Curso)
            .Include(m => m.Usuario)
            .ToListAsync();
            
        Console.WriteLine($"✅ Matrículas verificadas: {matriculasVerificadas.Count}");
        foreach (var matricula in matriculasVerificadas)
        {
            Console.WriteLine($"   - {matricula.Curso?.Nombre} -> {matricula.Usuario?.Email} ({matricula.Estado})");
        }
    }

    Console.WriteLine("========================================\n\n");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();