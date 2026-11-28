using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Catalogo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public string tipo { get; set; } = default!;
        public string codigo { get; set; } = default!;
        public string descripcion { get; set; } = default!;

        // Campos opcionales: cada tipo de catálogo usa solo algunos
        public bool? ocupaInventario { get; set; }
        public string? estadoVisual { get; set; }
        public bool? activo { get; set; }
        public bool? activa { get; set; }
        public int? longitudNumero { get; set; }
        public int? longitudCodigoSeguridad { get; set; }
        public string? prefijosReferencia { get; set; }
        public string? rutaLogo { get; set; }
    }
}
