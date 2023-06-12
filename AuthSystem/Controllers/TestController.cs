using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using DCMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Security.Cryptography;

namespace AuthSystem.Controllers
{
    public class TestController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _test;
        [ActivatorUtilitiesConstructor]
        public TestController(AuthDbContext test, UserManager<ApplicationUser> userManager)
        {

            _test = test;
            _userManager = userManager;


        }
        [Authorize]

        [HttpGet]
        public IActionResult Test()
        {
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Include(td => td.Test).ToList(),
            };


            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string testName, int[] selectedSubjectIds, Dictionary<int, int> percentages, int duration , int timeSpan)
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
                return RedirectToAction("Test");
            }

            var test = new Test { TestName = testName, CreatedBy = "Admin", Duration = duration, TimeSpan = timeSpan};
            _test.Tests.Add(test);
            await _test.SaveChangesAsync();

            

            foreach (var subjectId in selectedSubjectIds)
            {
                var testDetail = new TestDetail
                {
                    TestId = test.Id,
                    SubjectId = subjectId,
                    Percentage = percentages[subjectId]
                };

                _test.TestsDetail.Add(testDetail);
            }

            await _test.SaveChangesAsync();

            return RedirectToAction("Test");
        }

        [HttpPost]
        public IActionResult CreateCalendar(int testId , DateOnly date , TimeOnly startTime)
        {
            try

            {
                 var test =  _test.Tests.Where(q => q.Id == testId).FirstOrDefault();
                var calendarToken = RandomNumberGenerator.Create();
                byte[] randomBytes = new byte[16];
                calendarToken.GetBytes(randomBytes);
                string token = Convert.ToBase64String(randomBytes);
                string tokenValue = Convert.ToBase64String(randomBytes);
                calendarToken.ToInt();
                var calendar = new TestCalenders
                {
                    TestId = testId,
                    Date = date,
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(test.TimeSpan),
                    CalendarToken = token

                };

                    _test.TestCalenders.Add(calendar);

                _test.SaveChanges();

                return RedirectToAction("Test");
            }
            catch (Exception ex)
            {
                return Json("Error creating calendars: " + ex.Message);
            }
        }


        public IActionResult GetTestEndTime(int testId, TimeOnly startTime)
        {
            try
            {
                Console.Write( startTime);
                Console.Write(testId);


                var test = _test.Tests.SingleOrDefault(q => q.Id == testId);
                if (test == null)
                {
                    return NotFound(); // Return a 404 Not Found response if the test is not found
                }

                if (startTime == null)
                {
                    return BadRequest("Invalid start time"); // Return a 400 Bad Request response if the start time is not provided
                }

                var testEndTime = startTime.AddMinutes(test.TimeSpan);
                return Content(testEndTime.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message); // Return a 500 Internal Server Error response with the exception message
            }
        }



        public IActionResult DemoTest(int Id, int C_Id, string C_token)
        {
            var test = _test.Tests.FirstOrDefault(x => x.Id == Id);
            var testCalendar = _test.TestCalenders.FirstOrDefault(x => x.Id == C_Id && x.TestId == Id  );

            if (testCalendar == null)
                
            {
                return Content("Not Available");
            }

            if (testCalendar.Date.Day != DateTime.Today.Day ||
                testCalendar.StartTime.ToTimeSpan() > DateTime.Now.TimeOfDay ||
                testCalendar.EndTime.ToTimeSpan() <= DateTime.Now.TimeOfDay ||
                testCalendar.CalendarToken != C_token)
            {
                return Content("Not Available");
            }


            else
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
            return Json(new { success = "Done!" });
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
        public IActionResult GetTestDuration(int testId)
        {
            var test = _test.Tests.FirstOrDefault(q => q.Id == testId);
            if (test != null)
            {
                var duration = test.Duration;
                return Json(duration);
            }
            return NotFound();

        }
        public IActionResult SaveStartTime(int testId)
        {
            var userId = 33;
            var testSession = _test.UserTestSessions.FirstOrDefault(q => q.TestId == testId && q.UserId == userId);
            if (testSession == null)
            {
                testSession = new UserTestSession
                {
                    TestId = testId,
                    UserId = userId,
                    StartTime = DateTime.Now,
                };
                _test.UserTestSessions.Add(testSession);
            }
            _test.SaveChanges();
            return Content("Done");
        }
        public IActionResult GetRemainingTime(int testId)
        {
            var userId = 33;
            var testSession = _test.UserTestSessions.FirstOrDefault(q => q.TestId == testId && q.UserId == userId);
            var test = _test.Tests.FirstOrDefault(q => q.Id == testId);

            if (testSession != null && test != null)
            {
                var currentTime = DateTime.Now;
                var elapsedTime = currentTime - testSession.StartTime;
                var remainingTime = test.Duration - elapsedTime.TotalMinutes;
                return Json(remainingTime);
            }
            return (Json(test.Duration));
        }

        public IActionResult CheckTestName(string testName)
        {
            try
            {
                var test = _test.Tests.FirstOrDefault(t => t.TestName == testName);

                if (test != null)
                {
                    return Json("Test name already exists.");
                }
                else
                {
                    return Json( "Test name is available." );
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

    }
}