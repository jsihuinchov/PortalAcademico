using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // ← ¡IMPORTANTE! Agregar soporte para Roles
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ↓↓↓↓ VERIFICACIÓN DE DATOS - TEMPORAL ↓↓↓↓
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var cursos = db.Cursos.ToList();
    
    Console.WriteLine("\n\n=== VERIFICACIÓN DE CURSOS ===");
    Console.WriteLine($"Se encontraron {cursos.Count} cursos:");
    foreach (var curso in cursos)
    {
        Console.WriteLine($"  {curso.Codigo}: {curso.Nombre} - {curso.Creditos} créditos");
    }
    Console.WriteLine("===============================\n\n");

    // Crear rol Coordinador
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
        Console.WriteLine("✅ Rol 'Coordinador' creado");
    }
    
    // Crear usuario coordinador
    var coordinadorEmail = "coordinador@universidad.edu";
    var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
    if (coordinador == null)
    {
        coordinador = new IdentityUser { UserName = coordinadorEmail, Email = coordinadorEmail, EmailConfirmed = true };
        await userManager.CreateAsync(coordinador, "Coordinador123!");
        await userManager.AddToRoleAsync(coordinador, "Coordinador");
        Console.WriteLine("✅ Usuario coordinador creado: coordinador@universidad.edu / Coordinador123!");
    }
}
// ↑↑↑↑ VERIFICACIÓN TEMPORAL ↑↑↑↑

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
app.UseStaticFiles();  // ← Esto es UseStaticFiles(), no MapStaticAssets()
app.UseRouting();

app.UseAuthentication();  // ← ¡IMPORTANTE! UseAuthentication ANTES de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();