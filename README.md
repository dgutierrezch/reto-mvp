# Plataforma de Eventos — MVP técnico

MVP de 2 APIs .NET 9 comunicadas asíncronamente vía RabbitMQ (MassTransit), persistencia en
PostgreSQL con EF Core, cache con Redis, y un frontend React mínimo para registrar eventos.

> Arquitectura completa del sistema (visión objetivo, más allá del MVP): ver `docs/architecture.md`.

## Estructura del repositorio

```
event-platform-mvp/
├── docker-compose.yml
├── db/init.sql                      # crea las 2 bases de datos al levantar postgres
├── src/
│   ├── Shared/EventPlatform.Contracts/   # contrato del mensaje EventCreated
│   ├── EventService/                     # API 1: catálogo de eventos y zonas
│   │   ├── EventService.Domain
│   │   ├── EventService.Application
│   │   ├── EventService.Infrastructure
│   │   └── EventService.Api
│   └── NotificationService/              # API 2: consumidor + envío de correo
│       ├── NotificationService.Domain
│       ├── NotificationService.Application
│       ├── NotificationService.Infrastructure
│       └── NotificationService.Api
└── frontend/                        # React + Vite + TypeScript + Tailwind
```

Cada servicio sigue **arquitectura limpia**: `Domain` (entidades y reglas de negocio, sin
dependencias) → `Application` (casos de uso, MediatR, interfaces/puertos) → `Infrastructure`
(EF Core, MassTransit, Redis, MailKit — implementaciones concretas) → `Api` (controllers, DI, auth).

## Prerrequisitos

- Docker y Docker Compose (o Podman + podman-compose)
- .NET 9 SDK (solo si quieres correr algo fuera de Docker o generar migraciones)
- Node 18+ (solo si quieres correr el frontend fuera de Docker)

> **Nota:** este código se escribió y estructuró en un entorno sin acceso a `nuget.org`, por lo
> que no pudo compilarse ahí. Al levantarlo con Docker (que sí tiene internet completo), el
> `dotnet restore` dentro del Dockerfile debería resolver todos los paquetes sin problema. El
> frontend sí fue compilado y verificado (`npm run build` exitoso).

## 1) Generar las migraciones de EF Core (ya están generadas, pero si hubiera un problema, ejecuta estos comandos antes del primer `docker compose up`)

Los `DbContext` ya están definidos, pero las migraciones no vienen generadas en el repo (requieren
el SDK de .NET con acceso a NuGet). Genéralas así:

```bash
# EventService
cd src/EventService/EventService.Api
dotnet ef migrations add InitialCreate --project ../EventService.Infrastructure --startup-project .

# NotificationService
cd ../../NotificationService/NotificationService.Api
dotnet ef migrations add InitialCreate --project ../NotificationService.Infrastructure --startup-project .
```

Si no tienes `dotnet-ef` instalado: `dotnet tool install --global dotnet-ef`.

Ambas APIs aplican las migraciones automáticamente al iniciar (`db.Database.Migrate()` en
`Program.cs`), así que no necesitas ejecutar `dotnet ef database update` manualmente — solo
generar los archivos de migración una vez para que existan al momento del build.

## 2) Levantar todo con Docker Compose

```bash
docker compose up --build
```

Esto levanta: PostgreSQL (con las 2 bases ya creadas), Redis, RabbitMQ (panel en
http://localhost:15672, usuario/clave `guest`/`guest`), MailHog (UI en http://localhost:8025),
`api-event` (puerto 5001), `api-notifications` (puerto 5002) y el frontend (puerto 5173).

## 3) Probar el flujo end-to-end

**a) Obtener un token JWT de demo (rol Admin):**
Usa este comando si la implementación es en Linux:
```bash en Linux
curl -X POST http://localhost:5001/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","role":"Admin"}'
```

Usa este comando si la implementación es en Windows (PowerShell): 
```bash en Windows (PowerShell)
$body = @{ username = "admin"; role = "Admin" } | ConvertTo-Json
$response = Invoke-RestMethod `
    -Uri "http://localhost:5001/auth/token" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
