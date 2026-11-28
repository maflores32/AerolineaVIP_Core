using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(AeronaveMetadata))]
    public partial class Aeronave : IValidatableObject
    {
        [BsonIgnore] public string ModeloTexto => modelo == null ? "—" : modelo.fabricante + " " + modelo.nombre;
        [BsonIgnore] public int Capacidad => modelo?.capacidadAsientos ?? 0;
        [BsonIgnore] public int TotalAsientos => asientos?.Count ?? 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (modelo == null)
            {
                yield return new ValidationResult("El modelo de la aeronave es obligatorio.", new[] { nameof(modelo) });
                yield break;
            }
            if (string.IsNullOrWhiteSpace(modelo.fabricante) || string.IsNullOrWhiteSpace(modelo.nombre))
                yield return new ValidationResult("El fabricante y el nombre del modelo son obligatorios.", new[] { nameof(modelo) });
            if (modelo.capacidadAsientos < 1 || modelo.capacidadAsientos > 500)
                yield return new ValidationResult("La capacidad debe estar entre 1 y 500 asientos.", new[] { nameof(modelo) });
            if (asientos != null && asientos.Count > 0 && asientos.Count != modelo.capacidadAsientos)
                yield return new ValidationResult("La cantidad de asientos no coincide con la capacidad del modelo.", new[] { nameof(asientos) });
            if (asientos != null && asientos.Select(a => a.numeroAsiento).Distinct().Count() != asientos.Count)
                yield return new ValidationResult("Hay números de asiento repetidos.", new[] { nameof(asientos) });
        }
    }

    public class AeronaveMetadata
    {
        [Display(Name = "Matrícula")]
        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [RegularExpression(@"^[A-Za-z0-9\-]{3,10}$", ErrorMessage = "La matrícula debe tener de 3 a 10 caracteres (letras, números o guion).")]
        public string matricula { get; set; } = default!;

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no puede superar {1} caracteres.")]
        public string estado { get; set; } = default!;
    }
}
