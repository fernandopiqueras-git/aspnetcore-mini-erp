# ASP.NET Core Mini ERP

Mini ERP construido con ASP.NET Core MVC, Entity Framework Core y SQL Server.

## Primer módulo

- Clientes con identificación fiscal y datos de contacto
- Artículos con SKU, precio y stock
- Pedidos de venta con cliente, fecha y estado
- Líneas de pedido con cantidad, precio, descuento y total calculado
- Relaciones y restricciones configuradas con Entity Framework Core
- Migración inicial para SQL Server LocalDB
- Datos de demostración idempotentes
- Panel inicial de indicadores y últimos pedidos
- Pruebas de validación, relaciones, índices, cálculos y datos iniciales

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

1. CRUD completo de clientes y artículos
2. Gestión completa de pedidos de venta
3. Proveedores y compras
4. Almacenes y movimientos de stock
5. Facturación, cobros y pagos
6. Usuarios, roles y auditoría
7. API REST, Docker, integración continua y despliegue
