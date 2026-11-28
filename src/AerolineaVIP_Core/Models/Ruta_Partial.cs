using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(RutaMetadata))]
    public partial class Ruta : IValidatableObject
    {
        [BsonIgnore] public string RutaTexto => origen.codigoIATA + " → " + destino.codigoIATA;
        [BsonIgnore] public string PrecioTexto => "$ " + tarifaReferencial.ToString("N2", CultureInfo.InvariantCulture);
        [BsonIgnore] public int FrecuenciasActivas => frecuencias.Count(f => f.activa);
        [BsonIgnore] public string EstadoTexto => activa ? "Activa" : "Inactiva";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (origen == null || string.IsNullOrWhiteSpace(origen.aeropuertoId))
                yield return new ValidationResult("Seleccione el aeropuerto de origen.", new[] { nameof(origen) });
            if (destino == null || string.IsNullOrWhiteSpace(destino.aeropuertoId))
                yield return new ValidationResult("Seleccione el aeropuerto de destino.", new[] { nameof(destino) });
            if (origen != null && destino != null && origen.aeropuertoId == destino.aeropuertoId)
                yield return new ValidationResult("El origen y el destino deben ser distintos.", new[] { nameof(destino) });

            foreach (var f in frecuencias ?? new List<FrecuenciaRuta>())
            {
                if (f.diaSemana < 1 || f.diaSemana > 7)
                    yield return new ValidationResult("El día de la semana debe estar entre 1 y 7.", new[] { nameof(frecuencias) });
                if (!TimeSpan.TryParseExact(f.horaSalida, @"hh\:mm", CultureInfo.InvariantCulture, out _) ||
                    !TimeSpan.TryParseExact(f.horaLlegada, @"hh\:mm", CultureInfo.InvariantCulture, out _))
                    yield return new ValidationResult("Las horas de la frecuencia deben tener formato HH:mm.", new[] { nameof(frecuencias) });
            }
        }
    }

    public class RutaMetadata
    {
        [Display(Name = "Tarifa referencial (USD)")]
        [Required(ErrorMessage = "La tarifa es obligatoria.")]
        [Range(typeof(decimal), "0.01", "10000", ErrorMessage = "La tarifa debe estar entre {1} y {2}.")]
        [DataType(DataType.Currency)]
        public decimal tarifaReferencial { get; set; }

        [Display(Name = "Activa")]
        public bool activa { get; set; }
    }
}
