# ASP.NET Core Mini ERP

Mini ERP construido con ASP.NET Core MVC, Entity Framework Core y SQL Server.

## Funciones actuales

- Panel con indicadores y últimos pedidos
- CRUD completo de clientes y artículos
- Búsqueda y filtros de maestros y pedidos
- Pedidos de venta con cabecera y líneas dinámicas
- Proveedores y pedidos de compra con recepción de existencias
- Almacenes, existencias por ubicación, entradas, salidas, transferencias y regularizaciones
- Historial trazable de movimientos y control de concurrencia
- Facturas de venta y compra, impuestos, vencimientos y pagos parciales
- Cálculos de subtotal, descuentos y total
- Flujo borrador, confirmado, completado y cancelado
- Descuento transaccional de existencias al completar
- Protección contra stock negativo, duplicados y líneas repetidas
- Validaciones de NIF, NIE, CIF, correo, precios y existencias
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
- ViewModels contiene los contratos de los formularios.
- Data contiene AppDbContext y la carga de datos de demostración.
- Controllers y Views contienen la interfaz MVC.
- Migrations contiene la evolución del esquema SQL Server.
- MiniErp.Tests contiene las pruebas automatizadas.

## Evolución prevista

1. Facturación, cobros y pagos
2. Usuarios, roles y auditoría
3. API REST, Docker, integración continua y despliegue
