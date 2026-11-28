using AerolineaVIP_Core.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AerolineaVIP_Core.Controllers
{
    // Acceso restringido para el Administrador y el SuperUsuario
    [Authorize(Roles = "ROL-ADM,ROL-SUP")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ServicioPermisos _servicioPermisos; 
        private readonly ILogger<UsuariosController> _logger;

        // Inyección de dependencias corregida
        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ServicioPermisos servicioPermisos, ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _servicioPermisos = servicioPermisos;
            _logger = logger;
        }

        // Muestra la lista de usuarios registrados y pasa sus roles mediante ViewBag
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _userManager.Users.ToListAsync();
                var rolesPorUsuario = new Dictionary<string, IList<string>>();

                foreach (var user in usuarios)
                {
                    rolesPorUsuario[user.Id] = await _userManager.GetRolesAsync(user);
                }

                ViewBag.RolesPorUsuario = rolesPorUsuario;
                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de usuarios y roles.");
                TempData["Error"] = "No se pudo cargar la lista de usuarios. Intente nuevamente.";
                ViewBag.RolesPorUsuario = new Dictionary<string, IList<string>>();
                return View(new List<IdentityUser>());
            }
        }

        public async Task<IActionResult> EditarRoles(string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId)) return NotFound();

            try
            {
                var user = await _userManager.FindByIdAsync(usuarioId);
                if (user == null) return NotFound();

                ViewBag.RolesDisponibles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                ViewBag.RolesAsignados = await _userManager.GetRolesAsync(user);

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los roles del usuario {UsuarioId}.", usuarioId);
                TempData["Error"] = "No se pudieron cargar los roles del usuario seleccionado.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRoles(string usuarioId, string[] rolesSeleccionados)
        {
            if (string.IsNullOrEmpty(usuarioId)) return NotFound();

            try
            {
                var user = await _userManager.FindByIdAsync(usuarioId);
                if (user == null) return NotFound();

                // =========================================================================
                // REGLA DE NEGOCIO RN-R03: Un usuario no puede modificarse sus propios roles
                // =========================================================================
                var usuarioActualId = _userManager.GetUserId(User);
                if (user.Id == usuarioActualId)
                {
                    TempData["Error"] = "Por seguridad (RN-R03), no puedes modificar tus propios roles.";
                    return RedirectToAction(nameof(Index));
                }

                // =========================================================================
                // REGLA DE NEGOCIO RN-R04: Proteger al último administrador
                // =========================================================================
                var rolesActuales = await _userManager.GetRolesAsync(user);
                bool eraAdmin = rolesActuales.Contains("ROL-ADM");
                bool seguiraSiendoAdmin = rolesSeleccionados != null && rolesSeleccionados.Contains("ROL-ADM");

                if (eraAdmin && !seguiraSiendoAdmin)
                {
                    var admins = await _userManager.GetUsersInRoleAsync("ROL-ADM");
                    if (admins.Count == 1 && admins[0].Id == user.Id)
                    {
                        TempData["Error"] = "Por seguridad (RN-R04), no puedes eliminar el rol del último Administrador activo.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // 1. Se remueven todos los roles actuales del usuario
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesActuales);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "No se pudieron remover los roles actuales del usuario.";
                    return RedirectToAction(nameof(Index));
                }

                // 2. Se asignan los nuevos roles seleccionados en los checkboxes
                if (rolesSeleccionados != null && rolesSeleccionados.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesSeleccionados);

                    if (!addResult.Succeeded)
                    {
                        TempData["Error"] = "No se pudieron asignar algunos roles al usuario.";
                        return RedirectToAction(nameof(Index));
                    }

                    // 3. Sincronizamos los Claims (Permisos) del rol al usuario
                    foreach (var rol in rolesSeleccionados)
                    {
                        await _servicioPermisos.AsignarPermisosPredeterminadosAUsuario(user.Id, rol);
                    }
                }

                // Actualiza la estampa de seguridad para que el cambio de rol surta efecto
                await _userManager.UpdateSecurityStampAsync(user);

                // =========================================================================
                // REGLA DE NEGOCIO RN-R05: Auditoría en MongoDB
                // TODO: Cuando conectes MongoDB, insertar aquí el registro: 
                // Quién (usuarioActualId) le cambió los roles a Quién (user.Id), cuándo y qué roles.
                // =========================================================================

                TempData["Exito"] = "Roles actualizados correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar los roles del usuario {UsuarioId}.", usuarioId);
                TempData["Error"] = "Ocurrió un error inesperado al actualizar los roles del usuario.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}