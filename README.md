# ASP.NET Core Mini ERP

Mini ERP construido con ASP.NET Core MVC, Entity Framework Core y SQL Server.

## Funciones actuales

- Panel con indicadores y últimos pedidos
- CRUD completo de clientes
- CRUD completo de artículos
- Búsqueda y filtro por estado activo
- Validación de NIF, NIE, CIF, correo, precio y stock
- Control de duplicados de identificación fiscal y SKU
- Bloqueo de eliminación de maestros utilizados en pedidos
- Pedidos y líneas como núcleo de dominio preparado para el siguiente módulo
- Datos de demostración idempotentes
- Pruebas automatizadas de dominio, persistencia, validaciones y controladores

## Tecnologías

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8.0.11
- SQL Server LocalDB
- Razor
- xUnit

## Ejecución

Requisitos: .NET 8 SDK, Visual Studio 2022 y SQL Server LocalDB.

    dotnet restore
    dotnet run

La aplicación aplica la migración y carga datos de ejemplo la primera vez que arranca.

## Pruebas

    dotnet test

## Estructura

- Models contiene las entidades y validaciones del dominio.
- Data contiene AppDbContext y la carga de datos de demostración.
- Controllers y Views contienen la interfaz MVC.
- Migrations contiene la evolución del esquema SQL Server.
- MiniErp.Tests contiene las pruebas automatizadas.

## Evolución prevista

1. Gestión completa de pedidos de venta
2. Proveedores y compras
3. Almacenes y movimientos de stock
4. Facturación, cobros y pagos
5. Usuarios, roles y auditoría
6. API REST, Docker, integración continua y despliegue
