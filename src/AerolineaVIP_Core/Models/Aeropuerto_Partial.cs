using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(AeropuertoMetadata))]
    public partial class Aeropuerto
    {
        [BsonIgnore] public string EstadoTexto => activo ? "Activo" : "Inactivo";
        [BsonIgnore] public string ClaseEstado => activo ? "text-bg-success" : "text-bg-secondary";
    }

    public class AeropuertoMetadata
    {
        [Display(Name = "Código IATA")]
        [Required(ErrorMessage = "El código IATA es obligatorio.")]
        [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "El código IATA debe tener exactamente 3 letras.")]
        public string codigoIATA { get; set; } = default!;

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string nombre { get; set; } = default!;

        [Display(Name = "Localidad")]
        [Required(ErrorMessage = "La localidad es obligatoria.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La localidad debe tener entre {2} y {1} caracteres.")]
        public string localidad { get; set; } = default!;

        [Display(Name = "Activo")]
        public bool activo { get; set; }
    }
}
