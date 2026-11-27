using System.Globalization;

namespace AerolineaVIP_Core.Models
{
    public partial class Aeropuerto
    {
        public string EstadoTexto => activo ? "Activo" : "Inactivo";
    }

    public partial class Ruta
    {
        public string RutaTexto => origen.codigoIATA + " → " + destino.codigoIATA;
        public string PrecioTexto => "$ " + tarifaReferencial.ToString("N2", CultureInfo.InvariantCulture);
        public int FrecuenciasActivas => frecuencias.Count(f => f.activa);
        public string EstadoTexto => activa ? "Activa" : "Inactiva";
    }

    public partial class Itinerario
    {
        public int NumeroTramos => tramos.Count;
    }

    public partial class Reserva
    {
        public string CodigoTexto => string.IsNullOrEmpty(codigoReserva) ? "Sin codigo" : codigoReserva;
        public string FechaTexto => fechaCreacion.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        public string ExpiraTexto => fechaExpiracion == null ? "No expira" : fechaExpiracion.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    public partial class Compra
    {
        public string FechaTexto => fechaCompra.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        public string ImporteTexto => "$ " + importe.ToString("N2", CultureInfo.InvariantCulture) + " " + moneda;
    }
}
