# BankSystem.UI

Aplicación Avalonia (net10.0) que consume las capas `BankSystem.Domain` y `BankSystem.Services` para ofrecer una interfaz gráfica tipo banca.

## Ejecutar
1. `dotnet restore`
2. `dotnet build BankSystem_v4_Audit.sln` (o `dotnet build BankSystem.UI/BankSystem.UI.csproj`)
3. `dotnet run --project BankSystem.UI/BankSystem.UI.csproj`

## Qué probar
- **Dashboard**: ver KPIs y próximos pagos automáticos.
- **Cuentas**: seleccionar cuenta seed y probar depositar/retirar/freezar. Abrir modal de transferencia.
- **Préstamos**: crear préstamo (1/2/3) y ver tabla de amortización.
- **AutoPay**: revisar schedule seed y ejecutar hoy. Crear nuevo en el wizard.
- **Reportes**: generar CSV/PDF y e-statement.
- **Auditoría**: refrescar log en `./logs/audit.log`.
- **Admin**: crear cliente/cuenta y actualizar estado.

El seeding inicial replica el de la consola (dos clientes y cuentas). Ajusta rutas de ProjectReference si la estructura difiere.
