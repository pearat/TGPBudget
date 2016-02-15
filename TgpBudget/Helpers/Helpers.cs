using Microsoft.AspNet.Identity;
using Owin.Security.Providers.ArcGISOnline.Provider;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TgpBudget.Models;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace TgpBudget.Helpers
    {
    public class Helper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUser FetchUser(IPrincipal User)
        {

            return db.Users.Find(User.Identity.GetUserId());
        }

        //public ApplicationUser GetCurentUser()
        //{
        //    return db.Users.Find(User.Identity.GetUserId());
        //}

        //    public Household GetHousehold(int hhId)
        //    {
        //        return db.Households.Find(hhId);
        //    }

        //    public Household GetHouseholdFromUser(string userId)
        //    {
        //        return db.Households.Find(db.Users.Find(userId));
        //    }

    }
}

