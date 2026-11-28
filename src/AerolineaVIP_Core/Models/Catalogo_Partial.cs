using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(CatalogoMetadata))]
    public partial class Catalogo : IValidatableObject
    {
        [BsonIgnore] public string ActivoTexto => (activo ?? activa ?? true) ? "Activo" : "Inactivo";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (longitudNumero != null && (longitudNumero < 1 || longitudNumero > 30))
                yield return new ValidationResult("La longitud del número debe estar entre 1 y 30.", new[] { nameof(longitudNumero) });
            if (longitudCodigoSeguridad != null && (longitudCodigoSeguridad < 1 || longitudCodigoSeguridad > 6))
                yield return new ValidationResult("La longitud del código de seguridad debe estar entre 1 y 6.", new[] { nameof(longitudCodigoSeguridad) });
        }
    }

    public class CatalogoMetadata
    {
        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El tipo de catálogo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo no puede superar {1} caracteres.")]
        public string tipo { get; set; } = default!;

        [Display(Name = "Código")]
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede superar {1} caracteres.")]
        public string codigo { get; set; } = default!;

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(150, ErrorMessage = "La descripción no puede superar {1} caracteres.")]
        public string descripcion { get; set; } = default!;

        [Display(Name = "Ocupa inventario")]
        public bool? ocupaInventario { get; set; }

        [Display(Name = "Estado visual")]
        [StringLength(30)]
        public string? estadoVisual { get; set; }

        [Display(Name = "Activo")]
        public bool? activo { get; set; }

        [Display(Name = "Activa")]
        public bool? activa { get; set; }

        [Display(Name = "Longitud del número")]
        public int? longitudNumero { get; set; }

        [Display(Name = "Longitud del código de seguridad")]
        public int? longitudCodigoSeguridad { get; set; }

        [Display(Name = "Prefijos de referencia")]
        [StringLength(100)]
        public string? prefijosReferencia { get; set; }

        [Display(Name = "Ruta del logo")]
        [StringLength(200)]
        public string? rutaLogo { get; set; }
    }
}
