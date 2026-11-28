using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Compra
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string reservaId { get; set; } = default!;

        public DateTime fechaCompra { get; set; }
        public decimal importe { get; set; }
        public string moneda { get; set; } = default!;
        public string estado { get; set; } = default!;
        public string claveIdempotencia { get; set; } = default!;
        public List<PagoCompra> pagos { get; set; } = new();
        public List<ComprobanteCompra> comprobantes { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class PagoCompra
    {
        public int idSql { get; set; }
        public DateTime fechaPago { get; set; }
        public decimal importe { get; set; }
        public string estado { get; set; } = default!;
        public string metodoPago { get; set; } = default!;
        public string marcaTarjeta { get; set; } = default!;
        public string ultimosCuatroDigitos { get; set; } = default!;
        public string? nombreTitular { get; set; }
        public string codigoOperacion { get; set; } = default!;
        public bool esSimulado { get; set; }
        public string? mensajeResultado { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ComprobanteCompra
    {
        public int idSql { get; set; }
        public string tipo { get; set; } = default!;
        public string numero { get; set; } = default!;
        public DateTime fechaEmision { get; set; }
        public string estado { get; set; } = default!;
    }
}
