using BankSystem.Domain;
using BankSystem.Services;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BankSystem;

public class Program
{
    private static BankService _bank = new();
    private static SchedulerService _scheduler = new();

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        AuditLogger.Init("./logs/audit.log"); // inicializa ruta de log
        SeedSampleData();
        MainMenu();
    }

    static void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========== BANCO - MENÚ PRINCIPAL ==========");
            Console.WriteLine("1) Clientes");
            Console.WriteLine("2) Cuentas");
            Console.WriteLine("3) Transacciones");
            Console.WriteLine("4) Préstamos");
            Console.WriteLine("5) Pagos Automáticos");
            Console.WriteLine("6) Reportes / E-Statements");
            Console.WriteLine("0) Salir");
            Console.Write("Opción: ");
            switch (Console.ReadLine())
            {
                case "1": ClientesMenu(); break;
                case "2": CuentasMenu(); break;
                case "3": TransaccionesMenu(); break;
                case "4": PrestamosMenu(); break;
                case "5": PagosAutoMenu(); break;
                case "6": ReportesMenu(); break;
                case "0": AuditLogger.Log("APP.EXIT", "Aplicación cerrada por usuario"); return;
                default: Pause("Inválida"); break;
            }
        }
    }

    static void ClientesMenu()
    {
        Console.Clear();
        Console.WriteLine("-- CLIENTES --");
        Console.Write("Nombre: "); var name = Console.ReadLine() ?? "";
        var email = ReadEmail();
        var phone = ReadPhoneNumber();
        Console.Write("ID (se enmascara): "); var gid = Console.ReadLine() ?? "";
        var c = _bank.CreateCustomer(name, email, phone, gid);
        Pause($"Cliente creado Id={c.Id}");
    }

    static void CuentasMenu()
    {
        Console.Clear();
        Console.WriteLine("-- CUENTAS --");
        Console.Write("Id cliente: "); if (!int.TryParse(Console.ReadLine(), out int id)) { Pause("Inválido"); return; }
        Console.Write("Tipo (1=Ahorro, 2=Corriente, 3=CD): "); var t = Console.ReadLine() ?? "";
        var cur = ReadCurrency();
        var acc = AccountFactory.CreateAccount(id, t, cur);
        if (acc == null) { Pause("Tipo inválido"); return; }
        _bank.Accounts.Add(acc);
        AuditLogger.Log("ACCOUNT.CREATE", $"Cuenta {acc.Number} creada para cliente {id} ({cur})");
        Console.Write("¿Freeze por fraude? (s/n): "); if ((Console.ReadLine() ?? "n").ToLower() == "s") _bank.FreezeAccount(acc.Number);
        Pause($"Cuenta {acc.Number} creada");
    }

    static void TransaccionesMenu()
    {
        Console.Clear();
        Console.WriteLine("-- TRANSACCIONES --");
        Console.Write("1) Depósito  2) Retiro  3) Transferencia  4) Freeze/Unfreeze: ");
        var op = Console.ReadLine();
        if (op == "1") { Console.Write("Cuenta: "); var n = Console.ReadLine() ?? ""; Console.Write("Monto: "); if (decimal.TryParse(Console.ReadLine(), out var m)) Pause(_bank.Deposit(n, m) ? "OK" : "Falló"); }
        else if (op == "2") { Console.Write("Cuenta: "); var n = Console.ReadLine() ?? ""; Console.Write("Monto: "); if (decimal.TryParse(Console.ReadLine(), out var m)) Pause(_bank.Withdraw(n, m) ? "OK" : "Falló"); }
        else if (op == "3") { Console.Write("Origen: "); var a = Console.ReadLine() ?? ""; Console.Write("Destino: "); var b = Console.ReadLine() ?? ""; Console.Write("Monto: "); if (decimal.TryParse(Console.ReadLine(), out var m)) Pause(_bank.Transfer(a, b, m) ? "OK" : "Falló"); }
        else if (op == "4") { Console.Write("Cuenta: "); var n = Console.ReadLine() ?? ""; Console.Write("(f)reeze / (u)nfreeze: "); var s = (Console.ReadLine() ?? "").ToLower(); Pause((s == "f" ? _bank.FreezeAccount(n) : s == "u" ? _bank.UnfreezeAccount(n) : false) ? "OK" : "Falló"); }
    }

    static void PrestamosMenu()
    {
        Console.Clear();
        Console.WriteLine("-- PRÉSTAMOS --");
        Console.Write("Id cliente: "); if (!int.TryParse(Console.ReadLine(), out int id)) { Pause("Inválido"); return; }
        var t = ReadLoanType();
        Console.Write("Principal: "); if (!decimal.TryParse(Console.ReadLine(), out var p)) { Pause("Inválido"); return; }
        Console.Write("Meses: "); if (!int.TryParse(Console.ReadLine(), out var m)) { Pause("Inválido"); return; }
        var cur = ReadCurrency();
        var loan = LoanFactory.CreateLoan(id, t, p, m, cur);
        if (loan == null) { Pause("Tipo inválido"); return; }
        _bank.Loans.Add(loan);
        AuditLogger.Log("LOAN.CREATE", $"Loan {loan.LoanId} ({loan.GetType().Name}) {cur} {loan.Principal} plazo {loan.TermMonths}m para cliente {id}");
        Pause($"Loan {loan.LoanId} creado");
    }

    static void PagosAutoMenu()
    {
        Console.Clear();
        Console.WriteLine("-- PAGOS AUTO --");
        Console.Write("Cuenta origen: "); var a = Console.ReadLine() ?? "";
        Console.Write("Destino (Cuenta o LoanId): "); var d = Console.ReadLine() ?? "";
        Console.Write("Monto: "); if (!decimal.TryParse(Console.ReadLine(), out var m)) { Pause("Inválido"); return; }
        Console.Write("Periodicidad (Daily/Monthly): "); var per = Console.ReadLine() ?? "Monthly";
        Pause(_bank.SchedulePayment(a, d, m, per) ? "Programado" : "Falló");
    }

    static void ReportesMenu()
    {
        Console.Clear();
        Console.WriteLine("-- REPORTES / E-STATEMENTS --");
        Console.WriteLine("1) Extracto mensual (CSV + PDF + E-Statement)");
        Console.WriteLine("2) Exportar clientes a Excel (CSV)");
        Console.Write("Opción: ");
        var op = Console.ReadLine();
        if (op == "1")
        {
            Console.Write("Cuenta #: "); var n = Console.ReadLine() ?? "";
            Console.Write("Mes (1-12): "); if (!int.TryParse(Console.ReadLine(), out var mo)) { Pause("Inválido"); return; }
            Console.Write("Año: "); if (!int.TryParse(Console.ReadLine(), out var y)) { Pause("Inválido"); return; }
            var csv = ReportService.ExportMonthlyStatementCSV(_bank, n, mo, y);
            var pdf = ReportService.ExportMonthlyStatementPDF(_bank, n, mo, y);
            var sent = (pdf != null) && ReportService.SendMonthlyEStatement(_bank, n, mo, y, pdf);
            Pause($"CSV: {csv}\nPDF: {pdf}\nE-Statement: {sent}");
        }
        else if (op == "2")
        {
            var path = ReportService.ExportAllCustomersCsv(_bank);
            Pause($"Clientes exportados a: {path}");
        }
        else
        {
            Pause("Opción inválida");
        }
    }

    static void Pause(string? msg = null) { if (!string.IsNullOrEmpty(msg)) Console.WriteLine(msg); Console.WriteLine("Presione tecla..."); Console.ReadKey(); }

    static string ReadEmail()
    {
        while (true)
        {
            Console.Write("Email: ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return input;
            Console.WriteLine("Inválido: el email debe contener '@' y un dominio (ej. usuario@ejemplo.com).");
        }
    }

    static string ReadPhoneNumber()
    {
        while (true)
        {
            Console.Write("Teléfono (###-###-####): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (Regex.IsMatch(input, @"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")) return input;
            Console.WriteLine("Inválido: formato requerido 10 dígitos como ###-###-####.");
        }
    }

    static string ReadLoanType()
    {
        while (true)
        {
            Console.Write("Tipo (1=Personal, 2=Hipotecario, 3=Auto): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (input is "1" or "2" or "3") return input;
            Console.WriteLine("Inválido: seleccione 1, 2 o 3.");
        }
    }

    static string ReadCurrency()
    {
        while (true)
        {
            Console.Write("Moneda (1=USD, 2=EUR, 3=JPY, 4=KRW, 5=BOB, 6=GBP): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            switch (input)
            {
                case "1": return "USD";
                case "2": return "EUR";
                case "3": return "JPY";
                case "4": return "KRW";
                case "5": return "BOB";
                case "6": return "GBP";
            }
            Console.WriteLine("Inválido: elija una opción numérica de la lista.");
        }
    }

    static void SeedSampleData()
    {
        var c1 = _bank.CreateCustomer("Ana López", "ana@example.com", "787-000-0000", "123-45-6789");
        var a1 = AccountFactory.CreateAccount(c1.Id, "1", "USD")!; _bank.Accounts.Add(a1); _bank.Deposit(a1.Number, 1200);
        var c2 = _bank.CreateCustomer("Luis Pérez", "luis@example.com", "787-111-1111", "987-65-4321");
        var a2 = AccountFactory.CreateAccount(c2.Id, "2", "USD")!; _bank.Accounts.Add(a2); _bank.Deposit(a2.Number, 300);
        _bank.Transfer(a1.Number, a2.Number, 100);
    }
}
