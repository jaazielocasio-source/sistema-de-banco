# BankSystem v4 (Audit Logger) — .NET 10

- **Audit log**: `./logs/audit.log` con timestamp UTC, acción, usuario de máquina, IP simulada, y datos sensibles enmascarados.
- **Funcionalidad**: Freeze/Unfreeze, CSV/PDF, e-statement (.eml), Factory y Strategy de intereses, operador `Money +`.
- **Monedas**: USD, EUR, JPY, KRW, BOB, GBP.

## Ejecutar
```bash
dotnet run
```

## Dónde ver el log
`./logs/audit.log` (se crea automáticamente).

## Puntos de auditoría
- Creación de cliente/cuenta/préstamo
- Depósitos, retiros, transferencias
- Cambios de estado (Active/Closed/Frozen)
- Programación y ejecución de pagos automáticos
- Exportación de reportes (CSV, PDF) y generación de e-statement
- Batch de intereses
