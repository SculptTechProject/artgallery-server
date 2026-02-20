using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;

namespace artgallery_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ReportController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Generuje przykładowy raport PDF
    /// </summary>
    [HttpGet("sample")]
    public IActionResult GenerateSampleReport()
    {
        // Ścieżka do szablonu raportu (.frx)
        var reportPath = Path.Combine(_env.ContentRootPath, "Reports", "SampleReport.frx");

        if (!System.IO.File.Exists(reportPath))
        {
            return NotFound($"Szablon raportu nie został znaleziony: {reportPath}");
        }

        using var report = new Report();
        
        // Wczytaj szablon
        report.Load(reportPath);

        // Ustaw dane (przykładowe)
        report.SetParameterValue("ReportTitle", "Raport Galerii Sztuki");
        report.SetParameterValue("GeneratedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

        // Przygotuj raport
        if (!report.Prepare())
        {
            return StatusCode(500, "Nie udało się przygotować raportu");
        }

        // Eksportuj do PDF
        using var pdfExport = new PDFSimpleExport();
        using var ms = new MemoryStream();
        
        pdfExport.Export(report, ms);
        ms.Position = 0;

        return File(ms.ToArray(), "application/pdf", "raport.pdf");
    }
}
