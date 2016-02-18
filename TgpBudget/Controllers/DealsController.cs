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
    [Authorize]
    public class DealsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Deals
        public ActionResult Index()
        {
            @ViewBag.ActiveHousehold = "";
            int HhId;
            if (User != null)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user != null && user.DisplayName != null && user.Household.Name != null)
                    @ViewBag.ActiveHousehold = user.Household.Name;
                HhId = (int)user.HouseholdId;
                if (HhId != 0)
                {
                    var hh = db.Households.Find(HhId);
                    var deals = hh.BankAccts.SelectMany(a => a.Deals).OrderByDescending(a => a.DealDate);
                    return View(deals.ToList());
                }
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Deals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deal deal = db.Deals.Find(id);
            if (deal == null)
            {
                return HttpNotFound();
            }
            return View(deal);
        }

        // GET: Deals/New
        public ActionResult New()
        {
            var newDeal = new DealViewModel();
            newDeal.DealDate = System.DateTimeOffset.Now;

            var user = db.Users.Find(User.Identity.GetUserId());

            ViewBag.BankAcctId = new SelectList(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId), "Id", "AccountName");

            // ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");

            ViewBag.ExpenseId = new SelectList(db.Categories.Where(c => c.IsExpense == true), "Id", "Name");
            ViewBag.IncomeId = new SelectList(db.Categories.Where(c => c.IsExpense == false), "Id", "Name");
            return View(newDeal);
        }

        // POST: Deals/New
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New([Bind(Include = "BankAcctId,CategoryId,ExpenseId,DealDate,IncomeToggle,IncomeId,Payee,Description,Amount,Reconciled")] DealViewModel dvm)
        {
            if (ModelState.IsValid)
            {
                Deal deal = new Deal();
                deal.BankAcctId = dvm.BankAcctId;
                if (dvm.IncomeToggle == "Income")
                    deal.CategoryId = dvm.ExpenseId;
                else
                    deal.CategoryId = dvm.IncomeId;
                deal.DealDate = dvm.DealDate;
                deal.Payee = dvm.Payee;
                deal.Description = dvm.Description;
                deal.Amount = dvm.Amount;
                deal.Reconciled = dvm.Reconciled;

                db.Deals.Add(deal);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            var user = db.Users.Find(User.Identity.GetUserId());
            ViewBag.BankAcctId = new SelectList(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId), "Id", "AccountName");
            ViewBag.ExpenseId = new SelectList(db.Categories.Where(c => c.IsExpense == true), "Id", "Name");
            ViewBag.IncomeId = new SelectList(db.Categories.Where(c => c.IsExpense == false), "Id", "Name");

            return View(dvm);
        }

        // GET: Deals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deal deal = db.Deals.Find(id);
            if (deal == null)
            {
                return HttpNotFound();
            }
            var editDeal = new DealViewModel();
            editDeal.Amount = deal.Amount;
            editDeal.BankAcctId = (int)deal.BankAcctId;
            editDeal.CategoryId = deal.CategoryId;
            editDeal.DealDate = deal.DealDate.Date;
            editDeal.Description = deal.Description;
            editDeal.Id = deal.Id;
            
            editDeal.Payee = deal.Payee;
            editDeal.Reconciled = deal.Reconciled;

            var user = db.Users.Find(User.Identity.GetUserId());

            ViewBag.BankAcctId = new SelectList(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId), "Id", "AccountName");
            if (deal.Category.IsExpense)
            {
                ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsExpense == true), "Id", "Name");
                editDeal.IsExpense = true;
            }
            else {
                ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsExpense == false), "Id", "Name");
                editDeal.IsExpense = false;
            }
            return View(editDeal);

        }

        // POST: Deals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Amount,BankAcctId,CategoryId,DealDate,Description,Id,IsExpense,Payee,Reconciled")] DealViewModel dvm)
        {
            if (ModelState.IsValid)
            {
                Deal deal = db.Deals.FirstOrDefault(d => d.Id == dvm.Id);
                if (deal != null)
                {
                    deal.Amount = dvm.Amount;
                    deal.BankAcctId = dvm.BankAcctId;
  
                    deal.CategoryId = dvm.CategoryId;
                    deal.DealDate = dvm.DealDate;
                    deal.Description = dvm.Description;
                    deal.Payee = dvm.Payee;
                    deal.Reconciled = dvm.Reconciled;

                    var oldDeal = db.Deals.AsNoTracking().FirstOrDefault(d => d.Id == dvm.Id);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            ViewBag.BankAcctId = new SelectList(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId), "Id", "AccountName");
            if (dvm.IsExpense)
            {
                ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.IsExpense == true), "Id", "Name");
            }
            else {
                ViewBag.IncomeId = new SelectList(db.Categories.Where(c => c.IsExpense == false), "Id", "Name");
            }
            return View(dvm);
        }

        // GET: Deals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deal deal = db.Deals.Find(id);
            if (deal == null)
            {
                return HttpNotFound();
            }
            return View(deal);
        }

        //// POST: Deals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Deal deal = db.Deals.Find(id);
            db.Deals.Remove(deal);
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
