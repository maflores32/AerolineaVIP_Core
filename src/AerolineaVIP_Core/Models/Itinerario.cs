using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Itinerario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public string referencia { get; set; } = default!;
        public string nombre { get; set; } = default!;
        public string estado { get; set; } = default!;
        public List<TramoItinerario> tramos { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class TramoItinerario
    {
        public int idSql { get; set; }
        public int orden { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string vueloId { get; set; } = default!;
    }
}
