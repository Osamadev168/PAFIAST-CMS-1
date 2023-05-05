using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using System.Runtime.InteropServices.JavaScript;

namespace AuthSystem.Controllers
{
    public class TestController : Controller
    {
        private readonly AuthDbContext _test;

        public TestController(AuthDbContext test) {

            _test = test;
            
        
        }
      
        public IActionResult Test()
        {
            var viewModel = new Test
            {
                TestList = _test.Tests.ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string testName, int[] selectedSubjectIds, Dictionary<int, int> percentages)
        {
                var test = new Test { TestName = testName , CreatedBy = "User" };
                _test.Tests.Add(test);
                await _test.SaveChangesAsync();

                foreach (var subjectId in selectedSubjectIds)
                {
                    var testDetails = new TestDetail
                    {
                        Id = test.Id,
                        SubjectId = subjectId,
                        Percentage = percentages[subjectId]
                    };

                    _test.TestsDetail.Add(testDetails);
                    Console.WriteLine(testDetails);
                }

                await _test.SaveChangesAsync();

                return RedirectToAction("Test");

        }

    }
}