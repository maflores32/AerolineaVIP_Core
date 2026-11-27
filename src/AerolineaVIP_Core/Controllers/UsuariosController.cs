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
        private readonly ILogger<UsuariosController> _logger;

        // Inyección de dependencias de UserManager, RoleManager y ILogger
        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // Muestra la lista de usuarios registrados y pasa sus roles mediante ViewBag
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _userManager.Users.ToListAsync();
                var rolesPorUsuario = new Dictionary<string, IList<string>>();

                // Se recorre cada usuario para obtener los roles que tiene asignados en la base de datos
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

        // Carga la pantalla de edición de roles pasando el IdentityUser y los roles en ViewBag
        public async Task<IActionResult> EditarRoles(string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId)) return NotFound();

            try
            {
                var user = await _userManager.FindByIdAsync(usuarioId);
                if (user == null) return NotFound();

                // Se envían a la vista todos los roles disponibles y los que ya tiene el usuario
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

        // Actualiza los roles del usuario recibiendo el arreglo de checkboxes seleccionados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRoles(string usuarioId, string[] rolesSeleccionados)
        {
            if (string.IsNullOrEmpty(usuarioId)) return NotFound();

            try
            {
                var user = await _userManager.FindByIdAsync(usuarioId);
                if (user == null) return NotFound();

                // 1. Se remueven todos los roles actuales del usuario
                var rolesActuales = await _userManager.GetRolesAsync(user);
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
                }

                // 3. Se actualiza la estampa de seguridad para que el cambio de rol surta efecto
                await _userManager.UpdateSecurityStampAsync(user);

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
