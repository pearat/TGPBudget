using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBudget.Helpers;
using TgpBudget.Models;

namespace TgpBudget.Controllers
{
    public enum ReservedAccounts
    {
        deletedAccountHouseholdID = 13,
        newAccountHouseholdId,
    }
    [RequireHttps]
    [AuthorizeHouseholdRequired]
    public class BankAcctsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BankAccts
        public ActionResult Index()
        {

            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null || user.Household==null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View( db.BankAccts.Where(b=>b.HouseholdId==user.HouseholdId).OrderBy(b=>b.AccountName).ToList() );
        }


        // GET: BankAccts
        public ActionResult _Index()
        {

            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null || user.Household == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return PartialView(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId).OrderBy(b => b.AccountName).ToList().Take(4));
        }


        // GET: BankAccts
        public ActionResult Recalc()
        {

            //var user = db.Users.Find(User.Identity.GetUserId());
            
            var Household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            //var HhBanks = Household.BankAccts
            // var HhBanks = db.BankAccts.Where(b => b.HouseholdId == HhId);
            foreach (var bank in Household.BankAccts)
            {
                bank.BalanceCurrent = bank.BalanceReconciled = 0;
                foreach (var deal in bank.Deals)
                {
                    bank.BalanceCurrent += (deal.Category.IsExpense?-1:1)* deal.Amount;
                    if (deal.Reconciled)
                        bank.BalanceReconciled += (deal.Category.IsExpense ? -1 : 1) * deal.Amount;
                }
                
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // GET: BankAccts/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(User.Identity.GetUserId());

            BankAcct bankAcct = db.BankAccts.Find(id);
            if (bankAcct == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Index");
        }

        // GET: BankAccts/Create
        public ActionResult Open()
        {

            if (User == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            if(user==null)
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
        public ActionResult Open([Bind(Include = "HouseholdId,AccountName,HeldAt,AccountNumber,BalanceCurrent")] BankAcctViewModel bankAcct)
        {
            if (ModelState.IsValid)
            {
                bankAcct.AccountName = bankAcct.HeldAt + "::" + bankAcct.AccountNumber;
                
                if (db.BankAccts.Any(a => a.AccountName == bankAcct.AccountName))
                {
                    ModelState.AddModelError("AccountName", "Please enter a unique Account number for this Institution.");
                    return View(bankAcct);
                }

                if (bankAcct.OpeningDate == null)
                    bankAcct.OpeningDate = System.DateTimeOffset.Now;
                if (bankAcct.BalanceOpening != 0)
                {
                    // generate opening transaction
                }
                var newBankAcct = new BankAcct();
                
                newBankAcct.AccountNumber = bankAcct.AccountNumber;
                newBankAcct.HeldAt = bankAcct.HeldAt;
                newBankAcct.AccountName = bankAcct.AccountName;
                newBankAcct.HouseholdId = bankAcct.HouseholdId;
                newBankAcct.Opened = bankAcct.OpeningDate;
                newBankAcct.BalanceCurrent = bankAcct.BalanceOpening;
                db.BankAccts.Add(newBankAcct);
                db.SaveChanges();
                return RedirectToAction("Index", "BankAccts");
            }

            return RedirectToAction("Index", "BankAccts");
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
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,HeldAt,AccountNumber,AccountName,Opened")] BankAcct bankAcct)
        {
            if (ModelState.IsValid)
            {
                db.BankAccts.Attach(bankAcct);
                db.Entry(bankAcct).Property(b => b.AccountNumber).IsModified = true;
                db.Entry(bankAcct).Property(b => b.HeldAt).IsModified = true;
                bankAcct.AccountName = bankAcct.HeldAt + "::" + bankAcct.AccountNumber;
                db.Entry(bankAcct).Property(b => b.AccountName).IsModified = true;
                db.Entry(bankAcct).Property(b => b.Opened).IsModified = true;
                // db.Entry(bankAcct).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // GET: BankAccts/Close/5
        //public ActionResult Close(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BankAcct bankAcct = db.BankAccts.Find(id);
        //    if (bankAcct == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(bankAcct);
        //}

        // POST: BankAccts/Close/5
        //[HttpPost, ActionName("Close")]
        //[ValidateAntiForgeryToken]
        //public ActionResult CloseConfirmed(int id)

        public ActionResult Close(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAcct bankAcct = db.BankAccts.Find(id);
            if (bankAcct.Closed == null)
                bankAcct.Closed = System.DateTimeOffset.Now;
            db.Entry(bankAcct).Property(b => b.Closed).IsModified = true;
            bankAcct.HouseholdId = (int)ReservedAccounts.deletedAccountHouseholdID;

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
