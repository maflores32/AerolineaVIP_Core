using AerolineaVIP_Core.Models;
using MongoDB.Driver;

namespace AerolineaVIP_Core.Data
{
    public class AerolineaMongoContext
    {
        public IMongoCollection<Vuelo> Vuelos { get; }
        public IMongoCollection<Aeronave> Aeronaves { get; }
        public IMongoCollection<Asignacion> Asignaciones { get; }
        public IMongoCollection<Catalogo> Catalogos { get; }
        public IMongoCollection<Pasajero> Pasajeros { get; }
        public IMongoCollection<Aeropuerto> Aeropuertos { get; }
        public IMongoCollection<Ruta> Rutas { get; }
        public IMongoCollection<Itinerario> Itinerarios { get; }
        public IMongoCollection<Reserva> Reservas { get; }
        public IMongoCollection<Compra> Compras { get; }

        public AerolineaMongoContext(IMongoDatabase database)
        {
            Vuelos = database.GetCollection<Vuelo>("vuelos");
            Aeronaves = database.GetCollection<Aeronave>("aeronaves");
            Asignaciones = database.GetCollection<Asignacion>("asignaciones");
            Catalogos = database.GetCollection<Catalogo>("catalogos");
            Pasajeros = database.GetCollection<Pasajero>("pasajeros");
            Aeropuertos = database.GetCollection<Aeropuerto>("aeropuertos");
            Rutas = database.GetCollection<Ruta>("rutas");
            Itinerarios = database.GetCollection<Itinerario>("itinerarios");
            Reservas = database.GetCollection<Reserva>("reservas");
            Compras = database.GetCollection<Compra>("compras");
        }

        // Estados de asignacion que ocupan un asiento (PENDIENTE y CONFIRMADA en el catalogo)
        public async Task<List<Asignacion>> ObtenerAsignacionesQueOcupanAsync(FilterDefinition<Asignacion> filtro)
        {
            var estadosQueOcupan = await Catalogos
                .Find(c => c.tipo == "EstadoAsignacion" && c.ocupaInventario == true)
                .Project(c => c.codigo)
                .ToListAsync();

            var filtroOcupan = Builders<Asignacion>.Filter.In(a => a.estado, estadosQueOcupan);

            return await Asignaciones
                .Find(Builders<Asignacion>.Filter.And(filtro, filtroOcupan))
                .ToListAsync();
        }
    }
}
