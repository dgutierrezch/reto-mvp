-- Se ejecuta automáticamente al levantar el contenedor de Postgres (docker-entrypoint-initdb.d).
-- Crea las dos bases de datos, una por microservicio (database-per-service).
-- Las tablas dentro de cada una las crea EF Core via migraciones al iniciar cada API.
CREATE DATABASE eventdb;
CREATE DATABASE notificationdb;
