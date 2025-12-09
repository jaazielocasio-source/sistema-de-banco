using BankSystem.Domain;
using System.Text;
namespace BankSystem.Services;
public static class ReportService
{
    public static string ExportAllCustomersCsv(BankService bank)
    {
        var file = $"Clientes_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var sb = new StringBuilder();
        sb.AppendLine("Id,Nombre,Email,Telefono,GobId");
        foreach (var c in bank.Customers)
            sb.AppendLine($"{c.Id},\"{c.Name.Replace("\"","\"\"")}\",{c.Email},{c.Phone},{c.GovernmentId}");
        File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        AuditLogger.Log("REPORT.CUSTOMERS", $"Export clientes -> {file}");
        return Path.GetFullPath(file);
    }

    public static string? ExportMonthlyStatementCSV(BankService bank, string accountNumber, int month, int year)
    {
        var acc=bank.Accounts.FirstOrDefault(a=>a.Number==accountNumber); if(acc==null) return null;
        var tx=acc.Transactions.Where(t=>t.Date.Month==month && t.Date.Year==year).ToList();
        var file=$"Extracto_{accountNumber}_{year}-{month:00}.csv"; var sb=new StringBuilder();
        sb.AppendLine("Date,Type,Amount,Description,Counterparty");
        foreach(var t in tx) sb.AppendLine($"{t.Date:yyyy-MM-dd HH:mm},{t.Type},{t.Amount},{t.Description},{t.Counterparty}");
        File.WriteAllText(file,sb.ToString());
        AuditLogger.Log("REPORT.CSV",$"CSV extracto {accountNumber} {year}-{month:00}", accountNumber);
        return Path.GetFullPath(file);
    }
    public static string? ExportMonthlyStatementPDF(BankService bank, string accountNumber, int month, int year)
    {
        var acc=bank.Accounts.FirstOrDefault(a=>a.Number==accountNumber); if(acc==null) return null;
        var lines=new List<string>(); lines.Add($"Extracto {accountNumber}  {year}-{month:00}");
        lines.Add($"Cliente: {acc.CustomerId}   Estado: {acc.Status}   Moneda: {acc.Currency}   Saldo: {acc.Balance:F2}");
        lines.Add("Fecha\\tTipo\\tMonto\\tDescripción\\tContraparte");
        foreach(var t in acc.Transactions.Where(t=>t.Date.Month==month && t.Date.Year==year))
            lines.Add($"{t.Date:yyyy-MM-dd HH:mm}\\t{t.Type}\\t{t.Amount}\\t{t.Description}\\t{t.Counterparty}");
        var content=string.Join("\\n",lines); var pdfPath=$"Extracto_{accountNumber}_{year}-{month:00}.pdf";
        MinimalPdfWriter.WriteSinglePageText(pdfPath, content);
        AuditLogger.Log("REPORT.PDF",$"PDF extracto {accountNumber} {year}-{month:00}", accountNumber);
        return Path.GetFullPath(pdfPath);
    }
    public static bool SendMonthlyEStatement(BankService bank, string accountNumber, int month, int year, string pdfFullPath)
    {
        var acc=bank.Accounts.FirstOrDefault(a=>a.Number==accountNumber); if(acc==null) return false;
        var customer=bank.Customers.FirstOrDefault(c=>c.Id==acc.CustomerId); var to=customer?.Email ?? "cliente@example.com";
        var eml=$"To: {to}\\nSubject: E-Statement {accountNumber} {year}-{month:00}\\n\\nAdjunto su estado de cuenta en PDF: {pdfFullPath}\\n";
        var path=$"EStatement_{accountNumber}_{year}-{month:00}.eml"; File.WriteAllText(path, eml);
        AuditLogger.Log("REPORT.ESTATEMENT",$"E-Statement generado para {accountNumber} {year}-{month:00} -> {to}", accountNumber);
        return true;
    }
}
internal static class MinimalPdfWriter
{
    public static void WriteSinglePageText(string path, string text)
    {
        var contentStream=$"BT /F1 10 Tf 50 750 Td ({Escape(text)}) Tj ET";
        var pdf=BuildPdf(contentStream); File.WriteAllBytes(path,pdf);
    }
    static byte[] BuildPdf(string content)
    {
        var objects=new List<byte[]>();
        objects.Add(Obj(1, $"<< /Type /Catalog /Pages 2 0 R >>"));
        objects.Add(Obj(2, $"<< /Type /Pages /Kids [3 0 R] /Count 1 >>"));
        objects.Add(Obj(3, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 5 0 R /Resources << /Font << /F1 4 0 R >> >> >>"));
        objects.Add(Obj(4, $"<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>"));
        objects.Add(Obj(5, $"<< /Length {content.Length} >>\\nstream\\n{content}\\nendstream"));
        using var ms=new MemoryStream(); var w=new StreamWriter(ms, Encoding.ASCII);
        w.Write("%PDF-1.4\\n"); w.Flush();
        var offsets=new List<int>(); offsets.Add(0);
        foreach(var o in objects){ offsets.Add((int)ms.Position); ms.Write(o,0,o.Length); w.Flush(); }
        var xrefPos=(int)ms.Position; var x=new StreamWriter(ms, Encoding.ASCII);
        x.Write("xref\\n0 6\\n"); x.Write("0000000000 65535 f \\n");
        for(int i=1;i<=5;i++) x.Write($"{offsets[i]:0000000000} 00000 n \\n");
        x.Write("trailer\\n<< /Size 6 /Root 1 0 R >>\\nstartxref\\n"); x.Write(xrefPos.ToString()); x.Write("\\n%%EOF"); x.Flush();
        return ms.ToArray();
    }
    static byte[] Obj(int id, string body){ var s=$"{id} 0 obj\\n{body}\\nendobj\\n"; return Encoding.ASCII.GetBytes(s); }
    static string Escape(string s)=> s.Replace("\\","\\\\").Replace("(","\\(").Replace(")","\\)").Replace("\\r","").Replace("\\n","\\n");
}
