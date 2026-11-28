using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Asignacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string reservaId { get; set; } = default!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string pasajeroId { get; set; } = default!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string vueloId { get; set; } = default!;

        public int numeroAsiento { get; set; }
        public decimal importeTramo { get; set; }
        public string estado { get; set; } = default!;
        public DateTime fechaAsignacion { get; set; }
        public CheckInAsignacion? checkIn { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CheckInAsignacion
    {
        public int idSql { get; set; }
        public DateTime fechaRegistro { get; set; }
        public string estado { get; set; } = default!;
    }
}
