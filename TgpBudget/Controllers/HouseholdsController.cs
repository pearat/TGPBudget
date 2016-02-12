using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBudget.Models;

namespace TgpBudget.Controllers
{
    [RequireHttps]
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

                    return RedirectToAction("Create", "Households");

                }
                else
                {
                    // add error checking to be sure that code exists, matches and hasn't expired
                    user.HouseholdId = db.Invitations.FirstOrDefault(i => i.InvitationCode == user.InvitationCode).HouseholdId;
                    db.SaveChanges();
                }
            }

            //return View(db.Households.ToList());
            return RedirectToAction("EditUserProfile", "Manage");
        }

        // GET: Households/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // GET: Households/Create
        public ActionResult Create()
        {
            ViewBag.Msg = "Welcome to TGP-Budget!";
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Address,TaxId,TryCode,InvitationCode")] HouseholdViewModel HhVM)
        {
            if (ModelState.IsValid)
            {
                var now = System.DateTimeOffset.Now;
                var user = db.Users.Find(User.Identity.GetUserId());
                if (HhVM.InvitationCode == "")
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
                    db.SaveChanges();
                    return RedirectToAction("Index", new { HhId = hh.Id });
                }
                else
                {
                    var invitation = db.Invitations.FirstOrDefault(i => i.InvitationCode == HhVM.InvitationCode);
                    if (invitation == null)
                    {
                        ViewBag.Msg = "Invalid code, please try entering it again.";
                        return View();
                    }
                    if (now> invitation.InvalidAfter)
                    {
                        ViewBag.Msg = "This code has expired, please request a new one and enter it promptly.";
                        return View();
                    }
                    user.HouseholdId = invitation.HouseholdId;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { HhId = user.HouseholdId });
                }

            }

            return View();
        }

        // GET: Households/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated,Address,TaxId")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        // GET: Households/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }



        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            db.Households.Remove(household);
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
