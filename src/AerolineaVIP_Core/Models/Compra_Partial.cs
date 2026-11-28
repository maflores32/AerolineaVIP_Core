using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(CompraMetadata))]
    public partial class Compra : IValidatableObject
    {
        [BsonIgnore] public string FechaTexto => fechaCompra.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        [BsonIgnore] public string ImporteTexto => "$ " + importe.ToString("N2", CultureInfo.InvariantCulture) + " " + moneda;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // DP-04: la moneda del sistema es USD
            if (!string.IsNullOrEmpty(moneda) && moneda != "USD")
                yield return new ValidationResult("La moneda debe ser USD.", new[] { nameof(moneda) });

            foreach (var p in pagos ?? new List<PagoCompra>())
            {
                // DP-06: pagos simulados, solo se guarda marca, titular y últimos 4 dígitos
                if (!Regex.IsMatch(p.ultimosCuatroDigitos ?? "", @"^\d{4}$"))
                    yield return new ValidationResult("Los últimos dígitos de la tarjeta deben ser exactamente 4 números.", new[] { nameof(pagos) });
                if (p.importe <= 0)
                    yield return new ValidationResult("El importe de cada pago debe ser mayor a cero.", new[] { nameof(pagos) });
            }
        }
    }

    public class CompraMetadata
    {
        [Display(Name = "Reserva")]
        [Required(ErrorMessage = "La reserva es obligatoria.")]
        public string reservaId { get; set; } = default!;

        [Display(Name = "Fecha de compra")]
        [Required(ErrorMessage = "La fecha de compra es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime fechaCompra { get; set; }

        [Display(Name = "Importe")]
        [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "El importe debe estar entre {1} y {2}.")]
        [DataType(DataType.Currency)]
        public decimal importe { get; set; }

        [Display(Name = "Moneda")]
        [Required(ErrorMessage = "La moneda es obligatoria.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La moneda debe tener 3 letras.")]
        public string moneda { get; set; } = default!;

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        public string estado { get; set; } = default!;

        [Display(Name = "Clave de idempotencia")]
        [Required(ErrorMessage = "La clave de idempotencia es obligatoria.")]
        [StringLength(80)]
        public string claveIdempotencia { get; set; } = default!;
    }
}
