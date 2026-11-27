using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AerolineaVIP_Core.Roles
{
    public class RolesUsuarios
    {
        /// <summary>
        /// Inicializa roles y usuarios predeterminados en la base de datos de Identity
        /// </summary>
        public static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<RolesUsuarios>>();
            try
            {
                // Resolución de servicios para gestionar roles y usuarios
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // 1. Definir los roles de la Aerolínea VIP
                string[] roles = { "ROL-CLI", "ROL-CNT", "ROL-ADM", "ROL-SUP" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // 2. Usuarios iniciales (semilla)
                var users = new[]
                {
                    new { Username = "Counter", Email = "counter@aerolineavip.com", Password = "123", Roles = new[] { "ROL-CNT" } },
                    new { Username = "Administrador", Email = "admin@aerolineavip.com", Password = "123", Roles = new[] { "ROL-ADM" } },
                    new { Username = "SuperUsuario", Email = "superusuario@aerolineavip.com", Password = "123", Roles = new[] { "ROL-SUP" } }
                };

                foreach (var userInfo in users)
                {
                    var user = await userManager.FindByNameAsync(userInfo.Username);
                    if (user == null)
                    {
                        user = new IdentityUser { UserName = userInfo.Username, Email = userInfo.Email, EmailConfirmed = true };
                        var result = await userManager.CreateAsync(user, userInfo.Password);
                        if (result.Succeeded)
                        {
                            foreach (var role in userInfo.Roles)
                                await userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error al inicializar roles y usuarios semilla.");
            }
        }
    }
}