﻿using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TgpBudget.Models;
using TgpBudget.Helpers;

namespace TgpBudget.Controllers
{
    [RequireHttps]
    [Authorize]
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Households
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (user.HouseholdId == null)
            {
                if (user.InvitationCode == null)
                {
                    // Join w/ Code or Create new Household

                    return RedirectToAction("JoinCreate");
                }
                else
                {
                    // add error checking to be sure that code exists, matches and hasn't expired
                    Invitation invitation = db.Invitations.FirstOrDefault(i => i.InvitationCode == user.InvitationCode);
                    if (invitation == null)
                    {
                        ViewBag.Msg = "Invitation code not found, please try again.";
                        return RedirectToAction("JoinCreate");
                    }
                    if (System.DateTimeOffset.Now > invitation.InvalidAfter)
                    {
                        ViewBag.Msg = "Invitation code has expired, please request a new one and enter it promptly.";
                        return RedirectToAction("JoinCreate");
                    }
                    user.HouseholdId = invitation.HouseholdId;
                    db.Invitations.Remove(invitation);
                    db.SaveChanges();
                }
            }
            @ViewBag.ActiveHousehold = user.Household.Name;
            //return View(db.Households.ToList());
            return View();
        }

        // GET: Households/List/5
        [AuthorizeHouseholdRequired]
        public ActionResult ListMembers()
        {
            // var user = db.Users.Find(User.Identity.GetUserId());
            Helper helper = new Helper();
            var user = helper.FetchUser(User);
            if (user == null)
            {
                return RedirectToAction("JoinCreate");
            }
            var household = db.Households.Find(user.HouseholdId);
            var model = household.Users;
            return View(model);

        }

        // GET: Households/Create
        public ActionResult JoinCreate()
        {
            var HhVm = new HouseholdViewModel();
            return View(HhVm);
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinCreate([Bind(Include = "Name,Address,TaxId,TryCode,InvitationCode")] HouseholdViewModel HhVM)
        {
            if (ModelState.IsValid)
            {
                var now = System.DateTimeOffset.Now;
                var user = db.Users.Find(User.Identity.GetUserId());
                if (HhVM.Name != "Demo")
                {
                    var household = new Household();
                    household.Name = HhVM.Name;
                    household.Address = HhVM.Address;
                    household.TaxId = HhVM.TaxId;
                    household.Created = now;
                    db.Households.Add(household);
                    db.SaveChanges();
                    var hh = db.Households.FirstOrDefault(h => h.Name == household.Name);
                    user.HouseholdId = hh.Id;
                    var startingHouseholdId = 14;

                    var startingCategories = db.Categories.Where(c => c.HouseholdId == startingHouseholdId).ToList();

                    foreach (var cat in startingCategories)
                    {
                        var inheritedCategory = new Category();
                        inheritedCategory.HouseholdId = hh.Id;
                        inheritedCategory.Name = cat.Name;
                        inheritedCategory.IsExpense = cat.IsExpense;
                        inheritedCategory.IsProtected = cat.IsProtected;
                        inheritedCategory.BudgetAmount = cat.BudgetAmount;
                        db.Categories.Add(inheritedCategory);
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index"); // , new { HhId = hh.Id });
                }
                else
                {
                    var invitation = db.Invitations.FirstOrDefault(i => i.InvitationCode == HhVM.InvitationCode);
                    if (invitation == null)
                    {
                        ViewBag.Msg = "Invalid code, please try entering it again.";
                        return View(HhVM);
                    }
                    if (now > invitation.InvalidAfter)
                    {
                        ViewBag.Msg = "This code has expired, please request a new one and enter it promptly.";
                        return View(HhVM);
                    }
                    user.HouseholdId = invitation.HouseholdId;
                    db.Invitations.Remove(invitation);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(HhVM);
        }
        // GET: Households/Edit/5
        [AuthorizeHouseholdRequired]
        public ActionResult Edit(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (user == null)
            {
                return RedirectToAction("JoinCreate");
            }
            id = user.HouseholdId;
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeHouseholdRequired]
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated,Address,TaxId")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                household.Updated = System.DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        // GET: Households/Leave/5
        [AuthorizeHouseholdRequired]
        public ActionResult Leave()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (user == null)
            {
                return RedirectToAction("JoinCreate");
            }
            var id = user.HouseholdId;
            Household household = db.Households.Find(id);
            // Household household = user.Household;
            if (household == null)
            {
                return HttpNotFound();
            }
            @ViewBag.ActiveHousehold = "";
            return View(household);
        }



        // POST: Households/Leave/5
        [HttpPost, ActionName("Leave")]
        [ValidateAntiForgeryToken]
        [AuthorizeHouseholdRequired]
        public ActionResult LeaveConfirmed()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            user.HouseholdId = null;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
