using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

public class ReportController : Controller
{
    private readonly string _connectionString;

    private readonly ApiService _apiService;

    public ReportController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _apiService.GetAllEmployeesAsync();

        if (employees == null)
        {
            ViewBag.Error = "Unable to fetch employee data from the API.";
            return View(new List<EmployeeResponse>());
        }

        return View(employees);
    }

   

    private List<EmployeeReportViewModel> GetEmployeesWithDocuments()
    {
        var employees = new List<EmployeeReportViewModel>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var cmd = new SqlCommand(@"
        SELECT e.Id AS EmployeeId, e.EmployeeName, e.Department, e.Designation, e.Age, e.Gender, e.Address,
               d.Id AS DocumentId, d.FileName, d.ImageData
        FROM Employees e
        LEFT JOIN EmployeeDocuments d ON e.Id = d.EmployeeId
        ORDER BY e.EmployeeName", conn);

        using var reader = cmd.ExecuteReader();

        var dict = new Dictionary<int, EmployeeReportViewModel>();

        while (reader.Read())
        {
            int empId = Convert.ToInt32(reader["EmployeeId"]);

            if (!dict.ContainsKey(empId))
            {
                dict[empId] = new EmployeeReportViewModel
                {
                    EmployeeId = empId,
                    EmployeeName = reader["EmployeeName"].ToString(),
                    Department = reader["Department"].ToString(),
                    Designation = reader["Designation"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),
                    Gender = reader["Gender"].ToString(),
                    Address = reader["Address"].ToString(),
                };
            }

            if (reader["DocumentId"] != DBNull.Value)
            {
                dict[empId].Documents.Add(new EmployeeDocumentViewModel
                {
                    DocumentId = Convert.ToInt32(reader["DocumentId"]),
                    FileName = reader["FileName"].ToString(),
                    ImageData = reader["ImageData"] as byte[]
                });
            }
        }

        return dict.Values.ToList();
    }
    public IActionResult DownloadReport()
    {
        var employees = GetEmployeesWithDocuments(); // reuse existing ADO.NET logic

        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 25, 25, 30, 30);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        doc.Add(new Paragraph("Employee Report") { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20f });

        foreach (var emp in employees)
        {
            doc.Add(new Paragraph($"Name: {emp.EmployeeName}"));
            doc.Add(new Paragraph($"Department: {emp.Department}"));
            doc.Add(new Paragraph($"Designation: {emp.Designation}"));
            doc.Add(new Paragraph($"Age: {emp.Age}, Gender: {emp.Gender}"));
            doc.Add(new Paragraph($"Address: {emp.Address}"));
            doc.Add(new Paragraph("Documents:"));

            if (emp.Documents.Any())
            {
                foreach (var docItem in emp.Documents)
                {
                    doc.Add(new Paragraph($"- {docItem.FileName}"));

                    if (docItem.ImageData != null && docItem.ImageData.Length > 0)
                    {
                        var img = iTextSharp.text.Image.GetInstance(docItem.ImageData);
                        img.ScaleToFit(150f, 150f);
                        img.SpacingAfter = 10f;
                        doc.Add(img);
                    }
                }
            }
            else
            {
                doc.Add(new Paragraph("No documents"));
            }

            doc.Add(new Paragraph("-------------------------------------------------"));
        }

        doc.Close();
        return File(ms.ToArray(), "application/pdf", "EmployeeReport.pdf");
    }

    [HttpGet("DownloadEmployeeReport/{id}")]
    public async Task<IActionResult> DownloadEmployeeReport(int id)
    {
        var employee = await _apiService.getEmployeeById(id);
        if (employee == null)
            return NotFound();

        using var ms = new MemoryStream();
        var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        doc.Add(new iTextSharp.text.Paragraph($"Employee Report - {employee.employeeName}")
        {
            Alignment = iTextSharp.text.Element.ALIGN_CENTER,
            SpacingAfter = 20f
        });

        doc.Add(new iTextSharp.text.Paragraph($"Name: {employee.employeeName}"));
        doc.Add(new iTextSharp.text.Paragraph($"Department: {employee.department}"));
        doc.Add(new iTextSharp.text.Paragraph($"Designation: {employee.designation}"));
        doc.Add(new iTextSharp.text.Paragraph($"Age: {employee.age}, Gender: {employee.gender}"));
        doc.Add(new iTextSharp.text.Paragraph($"Address: {employee.address}"));
        doc.Add(new iTextSharp.text.Paragraph("Documents:"));

        if (employee.employeeDocuments != null && employee.employeeDocuments.Any())
        {
            foreach (var docItem in employee.employeeDocuments)
            {
                doc.Add(new iTextSharp.text.Paragraph($"- {docItem.FileName}"));

                if (docItem.ImageData != null && docItem.ImageData.Length > 0)
                {
                    try
                    {
                        var img = iTextSharp.text.Image.GetInstance(docItem.ImageData);
                        img.ScaleToFit(150f, 150f);
                        img.SpacingAfter = 10f;
                        doc.Add(img);
                    }
                    catch
                    {
                        doc.Add(new iTextSharp.text.Paragraph("[Invalid image data]"));
                    }
                }
            }
        }
        else
        {
            doc.Add(new iTextSharp.text.Paragraph("No documents uploaded."));
        }

        doc.Close();

        string fileName = $"{employee.employeeName}_Report.pdf";
        return File(ms.ToArray(), "application/pdf", fileName);
    }

    [HttpGet("ViewDocument/{id}")]
    public async Task<IActionResult> ViewDocument(int id)
    {
        var employee = await _apiService.getEmployeeById(id);
        if (employee == null)
            return NotFound();
        var document = employee.employeeDocuments?.FirstOrDefault();

        if (document == null || string.IsNullOrWhiteSpace(document.FilePath))
            return NotFound("No document found for this employee.");

        // ✅ Use the document's actual FilePath
        var filePath = document.FilePath;

        // ✅ Build the physical path from wwwroot
        var fullPath = Path.Combine("file:///C:/Users/ITSOLUTION/source/repos/EmployeeApi/EmployeeApi/", "wwwroot", filePath.TrimStart('/'));

        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found on server.");

        // ✅ Detect content type
        var extension = Path.GetExtension(fullPath).ToLower();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        // ✅ Make it display inline in browser (not download)
        Response.Headers.Add("Content-Disposition", $"inline; filename={Path.GetFileName(fullPath)}");

        return PhysicalFile(fullPath, contentType);
    }

    [HttpPost]
    public async Task<IActionResult> DelEmployee(int id)
    {
        var result = await _apiService.DeleteEmp(id);

        if (result.employeeId!=0)
        {
            TempData["Message"] = "Employee deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete employee.";
        }

        return RedirectToAction("Index");
    }

}
