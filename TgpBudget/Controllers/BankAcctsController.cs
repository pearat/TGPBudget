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
    public class BankAcctsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BankAccts
        public ActionResult Index()
        {
            return View(db.BankAccts.ToList());
        }

        // GET: BankAccts/Details/5
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null)
            {
                return RedirectToAction("JoinCreate");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAcct bankAcct = db.BankAccts.Find(id);
            if (bankAcct == null)
            {
                return HttpNotFound();
            }
            return View(bankAcct);
        }

        // GET: BankAccts/Create
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null)
            {
                return RedirectToAction("JoinCreate","Households");
            }
            var bankAccount = new BankAcctViewModel();
            bankAccount.HouseholdId = user.Household.Id;
            bankAccount.HouseholdName = user.Household.Name;
            return View(bankAccount);
        }

        // POST: BankAccts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HouseholdId,AccountName,HeldAt,AcctNumber,BalanceCurrent")] BankAcctViewModel bankAcct)
        {
            if (ModelState.IsValid)
            {
                if (db.BankAccts.Any(a => a.AcctNumber == bankAcct.AcctNumber && a.HeldAt == bankAcct.HeldAt))
                {
                    ModelState.AddModelError("AccountName", "Pls enter a unique Account number for this Institution.");
                    return View(bankAcct);
                }

                if (bankAcct.OpeningDate == null)
                    bankAcct.OpeningDate = System.DateTimeOffset.Now;
                if (bankAcct.BalanceOpening != 0)
                {
                    // generate opening transaction
                }
                var newBankAcct = new BankAcct();
                newBankAcct.AccountName = bankAcct.AccountName;
                newBankAcct.AcctNumber = bankAcct.AcctNumber;
                newBankAcct.HeldAt = bankAcct.HeldAt;
                newBankAcct.HouseholdId = bankAcct.HouseholdId;
                newBankAcct.Opened = bankAcct.OpeningDate;
                newBankAcct.BalanceCurrent = bankAcct.BalanceOpening;
                db.BankAccts.Add(newBankAcct);
                db.SaveChanges();
                return RedirectToAction("Index", "Households");
            }

            return View(bankAcct);
        }

        // GET: BankAccts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAcct bankAcct = db.BankAccts.Find(id);
            if (bankAcct == null)
            {
                return HttpNotFound();
            }
            return View(bankAcct);
        }

        // POST: BankAccts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,AccountName,HeldAt,AcctNumber,Created,Closed,BalanceCurrent,BalanceReconciled")] BankAcct bankAcct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bankAcct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bankAcct);
        }

        // GET: BankAccts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAcct bankAcct = db.BankAccts.Find(id);
            if (bankAcct == null)
            {
                return HttpNotFound();
            }
            return View(bankAcct);
        }

        // POST: BankAccts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BankAcct bankAcct = db.BankAccts.Find(id);
            db.BankAccts.Remove(bankAcct);
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
