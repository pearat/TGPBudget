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
    public class DealsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Deals
        public ActionResult Index()
        {
            @ViewBag.ActiveHousehold = "";
            if (User != null)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user != null && user.DisplayName != null && user.Household.Name != null)
                    @ViewBag.ActiveHousehold = user.Household.Name;
            }
            
            var deals = db.Deals.Include(d => d.BankAcct).Include(d => d.Category);
            return View(deals.ToList());
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
            
            ViewBag.BankAcctName = new SelectList(db.BankAccts.Where(b => b.HouseholdId == user.HouseholdId), "Id", "AccountName");


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
        public ActionResult New([Bind(Include = "ExpenseId,DealDate,IsExpense,IncomeId,Payee,Description,Amount,Reconciled")] DealViewModel dvm)
        {
            if (ModelState.IsValid)
            {
                Deal deal = new Deal();
                deal.BankAcctId = db.BankAccts.FirstOrDefault(b => b.AccountName == dvm.BankAcctName).Id;
                if (dvm.IsExpense)
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
            ViewBag.BankAcctId = new SelectList(db.BankAccts, "Id", "AccountName", deal.BankAcctId);

            return View(deal);
        }

        // POST: Deals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BankAcctId,CategoryId,Created,Payee,Description,Amount,Reconciled")] Deal deal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(deal);
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
