// ==========================================================================
//  SCRIPT 1 - ESTRUCTURA DE LA BASE DE DATOS (AerolineaVIP_MongoBD)
//  Autor : Jeremy Vilaña - Grupo 03 - Desarrollo Web (ASP.NET Core)
//
//  QUE HACE: crea la base AerolineaVIP_MongoBD (borrando la anterior si
//  existe), las 11 colecciones con su validador $jsonSchema y los indices.
//  NO inserta datos (eso lo hace el Script 2).
//
//  COMO EJECUTARLO (en este orden):
//    mongosh --file 01_Estructura_AerolineaVIP_Mongo.js
//    mongosh --file 02_Datos_AerolineaVIP_Mongo.js
//  (o copiar y pegar el contenido en la pestana mongosh de MongoDB Compass)
//
// --------------------------------------------------------------------------
//  DISEÑO: TABLA SQL (AerolineaVIP, 31 tablas) -> COLECCION MONGO (11)
//
//  Tabla(s) SQL                          Coleccion      Tipo        Por que
//  ------------------------------------  -------------  ----------  --------------------------------------------
//  14 catalogos (Estado*, TipoDocumento, catalogos      Catalogo    Tablas diminutas de codigo+descripcion: una sola
//  TipoComprobante, Moneda, MetodoPago,                             coleccion con campo "tipo". Los demas documentos
//  MarcaTarjeta)                                                    guardan solo el "codigo" (ej. "PROGRAMADO").
//  Aeropuerto                            aeropuertos    Independ.   Solo 4 registros; se consulta por separado.
//  Ruta + Frecuencia                     rutas          Embebido    La ruta lleva origen y destino (aeropuertos) y
//                                                                   sus frecuencias[]: no existen sin la ruta.
//  ModeloAeronave + Asiento + Aeronave   aeronaves      Embebido    La aeronave lleva su modelo y sus asientos[]:
//                                                                   siempre se leen juntos.
//  Vuelo                                 vuelos         Embebido    Lleva ruta (con origen/destino), frecuencia y
//                                                                   aeronave: Listar/Detalle sin ningun JOIN.
//  Pasajero                              pasajeros      Independ.   Lleva su tipoDocumento embebido.
//  Itinerario + TramoItinerario          itinerarios    Emb.+Ref.   tramos[] embebidos; cada tramo referencia a
//                                                                   un vuelo (vueloId).
//  Reserva                               reservas       Independ.   Crece con el tiempo. usuarioId es el Id de
//                                                                   AspNetUsers (SQL Server), sin relacion fisica.
//  ReservaPasajeroVuelo + CheckIn        asignaciones   Ref.+Emb.   Tabla puente reserva-pasajero-vuelo; crece
//                                                                   mucho, por eso va con referencias. El check-in
//                                                                   (1:1) va embebido.
//  Compra + Pago + Comprobante           compras        Emb.+Ref.   pagos[] y comprobantes[] embebidos (pocos y
//                                                                   se leen con la compra); reservaId referencia.
//  DatosEmisor                           datosEmisor    Independ.   Un solo documento.
//
//  REGLAS COMUNES
//  - idSql  : guarda la clave primaria original de SQL Server.
//  - _id    : ObjectId fijo (lo arma la funcion oid() del Script 2) para
//             conservar las relaciones. Referencia = ObjectId del otro documento.
//  - Estados: se guardan como texto del codigo (clave natural de SQL).
//  - No se migran AspNetUsers/AspNetRoles...: el login sigue en SQL Server.
// ==========================================================================

use("AerolineaVIP_MongoBD");

// Limpia la base para poder ejecutar el script varias veces.
db.dropDatabase();

use("AerolineaVIP_MongoBD");

// --------------------------------------------------------------------------
// Tipos reutilizables para los validadores
// --------------------------------------------------------------------------
const tOid     = { bsonType: "objectId" };
const tTexto   = { bsonType: "string" };
const tTextoN  = { bsonType: ["string", "null"] };
const tNumero  = { bsonType: "number" };
const tEntero  = { bsonType: "int" };
const tBool    = { bsonType: "bool" };
const tFecha   = { bsonType: "date" };
const tFechaN  = { bsonType: ["date", "null"] };

// Aeropuerto resumido que se embebe dentro de rutas y vuelos
const tAeropuertoEmb = {
  bsonType: "object",
  required: ["aeropuertoId", "codigoIATA", "nombre", "localidad"],
  properties: { aeropuertoId: tOid, codigoIATA: tTexto, nombre: tTexto, localidad: tTexto }
};

// --------------------------------------------------------------------------
// 1. catalogos
// --------------------------------------------------------------------------
db.createCollection("catalogos", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "tipo", "codigo", "descripcion"],
    properties: {
      _id: tOid, tipo: tTexto, codigo: tTexto, descripcion: tTexto,
      ocupaInventario: tBool, estadoVisual: tTexto, activo: tBool, activa: tBool,
      longitudNumero: tNumero, longitudCodigoSeguridad: tNumero,
      prefijosReferencia: tTexto, rutaLogo: tTexto
    }
  } }
});

