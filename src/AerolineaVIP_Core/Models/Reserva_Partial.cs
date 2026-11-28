using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(ReservaMetadata))]
    public partial class Reserva : IValidatableObject
    {
        [BsonIgnore] public string CodigoTexto => string.IsNullOrEmpty(codigoReserva) ? "Sin código" : codigoReserva;
        [BsonIgnore] public string FechaTexto => fechaCreacion.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        [BsonIgnore] public string ExpiraTexto => fechaExpiracion == null ? "No expira" : fechaExpiracion.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // DP-07: la reserva pendiente vence a las 24 horas
            if (fechaExpiracion != null && fechaExpiracion <= fechaCreacion)
                yield return new ValidationResult("La fecha de expiración debe ser posterior a la de creación.", new[] { nameof(fechaExpiracion) });
        }
    }

    public class ReservaMetadata
    {
        [Display(Name = "Código de reserva")]
        [StringLength(20, ErrorMessage = "El código no puede superar {1} caracteres.")]
        public string? codigoReserva { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string usuarioId { get; set; } = default!;

        [Display(Name = "Fecha de creación")]
        [Required(ErrorMessage = "La fecha de creación es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime fechaCreacion { get; set; }

        [Display(Name = "Fecha de expiración")]
        [DataType(DataType.DateTime)]
        public DateTime? fechaExpiracion { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        public string estado { get; set; } = default!;
    }
}