$response | Format-List *
```

Copia el `accessToken` de la respuesta.

**b) Crear un evento (dispara `EventCreated` de forma asíncrona):**
Usa este comando si la implementación es en Linux:
```bash en Linux
curl -X POST http://localhost:5001/events \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{
    "name": "Festival de Rock 2026",
    "date": "2026-12-01T20:00:00Z",
    "location": "Estadio Nacional",
    "zones": [
      { "name": "General", "price": 50, "capacity": 5000 },
      { "name": "VIP", "price": 150, "capacity": 500 }
    ]
  }'
```

Usa este comando si la implementación es en Windows (PowerShell): 
```bash en Windows (PowerShell)
$token = "<TOKEN>"
$body = @{
    name     = "Festival de Rock 2026"
    date     = "2026-12-01T20:00:00Z"
    location = "Estadio Nacional"
    zones    = @(
        @{
            name     = "General"
            price    = 50
            capacity = 5000
        },
        @{
            name     = "VIP"
            price    = 150
            capacity = 500
        }
    )
} | ConvertTo-Json -Depth 5
$response = Invoke-RestMethod `
    -Uri "http://localhost:5001/events" `
    -Method Post `
    -ContentType "application/json" `
    -Headers @{
        Authorization = "Bearer $token"
    } `
    -Body $body
$response
```

**c) Listar eventos (con cache Redis, TTL 60s):**
Usa este comando si la implementación es en Linux:
```bash en Linux
curl http://localhost:5001/events -H "Authorization: Bearer <TOKEN>"
```

Usa este comando si la implementación es en Windows (PowerShell): 
```bash en Windows (PowerShell)
$token = "<TOKEN>"
$response = Invoke-RestMethod `
    -Uri "http://localhost:5001/events" `
    -Method Get `
    -Headers @{
        Authorization = "Bearer $token"
    }
$response
```

**d) Verificar que NotificationService procesó el evento:**
Usa este comando si la implementación es en Linux:
```bash en Linux
curl http://localhost:5002/notifications
```

Usa este comando si la implementación es en Windows (PowerShell): 
```bash en Windows (PowerShell)
$response = Invoke-RestMethod `
    -Uri "http://localhost:5002/notifications" `
    -Method GET
$response
```

Deberías ver un registro con `Status: "Processed"`. Revisa también http://localhost:8025 — ahí
aparece el correo capturado por MailHog.

**e) Usar el frontend:** copia el token del paso (a) en `frontend/.env` como `VITE_DEMO_JWT` (ver
`.env.example`), luego reconstruye el frontend `docker compose up --build frontend` y 
abre http://localhost:5173 para registrar eventos desde el formulario.

## 4) Verificar idempotencia, reintentos y DLQ

- **Idempotencia:** el índice único sobre `MessageId` en `notification_logs` impide procesar el
  mismo mensaje dos veces. Si RabbitMQ reentrega un mensaje (ej. por un ack perdido),
  `EventCreatedConsumer` lo detecta y lo omite sin duplicar el correo.
- **Reintentos:** configurados en `NotificationService.Infrastructure/DependencyInjection.cs`
  (3 intentos, 5s de intervalo). Para probarlo, detén MailHog (`docker compose stop mailhog`) antes
  de crear un evento — verás los reintentos en los logs de `api-notifications`.
- **DLQ:** tras agotar los reintentos, MassTransit mueve el mensaje a la cola
  `event-created-queue_error` (visible en el panel de RabbitMQ, http://localhost:15672 → Queues) y
  `EventCreatedFaultConsumer` registra el estado `Failed` en `notification_logs`.

## Seguridad implementada

- JWT con issuer propio (`POST /auth/token`), validado en `EventService.Api`.
- Autorización por rol: `POST /events` requiere `Admin`; `GET /events` requiere estar autenticado.
- Rate limiting fijo (30 req / 10s por IP) en `EventService.Api`.
- Manejo de errores centralizado (`ExceptionHandlingMiddleware`): nunca se expone stack trace ni
  detalles de la base de datos al cliente.
- Sin PII ni tokens en logs.
