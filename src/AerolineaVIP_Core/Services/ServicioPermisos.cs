//******************** Servicio de Verificación de Permisos ********************
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AerolineaVIP_Core.Services
{
    public class ServicioPermisos
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ServicioPermisos> _logger;

        // Inyección de dependencias para consultar usuarios, roles y registrar errores
        public ServicioPermisos(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ServicioPermisos> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // Valida en tiempo real si el usuario autenticado tiene el permiso requerido en su rol
        public async Task<bool> TienePermisoAsync(ClaimsPrincipal user, string modulo, string accion)
        {
            if (user == null || user.Identity?.IsAuthenticated != true) return false;
            if (string.IsNullOrWhiteSpace(modulo) || string.IsNullOrWhiteSpace(accion)) return false;

            // El SuperUsuario (ROL-SUP) cuenta con acceso irrestricto a todo el sistema
            if (user.IsInRole("ROL-SUP")) return true;

            try
            {
                var usuario = await _userManager.GetUserAsync(user);
                if (usuario == null) return false;

                var roles = await _userManager.GetRolesAsync(usuario);
                foreach (var rolNombre in roles)
                {
                    var rol = await _roleManager.FindByNameAsync(rolNombre);
                    if (rol != null)
                    {
                        // Consulta los permisos guardados en la tabla AspNetRoleClaims
                        var claims = await _roleManager.GetClaimsAsync(rol);
                        if (claims.Any(c => c.Type == "Permission" && c.Value == $"{modulo}.{accion}"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el permiso {Modulo}.{Accion} para el usuario {User}.", modulo, accion, user.Identity?.Name);
                // Ante un fallo de consulta se deniega el acceso por seguridad
                return false;
            }
        }
    }
}
//******************************************************************************