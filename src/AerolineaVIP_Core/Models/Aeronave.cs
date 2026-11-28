using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [BsonIgnoreExtraElements]
    public partial class Aeronave
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = default!;
        public int idSql { get; set; }
        public string matricula { get; set; } = default!;
        public string estado { get; set; } = default!;
        public ModeloAeronave modelo { get; set; } = default!;
        public List<AsientoAeronave> asientos { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class ModeloAeronave
    {
        public int idSql { get; set; }
        public string fabricante { get; set; } = default!;
        public string nombre { get; set; } = default!;
        public int capacidadAsientos { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AsientoAeronave
    {
        public int numeroAsiento { get; set; }
        public string codigo { get; set; } = default!;
        public int fila { get; set; }
        public string lado { get; set; } = default!;
        public string ubicacion { get; set; } = default!;
    }
}
