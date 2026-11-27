using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public class Catalogo
    {
        [Key]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public string tipo { get; set; } = default!;
        public string codigo { get; set; } = default!;
        public string descripcion { get; set; } = default!;

        // Campos opcionales: cada tipo de catalogo usa solo algunos (por eso son con ?)
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
