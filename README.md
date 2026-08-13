# E-Shop — Microservicios (.NET 9) + React

Aplicación de tienda en línea (E-Shop) con arquitectura de microservicios. El flujo de compra incluye carrito por usuario, orden de compra (comprobante con subtotal, impuestos y total), estados de orden, y generación de tickets PDF.

## Arquitectura

| Servicio | Responsabilidad | Base de datos | URL pública |
|---|---|---|---|
| **Catalog.API** | Productos del catálogo | PostgreSQL | https://catalog-production-3284.up.railway.app |
| **Basket.API** | Carrito por usuario | Redis | https://basket-production-fd53.up.railway.app |
| **Ordering.API** ("Pago") | Órdenes de compra y estados | MongoDB | https://pago-production-36a1.up.railway.app |
| **Pdf.API** | Tickets PDF (QuestPDF) | — (consume a Ordering) | https://pdfapi-production-c487.up.railway.app |
| **Frontend (React + Vite)** | Interfaz | — | https://reactaplicativo.netlify.app |

```
Frontend (React / Netlify)
   ├── Catalog.API  (productos, PostgreSQL)
   ├── Basket.API   (carrito por usuario, Redis)
   ├── Ordering.API (órdenes de compra, MongoDB)  ← consume Basket y Catalog por HTTP
   └── Pdf.API      (tickets PDF, consulta a Ordering por HTTP)
```

Patrones y librerías: CQRS con MediatR, Mapster, FluentValidation, Carter (endpoints modulares), BuildingBlocks (excepciones y CORS compartidos), QuestPDF.

## Funcionalidades nuevas

- **Carrito por usuario**: 10 usuarios (comprador1…comprador10), carrito guardado en el servidor (Redis) por nombre de usuario.
- **Orden de compra (comprobante)**: al confirmar la compra, Ordering.API valida el carrito contra el catálogo, calcula subtotal, impuestos (16%) y total, y guarda la orden en MongoDB con estado `Pending`.
- **Idempotencia**: el header `Idempotency-Key` evita órdenes duplicadas (chequeo en el handler + índice único en MongoDB).
- **Vaciado del carrito**: al confirmarse la orden, Ordering.API elimina el carrito del usuario en Basket.API.
- **Estados**: `Pending → Confirmed` y `Pending → Cancelled`; transiciones inválidas devuelven 409.
- **Tickets PDF**: `Pdf.API` genera el ticket de una orden (A6) y el resumen de compras de un cliente (A5).
- **Vistas web**: Mis Pedidos y Todas las compras, con descarga de tickets.

## Ejecutar en local

Requisitos: .NET 9 SDK, Node.js 20+, Docker Desktop.

```bash
# 1) Bases de datos (PostgreSQL, Redis, MongoDB) y Pdf.API
docker compose up -d

# 2) Microservicios (cada uno en una terminal)
dotnet run --project src/Catalog.API
dotnet run --project src/Basket/Basket.API
dotnet run --project src/Ordering.API
dotnet run --project src/Pdf.API

# 3) Frontend
cd <repo del frontend: https://github.com/DxRxXgSo/Aplicativo>
npm install
npm run dev        # http://localhost:5173
```

Puertos locales: Catalog 8080, Basket 8082, Ordering 8083, Pdf 8084, Frontend 5173.

## Endpoints (Swagger/OpenAPI)

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/orders` | Crear orden (requiere header `Idempotency-Key`; 201 si se crea, 200 si ya existía) |
| GET | `/api/orders/{id}` | Consultar una orden |
| GET | `/api/orders/customer/{customerId}` | Órdenes de un cliente |
| GET | `/api/orders` | Todas las órdenes |
| PATCH | `/api/orders/{id}/status` | Cambiar estado (`Pending`→`Confirmed`/`Cancelled`; inválido → 409) |
| POST | `/api/basket` | Guardar carrito del usuario |
| GET | `/api/basket/{userName}` | Obtener carrito del usuario |
| DELETE | `/api/basket/{userName}` | Eliminar carrito |
| GET | `/api/tickets/{orderId}` | Ticket PDF de una orden |
| GET | `/api/tickets/customer/{customerId}` | Resumen PDF de un cliente |

Swagger en producción:
- https://pago-production-36a1.up.railway.app/swagger
- https://pdfapi-production-c487.up.railway.app/swagger
- https://catalog-production-3284.up.railway.app/swagger

## Configuración (variables de entorno)

Producción (Railway) — notación `Seccion__Clave`:

```
# Ordering.API (Pago)
Ordering__MongoDbConnectionString=mongodb://mongo:JFvAEhDBBKeAVsaJwmKJRSWUlbtmMYuY@mongodb.railway.internal:27017
Ordering__BasketApiBaseUrl=https://basket-production-fd53.up.railway.app
Ordering__CatalogApiBaseUrl=https://catalog-production-3284.up.railway.app
Ordering__TaxRate=0.16

# Pdf.API
Pdf__OrderingApiBaseUrl=https://pago-production-36a1.up.railway.app
Pdf__StoreName=E-Shop
Pdf__TaxRate=0.16
```

Frontend (Netlify) — variables de compilación `VITE_*`:

```
VITE_CATALOG_API_URL=https://catalog-production-3284.up.railway.app
VITE_BASKET_API_URL=https://basket-production-fd53.up.railway.app
VITE_ORDERS_API_URL=https://pago-production-36a1.up.railway.app
VITE_PDF_API_URL=https://pdfapi-production-c487.up.railway.app
```

## MongoDB: Atlas vs Railway

El desarrollo local usa MongoDB en Docker (`mongodb://localhost:27017`, base `OrderingDb`, colección `Orders`). En producción se usa el **plugin MongoDB de Railway** (`mongodb.railway.internal:27017`) porque **Atlas rechaza el handshake TLS desde la red de Railway** (error `tlsv1 alert internal error` / `SSL_ERROR_SSL`, evidenciado en los logs del servicio).

Para reproducir la estructura en **MongoDB Atlas** (base `OrderingDb`, colección `Orders`):
1. Crear un cluster M0 (gratuito) en https://cloud.mongodb.com
2. Crear usuario de base de datos y agregar `0.0.0.0/0` a las reglas de red
3. Importar los documentos de `Atlas/orders-collection-export.json` (Compass → Import Data, o `mongoimport --db OrderingDb --collection Orders atlas/orders-collection-export.json`)
4. Usar la cadena de conexión en `Ordering__MongoDbConnectionString` para ejecución local

## Evidencias de pruebas

Ver `EVIDENCIAS-PRUEBAS.md`: creación (201), consulta (200), basket vacío (404 tras la compra), idempotencia (200 con la misma orden) y cambio de estado (200 y transición inválida 409), ejecutadas contra el microservicio desplegado.

## Script de datos de prueba

`src/Ordering.API/seed-ordenes.ps1` siembra carrito → compra → estado para los 10 usuarios (órdenes Pending, Confirmed y Cancelled).