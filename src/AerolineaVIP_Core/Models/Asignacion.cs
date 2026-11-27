using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public class Asignacion
    {
        [Key]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; } = default!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string reservaId { get; set; } = default!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string pasajeroId { get; set; } = default!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string vueloId { get; set; } = default!;

        public int numeroAsiento { get; set; } = default!;
        public decimal importeTramo { get; set; } = default!;
        public string estado { get; set; } = default!;
        public DateTime fechaAsignacion { get; set; } = default!;
        public CheckInAsignacion? checkIn { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CheckInAsignacion
    {
        public int idSql { get; set; } = default!;
        public DateTime fechaRegistro { get; set; } = default!;
        public string estado { get; set; } = default!;
    }
}
