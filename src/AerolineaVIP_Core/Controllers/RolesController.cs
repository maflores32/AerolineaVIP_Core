using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AerolineaVIP_Core.Controllers
{
    // Acceso restringido exclusivamente al SuperUsuario (ROL-SUP)
    [Authorize(Roles = "ROL-SUP")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RolesController> _logger;

        // Lista de roles base que no pueden ser eliminados ni editados por seguridad
        private static readonly HashSet<string> _rolesProtegidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "ROL-CLI",
            "ROL-CNT",
            "ROL-ADM",
            "ROL-SUP"
        };

        // Inyección de dependencias de RoleManager y ILogger para manipular AspNetRoles
        public RolesController(RoleManager<IdentityRole> roleManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        // Muestra la lista de roles registrados en la base de datos
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de roles.");
                TempData["Error"] = "No se pudo cargar la lista de roles. Intente nuevamente.";
                return View(new List<IdentityRole>());
            }
        }

        // Muestra el formulario para registrar un nuevo rol
        public IActionResult Create()
        {
            return View();
        }

        // Inserta un nuevo rol en la tabla AspNetRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "El nombre del rol es obligatorio.");
                return View();
            }

            try
            {
                var nombreLimpio = roleName.Trim().ToUpper();
                if (await _roleManager.RoleExistsAsync(nombreLimpio))
                {
                    ModelState.AddModelError("", $"El rol '{nombreLimpio}' ya existe.");
                    return View();
                }

                var resultado = await _roleManager.CreateAsync(new IdentityRole(nombreLimpio));
                if (resultado.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el rol {RoleName}.", roleName);
                ModelState.AddModelError("", "Ocurrió un error inesperado al crear el rol.");
            }

            return View();
        }

        // Muestra el formulario para editar el nombre de un rol existente
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null) return NotFound();

                // Validación: No permitir editar roles protegidos del sistema
                if (!string.IsNullOrEmpty(role.Name) && _rolesProtegidos.Contains(role.Name))
                {
                    TempData["Error"] = $"El rol '{role.Name}' es un rol del sistema y no se puede editar.";
                    return RedirectToAction(nameof(Index));
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar el rol {RoleId} para edición.", id);
                TempData["Error"] = "No se pudo cargar el rol seleccionado.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Actualiza el nombre del rol en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            IdentityRole? role = null;
            try
            {
                role = await _roleManager.FindByIdAsync(id);
                if (role == null) return NotFound();

                // Validación: No permitir modificar roles protegidos del sistema
                if (!string.IsNullOrEmpty(role.Name) && _rolesProtegidos.Contains(role.Name))
                {
                    TempData["Error"] = $"El rol '{role.Name}' es un rol del sistema y no se puede modificar.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    ModelState.AddModelError("", "El nombre del rol no puede quedar vacío.");
                    return View(role);
                }

                role.Name = roleName.Trim().ToUpper();
                var resultado = await _roleManager.UpdateAsync(role);
                if (resultado.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol {RoleId}.", id);
                ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar el rol.");
            }

            return View(role ?? new IdentityRole { Id = id, Name = roleName });
        }

        // Elimina el rol seleccionado de AspNetRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Identificador de rol inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    TempData["Error"] = "El rol que intenta eliminar no existe.";
                    return RedirectToAction(nameof(Index));
                }

                // Validación: No permitir eliminar roles protegidos del sistema
                if (!string.IsNullOrEmpty(role.Name) && _rolesProtegidos.Contains(role.Name))
                {
                    TempData["Error"] = $"El rol '{role.Name}' es un rol del sistema y no se puede eliminar.";
                    return RedirectToAction(nameof(Index));
                }

                var resultado = await _roleManager.DeleteAsync(role);
                if (!resultado.Succeeded)
                {
                    TempData["Error"] = "No se pudo eliminar el rol: " + string.Join(", ", resultado.Errors.Select(e => e.Description));
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de integridad al eliminar el rol {RoleId}.", id);
                TempData["Error"] = "No se puede eliminar el rol porque tiene usuarios o permisos asociados.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el rol {RoleId}.", id);
                TempData["Error"] = "Ocurrió un error inesperado al eliminar el rol.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}