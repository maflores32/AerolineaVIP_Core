using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Ruta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public AeropuertoResumen origen { get; set; } = default!;
        public AeropuertoResumen destino { get; set; } = default!;
        public decimal tarifaReferencial { get; set; }
        public bool activa { get; set; }
        public List<FrecuenciaRuta> frecuencias { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class FrecuenciaRuta
    {
        public int idSql { get; set; }
        public int diaSemana { get; set; }
        public string horaSalida { get; set; } = default!;
        public string horaLlegada { get; set; } = default!;
        public bool activa { get; set; }
    }
}
