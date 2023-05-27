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
            if (string.IsNullOrEmpty(testName))
            {
                ModelState.AddModelError("TestName", "Test name is required.");
            }

            if (selectedSubjectIds == null || !selectedSubjectIds.Any())
            {
                ModelState.AddModelError("selectedSubjectIds", "At least one subject must be selected.");
            }

            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return RedirectToAction("Test");
            }

            var test = new Test { TestName = testName, CreatedBy = "Admin" };
            _test.Tests.Add(test);
            await _test.SaveChangesAsync();

            foreach (var subjectId in selectedSubjectIds)
            {
                var testDetails = new TestDetail
                {
                    TestId = test.Id,
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
            var userId = 33;
            var assignedQuestions = _test.AssignedQuestions
                .Include(aq => aq.Question)
                .Where(aq => aq.UserId == userId && aq.TestDetailId == Id)
                .ToList();

            List<MCQ> questionsList;

            if (assignedQuestions.Count == 0)
            {
                var testDetails = _test.TestsDetail
                    .Include(td => td.Test)
                    .Where(td => td.TestId == Id)
                    .ToList();

                var testQuestions = new List<MCQ>();
                foreach (var testDetail in testDetails)
                {
                    var subjectQuestions = _test.MCQs.Include(q => q.Subject)
                        .Where(q => q.SubjectId == testDetail.SubjectId)
                        .OrderBy(x => Guid.NewGuid()) // randomize order of questions
                        .Take(Math.Max((int)(testDetail.Percentage / 100.0 * 100), 1))
                        .ToList();

                    testQuestions.AddRange(subjectQuestions);
                }

                var rng = new Random();
                testQuestions = testQuestions.OrderBy(q => rng.Next()).ToList();

                var totalQuestions = testQuestions.OrderBy(x => x.Subject.SubjectName).Take(100).ToList();

                // save the assigned questions for the user in the database
                foreach (var question in totalQuestions)
                {
                    var assignedQuestion = new AssignedQuestions
                    {
                        UserId = userId,
                        QuestionId = question.Id,
                        TestDetailId = Id,
                        Question = question
                    };

                    _test.AssignedQuestions.Add(assignedQuestion);
                }

                _test.SaveChanges();

                questionsList = totalQuestions;
            }
            else
            {
                var assignedQuestionIds = assignedQuestions.Select(aq => aq.QuestionId).ToList();

                var testDetails = _test.TestsDetail
                    .Include(td => td.Test)
                    .Where(td => td.TestId == Id)
                    .ToList();

                var testQuestions = new List<MCQ>();
                foreach (var testDetail in testDetails)
                {
                    var subjectQuestions = _test.MCQs.Include(q => q.Subject)
                        .Where(q => q.SubjectId == testDetail.SubjectId && assignedQuestionIds.Contains(q.Id))
                        .ToList();

                    testQuestions.AddRange(subjectQuestions);
                }

                var rng = new Random();
                testQuestions = testQuestions.ToList();

                var totalQuestions = testQuestions.OrderBy(x => x.Subject.SubjectName).Take(100).ToList();

                questionsList = totalQuestions;
            }


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
        [HttpPost]

        public IActionResult SaveUserResponse([FromBody] Dictionary<int, string> answers, int testId)
        {
            var userId = 33;
            foreach (var answer in answers)
            {
                var questionId = answer.Key;
                var selectedAnswer = answer.Value;

                var question = _test.AssignedQuestions.Where(u => u.QuestionId == questionId && u.UserId == userId && u.TestDetailId == testId).FirstOrDefault();
                {
                    Console.WriteLine(questionId);
                    Console.WriteLine(selectedAnswer);


                    question.UserResponse = selectedAnswer;
                }
            }

            _test.SaveChanges();

            return Json(new { success = "Good" });
        }
        [HttpGet]
        public IActionResult FetchUserResponses(int testId)
        {
            var userId = 33;

            var assignedQuestions = _test.AssignedQuestions
                .Where(aq => aq.UserId == userId && aq.TestDetailId == testId)
                .Select(aq => new
                {
                    QuestionId = aq.QuestionId,
                    UserResponse = aq.UserResponse
                })
                .ToList();

            return Json(assignedQuestions);
        }
        public IActionResult GetNumberOfQuestions(int subjectId)
        {
            var questionsCount = _test.MCQs.Count(q => q.SubjectId == subjectId);
            var subject = _test.Subjects.FirstOrDefault(q => q.SubjectId == subjectId);
            var subjectName = subject != null ? subject.SubjectName : string.Empty;
            var data = new
            {
                count = questionsCount,
                SubjectName = subjectName
            };

            return Json(data);
        }


        public IActionResult GetTestName(int testId)
        {
            var test = _test.Tests.FirstOrDefault(q => q.Id == testId);

            if (test != null)
            {
                var testName = test.TestName;
                return Json(testName);
            }

            return NotFound();
        }







    }
}