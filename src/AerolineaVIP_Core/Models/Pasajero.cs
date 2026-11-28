using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Pasajero
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public TipoDocumento tipoDocumento { get; set; } = default!;
        public string numeroDocumento { get; set; } = default!;
        public string nombres { get; set; } = default!;
        public string apellidos { get; set; } = default!;
        public DateTime? fechaNacimiento { get; set; }
        public string? correo { get; set; }
        public string? telefono { get; set; }
        public DateTime fechaRegistro { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class TipoDocumento
    {
        public string codigo { get; set; } = default!;
        public string descripcion { get; set; } = default!;
    }
}
