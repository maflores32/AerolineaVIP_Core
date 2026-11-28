//******************** Filtro de Autorización Personalizado por Permiso ********************
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AerolineaVIP_Core.Roles
{
    // Atributo para proteger acciones y controladores según permisos de módulo
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class AutorizarPermisoAttribute : TypeFilterAttribute
    {
        public AutorizarPermisoAttribute(string modulo, string accion)
            : base(typeof(AutorizarPermisoFilter))
        {
            Arguments = new object[] { modulo, accion };
        }
    }

    // Filtro asíncrono que valida los permisos del usuario antes de ejecutar la acción
    public class AutorizarPermisoFilter : IAsyncAuthorizationFilter
    {
        private readonly string _modulo;
        private readonly string _accion;
        private readonly ServicioPermisos _servicioPermisos;
        private readonly ILogger<AutorizarPermisoFilter> _logger;

        public AutorizarPermisoFilter(string modulo, string accion, ServicioPermisos servicioPermisos, ILogger<AutorizarPermisoFilter> logger)
        {
            _modulo = modulo;
            _accion = accion;
            _servicioPermisos = servicioPermisos;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var usuario = context.HttpContext.User;

            if (usuario == null || usuario.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            try
            {
                var tienePermiso = await _servicioPermisos.TienePermisoAsync(usuario, _modulo, _accion);
                if (!tienePermiso)
                {
                    // Bloquea el acceso si no posee el permiso activo
                    context.Result = new ForbidResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en AutorizarPermisoFilter al validar {Modulo}.{Accion}.", _modulo, _accion);
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
//*****************************************************************************************