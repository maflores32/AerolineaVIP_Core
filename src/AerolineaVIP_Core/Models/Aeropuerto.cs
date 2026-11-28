using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Aeropuerto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public string codigoIATA { get; set; } = default!;
        public string nombre { get; set; } = default!;
        public string localidad { get; set; } = default!;
        public bool activo { get; set; }
    }
}
