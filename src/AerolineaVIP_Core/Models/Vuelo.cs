using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Vuelo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public DateTime fechaOperacion { get; set; }
        public string estado { get; set; } = default!;
        public RutaVuelo ruta { get; set; } = default!;
        public FrecuenciaVuelo frecuencia { get; set; } = default!;
        public AeronaveVuelo? aeronave { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class RutaVuelo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string rutaId { get; set; } = default!;
        public decimal tarifaReferencial { get; set; }
        public AeropuertoResumen origen { get; set; } = default!;
        public AeropuertoResumen destino { get; set; } = default!;
    }

    [BsonIgnoreExtraElements]
    public class AeropuertoResumen
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string aeropuertoId { get; set; } = default!;
        public string codigoIATA { get; set; } = default!;
        public string nombre { get; set; } = default!;
        public string localidad { get; set; } = default!;
    }

    [BsonIgnoreExtraElements]
    public class FrecuenciaVuelo
    {
        public int idSql { get; set; }
        public int diaSemana { get; set; }
        public string horaSalida { get; set; } = default!;
        public string horaLlegada { get; set; } = default!;
    }

    [BsonIgnoreExtraElements]
    public class AeronaveVuelo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string aeronaveId { get; set; } = default!;
        public string matricula { get; set; } = default!;
        public string modelo { get; set; } = default!;
        public string fabricante { get; set; } = default!;
        public int capacidadAsientos { get; set; }
    }
}
