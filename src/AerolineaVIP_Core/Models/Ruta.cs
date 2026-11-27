using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Ruta
    {
        [Key]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; } = default!;
        public AeropuertoResumen origen { get; set; } = default!;
        public AeropuertoResumen destino { get; set; } = default!;
        public decimal tarifaReferencial { get; set; } = default!;
        public bool activa { get; set; } = default!;
        public List<FrecuenciaRuta> frecuencias { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class FrecuenciaRuta
    {
        public int idSql { get; set; } = default!;
        public int diaSemana { get; set; } = default!;
        public string horaSalida { get; set; } = default!;
        public string horaLlegada { get; set; } = default!;
        public bool activa { get; set; } = default!;
    }
}
