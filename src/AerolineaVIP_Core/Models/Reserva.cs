using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Reserva
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public string? codigoReserva { get; set; }
        public string usuarioId { get; set; } = default!;
        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaExpiracion { get; set; }
        public string estado { get; set; } = default!;
    }
}
