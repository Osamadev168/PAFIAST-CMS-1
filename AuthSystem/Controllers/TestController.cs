using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using DCMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            ViewBag.TestCenters = new SelectList(_test.TestCenters, "Id", "TestCenterName");
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Include(td => td.Test).Include(td => td.TestCenter).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string testName, int[] selectedSubjectIds, Dictionary<int, int> percentages, int duration, int timeSpan)
        {
            if (string.IsNullOrEmpty(testName))
            {
                TempData["testName"] = "Test name must be provided";
            }
            else if (_test.Tests.Any(t => t.TestName == testName))
            {
                TempData["testName"] = "Test name must be unique.";
            }
            else if (selectedSubjectIds == null || !selectedSubjectIds.Any())
            {
                TempData["selectedSubjectIds"] = "At least one subject must be selected.";
            }
            else if (duration <= 0)
            {
                TempData["duration"] = "Please provide a valid duration for this test";
            }
            else
            {
                var test = new Test { TestName = testName, CreatedBy = "Admin", Duration = duration };

                if (timeSpan <= 0)
                {
                    test.TimeSpan = duration;
                }
                else
                {
                    test.TimeSpan = timeSpan;
                }

                _test.Tests.Add(test);
                await _test.SaveChangesAsync();

                if (selectedSubjectIds != null)
                {
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
                }

                await _test.SaveChangesAsync();

                return RedirectToAction("Test");
            }



            return RedirectToAction("Test");
        }
        [HttpPost]
        public IActionResult CreateCalendar(int testId, DateOnly date, TimeOnly startTime, int centerId)
        {
            try

            {
                var test = _test.Tests.Where(q => q.Id == testId).FirstOrDefault();
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
                    TestCenterId = centerId,
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
                var test = _test.Tests.SingleOrDefault(q => q.Id == testId);
                if (test == null)
                {
                    return NotFound();
                }

                if (startTime == null)
                {
                    return BadRequest("Invalid start time");
                }

                var testEndTime = startTime.AddMinutes(test.TimeSpan);
                return Content(testEndTime.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }



        public async Task<IActionResult> DemoTest(int Id, int C_Id, string C_token)
        {
            var test = _test.Tests.FirstOrDefault(x => x.Id == Id);
            var testCalendar = _test.TestCalenders.FirstOrDefault(x => x.Id == C_Id && x.TestId == Id);

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

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Content("User not found");

            }
            var userId = user.Id;

            List<MCQ> questionsList;

            var assignedQuestions = _test.AssignedQuestions
                .Include(aq => aq.Question)
                .Where(aq => aq.UserId == userId && aq.TestDetailId == Id)
                .ToList();

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

                await _test.SaveChangesAsync();

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
        public async Task<IActionResult> SaveUserResponseAsync([FromBody] Dictionary<int, string> answers, int testId)
        {
            var user = await _userManager.GetUserAsync(User);


            var userId = user.Id;

            foreach (var answer in answers)
            {
                var questionId = answer.Key;
                var selectedAnswer = answer.Value;

                var question = _test.AssignedQuestions.Where(u => u.QuestionId == questionId && u.UserId == userId && u.TestDetailId == testId).FirstOrDefault();
                {



                    question.UserResponse = selectedAnswer;
                }
            }
            _test.SaveChanges();
            return Json(new { success = "Done!" });
        }
        [HttpGet]
        public async Task<IActionResult> FetchUserResponsesAsync(int testId)
        {
            var user = await _userManager.GetUserAsync(User);


            var userId = user.Id;

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
        public IActionResult GetTestDuration(int testId, int C_Id)
        {
            var test = _test.Tests.FirstOrDefault(q => q.Id == testId);
            var testCalendar = _test.TestCalenders.FirstOrDefault(q => q.TestId == testId && q.Id == C_Id);

            if (test != null && testCalendar != null)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var endTime = testCalendar.EndTime.ToTimeSpan();

                var minutesPassed = (endTime - currentTime).TotalMinutes;
                Console.WriteLine(minutesPassed + "Minutes");
                if (minutesPassed >= test.Duration)
                {
                    var duration = test.Duration;
                    return Json(duration);
                }
                else if (minutesPassed < test.Duration)
                {
                    var remainingMinutes = (endTime - currentTime).TotalMinutes;
                    return Json(remainingMinutes);
                }
            }

            return NotFound();
        }
        public async Task<IActionResult> SaveStartTimeAsync(int testId)
        {
            var user = await _userManager.GetUserAsync(User);


            var userId = user.Id;

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

        public async Task<IActionResult> GetRemainingTimeAsync(int testId, int C_Id)
        {
            var user = await _userManager.GetUserAsync(User);


            var userId = user.Id;
            var testSession = _test.UserTestSessions.FirstOrDefault(q => q.TestId == testId && q.UserId == userId);
            var test = _test.Tests.FirstOrDefault(q => q.Id == testId);
            var testCalendar = _test.TestCalenders.FirstOrDefault(q => q.TestId == testId && q.Id == C_Id);
            var currentTime = DateTime.Now.TimeOfDay;
            var endTime = testCalendar.EndTime.ToTimeSpan();

            var minutesPassed = (endTime - currentTime).TotalMinutes;

            if (testSession != null && test != null)
            {
                var elapsedTime = currentTime - testSession.StartTime.TimeOfDay;
                var remainingTime = test.Duration - minutesPassed;
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
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
        public IActionResult SystemStats() {

            var tests = _test.Tests.ToList();
            var totalTests = tests.Count;

            ViewBag.TotalTests = totalTests;
            return View(tests);
        
        }

    }
}