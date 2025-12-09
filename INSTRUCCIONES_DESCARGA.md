# 📥 Cómo Descargar y Ejecutar BankSystem

## 🎮 Opción 1: Ejecutable Directo (MÁS FÁCIL)

### Para Windows:

1. Ve a la sección **Releases** en GitHub:
   https://github.com/jaazielocasio-source/sistema-de-banco/releases

2. Descarga el archivo `BankSystem.UI.exe` (aprox. 78 MB)

3. Haz doble clic en `BankSystem.UI.exe` para ejecutar

4. ¡Listo! La aplicación se abrirá automáticamente

**Nota**: Windows puede mostrar una advertencia de seguridad. Haz clic en "Más información" → "Ejecutar de todas formas"

---

## 💻 Opción 2: Desde el Código Fuente

### Requisitos:
- [Git](https://git-scm.com/download/win)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Pasos:

```powershell
# 1. Clonar el repositorio
git clone https://github.com/jaazielocasio-source/sistema-de-banco.git
cd sistema-de-banco

# 2. Ejecutar la interfaz de Avalonia
dotnet run --project BankSystem.UI\BankSystem.UI.csproj
```

---

## 📸 Screenshots

### Dashboard
![Dashboard con tarjetas estadísticas y acciones rápidas]

### Mis Cuentas
![Vista de tarjetas bancarias con gradientes modernos]

### Panel de Admin
![Formularios para crear clientes y cuentas]

---

## ⚙️ Características

- ✅ Interfaz moderna inspirada en Discovery
- ✅ Gestión de cuentas bancarias
- ✅ Desactivar/Activar tarjetas (Freeze/Unfreeze)
- ✅ Transferencias entre cuentas
- ✅ Sistema de préstamos
- ✅ Pagos automáticos
- ✅ Auditoría completa de transacciones
- ✅ Reportes en CSV/PDF

---

## 🐛 Problemas?

Si tienes problemas, abre un [Issue](https://github.com/jaazielocasio-source/sistema-de-banco/issues)
