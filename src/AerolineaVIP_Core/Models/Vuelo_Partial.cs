using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(VueloMetadata))]
    public partial class Vuelo : IValidatableObject
    {
        private static readonly string[] Dias = { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

        // ---------- Cálculos (no se guardan en Mongo) ----------
        [BsonIgnore] public int AsientosOcupados { get; set; }
        [BsonIgnore] public List<AsientoVuelo> Asientos { get; set; } = new();
        [BsonIgnore] public decimal Precio => ruta.tarifaReferencial;
        [BsonIgnore] public string PrecioTexto => "$ " + Precio.ToString("N2", CultureInfo.InvariantCulture);
        [BsonIgnore] public string RutaTexto => ruta.origen.codigoIATA + " → " + ruta.destino.codigoIATA;
        [BsonIgnore] public string OrigenTexto => ruta.origen.nombre + " (" + ruta.origen.localidad + ")";
        [BsonIgnore] public string DestinoTexto => ruta.destino.nombre + " (" + ruta.destino.localidad + ")";
        [BsonIgnore] public string FechaTexto => fechaOperacion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        [BsonIgnore] public string DiaTexto => frecuencia.diaSemana >= 1 && frecuencia.diaSemana <= 7 ? Dias[frecuencia.diaSemana] : "";
        [BsonIgnore] public string HorarioTexto => Hora(frecuencia.horaSalida) + " - " + Hora(frecuencia.horaLlegada);
        [BsonIgnore] public bool TieneAeronave => aeronave != null;
        [BsonIgnore] public string AeronaveTexto => aeronave == null ? "Sin aeronave asignada" : aeronave.matricula + " · " + aeronave.fabricante + " " + aeronave.modelo;
        [BsonIgnore] public int? Capacidad => aeronave?.capacidadAsientos;
        [BsonIgnore] public int? AsientosDisponibles => aeronave == null ? null : Math.Max(0, aeronave.capacidadAsientos - AsientosOcupados);
        [BsonIgnore] public string DisponibilidadTexto => AsientosDisponibles == null ? "Sin aeronave" : AsientosDisponibles + " de " + Capacidad;
        [BsonIgnore] public string ClaseDisponibilidad => AsientosDisponibles == null ? "text-bg-secondary" : AsientosDisponibles == 0 ? "text-bg-danger" : "text-bg-success";

        private static string Hora(string hora) => hora != null && hora.Length >= 5 ? hora.Substring(0, 5) : hora ?? "";

        // Cuenta cuántos asientos ocupa cada vuelo y lo asigna a cada vuelo de la lista
        public static void CalcularOcupacion(IEnumerable<Vuelo> vuelos, IEnumerable<Asignacion> asignacionesQueOcupan)
        {
            var ocupadosPorVuelo = asignacionesQueOcupan
                .GroupBy(a => a.vueloId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var vuelo in vuelos)
            {
                vuelo.AsientosOcupados = ocupadosPorVuelo.GetValueOrDefault(vuelo._id, 0);
            }
        }

        // Arma el listado de asientos de la aeronave marcando los ocupados
        public void CalcularAsientos(Aeronave? aeronaveCompleta, IEnumerable<Asignacion> asignacionesQueOcupan)
        {
            var numerosOcupados = asignacionesQueOcupan
                .Where(a => a.vueloId == _id)
                .Select(a => a.numeroAsiento)
                .ToHashSet();

            AsientosOcupados = numerosOcupados.Count;
            Asientos = aeronaveCompleta == null
                ? new List<AsientoVuelo>()
                : aeronaveCompleta.asientos
                    .OrderBy(a => a.numeroAsiento)
                    .Select(a => new AsientoVuelo
                    {
                        Codigo = a.codigo,
                        Ubicacion = a.ubicacion,
                        Ocupado = numerosOcupados.Contains(a.numeroAsiento)
                    })
                    .ToList();
        }

        // ---------- Validación de negocio en servidor (RF-051) ----------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ruta == null || ruta.origen == null || ruta.destino == null)
            {
                yield return new ValidationResult("La ruta es obligatoria.", new[] { nameof(ruta) });
                yield break;
            }
            if (ruta.origen.aeropuertoId == ruta.destino.aeropuertoId)
                yield return new ValidationResult("El origen y el destino deben ser distintos.", new[] { nameof(ruta) });
            if (frecuencia == null)
            {
                yield return new ValidationResult("La frecuencia es obligatoria.", new[] { nameof(frecuencia) });
                yield break;
            }
            if (frecuencia.diaSemana < 1 || frecuencia.diaSemana > 7)
                yield return new ValidationResult("El día de la semana debe estar entre 1 (lunes) y 7 (domingo).", new[] { nameof(frecuencia) });
            if (!HoraValida(frecuencia.horaSalida) || !HoraValida(frecuencia.horaLlegada))
                yield return new ValidationResult("Las horas deben tener formato HH:mm.", new[] { nameof(frecuencia) });
        }

        private static bool HoraValida(string? h) => TimeSpan.TryParseExact(h, @"hh\:mm", CultureInfo.InvariantCulture, out _);

        // Asiento para el mapa de asientos (verde disponible / rojo ocupado)
        public class AsientoVuelo
        {
            public string Codigo { get; set; } = "";
            public string Ubicacion { get; set; } = "";
            public bool Ocupado { get; set; }
            public string EstadoTexto => Ocupado ? "Ocupado" : "Disponible";
            public string ClaseEstado => Ocupado ? "text-bg-danger" : "text-bg-success";
        }
    }

    // ---------- Metadata: Data Annotations centralizadas ----------
    public class VueloMetadata
    {
        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La fecha de operación es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime fechaOperacion { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no puede superar {1} caracteres.")]
        public string estado { get; set; } = default!;
    }
}
