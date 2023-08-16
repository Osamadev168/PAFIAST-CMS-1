using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    public class CalendarController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _test;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CalendarController(AuthDbContext test, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _test = test;
            _userManager = userManager;
            _hostingEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Where(t => t.Date.Day >= DateTime.UtcNow.Day).Include(td => td.Test).Include(td => td.TestCenter).ToList(),
            };
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userId = user.Id;
                ViewBag.UserId = userId;
            }
            return View(viewModel);
        }

        public IActionResult SelectTest(int testId, string UserId)
        {
            var userId = UserId;
            var user = _userManager.FindByIdAsync(userId).Result;
            var testName = _test.Tests.Where(t => t.Id == testId).FirstOrDefault()?.TestName;
            var applicationID = _test.TestApplications.Where(a => a.UserId == userId && a.TestId == testId && a.CalendarId == null).FirstOrDefault()?.Id;
            if (user != null && user.ProfilePhoto == null || user.FatherName == null || user.City == null || user.Address == null || user.CNIC == null || user.Country == null || user.Province == null)
            {
                return RedirectToAction("ProfileIncomplete");
            }

            if (_test.TestApplications.Any(t => t.UserId == userId && t.TestId == testId && t.CalendarId == null) == true)
            {
                return Content("Please complete your application for " + testName + " with application ID: " + applicationID);
            }
            else
            {
                _test.TestApplications.Add(new TestApplication
                {
                    UserId = userId,
                    TestId = testId,
                    SelectionTime = DateTime.Now,
                });
                _test.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public IActionResult ProfileIncomplete()
        {
            return View();
        }

        public async Task<IActionResult> TestApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("User not found");
            }
            var userId = user.Id;
            var TestApplications = _test.TestApplications
                .Where(uc => uc.UserId == userId && uc.IsVerified == true)
                .Include(uc => uc.Test)
                .Include(uc => uc.Calendar).Include(uc => uc.Calendar.TestCenter)
                .ToList();
            if (TestApplications != null)
            {
                return View(TestApplications);
            }
            return Content("<h1>No Calendars Available</h1>");
        }

        public IActionResult PrintVoucher(int testId, string testName, string applicantName, string userId)
        {
            try
            {
                var test = _test.Tests.FirstOrDefault(q => q.Id == testId);
                if (test != null)
                {
                    var existingVoucher = _test.FeeVoucher.Where(w => w.ApplicantName == applicantName && w.TestName == testName && w.TestId == testId && w.isPaid == true).FirstOrDefault();
                    var voucherNumber = _test.TestApplications.Where(a => a.UserId == userId && a.TestId == testId && a.IsPaid == false).FirstOrDefault().Id + testId;
                    /*  if (existingVoucher != null)
                      {
                          return View("PrintVoucher", existingVoucher);
                      }*/
                    var feeVoucher = new Models.FeeVoucher
                    {
                        VoucherNumber = voucherNumber,
                        TestName = test.TestName,
                        Amount = 5000,
                        ApplicantName = applicantName,
                        isPaid = true,
                        TestId = testId
                    };
                    _test.FeeVoucher.Add(feeVoucher);
                    _test.SaveChanges();
                    return View("PrintVoucher", feeVoucher);
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public async Task<IActionResult> SelectCalendar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("User not found");
            }
            var userId = user.Id;
            var appliedTests = _test.TestCalenders
                .Where(tc => _test.TestApplications.Any(uc => uc.UserId == userId && uc.TestId == tc.TestId && uc.IsVerified == true) && tc.Date.Day >= DateTime.UtcNow.Day)
                .Include(tc => tc.Test)
                .Include(tc => tc.TestCenter)
                .ToList();
            if (appliedTests.Count > 0)
            {
                ViewBag.UserID = userId;
                return View("SelectCalendar", appliedTests);
            }
            return View("NoCalendars");
        }

        [HttpPost]
        public async Task<IActionResult> SelectCalendarUser(int testId, int calendarId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Content("User not found");
            }
            var userId = user.Id;
            var calendarToken = _test.TestCalenders.Where(t => t.Id == calendarId).FirstOrDefault()?.CalendarToken;
            var appliedTest = _test.TestApplications.FirstOrDefault(q => q.UserId == userId && q.TestId == testId && q.CalendarId == null && q.IsVerified == true);
            var calendarCode = _test.TestCalenders.Where(c => c.Id == calendarId).FirstOrDefault()?.Code;
            if (appliedTest != null)
            {
                appliedTest.CalenderToken = calendarToken;
                appliedTest.CalendarId = calendarId;
                appliedTest.CalendarCode = calendarCode;
                _test.SaveChanges();
            }
            else
            {
                return Content("Error from our side!");
            }
            return RedirectToAction("SelectCenter");
        }

        public IActionResult PrintAdmitCard(string testName, DateOnly date, TimeOnly startTime, TimeOnly endTime, string applicantName, string centerName, string centerLocation)
        {
            try
            {
                var admitCard = new AdmitCard
                {
                    ApplicantName = applicantName,
                    TestName = testName,
                    TestCenterName = centerName,
                    TestCenterLocation = centerLocation,
                    Date = date,
                    StartTime = startTime,
                    EndTime = endTime
                };
                _test.AdmitCards.Add(admitCard);
                _test.SaveChanges();
                return View("AdmitCard", admitCard);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public async Task<IActionResult> SubmitFeeDetails(int testId, int voucherNumber, string bankName, string branchName, string branchCode, IFormFile voucherPhoto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Content("User not found");
                }
                var userId = user.Id;
                var appliedTest = _test.TestApplications.FirstOrDefault(q => q.UserId == userId && q.TestId == testId && q.CalendarId == null);

                if (appliedTest != null)
                {
                    appliedTest.VoucherNumber = voucherNumber;
                    appliedTest.BankName = bankName;
                    appliedTest.BranchName = branchName;
                    appliedTest.BranchCode = branchCode;
                    appliedTest.IsPaid = true;
                    appliedTest.IsRejected = false;
                    appliedTest.IsVerified = false;

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(voucherPhoto.FileName);
                    string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "FeeVouchers");
                    Directory.CreateDirectory(uploadFolder);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await voucherPhoto.CopyToAsync(fileStream);
                    }
                    appliedTest.VoucherPhotoPath = Path.Combine("\\FeeVouchers", fileName);
                    _test.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [HttpGet]
        public IActionResult ViewSubmittedApplications()
        {
            try
            {
                var testApplications = _test.TestApplications.Where(w => w.IsPaid == true).Include(tc => tc.Test).ToList();
                return View(testApplications);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [HttpPost]
        public IActionResult VerifyFee(int testId, string userId)
        {
            try
            {
                var testApplication = _test.TestApplications.Where(w => w.TestId == testId && w.UserId == userId && w.IsPaid == true && w.CalendarId == null).FirstOrDefault();
                if (testApplication != null)
                {
                    testApplication.IsVerified = true;
                    _test.SaveChanges();
                }
                return RedirectToAction("ViewSubmittedApplications");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult DumpFee(int testId, string userId)
        {
            try
            {
                var testApplication = _test.TestApplications.Where(w => w.TestId == testId && w.UserId == userId && w.IsPaid == true).FirstOrDefault();
                if (testApplication != null)
                {
                    testApplication.IsVerified = false;
                    testApplication.IsRejected = true;
                    _test.SaveChanges();
                }
                return RedirectToAction("ViewSubmittedApplications");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult SendCenterChangeRequest(int testId, string userId, int newCalendarId, int applicationId)
        {
            try
            {
                var existingRequest = _test.CenterChangeRequests.Any(r => r.ApplicationId == applicationId && r.UserId == userId);
                var existingCalendar = _test.TestApplications.Where(c => c.UserId == userId && c.TestId == testId && c.CalendarId == newCalendarId).FirstOrDefault();

                if (existingRequest)
                {
                    return Json(new { Message = "Request already exists!" });
                }
                if (existingCalendar == null)
                {
                    var centerChangeRequest = new CenterChangeRequest
                    {
                        TestId = testId,
                        UserId = userId,
                        DesiredCalendarId = newCalendarId,
                        ApplicationId = applicationId
                    };

                    _test.CenterChangeRequests.Add(centerChangeRequest);
                    _test.SaveChanges();

                    return RedirectToAction("SelectCenter");
                }
                else
                {
                    return Content("Not Possible!");
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult CenterChangeRequests()
        {
            try
            {
                var centerChangeRequests = _test.CenterChangeRequests.Where(w => w.Approved == false).ToList();
                return View("CenterChangeRequests", centerChangeRequests);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [HttpPost]
        public IActionResult HandleCenterChange(string userId, int testId, int calendarId, string calendarToken, int applicationId)
        {
            try
            {
                var testApplication = _test.TestApplications.FirstOrDefault(w => w.TestId == testId && w.UserId == userId && w.Id == applicationId);
                var request = _test.CenterChangeRequests.FirstOrDefault(w => w.TestId == testId && w.UserId == userId && w.DesiredCalendarId == calendarId && w.Approved == false);
                var calendarCode = _test.TestCalenders.Where(c => c.Id == calendarId).FirstOrDefault()?.Code;
                if (testApplication != null && request != null)
                {
                    testApplication.CalendarId = calendarId;
                    testApplication.CalenderToken = calendarToken;
                    testApplication.HasChangedCenter = true;
                    testApplication.CalendarCode = calendarCode;
                    request.Approved = true;
                    _test.CenterChangeRequests.Remove(request);
                    _test.SaveChanges();
                }
                return RedirectToAction("CenterChangeRequests");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult HandleCenterReject(int testId, string userId, int calendarId)
        {
            try
            {
                var request = _test.CenterChangeRequests.FirstOrDefault(w => w.TestId == testId && w.UserId == userId && w.DesiredCalendarId == calendarId && w.Approved == false);
                if (request != null)
                {
                    _test.CenterChangeRequests.Remove(request);
                    _test.SaveChanges();
                }
                return RedirectToAction("CenterChangeRequests");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult CancleChangeRequest(int testId, string userId, int calendarId, int applicationId)
        {
            try
            {
                var request = _test.CenterChangeRequests.FirstOrDefault(w => w.TestId == testId && w.UserId == userId && w.DesiredCalendarId == calendarId && w.Approved == false && w.ApplicationId == applicationId);
                if (request != null)
                {
                    _test.CenterChangeRequests.Remove(request);
                    _test.SaveChanges();
                }

                return RedirectToAction("SelectCenter");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult Attendance()
        {
            try
            {
                var viewModel = new Test
                {
                    TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                    Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                    TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                    TestCalenders = _test.TestCalenders
                        .Include(td => td.Test)
                        .Include(td => td.TestCenter)
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult TestCalendars(int testId)
        {
            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var testCalendars = _test.TestCalenders
                    .Where(t => t.TestId == testId && t.Date == currentDate)
                    .ToList();

                return View(testCalendars);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult CalendarApplicants(int testId, int id)
        {
            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

                var calendarApplicants = _test.TestApplications
                    .Where(t => t.TestId == testId && t.CalendarId == id && t.IsVerified == true).Select(u => u.UserId)
                    .ToList();
                var applicants = _userManager.Users.Where(u => calendarApplicants.Contains(u.Id)).ToList();
                ViewBag.TestId = testId;

                ViewBag.CalendarId = id;
                var calendarCode = _test.TestCalenders.Where(c => c.Id == id).FirstOrDefault()?.Code;
                var calendarToken = _test.TestCalenders.Where(c => c.Id == id).FirstOrDefault()?.CalendarToken;

                ViewBag.CalendarCode = calendarCode;
                ViewBag.CalendarToken = calendarToken;
                return View(applicants);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult MarkAttendance(string[] userIds, int testId, int code)
        {
            try
            {
                string token = "";
                foreach (var userId in userIds)
                {
                    var testApplication = _test.TestApplications.FirstOrDefault(c => c.UserId == userId && c.TestId == testId && c.IsVerified == true && c.CalendarCode == code);

                    if (testApplication != null)
                    {
                        testApplication.IsPresent = true;
                        token = testApplication.CalenderToken;
                        _test.SaveChanges();
                    }
                }

                return RedirectToAction("AttendanceSheetOpen", new { code, token });
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult MarkAttendanceAdmin(string[] userIds, int testId, int id)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var testApplication = _test.TestApplications.FirstOrDefault(c => c.UserId == userId && c.TestId == testId && c.IsVerified == true && c.CalendarId == id);

                    if (testApplication != null)
                    {
                        testApplication.IsPresent = true;
                        _test.SaveChanges();
                    }
                }
                ViewBag.TestId = testId;

                ViewBag.CalendarId = id;
                var calendarCode = _test.TestCalenders.Where(c => c.Id == id).FirstOrDefault()?.Code;
                ViewBag.CalendarCode = calendarCode;

                return RedirectToAction("CalendarApplicants", new { testId, id });
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public IActionResult AttendanceSheet(string token)
        {
            ViewBag.CalendarToken = token;
            return View();
        }

        public IActionResult AttendanceSheetOpen(string token, int code)
        {
            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

                var calendarApplicants = _test.TestApplications
                    .Where(t => t.CalenderToken == token && t.CalendarCode == code && t.IsVerified == true)
                    .Select(u => u.UserId)
                    .ToList();

                if (calendarApplicants.Count == 0)
                {
                    return Content("Invalid Pin Code!");
                }

                var applicants = _userManager.Users
                    .Where(u => calendarApplicants.Contains(u.Id))
                    .ToList();

                if (applicants.Count == 0)
                {
                    return Content("Error: No matching users found.");
                }

                var testId = _test.TestCalenders
                    .Where(c => c.Code == code)
                    .FirstOrDefault()?.TestId;

                ViewBag.TestId = testId;
                ViewBag.Code = code;

                return View(applicants);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public async Task<IActionResult> SelectCenter()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Content("User not found");
                }
                var userId = user.Id;
                var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var testCalendars = _test.TestCalenders
                    /*                    .Where(c => c.Date >= today)*/
                    .Include(t => t.TestCenter)
                    .ToList();

                ViewBag.Calendars = testCalendars;

                ViewBag.UserId = userId;
                var testApplications = _test.TestApplications.Include(d => d.Test.TestDetails).Include(w => w.Calendar).Where(w => w.UserId == userId && w.IsVerified == true).ToList();
                return View(testApplications);
            }
            catch (Exception e)
            {
                return Json(new { Error = e });
            }
        }

        public IActionResult CancelRequest(string userId, int testId)
        {
            try
            {
                var application = _test.TestApplications.Where(t => t.UserId == userId && t.TestId == testId && t.CalendarId == null && t.IsPaid == false).FirstOrDefault();
                if (application == null)
                {
                    return Content("Application Not Found!");
                }
                _test.TestApplications.Remove(application);
                _test.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
    }
}