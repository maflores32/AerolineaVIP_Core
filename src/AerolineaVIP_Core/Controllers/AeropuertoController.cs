using AerolineaVIP_Core.Data;
using AerolineaVIP_Core.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AerolineaVIP_Core.Controllers
{
    public class AeropuertoController : Controller
    {
        private readonly AerolineaMongoContext _db;
        private readonly ILogger<AeropuertoController> _logger;

        public AeropuertoController(AerolineaMongoContext db, ILogger<AeropuertoController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: Aeropuertos
        public async Task<IActionResult> Index()
        {
            var lista = await _db.Aeropuertos
                .Find(_ => true)
                .SortBy(a => a.localidad).ThenBy(a => a.codigoIATA)
                .ToListAsync();
            return View(lista);
        }

        // GET: Aeropuertos/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var aeropuerto = await BuscarAsync(id);
            if (aeropuerto == null) return NotFound();
            return View(aeropuerto);
        }

        // GET: Aeropuertos/Create
        public IActionResult Create()
        {
            return View(new Aeropuerto { activo = true });
        }

        // POST: Aeropuertos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("codigoIATA,nombre,localidad")] Aeropuerto aeropuerto)
        {
            Normalizar(aeropuerto);
            await ValidarUnicidadAsync(aeropuerto, null);

            if (!ModelState.IsValid) return View(aeropuerto);

            try
            {
                // idSql es obligatorio y único en Mongo: se toma el siguiente número disponible
                var ultimo = await _db.Aeropuertos.Find(_ => true).SortByDescending(a => a.idSql).Limit(1).FirstOrDefaultAsync();
                aeropuerto.idSql = (ultimo?.idSql ?? 0) + 1;
                aeropuerto.activo = true;

                await _db.Aeropuertos.InsertOneAsync(aeropuerto);
                TempData["Mensaje"] = "Aeropuerto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                ModelState.AddModelError(nameof(Aeropuerto.codigoIATA), "Ya existe un aeropuerto con ese código IATA.");
                return View(aeropuerto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el aeropuerto.");
                ModelState.AddModelError(string.Empty, "No se pudo crear el aeropuerto. Intente nuevamente.");
                return View(aeropuerto);
            }
        }

        // GET: Aeropuertos/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var aeropuerto = await BuscarAsync(id);
            if (aeropuerto == null) return NotFound();
            return View(aeropuerto);
        }

        // POST: Aeropuertos/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("_id,codigoIATA,nombre,localidad,activo")] Aeropuerto aeropuerto)
        {
            if (id != aeropuerto._id || !ObjectId.TryParse(id, out _)) return NotFound();

            Normalizar(aeropuerto);
            await ValidarUnicidadAsync(aeropuerto, id);

            if (!ModelState.IsValid) return View(aeropuerto);

            try
            {
                var cambios = Builders<Aeropuerto>.Update
                    .Set(a => a.codigoIATA, aeropuerto.codigoIATA)
                    .Set(a => a.nombre, aeropuerto.nombre)
                    .Set(a => a.localidad, aeropuerto.localidad)
                    .Set(a => a.activo, aeropuerto.activo);

                var resultado = await _db.Aeropuertos.UpdateOneAsync(a => a._id == id, cambios);
                if (resultado.MatchedCount == 0) return NotFound();

                await SincronizarResumenesAsync(aeropuerto);

                TempData["Mensaje"] = "Aeropuerto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                ModelState.AddModelError(nameof(Aeropuerto.codigoIATA), "Ya existe un aeropuerto con ese código IATA.");
                return View(aeropuerto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el aeropuerto.");
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el aeropuerto. Intente nuevamente.");
                return View(aeropuerto);
            }
        }

        // GET: Aeropuertos/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var aeropuerto = await BuscarAsync(id);
            if (aeropuerto == null) return NotFound();
            return View(aeropuerto);
        }

        // POST: Aeropuertos/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var aeropuerto = await BuscarAsync(id);
            if (aeropuerto == null) return NotFound();

            // Regla: no se elimina un aeropuerto que ya está usado en una ruta
            var enUso = await _db.Rutas
                .Find(r => r.origen.aeropuertoId == id || r.destino.aeropuertoId == id)
                .AnyAsync();
            if (enUso)
            {
                TempData["Error"] = "El aeropuerto está asociado a una ruta. Puede desactivarlo desde Editar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _db.Aeropuertos.DeleteOneAsync(a => a._id == id);
                TempData["Mensaje"] = "Aeropuerto eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el aeropuerto.");
                TempData["Error"] = "No se pudo eliminar el aeropuerto. Intente nuevamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Métodos privados ----------------

        private async Task<Aeropuerto?> BuscarAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _)) return null;
            return await _db.Aeropuertos.Find(a => a._id == id).FirstOrDefaultAsync();
        }

        private static void Normalizar(Aeropuerto a)
        {
            a.codigoIATA = (a.codigoIATA ?? "").Trim().ToUpperInvariant();
            a.nombre = (a.nombre ?? "").Trim();
            a.localidad = (a.localidad ?? "").Trim();
        }

        // Unicidad del código IATA (necesita consultar Mongo, por eso no va en la partial)
        private async Task ValidarUnicidadAsync(Aeropuerto a, string? idActual)
        {
            if (string.IsNullOrEmpty(a.codigoIATA)) return;
            var existe = await _db.Aeropuertos
                .Find(x => x.codigoIATA == a.codigoIATA && x._id != idActual)
                .AnyAsync();
            if (existe)
                ModelState.AddModelError(nameof(Aeropuerto.codigoIATA), "Ya existe un aeropuerto con ese código IATA.");
        }

        // Rutas y vuelos guardan una copia resumida del aeropuerto; se actualiza para que no quede desfasada
        private async Task SincronizarResumenesAsync(Aeropuerto a)
        {
            await _db.Rutas.UpdateManyAsync(
                r => r.origen.aeropuertoId == a._id,
                Builders<Ruta>.Update.Combine(
                    Builders<Ruta>.Update.Set(r => r.origen.codigoIATA, a.codigoIATA),
                    Builders<Ruta>.Update.Set(r => r.origen.nombre, a.nombre),
                    Builders<Ruta>.Update.Set(r => r.origen.localidad, a.localidad)));

            await _db.Rutas.UpdateManyAsync(
                r => r.destino.aeropuertoId == a._id,
                Builders<Ruta>.Update.Combine(
                    Builders<Ruta>.Update.Set(r => r.destino.codigoIATA, a.codigoIATA),
                    Builders<Ruta>.Update.Set(r => r.destino.nombre, a.nombre),
                    Builders<Ruta>.Update.Set(r => r.destino.localidad, a.localidad)));

            await _db.Vuelos.UpdateManyAsync(
                v => v.ruta.origen.aeropuertoId == a._id,
                Builders<Vuelo>.Update.Combine(
                    Builders<Vuelo>.Update.Set(v => v.ruta.origen.codigoIATA, a.codigoIATA),
                    Builders<Vuelo>.Update.Set(v => v.ruta.origen.nombre, a.nombre),
                    Builders<Vuelo>.Update.Set(v => v.ruta.origen.localidad, a.localidad)));

            await _db.Vuelos.UpdateManyAsync(
                v => v.ruta.destino.aeropuertoId == a._id,
                Builders<Vuelo>.Update.Combine(
                    Builders<Vuelo>.Update.Set(v => v.ruta.destino.codigoIATA, a.codigoIATA),
                    Builders<Vuelo>.Update.Set(v => v.ruta.destino.nombre, a.nombre),
                    Builders<Vuelo>.Update.Set(v => v.ruta.destino.localidad, a.localidad)));
        }
    }
}
