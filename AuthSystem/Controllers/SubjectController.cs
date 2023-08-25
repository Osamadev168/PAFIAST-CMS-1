using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly AuthDbContext _test;

        public SubjectController(AuthDbContext test)
        {
            _test = test;
        }

        [Authorize]
        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Index(int SubjectId)
        {
            var subjects = _test.Subjects.ToList();
            var QuestionsCount = _test.MCQs.Count(q => q.SubjectId == SubjectId);
            var viewModel = new Subject
            {
                Subjects = subjects
            };
            return View(viewModel);
        }

        [Authorize]
        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Subjects()
        {
            var subjects = _test.Subjects.ToList();
            var viewModel = new Subject
            {
                Subjects = subjects
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Super Admin")]
        [HttpPost]
        public IActionResult Create(Subject model)
        {
            Subject newSubject = new Subject { SubjectName = model.SubjectName.Trim().Replace(" ", "-") };

            _test.Subjects.Add(newSubject);

            {
                _test.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult SelectSubject(int subjectId)
        {
            HttpContext.Session.SetInt32("SelectedSubjectId", subjectId);

            return RedirectToAction("Options");
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult DeleteSubject(int id)
        {
            var SubjectData = _test.Subjects.Find(id);
            var MCQ = _test.MCQs.Where(x => x.SubjectId == id);
            var FIB = _test.Blanks.Where(x => x.SubjectId == id);
            if (SubjectData == null)
            {
                return NotFound();
            }
            _test.Subjects.Remove(SubjectData);
            _test.MCQs.RemoveRange(MCQ);
            _test.Blanks.RemoveRange(FIB);
            _test.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Options()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult ViewQuestions(int subjectId)
        {


            var Questions_MCQ = _test.MCQs.Where(x => x.SubjectId == subjectId).Include(x => x.Subject);
           
    
            string subjectName = _test.Subjects.Where(s => s.SubjectId == subjectId).FirstOrDefault().SubjectName;
            ViewBag.SubjectId = subjectId;
            ViewBag.SubjectName = subjectName;


            return View(Questions_MCQ);
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Type()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult ViewQuestionsFIB(int subjectId)
        {

            var Questions_Blanks = _test.Blanks.Where(x => x.SubjectId == subjectId).Include(x => x.Subject);
            return View(Questions_Blanks);
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult GetQuestionsCount(int SubjectId)
        {
            var count = _test.MCQs.Count(q => q.SubjectId == SubjectId);
            var data = new { count = count };
            return Json(data);
        }
    }
}