using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AuthSystem.Controllers
{
    public class FIBController : Controller
    {
        private readonly AuthDbContext _test;

        public FIBController(AuthDbContext test)
        {
            _test = test;
        }

        [Authorize]
        [HttpGet]
        public IActionResult ViewBlanks()
        {
            IEnumerable<Blank> getBlanks = _test.Blanks.Include(q => q.Subject);
            return View(getBlanks);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult UploadFileFIB()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file to upload.");
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("file", "Invalid file format. Please upload an Excel file with .xlsx extension.");
                return View();
            }

            var questions = new List<Blank>();
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError("file", "Excel file is empty or has no worksheets.");
                        return View();
                    }

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var question = new Blank
                        {
                            Statement = worksheet.Cells[row, 1].Value?.ToString(),
                            Answer = worksheet.Cells[row, 2].Value?.ToString(),
                            Synonym1 = worksheet.Cells[row, 3].Value?.ToString(),
                            Synonym2 = worksheet.Cells[row, 4].Value?.ToString(),
                            Synonym3 = worksheet.Cells[row, 5].Value?.ToString(),
                            Difficulty = worksheet.Cells[row, 6].Value?.ToString(),
                            SubjectId = HttpContext.Session.GetInt32("SelectedSubjectId").Value
                        };

                        if (string.IsNullOrEmpty(question.Answer))
                        {
                            ModelState.AddModelError("Answer", $"The answer for row {row} is required.");
                            return View();
                        }

                        questions.Add(question);
                    }

                    if (!ModelState.IsValid)
                    {
                        return View();
                    }
                }
            }

            // Save the questions to the database
            _test.Blanks.AddRange(questions);
            _test.SaveChanges();
            return RedirectToAction("ViewBlanks");
        }
    }
}