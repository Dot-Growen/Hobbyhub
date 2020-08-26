using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // For Password Hashing
using belt_exam.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace belt_exam.Controllers {
    public class HomeController : Controller {

        //*********** CONTEXT
        private MyContext _context;

        public HomeController (MyContext context) {
            _context = context;
        }

        //*********** GET Request
        public IActionResult Index () {
            return View ();
        }

        [HttpGet ("loginpage")]
        public ViewResult LoginPage () {
            return View ();
        }

        [HttpGet ("hobby")]
        public IActionResult Hobby () {
            if (HttpContext.Session.GetInt32 ("UserId") == null) {
                return RedirectToAction ("loginpage");
            } else {
                ViewBag.Novice = _context.Hobbies
                    .Where (h => h.Novice > 0)
                    .OrderByDescending (h => h.Novice)
                    .ToList ();
                ViewBag.Intermediate = _context.Hobbies
                    .Where (h => h.Intermediate > 0)
                    .OrderByDescending (h => h.Intermediate)
                    .ToList ();
                ViewBag.Expert = _context.Hobbies
                    .Where (h => h.Expert > 0)
                    .OrderByDescending (h => h.Expert)
                    .ToList ();

                List<Hobby> AllHobbies = _context.Hobbies
                    .Include (h => h.Enthusiasts)
                    .ThenInclude (h => h.UsersHobbies)
                    .OrderByDescending(h =>h.Enthusiasts.Count)
                    .ToList ();
                return View (AllHobbies);
            }
        }

        [HttpGet ("hobby/{Id}")]
        public IActionResult HobbyView (int Id) {
            if (HttpContext.Session.GetInt32 ("UserId") == null) {
                return RedirectToAction ("loginpage");
            } else {
                List<Association> enthusiast1 = _context.Associations
                    .Include (a => a.HobbiesUsers)
                    .ThenInclude (a => a.addedHobbies)
                    .Where (a => a.HobbyId == Id)
                    .ToList ();

                ViewBag.ViewHobby = _context.Hobbies.SingleOrDefault (u => u.HobbyId == Id);
                ViewBag.User = _context.Users.FirstOrDefault (l => l.UserId == HttpContext.Session.GetInt32 ("UserId"));
                return View (enthusiast1);
            }
        }

        [HttpGet ("new")]
        public IActionResult New () {
            if (HttpContext.Session.GetInt32 ("UserId") == null) {
                return RedirectToAction ("loginpage");
            } else {
                ViewBag.User = _context.Users.SingleOrDefault (u => u.UserId == HttpContext.Session.GetInt32 ("UserId"));
                return View ();
            }
        }

        [HttpGet ("edit/{Id}")]
        public IActionResult Edit (int Id) {
            if (HttpContext.Session.GetInt32 ("UserId") == null) {
                return RedirectToAction ("loginpage");
            } else {
                Hobby ViewHob = _context.Hobbies.SingleOrDefault (h => h.HobbyId == Id);
                return View (ViewHob);
            }
        }

        [HttpGet ("logout")]
        public IActionResult Logout () {
            Console.WriteLine ($"I WAS login. My Id => {HttpContext.Session.GetInt32 ("UserId")}");
            HttpContext.Session.Clear ();
            Console.WriteLine ($"NOW IM out. Id => {HttpContext.Session.GetInt32 ("UserId")}");
            return View ("loginpage");
        }

        [HttpPost ("add")]
        public IActionResult Add (int HobbyId, int UserId, string Proficiency) {
            if (HttpContext.Session.GetInt32 ("UserId") == null) {
                return RedirectToAction ("loginpage");
            } else {
                User ViewUser = _context.Users
                    .Include (u => u.addedHobbies)
                    .ThenInclude (u => u.HobbiesUsers)
                    .SingleOrDefault (u => u.UserId == UserId);
                Hobby ViewHob = _context.Hobbies
                    .Include (h => h.Enthusiasts)
                    .ThenInclude (h => h.UsersHobbies)
                    .SingleOrDefault (h => h.HobbyId == HobbyId);
                if (ViewUser.addedHobbies.All (u => u.HobbyId != ViewHob.HobbyId)) {
                    if (Proficiency == "Novice") {
                        ViewHob.Novice += 1;
                        _context.SaveChanges ();
                    } else if (Proficiency == "Intermediate") {
                        ViewHob.Intermediate += 1;
                        _context.SaveChanges ();
                    } else if (Proficiency == "Expert") {
                        ViewHob.Expert += 1;
                        _context.SaveChanges ();
                    }
                    Association addHobby = new Association ();
                    addHobby.HobbyId = HobbyId;
                    addHobby.UserId = UserId;
                    addHobby.Proficiency = Proficiency;
                    _context.Associations.Add (addHobby);
                    _context.SaveChanges ();
                    return RedirectToAction ("hobby");
                } else {
                    return RedirectToAction ("hobby");
                }
            }
        }

        //*********** POST Request

        [HttpPost ("login")]
        public IActionResult Login (LoginUser log) {
            if (ModelState.IsValid) {
                User userInDb = _context.Users.FirstOrDefault (u => u.UserName == log.LoginUserName);
                Console.WriteLine (userInDb);
                if (userInDb == null) {
                    ModelState.AddModelError ("LoginUserName", "Invalid UserName/Password");
                    return View ("loginpage");
                } else {
                    var hasher = new PasswordHasher<LoginUser> ();
                    var result = hasher.VerifyHashedPassword (log, userInDb.Password, log.LoginPassword);
                    if (result == 0) {
                        ModelState.AddModelError ("LoginUserName", "Invalid UserName/Password");
                        return View ("loginpage");
                    } else {
                        HttpContext.Session.SetInt32 ("UserId", userInDb.UserId);
                        return RedirectToAction ("hobby");
                    }
                }
            } else {
                Console.WriteLine (log.LoginUserName);
                return View ("loginpage");
            }
        }

        [HttpPost ("register")]
        public IActionResult Register (User user) {
            if (ModelState.IsValid) {
                if (_context.Users.Any (u => u.UserName == user.UserName)) {
                    ModelState.AddModelError ("UserName", "UserName already in use!");
                    return View ("Index");
                } else {
                    PasswordHasher<User> Hasher = new PasswordHasher<User> ();
                    user.Password = Hasher.HashPassword (user, user.Password);
                    _context.Users.Add (user);
                    _context.SaveChanges ();
                    HttpContext.Session.SetInt32 ("UserId", user.UserId);
                    Console.WriteLine ($"User id: {user.UserId}\nFirst Name: {user.FirstName}\nLastName: {user.LastName}\nUserName: {user.UserName}\nSessionId: {HttpContext.Session.GetInt32("UserId")}");
                    return RedirectToAction ("hobby");
                }
            } else {
                return View ("Index");
            }
        }

        [HttpPost ("createhobby")]
        public IActionResult CreateHobby (Hobby newHobby) {

            if (ModelState.IsValid) {
                if (_context.Hobbies.Any (h => h.Name == newHobby.Name)) {
                    ModelState.AddModelError ("Name", "Name already in use!");
                    return View ("new");
                } else {
                    _context.Hobbies.Add (newHobby);
                    _context.SaveChanges ();
                    Console.WriteLine (newHobby.Name);
                    return Redirect ($"hobby/{newHobby.HobbyId}");
                }
            } else {
                return View ("New");
            }
        }

        [HttpPost ("edithobby")]
        public IActionResult EditHobby (Hobby update) {
            if (ModelState.IsValid) {
                Hobby GetHobby = _context.Hobbies.FirstOrDefault (h => h.HobbyId == update.HobbyId);
                GetHobby.Name = update.Name;
                GetHobby.Description = update.Description;
                _context.SaveChanges ();
                Console.WriteLine (update.Name);
                return Redirect ("hobby");
            } else {

                return View ("Edit", update);
            }
        }

    }
}