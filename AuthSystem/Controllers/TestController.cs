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
    [Authorize]
    public class TestController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _test;

        public TestController(AuthDbContext test, UserManager<ApplicationUser> userManager)
        {
            _test = test;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Super Admin")]
        [HttpGet]
        public IActionResult Test()
        {
            ViewBag.TestCenters = new SelectList(_test.TestCenters, "Id", "TestCenterName");
            ViewBag.Session = new SelectList(_test.Sessions, "Id", "SessionName");
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Include(td => td.Test).Include(td => td.TestCenter).ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Create()
        {
            ViewBag.TestCenters = new SelectList(_test.TestCenters, "Id", "TestCenterName");
            ViewBag.Session = new SelectList(_test.Sessions, "Id", "SessionName");
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Include(td => td.Test).Include(td => td.TestCenter).ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Super Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTest(string TestName, int[] selectedSubjectIds, Dictionary<int, int> percentages, int duration, int timeSpan, int sessionId, int easy, int medium, int hard)
        {
            try
            {
                if (string.IsNullOrEmpty(TestName))
                {
                    TempData["testName"] = "Test name must be provided";
                }
                else if (_test.Tests.Any(t => t.TestName == TestName))
                {
                    TempData["testName"] = "Test name must be unique.";
                    return RedirectToAction("Create");
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
                    var test = new Test { TestName = TestName, CreatedBy = "Admin", Duration = duration, SessionId = sessionId };

                    if (timeSpan <= 0)
                    {
                        test.TimeSpan = duration;
                    }
                    else
                    {
                        test.TimeSpan = timeSpan;
                    }

                    _test.Tests.Add(test);
                    _test.SaveChanges();

                    if (selectedSubjectIds != null)
                    {
                        foreach (var subjectId in selectedSubjectIds)
                        {
                            var testDetail = new TestDetail
                            {
                                TestId = test.Id,
                                SubjectId = subjectId,
                                Percentage = percentages[subjectId],
                                Easy = easy,
                                Medium = medium,
                                Hard = hard
                            };

                            _test.TestsDetail.Add(testDetail);
                        }

                        _test.SaveChanges();

                        return RedirectToAction("Test");
                    }
                }

                return RedirectToAction("Test");
            }
            catch (Exception e)
            {
                return Json(new { Error = e });
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        [HttpPost]
        public IActionResult CreateCalendar(int testId, DateOnly date, TimeOnly startTime, int centerId)
        {
            try

            {
                var test = _test.Tests.Where(q => q.Id == testId).FirstOrDefault();
                var calendarToken = RandomNumberGenerator.Create();
                int calendarCode;
                byte[] randomBytes = new byte[16];
                calendarToken.GetBytes(randomBytes);
                string token = Convert.ToBase64String(randomBytes);
                string tokenValue = Convert.ToBase64String(randomBytes);
                calendarToken.ToInt();
                int _min = 1000;
                int _max = 9999;
                Random _rdm = new Random();
                calendarCode = _rdm.Next(_min, _max);
                var existingCalendar = _test.TestCalenders.Where(c => c.StartTime == startTime && c.TestCenterId == centerId && c.Date == date).FirstOrDefault();
                if (existingCalendar == null)
                {
                    var calendar = new TestCalenders
                    {
                        TestId = testId,
                        Date = date,
                        StartTime = startTime,
                        EndTime = startTime.AddMinutes(test.TimeSpan),
                        TestCenterId = centerId,
                        CalendarToken = token,
                        Code = calendarCode
                    };

                    _test.TestCalenders.Add(calendar);

                    _test.SaveChanges();
                }
                else
                {
                    ViewBag.CalendarError = "Same calendar for that time already exists for that center!";
                    return View("CalendarError");
                }

                return RedirectToAction("Test");
            }
            catch (Exception ex)
            {
                return Json("Error creating calendars: " + ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult GetTestEndTime(int testId, TimeOnly startTime)
        {
            try
            {
                var test = _test.Tests.SingleOrDefault(q => q.Id == testId);
                if (test == null)
                {
                    return NotFound();
                }

                var testEndTime = startTime.AddMinutes(test.TimeSpan);
                return Content(testEndTime.ToString());
            }
            catch (Exception e)
            {
                return Json(new { Error = e });
            }
        }

        public async Task<IActionResult> DemoTest(int Id, int C_Id, string C_token, int applicationId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Content("User not found");
            }
            var userId = user.Id;
            var attempted = _test.TestApplications.Where(t => t.UserId == userId && t.TestId == Id && t.CalendarId == C_Id && t.CalenderToken == C_token && t.HasFinished == true).Any();
            ViewBag.TestId = Id;
            ViewBag.CalendarId = C_Id;
            ViewBag.ApplicationId = applicationId;
            if (!attempted)
            {
                var test = _test.Tests.FirstOrDefault(x => x.Id == Id);
                var testCalendar = _test.TestCalenders.FirstOrDefault(x => x.Id == C_Id && x.TestId == Id);
                var isPresent = _test.TestApplications.Where(a => a.UserId == userId && a.IsVerified == true && a.CalendarId == C_Id && a.TestId == Id && a.CalenderToken == C_token).FirstOrDefault()?.IsPresent == true;
                if (testCalendar == null)
                {
                    return Content("Internal Error");
                }

                if (testCalendar.Date.Day != DateTime.Today.Day ||
                    testCalendar.StartTime.ToTimeSpan() > DateTime.Now.TimeOfDay ||
                    testCalendar.EndTime.ToTimeSpan() <= DateTime.Now.TimeOfDay ||
                    testCalendar.CalendarToken != C_token)
                {
                    return Content("Not Available");
                }
                if (!isPresent)
                {
                    return Content("Attendance due!");
                }

                List<MCQ> questionsList;

                var assignedQuestions = _test.AssignedQuestions
                    .Include(aq => aq.Question)
                    .Where(aq => aq.UserId == userId && aq.TestDetailId == Id && aq.ApplicationId == applicationId)
                    .ToList();

                if (assignedQuestions.Count == 0)
                {
                    var testDetails = _test.TestsDetail
                        .Include(td => td.Test)
                        .Where(td => td.TestId == Id)
                        .ToList();
                    var testApplication = _test.TestApplications.Where(a => a.UserId == userId && a.TestId == Id && a.CalendarId == C_Id && a.CalenderToken == C_token && a.HasFinished == null).FirstOrDefault();
                    if (testApplication != null)
                    {
                        testApplication.HasAttempted = true;
                        _test.SaveChanges();
                    }
                    var testQuestions = new List<MCQ>();
                    foreach (var testDetail in testDetails)
                    {
                        var easy = testDetail.Easy;
                        var medium = testDetail.Medium;
                        var hard = testDetail.Hard;
                        var subjectQuestions = _test.MCQs.Include(q => q.Subject)
                            .Where(q => q.SubjectId == testDetail.SubjectId)
                            .OrderBy(x => Guid.NewGuid())
                            .Take(Math.Max((int)(testDetail.Percentage / 100.0 * 100), 1))
                            .ToList();

                        subjectQuestions.Where(q => q.Difficulty == "Easy").Take(easy).ToList();
                        subjectQuestions.Where(q => q.Difficulty == "Medium").Take(medium).ToList();
                        subjectQuestions.Where(q => q.Difficulty == "Hard").Take(hard).ToList();

                        testQuestions.AddRange(subjectQuestions);
                    }

                    var rng = new Random();
                    testQuestions = testQuestions.OrderBy(q => rng.Next()).ToList();

                    var totalQuestions = testQuestions.OrderBy(x => x.Subject.SubjectName).Take(100).ToList();

                    foreach (var question in totalQuestions)
                    {
                        var assignedQuestion = new AssignedQuestions
                        {
                            UserId = userId,
                            QuestionId = question.Id,
                            TestDetailId = Id,
                            Question = question,
                            ApplicationId = applicationId
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
                    var testApplication = _test.TestApplications.Where(a => a.UserId == userId && a.TestId == Id && a.CalendarId == C_Id && a.CalenderToken == C_token && a.HasFinished == null).FirstOrDefault();
                    if (testApplication != null)
                    {
                        testApplication.HasAttempted = true;
                        _test.SaveChanges();
                    }
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
            else
            {
                return Content("Max Pass reached");
            }
        }

        public async Task<IActionResult> SubmitResult(Dictionary<int, string> answers, int testId, int calendarId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Content("User not found");
            }
            var userId = user.Id;

            var application = _test.TestApplications.Where(t => t.UserId == userId && t.TestId == testId && t.CalendarId == calendarId).FirstOrDefault();
            var existingResult = _test.Results.Where(t => t.AttemptedBy == userId && t.TestId == testId && t.CalendarId == calendarId).Any();
            if (!existingResult)
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
                    AttemptedBy = userId,
                    Score = score,
                    TestId = testId,
                    CalendarId = calendarId
                };
                _test.Results.Add(result);
                _test.SaveChanges();
                if (application != null)
                {
                    application.HasFinished = true;
                    _test.SaveChanges();
                }
                return RedirectToAction("Results", "Applicant");
            }
            return Content("Result already submitted!");
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
        public async Task<IActionResult> FetchUserResponsesAsync(int testId, int applicationId)
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = user.Id;

            var assignedQuestions = _test.AssignedQuestions
                .Where(aq => aq.UserId == userId && aq.TestDetailId == testId && aq.ApplicationId == applicationId)
                .Select(aq => new
                {
                    QuestionId = aq.QuestionId,
                    UserResponse = aq.UserResponse
                })
                .ToList();
            return Json(assignedQuestions);
        }

        [Authorize(Roles = "Admin,Super Admin")]
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

        [Authorize(Roles = "Admin,Super Admin")]
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

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult SystemStats()
        {
            try
            {
                var sessions = _test.Sessions.OrderBy(w => w.StartDate).ToList();

                return View(sessions);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult SessionTests(int sessionId)
        {
            try
            {
                var viewModel = new Test
                {
                    TestList = _test.Tests.OrderByDescending(q => q.Id).Where(w => w.SessionId == sessionId).ToList(),
                    Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                    TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                    TestCalenders = _test.TestCalenders.Include(td => td.Test).Include(td => td.TestCenter).ToList()
                };

                return View(viewModel);
            }
            catch (Exception e)
            {
                return Json(new { Erorr = e.Message });
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult ApplicantsDetails(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult UnpaidApplicantsDetails(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId && w.IsPaid == false && w.IsVerified == false).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult VerifiedApplicantsDetails(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId && w.IsVerified == true).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult UnVerifiedApplicantsDetails(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId && w.IsPaid == true && w.IsVerified == false).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult NonSelectedTestCenters(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId && w.IsPaid == true && w.IsVerified == true && w.CalendarId == null).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult SelectedTestCenters(int testId)
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.TestId == testId && w.IsPaid == true && w.IsVerified == true && w.CalendarId != null).ToList();
                var userIds = testApplications.Select(ta => ta.UserId).ToList();

                var applicants = _userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
                var testName = _test.Tests.Where(w => w.Id == testId).FirstOrDefault().TestName;
                ViewBag.TestName = testName;
                return View(applicants);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> MyTests()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var currentTime = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay);
                var userId = user.Id;
                var tests = _test.TestApplications
                            .Where(a => a.UserId == userId && a.IsVerified == true
                                    && a.Calendar.Date == currentDate
                                    )
                            .Include(t => t.Test).Include(t => t.Test.TestDetails).Include(c => c.Calendar)
                            .ToList(); return View(tests);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
    }
}