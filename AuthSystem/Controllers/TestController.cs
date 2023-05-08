using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    public class TestController : Controller
    {
        private readonly AuthDbContext _test;

        public TestController(AuthDbContext test)
        {

            _test = test;


        }
        [Authorize]

        [HttpGet]
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
            var test = new Test { TestName = testName, CreatedBy = "Admin" };
            _test.Tests.Add(test);
            if (testName != null)
            {
                await _test.SaveChangesAsync();
            }

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
        public IActionResult DemoTest(int Id)
        {
            var testDetails = _test.TestsDetail
                .Include(td => td.Test)
                .Where(td => td.Id == Id)
                .ToList();

            var totalQuestions = testDetails.Sum(td => (int)(td.Percentage / 100.0 * _test.MCQs.Count(q => q.SubjectId == td.SubjectId)));

            var testQuestions = new List<MCQ>();
            foreach (var testDetail in testDetails)
            {
                var subjectQuestions = _test.MCQs.Include(q => q.Subject)
                    .Where(q => q.SubjectId == testDetail.SubjectId)
                    .OrderBy(q => Guid.NewGuid()) // randomize order of questions
                    .Take((int)(testDetail.Percentage / 100.0 * _test.MCQs.Count(q => q.SubjectId == testDetail.SubjectId)))
                    .ToList();

                testQuestions.AddRange(subjectQuestions);
            }

            var rng = new Random();
            testQuestions = testQuestions.OrderBy(q => rng.Next()).ToList();

            var questionsList = testQuestions.Take(totalQuestions).ToList();

            return View(questionsList);
        }

        public IActionResult SubmitResult(Dictionary<int, string> answers)
        {
            var score = 0;
            foreach (var question in _test.MCQs)
            {
                if (answers.TryGetValue(question.Id, out string answer) && question.Answer == answer)
                {
                    score++;
                }
            }
            var result = new Result
            {

                AttemptedBy = "Student",
                Score = score,


            };
            _test.Results.Add(result);
            _test.SaveChanges();
            return Content($"Your score is {score}");
        }







    }

}