// --------------------------------------------------------------------------
// 2. aeropuertos
// --------------------------------------------------------------------------
db.createCollection("aeropuertos", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "codigoIATA", "nombre", "localidad", "activo"],
    properties: {
      _id: tOid, idSql: tNumero, codigoIATA: tTexto, nombre: tTexto,
      localidad: tTexto, activo: tBool
    }
  } }
});

// --------------------------------------------------------------------------
// 3. rutas (origen, destino y frecuencias embebidos)
// --------------------------------------------------------------------------
db.createCollection("rutas", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "origen", "destino", "tarifaReferencial", "activa", "frecuencias"],
    properties: {
      _id: tOid, idSql: tNumero,
      origen: tAeropuertoEmb, destino: tAeropuertoEmb,
      tarifaReferencial: tNumero, activa: tBool,
      frecuencias: {
        bsonType: "array",
        items: {
          bsonType: "object",
          required: ["idSql", "diaSemana", "horaSalida", "horaLlegada", "activa"],
          properties: {
            idSql: tNumero,
            diaSemana: { bsonType: "number", minimum: 1, maximum: 7, description: "1 = lunes ... 7 = domingo" },
            horaSalida: tTexto, horaLlegada: tTexto, activa: tBool
          }
        }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 4. aeronaves (modelo y asientos embebidos)
// --------------------------------------------------------------------------
db.createCollection("aeronaves", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "matricula", "estado", "modelo", "asientos"],
    properties: {
      _id: tOid, idSql: tNumero, matricula: tTexto, estado: tTexto,
      modelo: {
        bsonType: "object",
        required: ["idSql", "fabricante", "nombre", "capacidadAsientos"],
        properties: { idSql: tNumero, fabricante: tTexto, nombre: tTexto, capacidadAsientos: tNumero }
      },
      asientos: {
        bsonType: "array",
        items: {
          bsonType: "object",
          required: ["numeroAsiento", "codigo", "fila", "lado", "ubicacion"],
          properties: {
            numeroAsiento: tNumero, codigo: tTexto, fila: tNumero, lado: tTexto, ubicacion: tTexto
          }
        }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 5. vuelos (ruta, aeropuertos, frecuencia y aeronave embebidos)
// --------------------------------------------------------------------------
db.createCollection("vuelos", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "fechaOperacion", "estado", "ruta", "frecuencia", "aeronave"],
    properties: {
      _id: tOid, idSql: tNumero, fechaOperacion: tFecha, estado: tTexto,
      ruta: {
        bsonType: "object",
        required: ["rutaId", "tarifaReferencial", "origen", "destino"],
        properties: { rutaId: tOid, tarifaReferencial: tNumero, origen: tAeropuertoEmb, destino: tAeropuertoEmb }
      },
      frecuencia: {
        bsonType: "object",
        required: ["idSql", "diaSemana", "horaSalida", "horaLlegada"],
        properties: { idSql: tNumero, diaSemana: tNumero, horaSalida: tTexto, horaLlegada: tTexto }
      },
      aeronave: {
        bsonType: ["object", "null"],
        description: "Puede ser null si el vuelo aun no tiene aeronave asignada.",
        required: ["aeronaveId", "matricula", "modelo", "fabricante", "capacidadAsientos"],
        properties: {
          aeronaveId: tOid, matricula: tTexto, modelo: tTexto, fabricante: tTexto, capacidadAsientos: tNumero
        }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 6. pasajeros
// --------------------------------------------------------------------------
db.createCollection("pasajeros", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "tipoDocumento", "numeroDocumento", "nombres", "apellidos", "fechaRegistro"],
    properties: {
      _id: tOid, idSql: tNumero,
      tipoDocumento: {
        bsonType: "object",
        required: ["codigo", "descripcion"],
        properties: { codigo: tTexto, descripcion: tTexto }
      },
      numeroDocumento: tTexto, nombres: tTexto, apellidos: tTexto,
      fechaNacimiento: tFechaN, correo: tTextoN, telefono: tTextoN, fechaRegistro: tFecha
    }
  } }
});

// --------------------------------------------------------------------------
// 7. itinerarios (tramos embebidos; vueloId es referencia a vuelos)
// --------------------------------------------------------------------------
db.createCollection("itinerarios", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "referencia", "nombre", "estado", "tramos"],
    properties: {
      _id: tOid, idSql: tNumero, referencia: tTexto, nombre: tTexto, estado: tTexto,
      tramos: {
        bsonType: "array",
        items: {
          bsonType: "object",
          required: ["idSql", "orden", "vueloId"],
          properties: { idSql: tNumero, orden: tNumero, vueloId: tOid }
        }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 8. reservas
// --------------------------------------------------------------------------
db.createCollection("reservas", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "usuarioId", "fechaCreacion", "estado"],
    properties: {
      _id: tOid, idSql: tNumero, codigoReserva: tTextoN,
      usuarioId: { bsonType: "string", description: "Id del usuario en AspNetUsers (SQL Server)." },
      fechaCreacion: tFecha, fechaExpiracion: tFechaN, estado: tTexto
    }
  } }
});

