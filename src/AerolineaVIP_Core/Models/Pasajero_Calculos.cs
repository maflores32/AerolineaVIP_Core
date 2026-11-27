using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    public partial class Pasajero
    {
        [BsonIgnore]
        public List<ViajePasajero> Viajes { get; set; } = new();

        [BsonIgnore]
        public string NombreCompleto => nombres + " " + apellidos;

        [BsonIgnore]
        public string DocumentoTexto => tipoDocumento.descripcion + ": " + numeroDocumento;

        [BsonIgnore]
        public string FechaNacimientoTexto => fechaNacimiento == null ? "No registrada" : fechaNacimiento.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        [BsonIgnore]
        public string EdadTexto => CalcularEdad() == null ? "—" : CalcularEdad() + " años";

        [BsonIgnore]
        public string CorreoTexto => string.IsNullOrWhiteSpace(correo) ? "—" : correo;

        [BsonIgnore]
        public string TelefonoTexto => string.IsNullOrWhiteSpace(telefono) ? "—" : telefono;

        [BsonIgnore]
        public string RegistroTexto => fechaRegistro.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        private int? CalcularEdad()
        {
            if (fechaNacimiento == null)
            {
                return null;
            }
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
            {
                edad--;
            }
            return edad;
        }

        // Une las asignaciones del pasajero con los datos del vuelo (referencia vueloId)
        public void CalcularViajes(IEnumerable<Asignacion> asignaciones, IEnumerable<Vuelo> vuelos)
        {
            var vuelosPorId = vuelos.ToDictionary(v => v._id);

            Viajes = asignaciones
                .Where(a => vuelosPorId.ContainsKey(a.vueloId))
                .Select(a => new ViajePasajero
                {
                    VueloId = a.vueloId,
                    Fecha = vuelosPorId[a.vueloId].fechaOperacion,
                    FechaTexto = vuelosPorId[a.vueloId].FechaTexto,
                    Ruta = vuelosPorId[a.vueloId].RutaTexto,
                    Asiento = a.numeroAsiento,
                    ImporteTexto = "$ " + a.importeTramo.ToString("N2", CultureInfo.InvariantCulture),
                    Estado = a.estado
                })
                .OrderBy(v => v.Fecha)
                .ToList();
        }
    }

    public class ViajePasajero
    {
        public string VueloId { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string FechaTexto { get; set; } = "";
        public string Ruta { get; set; } = "";
        public int Asiento { get; set; }
        public string ImporteTexto { get; set; } = "";
        public string Estado { get; set; } = "";
    }
}
