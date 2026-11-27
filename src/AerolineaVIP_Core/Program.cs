using AerolineaVIP_Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AerolineaVIP_Core.Roles;
using AerolineaVIP_Core.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AerolineaVIP_Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//******************** MongoDB: datos de la aerolinea *********************************
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});
//*******************************************************************************

builder.Services.AddSingleton<AerolineaMongoContext>();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<AerolineaVIP_Core.Services.ServicioPermisos>();

//******************** Servicio de Autenticación *********************************

/*
    Se agrega el servicio de autenticación al proyecto.
    Esto permite que la aplicación pueda iniciar sesión usando
    proveedores externos, en este caso Microsoft.
*/
builder.Services.AddAuthentication()

    /*
        Se configura Microsoft como proveedor externo de autenticación.
        Esto permitirá que el usuario pueda iniciar sesión con una cuenta Microsoft.
    */
    .AddMicrosoftAccount(opciones =>
    {
        /*
            Se obtiene el ID de cliente desde el archivo appsettings.json.
            Este valor corresponde al "Id. de aplicación cliente"
            copiado desde Microsoft Entra ID.
        */
        opciones.ClientId = builder.Configuration["MicrosoftClientId"]!;

        /*
            Se obtiene el secreto del cliente desde el archivo appsettings.json.
            Este valor corresponde al "Valor" del secreto creado en Azure.
            No se debe colocar el Id. de secreto.
        */
        opciones.ClientSecret = builder.Configuration["MicrosoftSecretId"]!;
    });

/*
    Se configuran opciones adicionales para la cookie de autenticación.
    Identity usa cookies para mantener la sesión iniciada del usuario.
*/
builder.Services.PostConfigure<CookieAuthenticationOptions>(
    IdentityConstants.ApplicationScheme,
    opciones =>
    {

        opciones.LoginPath = "/Identity/Account/Login";
        opciones.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });
//*******************************************************************************


//******************** Configurar la autenticación *********************************
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Desactivar los requisitos de la contraseña
    options.Password.RequireDigit = false; // No requiere dígitos
    options.Password.RequireLowercase = false; // No requiere minúsculas
    options.Password.RequireUppercase = false; // No requiere mayúsculas
    options.Password.RequireNonAlphanumeric = false; // No requiere caracteres especiales
    options.Password.RequiredLength = 3; // Longitud mínima de 3 caracteres

    // No requerimos la confirmación de cuenta para iniciar sesión
    options.SignIn.RequireConfirmedAccount = false;

    // Configuramos la validación de correo
    options.User.RequireUniqueEmail = false;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
//*******************************************************************************

var app = builder.Build();

//******************** Creación automática de roles y usuarios ********************

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RolesUsuarios.InitializeRoles(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles.");
    }
}

//************************************************************************************

//************** Aplicar Migraciones Automáticamente **********************
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Aplica las migraciones pendientes
    }
    catch (Exception ex)
    {
        // Registra errores en el log en caso de que falle
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}
//***********************************************************************

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

/*
    Habilita la autenticación.
    Aquí la aplicación verifica si el usuario ha iniciado sesión.
*/
app.UseAuthentication();

/*
    Habilita la autorización.
    Después de autenticar al usuario, verifica si tiene permisos
    para acceder a una página o recurso específico.
*/
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
