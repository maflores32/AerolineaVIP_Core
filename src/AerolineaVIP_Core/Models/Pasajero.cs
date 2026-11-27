using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Pasajero
    {
        [Key]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; } = default!;
        public TipoDocumento tipoDocumento { get; set; } = default!;

        [Display(Name = "Número de documento")]
        public string numeroDocumento { get; set; } = default!;

        [Display(Name = "Nombres")]
        public string nombres { get; set; } = default!;

        [Display(Name = "Apellidos")]
        public string apellidos { get; set; } = default!;

        [Display(Name = "Fecha de nacimiento")]
        public DateTime? fechaNacimiento { get; set; }

        [Display(Name = "Correo")]
        public string? correo { get; set; }

        [Display(Name = "Teléfono")]
        public string? telefono { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime fechaRegistro { get; set; } = default!;
    }

    [BsonIgnoreExtraElements]
    public class TipoDocumento
    {
        public string codigo { get; set; } = default!;
        public string descripcion { get; set; } = default!;
    }
}
