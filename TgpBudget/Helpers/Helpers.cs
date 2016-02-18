using Microsoft.AspNet.Identity;
using Owin.Security.Providers.ArcGISOnline.Provider;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TgpBudget.Models;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace TgpBudget.Helpers
{
    public static class Extension
    {
        public static string GetHouseholdId(this IIdentity user)
        {
            var ClaimUser = (ClaimsIdentity)user;
            var Claim = ClaimUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            if (Claim != null)
                return Claim.Value;  
            else
                return "";
        }
        public static bool IsInHousehold(this IIdentity user)
        {
            var cUser = (ClaimsIdentity)user;
            var hid = cUser.Claims.FirstOrDefault(c => cUser.AuthenticationType == "HouseholdId");
            return (hid != null && !string.IsNullOrWhiteSpace(hid.Value));
        }

        public static ICollection<ApplicationUser> UsersInHousehold(this Household h)
        {
            return h.Users.ToList();
        }

        //Helper Extensions
        //var usersInHousehld = hh.UsersInHousehold();
        //where hh is a record of type Household.(edited)

    }

    public class Helper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUser FetchUser(IPrincipal User)
        {

            return db.Users.Find(User.Identity.GetUserId());
        }
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


