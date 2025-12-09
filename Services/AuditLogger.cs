using System.Text;

namespace BankSystem.Services;

public static class AuditLogger
{
    private static readonly object _lock = new();
    private static string _path = "./logs/audit.log";
    private static string MachineUser => Environment.UserName ?? "unknown";
    private static string SimulatedIp => "127.0.0.1";

    public static void Init(string path) { _path = path; Directory.CreateDirectory(Path.GetDirectoryName(_path)!); }

    public static void Log(string action, string message, string? piiToMask = null)
    {
        var masked = piiToMask is null ? "" : $" | pii={Mask(piiToMask)}";
        var line = $"{DateTime.UtcNow:O} | action={action} | user={MachineUser} | ip={SimulatedIp} | {message}{masked}";
        lock(_lock) { File.AppendAllText(_path, line + Environment.NewLine, Encoding.UTF8); }
    }

    public static string Mask(string s) => string.IsNullOrWhiteSpace(s) || s.Length < 4 ? "****" : new string('*', Math.Max(0, s.Length - 4)) + s[^4..];
}
