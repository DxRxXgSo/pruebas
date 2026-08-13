# Ordering.API — Microservicio de Órdenes de Compra

Microservicio de órdenes de compra construido con **ASP.NET Core Minimal API (.NET 9)** y **MongoDB Atlas**, integrado por HTTP con los microservicios existentes **Basket.API** (carrito en Redis) y **Catalog.API** (catálogo en PostgreSQL/Marten).

## Arquitectura

```
┌────────────┐   HTTP    ┌──────────────┐   HTTP    ┌─────────────┐
│ Basket.API │◄──────────│ Ordering.API │──────────►│ Catalog.API │
│  (Redis)   │  GET basket│  (MongoDB   │  GET prod │  (Postgres) │
└────────────┘           │   Atlas)    │           └─────────────┘
                         └──────────────┘
```

Separación de responsabilidades:

```
src/Ordering.API/
├── Program.cs                  # Composición (Minimal API)
├── Domain/                     # Order, OrderItem, OrderStatus
├── Application/
│   ├── Contracts/              # IOrderRepository, IBasketApiClient, ICatalogApiClient
│   ├── Integration/            # BasketApiClient, CatalogApiClient (HTTP)
│   └── Orders/                 # CreateOrder, GetOrderById, GetOrdersByCustomer, UpdateOrderStatus
├── Infrastructure/
│   ├── Configuration/          # OrderingSettings (env vars)
│   └── Persistence/            # OrderingDbContext, MongoDbOrderRepository
└── Endpoints/                  # Mapa de endpoints Minimal API
```

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/orders` | Crea una orden de compra. **Header `Idempotency-Key` obligatorio**. Body: `{ customerId, basketId }`. Respuesta: `201 Created` (nueva) o `200 OK` (solicitud repetida con la misma clave → devuelve la orden previa, sin duplicados). |
| `GET` | `/api/orders/{id}` | Recupera una orden por su identificador único. `404` si no existe. |
| `GET` | `/api/orders/customer/{customerId}` | Lista las órdenes de un cliente. |
| `PATCH` | `/api/orders/{id}/status` | Actualiza el estado validando transiciones: `Pending → Confirmed`, `Pending → Cancelled`. Transición inválida → `409 Conflict`. |

Swagger/OpenAPI disponible en `/swagger`.

### Modelo de datos

**Order**: `Id` (GUID), `CustomerId`, `IdempotencyKey`, `CreatedAt` (UTC), `Status` (`Pending`/`Confirmed`/`Cancelled`), `Items`, `Subtotal`, `Tax`, `Total`.

**OrderItem**: `ProductId`, `ProductName`, `Quantity`, `UnitPrice` (precio al momento de comprar), `LineTotal`.

Cálculos: `LineTotal = Quantity × UnitPrice`, `Subtotal = Σ LineTotal`, `Tax = Subtotal × TaxRate` (16% configurable), `Total = Subtotal + Tax`.

## Reglas de negocio

- **Basket vacío o inexistente** → `400 Bad Request`.
- **Producto inexistente en el catálogo** o datos inconsistentes (cantidad/precio inválidos) → `400 Bad Request`.
- **Idempotencia**: si se reenvía la misma solicitud con la misma `Idempotency-Key`, no se crea una segunda orden y se devuelve la orden previamente generada. Existe un **índice único** en `IdempotencyKey` en MongoDB.
- **Ciclo de vida**: `Pending → Confirmed`, `Pending → Cancelled`. Una orden `Cancelled` no puede volver a `Confirmed`.
- **Error de persistencia o de integración** → `500 Internal Server Error` con mensaje genérico (sin stack traces ni información sensible).

## Configuración (sin secretos en el repositorio)

Toda la configuración sensible se lee de variables de entorno o user-secrets:

| Variable | Descripción |
|---|---|
| `Ordering__MongoDbConnectionString` | Cadena de conexión de MongoDB Atlas (**obligatoria**). |
| `Ordering__MongoDbDatabaseName` | Nombre de la base de datos (default: `OrderingDb`). |
| `Ordering__BasketApiBaseUrl` | URL base de Basket.API (default: `http://localhost:8082`). |
| `Ordering__CatalogApiBaseUrl` | URL base de Catalog.API (default: `http://localhost:8080`). |
| `Ordering__TaxRate` | Tasa de impuestos (default: `0.16`). |

### Ejecución local

```bash
# 1. Configurar la cadena de MongoDB Atlas (no se guarda en git)
dotnet user-secrets set "Ordering:MongoDbConnectionString" "mongodb+srv://<usuario>:<password>@<cluster>.mongodb.net/?retryWrites=true&w=majority" --project src/Ordering.API

# 2. Levantar Basket.API y Catalog.API (opcional si ya están publicados)
docker compose up --build basket.api catalog.api

# 3. Ejecutar
dotnet run --project src/Ordering.API
```

El servicio arranca en `http://localhost:8080` (perfil de desarrollo) y crea automáticamente los índices (incluido el índice único de `IdempotencyKey`).

### Docker Compose

```bash
export MONGODB_CONNECTION_STRING="mongodb+srv://<usuario>:<password>@<cluster>.mongodb.net/?retryWrites=true&w=majority"
docker compose up --build ordering.api
```

## Ejemplo de uso

```bash
# Crear orden (201 la primera vez)
curl -i -X POST http://localhost:8083/api/orders \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: 11111111-1111-1111-1111-111111111111" \
  -d '{"customerId":"comprador1","basketId":"comprador1"}'

# Repetir la misma solicitud (200, misma orden, sin duplicado)
curl -i -X POST http://localhost:8083/api/orders \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: 11111111-1111-1111-1111-111111111111" \
  -d '{"customerId":"comprador1","basketId":"comprador1"}'

# Consultar orden
curl http://localhost:8083/api/orders/<id>

# Órdenes por cliente
curl http://localhost:8083/api/orders/customer/comprador1

# Cambiar estado (Pending → Confirmed)
curl -i -X PATCH http://localhost:8083/api/orders/<id>/status \
  -H "Content-Type: application/json" \
  -d '{"status":"Confirmed"}'
```

## Frontend (Aplicativo / React)

La tienda React integra el flujo: catálogo → carrito → **Finalizar compra** (ruta `/checkout`) → confirmación visible de la orden. La URL del servicio de órdenes se configura con `VITE_ORDERS_API_URL` (default `http://localhost:8083`).

```bash
VITE_ORDERS_API_URL=https://<tu-servicio-publicado>.up.railway.app npm run dev
```