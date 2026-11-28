using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(AsignacionMetadata))]
    public partial class Asignacion
    {
        [BsonIgnore] public string ImporteTexto => "$ " + importeTramo.ToString("N2", CultureInfo.InvariantCulture);
        [BsonIgnore] public string FechaTexto => fechaAsignacion.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        [BsonIgnore] public bool TieneCheckIn => checkIn != null;
    }

    public class AsignacionMetadata
    {
        [Display(Name = "Reserva")]
        [Required(ErrorMessage = "La reserva es obligatoria.")]
        public string reservaId { get; set; } = default!;

        [Display(Name = "Pasajero")]
        [Required(ErrorMessage = "El pasajero es obligatorio.")]
        public string pasajeroId { get; set; } = default!;

        [Display(Name = "Vuelo")]
        [Required(ErrorMessage = "El vuelo es obligatorio.")]
        public string vueloId { get; set; } = default!;

        [Display(Name = "Asiento")]
        [Range(1, 500, ErrorMessage = "El número de asiento debe estar entre {1} y {2}.")]
        public int numeroAsiento { get; set; }

        [Display(Name = "Importe del tramo (USD)")]
        [Range(typeof(decimal), "0.01", "10000", ErrorMessage = "El importe debe estar entre {1} y {2}.")]
        [DataType(DataType.Currency)]
        public decimal importeTramo { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string estado { get; set; } = default!;

        [Display(Name = "Fecha de asignación")]
        [DataType(DataType.DateTime)]
        public DateTime fechaAsignacion { get; set; }
    }
}
