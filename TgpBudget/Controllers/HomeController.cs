using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TgpBudget.Helpers;
using TgpBudget.Models;

namespace TgpBudget.Controllers
{


    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [RequireHttps]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [RequireHttps]
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Dashboard()
        {
            if (User == null)
            {
                return RedirectToAction("Login", "User");
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null || user.DisplayName == null)
            {
                return RedirectToAction("Login", "User");
            }
            if (user.HouseholdId == null)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                @ViewBag.ActiveHousehold = user.Household.Name;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}