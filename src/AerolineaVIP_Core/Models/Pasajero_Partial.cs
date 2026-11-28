using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace AerolineaVIP_Core.Models
{
    [MetadataType(typeof(PasajeroMetadata))]
    public partial class Pasajero : IValidatableObject
    {
        [BsonIgnore] public List<ViajePasajero> Viajes { get; set; } = new();
        [BsonIgnore] public string NombreCompleto => nombres + " " + apellidos;
        [BsonIgnore] public string DocumentoTexto => tipoDocumento.descripcion + ": " + numeroDocumento;
        [BsonIgnore] public string FechaNacimientoTexto => fechaNacimiento == null ? "No registrada" : fechaNacimiento.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        [BsonIgnore] public string EdadTexto => CalcularEdad() == null ? "—" : CalcularEdad() + " años";
        [BsonIgnore] public string CorreoTexto => string.IsNullOrWhiteSpace(correo) ? "—" : correo;
        [BsonIgnore] public string TelefonoTexto => string.IsNullOrWhiteSpace(telefono) ? "—" : telefono;
        [BsonIgnore] public string RegistroTexto => fechaRegistro.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        private int? CalcularEdad()
        {
            if (fechaNacimiento == null) return null;
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (tipoDocumento == null || string.IsNullOrWhiteSpace(tipoDocumento.codigo))
                yield return new ValidationResult("Seleccione el tipo de documento.", new[] { nameof(tipoDocumento) });
            if (fechaNacimiento != null && fechaNacimiento.Value.Date > DateTime.Today)
                yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", new[] { nameof(fechaNacimiento) });
            if (fechaNacimiento != null && fechaNacimiento.Value.Year < 1900)
                yield return new ValidationResult("La fecha de nacimiento no es válida.", new[] { nameof(fechaNacimiento) });
        }

        // Fila del historial de viajes del pasajero
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

    public class PasajeroMetadata
    {
        [Display(Name = "Número de documento")]
        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [RegularExpression(@"^[A-Za-z0-9]{5,20}$", ErrorMessage = "El documento debe tener de 5 a 20 caracteres alfanuméricos.")]
        public string numeroDocumento { get; set; } = default!;

        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[\p{L}\s'\-\.]+$", ErrorMessage = "Los nombres solo pueden contener letras.")]
        public string nombres { get; set; } = default!;

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[\p{L}\s'\-\.]+$", ErrorMessage = "Los apellidos solo pueden contener letras.")]
        public string apellidos { get; set; } = default!;

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? fechaNacimiento { get; set; }

        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(120)]
        public string? correo { get; set; }

        [Display(Name = "Teléfono")]
        [RegularExpression(@"^[0-9+\s\-]{7,15}$", ErrorMessage = "El teléfono debe tener de 7 a 15 dígitos.")]
        public string? telefono { get; set; }

        [Display(Name = "Fecha de registro")]
        [DataType(DataType.DateTime)]
        public DateTime fechaRegistro { get; set; }
    }
}
