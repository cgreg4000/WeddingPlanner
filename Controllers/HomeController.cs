using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already in use.");
                    return View("Index");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", newUser.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser loginSubmission)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(u => u.Email == loginSubmission.LogEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LogEmail", "Invalid Email/Password");
                    return View("Index");
                }
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

                var result = hasher.VerifyHashedPassword(loginSubmission, userInDb.Password, loginSubmission.LogPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LogEmail", "Invalid Email/Password");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.AllWeddings = _context.Weddings.Include(a => a.Attendees).ThenInclude(a => a.User).OrderBy(w => w.Date).ToList();
            ViewBag.LoggedInUser = _context.Users.Include( s=>s.Attending).ThenInclude(s => s.Wedding).FirstOrDefault(a => a.UserId == (int)HttpContext.Session.GetInt32("UserId"));
            // Console.WriteLine(ViewBag.LoggedInUser.Attending);
            return View();
        }

        [HttpGet("newWedding")]
        public IActionResult NewWedding()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost("createWedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if (ModelState.IsValid)
            {
                if (newWedding.Date.Year > DateTime.Now.Year)
                {   
                    newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");
                    _context.Add(newWedding);
                    _context.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
                else if (newWedding.Date.Year == DateTime.Now.Year)
                {
                    if (newWedding.Date.Month > DateTime.Now.Month)
                    {
                        newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");
                        _context.Add(newWedding);
                        _context.SaveChanges();
                        return RedirectToAction("Dashboard");
                    }
                    else if (newWedding.Date.Month == DateTime.Now.Month)
                    {
                        if (newWedding.Date.Day > DateTime.Now.Day)
                        {   
                            newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");
                            _context.Add(newWedding);
                            _context.SaveChanges();
                            return RedirectToAction("Dashboard");
                        }
                        else
                        {
                            ModelState.AddModelError("Date", "Date must be in the future.");
                            return View("NewWedding");
                        }
                    }
                    else
                    {   
                        ModelState.AddModelError("Date", "Date must be in the future.");
                        return View("NewWedding");
                    }
                }
                else
                {
                    ModelState.AddModelError("Date", "Date must be in the future.");
                    return View("NewWedding");
                }
            }
            else
            {
                return View("NewWedding");
            }
        }

        [HttpGet("wedding/{weddingId}")]
        public IActionResult OneWedding(int weddingId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.OneWedding = _context.Weddings.Include(a => a.Attendees).ThenInclude(b => b.User).FirstOrDefault(w => w.WeddingId == weddingId);
            return View();
        }

        [HttpGet("delete/{weddingId}")]
        public IActionResult DeleteWedding(int weddingId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            Wedding RetrievedWedding = _context.Weddings.SingleOrDefault(w => w.WeddingId == weddingId);
            _context.Weddings.Remove(RetrievedWedding);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpPost("attending")]
        public IActionResult AttendWedding(Attendance newAttendance)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            _context.Attendances.Add(newAttendance);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("notAttending/{attendanceId}")]
        public IActionResult UnAttendWedding(int attendanceId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            Attendance RetrievedAttendance = _context.Attendances.SingleOrDefault(a => a.AttendanceId == attendanceId);
            _context.Attendances.Remove(RetrievedAttendance);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