// --------------------------------------------------------------------------
// 9. asignaciones (reserva + pasajero + vuelo; check-in embebido)
// --------------------------------------------------------------------------
db.createCollection("asignaciones", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "reservaId", "pasajeroId", "vueloId", "numeroAsiento", "importeTramo", "estado", "fechaAsignacion", "checkIn"],
    properties: {
      _id: tOid, idSql: tNumero,
      reservaId: tOid, pasajeroId: tOid, vueloId: tOid,
      numeroAsiento: tNumero, importeTramo: tNumero, estado: tTexto, fechaAsignacion: tFecha,
      checkIn: {
        bsonType: ["object", "null"],
        required: ["idSql", "fechaRegistro", "estado"],
        properties: { idSql: tNumero, fechaRegistro: tFecha, estado: tTexto }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 10. compras (pagos y comprobantes embebidos)
// --------------------------------------------------------------------------
db.createCollection("compras", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "reservaId", "fechaCompra", "importe", "moneda", "estado", "claveIdempotencia", "pagos", "comprobantes"],
    properties: {
      _id: tOid, idSql: tNumero, reservaId: tOid, fechaCompra: tFecha,
      importe: tNumero, moneda: tTexto, estado: tTexto, claveIdempotencia: tTexto,
      pagos: {
        bsonType: "array",
        items: {
          bsonType: "object",
          required: ["idSql", "fechaPago", "importe", "estado", "metodoPago", "marcaTarjeta", "ultimosCuatroDigitos", "codigoOperacion", "esSimulado"],
          properties: {
            idSql: tNumero, fechaPago: tFecha, importe: tNumero, estado: tTexto,
            metodoPago: tTexto, marcaTarjeta: tTexto, ultimosCuatroDigitos: tTexto,
            nombreTitular: tTextoN, codigoOperacion: tTexto, esSimulado: tBool, mensajeResultado: tTextoN
          }
        }
      },
      comprobantes: {
        bsonType: "array",
        items: {
          bsonType: "object",
          required: ["idSql", "tipo", "numero", "fechaEmision", "estado"],
          properties: { idSql: tNumero, tipo: tTexto, numero: tTexto, fechaEmision: tFecha, estado: tTexto }
        }
      }
    }
  } }
});

// --------------------------------------------------------------------------
// 11. datosEmisor
// --------------------------------------------------------------------------
db.createCollection("datosEmisor", {
  validator: { $jsonSchema: {
    bsonType: "object",
    required: ["_id", "idSql", "nombreComercial", "identificacion", "direccion", "leyendaComprobante"],
    properties: {
      _id: tOid, idSql: tNumero, nombreComercial: tTexto, identificacion: tTexto,
      direccion: tTexto, telefono: tTextoN, correo: tTextoN, leyendaComprobante: tTexto
    }
  } }
});

// ==========================================================================
//  INDICES
// ==========================================================================
db.catalogos.createIndex({ tipo: 1, codigo: 1 }, { unique: true });
db.aeropuertos.createIndex({ idSql: 1 }, { unique: true });
db.aeropuertos.createIndex({ codigoIATA: 1 }, { unique: true });
db.rutas.createIndex({ idSql: 1 }, { unique: true });
db.aeronaves.createIndex({ idSql: 1 }, { unique: true });
db.aeronaves.createIndex({ matricula: 1 }, { unique: true });
db.vuelos.createIndex({ idSql: 1 }, { unique: true });
db.vuelos.createIndex({ fechaOperacion: 1 });
db.pasajeros.createIndex({ idSql: 1 }, { unique: true });
db.pasajeros.createIndex({ "tipoDocumento.codigo": 1, numeroDocumento: 1 }, { unique: true });
db.itinerarios.createIndex({ idSql: 1 }, { unique: true });
db.itinerarios.createIndex({ referencia: 1 }, { unique: true });
db.reservas.createIndex({ idSql: 1 }, { unique: true });
db.reservas.createIndex({ usuarioId: 1 });
db.asignaciones.createIndex({ idSql: 1 }, { unique: true });
db.asignaciones.createIndex({ vueloId: 1 });
db.asignaciones.createIndex({ pasajeroId: 1 });
db.asignaciones.createIndex({ reservaId: 1 });
db.compras.createIndex({ idSql: 1 }, { unique: true });
db.compras.createIndex({ reservaId: 1 });
db.datosEmisor.createIndex({ idSql: 1 }, { unique: true });

print("Base AerolineaVIP_MongoBD creada: " + db.getCollectionNames().length + " colecciones.");
print(db.getCollectionNames().join(", "));
