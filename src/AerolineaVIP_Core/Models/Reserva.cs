using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Reserva
    {
        [Key]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; } = default!;
        public string? codigoReserva { get; set; }
        public string usuarioId { get; set; } = default!;
        public DateTime fechaCreacion { get; set; } = default!;
        public DateTime? fechaExpiracion { get; set; }
        public string estado { get; set; } = default!;
    }
}
