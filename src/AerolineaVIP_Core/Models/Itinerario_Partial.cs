using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(ItinerarioMetadata))]
    public partial class Itinerario : IValidatableObject
    {
        [BsonIgnore] public int NumeroTramos => tramos?.Count ?? 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (tramos == null || tramos.Count == 0)
            {
                yield return new ValidationResult("El itinerario debe tener al menos un tramo.", new[] { nameof(tramos) });
                yield break;
            }
            if (tramos.Any(t => string.IsNullOrWhiteSpace(t.vueloId)))
                yield return new ValidationResult("Todos los tramos deben tener un vuelo.", new[] { nameof(tramos) });
            if (tramos.Select(t => t.orden).Distinct().Count() != tramos.Count)
                yield return new ValidationResult("El orden de los tramos no puede repetirse.", new[] { nameof(tramos) });
            if (tramos.Any(t => t.orden < 1))
                yield return new ValidationResult("El orden de los tramos empieza en 1.", new[] { nameof(tramos) });
        }
    }

    public class ItinerarioMetadata
    {
        [Display(Name = "Referencia")]
        [Required(ErrorMessage = "La referencia es obligatoria.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "La referencia debe tener entre {2} y {1} caracteres.")]
        public string referencia { get; set; } = default!;

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string nombre { get; set; } = default!;

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        public string estado { get; set; } = default!;
    }
}
