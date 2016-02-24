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

    [RequireHttps]
    [Authorize]
    [AuthorizeHouseholdRequired]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            @ViewBag.ActiveHousehold = "";
            if (User != null)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user != null && user.DisplayName != null && user.Household.Name != null)
                    @ViewBag.ActiveHousehold = user.Household.Name;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}