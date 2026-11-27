using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AerolineaVIP_Core.Controllers
{
    // Restringe el acceso al módulo de permisos únicamente al SuperUsuario
    [Authorize(Roles = "ROL-SUP")]
    public class PermisosController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<PermisosController> _logger;

        public PermisosController(RoleManager<IdentityRole> roleManager, ILogger<PermisosController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        // Muestra la lista de roles para seleccionar a cuál configurar permisos
        public async Task<IActionResult> Listar()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar los roles para configurar permisos.");
                TempData["Error"] = "No se pudo cargar la lista de roles.";
                return View(new List<IdentityRole>());
            }
        }

        // Carga la lista de permisos activos en ViewBag y envía el IdentityRole a la vista
        public async Task<IActionResult> Index(string rolId)
        {
            if (string.IsNullOrEmpty(rolId)) return RedirectToAction(nameof(Listar));

            try
            {
                var rol = await _roleManager.FindByIdAsync(rolId);
                if (rol == null) return NotFound();

                var claimsExistentes = await _roleManager.GetClaimsAsync(rol);
                ViewBag.PermisosActivos = claimsExistentes
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                return View(rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los permisos del rol {RolId}.", rolId);
                TempData["Error"] = "No se pudieron cargar los permisos del rol seleccionado.";
                return RedirectToAction(nameof(Listar));
            }
        }

        // Guarda los switches seleccionados directamente como claims en AspNetRoleClaims
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(string rolId, string[] permisosSeleccionados)
        {
            if (string.IsNullOrEmpty(rolId)) return NotFound();

            try
            {
                var rol = await _roleManager.FindByIdAsync(rolId);
                if (rol == null) return NotFound();

                // Elimina los claims previos para evitar duplicados
                var claimsActuales = await _roleManager.GetClaimsAsync(rol);
                foreach (var claim in claimsActuales.Where(c => c.Type == "Permission"))
                {
                    await _roleManager.RemoveClaimAsync(rol, claim);
                }

                // Inserta únicamente los permisos cuyos switches quedaron activados
                if (permisosSeleccionados != null)
                {
                    foreach (var permiso in permisosSeleccionados)
                    {
                        await _roleManager.AddClaimAsync(rol, new Claim("Permission", permiso));
                    }
                }

                return RedirectToAction("Listar", "Permisos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar los permisos del rol {RolId}.", rolId);
                TempData["Error"] = "Ocurrió un error al guardar los permisos. Intente nuevamente.";
                return RedirectToAction(nameof(Listar));
            }
        }
    }
}
