using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Runtime.InteropServices;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBudget.Models;
using TgpBudget.Helpers;
using Microsoft.AspNet.Identity;

namespace TgpBudget.Controllers
{
    [RequireHttps]
    [AuthorizeHouseholdRequired]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private List<string> protectedNameList = new List<string>(new string[] { "Other Income", "Other Expense", "Total" });

        public enum DateRange
        {
            currentMonth,
            priorMonth,
            last30Days,
            avg90Days
        };

        // GET: Categories
        public ActionResult Index(DateRange? period)
        {
            DateRange actualDates = period ?? DateRange.currentMonth;
            ViewBag.Period = actualDates;
            DateTimeOffset endDate = System.DateTimeOffset.Now;
            var startDate = new DateTimeOffset();
            var firstOfMonth = new DateTime(endDate.Year, endDate.Month, 1);
            switch (actualDates)
            {
                case DateRange.currentMonth:
                    startDate = firstOfMonth;
                    break;
                case DateRange.priorMonth:

                    startDate = firstOfMonth.AddMonths(-1);
                    endDate = firstOfMonth.AddDays(-1);
                    break;
                case DateRange.last30Days:
                    startDate = endDate.AddDays(-30);
                    break;
                case DateRange.avg90Days:
                    startDate = endDate.AddDays(-90);
                    break;
                default:
                    startDate = firstOfMonth;
                    break;
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            @ViewBag.ActiveHousehold = user.Household.Name;
            int? HhId = Convert.ToInt32(User.Identity.GetHouseholdId());
            //var categories = db.Categories.Where(c => c.HouseholdId ==user.HouseholdId).OrderBy(c=>c.Name).ToList();
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var deals = hh.BankAccts.SelectMany(a => a.Deals).OrderByDescending(a => a.DealDate).ToList();
            var categories = db.Categories.Where(c => c.HouseholdId == HhId).OrderBy(c => c.IsExpense).ThenBy(c => c.Name).ToList();
            var categoryViewModelList = new List<CategoryViewModel>();
            decimal totalActual = 0;
            decimal totalBudget = 0;
            decimal totalReconciled = 0;
            foreach (var cat in categories)
            {
                if (cat.IsExpense)
                    break;
                var cVM = new CategoryViewModel();
                cVM.category = cat;

                foreach (var d in deals)
                {
                    if (d.DealDate >= startDate && d.DealDate <= endDate)
                    {
                        if (d.CategoryId == cat.Id)
                        {
                            cVM.ActualAmount += d.Amount;
                            if (d.Reconciled)
                                cVM.ReconciledAmount += d.Amount;
                        }
                    }
                }
                if (actualDates == DateRange.avg90Days)
                {
                    cVM.ActualAmount /= 3;
                    cVM.ReconciledAmount /= 3;
                }
                cVM.UnreconciledAmount = cVM.ActualAmount - cVM.ReconciledAmount;
                cVM.Variance = cVM.ActualAmount - cat.BudgetAmount;
                categoryViewModelList.Add(cVM);
                totalActual += cVM.ActualAmount;
                totalBudget += cVM.category.BudgetAmount;
                totalReconciled += cVM.ReconciledAmount;
            }
            var iVM = new CategoryViewModel();
            iVM.category = new Category();
            iVM.category.Name = "Total Income";
            iVM.category.BudgetAmount = totalBudget;
            iVM.ActualAmount = totalActual;
            iVM.ReconciledAmount = totalReconciled;
            iVM.Variance = totalActual - totalBudget;
            iVM.UnreconciledAmount = totalActual - totalReconciled;
            iVM.IsTotal = true;
            categoryViewModelList.Add(iVM);

            totalActual = totalBudget = 0;
            foreach (var cat in categories)
            {
                if (!cat.IsExpense)
                    continue;
                var cVM = new CategoryViewModel();
                cVM.category = cat;

                foreach (var d in deals)
                {
                    if (d.DealDate >= startDate && d.DealDate <= endDate)
                    {
                        if (d.CategoryId == cat.Id)
                        {
                            cVM.ActualAmount -= d.Amount;
                            if (d.Reconciled)
                                cVM.ReconciledAmount -= d.Amount;
                        }
                    }
                }
                if (actualDates == DateRange.avg90Days)
                {
                    cVM.ActualAmount /= 3;
                    cVM.ReconciledAmount /= 3;
                }

                cVM.category.BudgetAmount *= -1;
                cVM.UnreconciledAmount = cVM.ActualAmount - cVM.ReconciledAmount;
                cVM.Variance = cVM.ActualAmount - cat.BudgetAmount;
                categoryViewModelList.Add(cVM);
                totalActual += cVM.ActualAmount;
                totalBudget += cVM.category.BudgetAmount;
                totalReconciled += cVM.ReconciledAmount;
            }
            var eVM = new CategoryViewModel();
            eVM.category = new Category();
            eVM.category.Name = "Total Expense";
            eVM.category.BudgetAmount = totalBudget;
            eVM.ActualAmount = totalActual;
            eVM.Variance = totalActual - totalBudget;
            eVM.ReconciledAmount = totalReconciled;
            eVM.UnreconciledAmount = totalActual - totalReconciled;

            eVM.IsTotal = true;
            categoryViewModelList.Add(eVM);

            var tVM = new CategoryViewModel();
            tVM.category = new Category();
            tVM.category.Name = "Combined Total";
            tVM.category.BudgetAmount = iVM.category.BudgetAmount + eVM.category.BudgetAmount;
            tVM.ActualAmount = iVM.ActualAmount + eVM.ActualAmount;
            tVM.Variance = iVM.Variance + eVM.Variance;
            tVM.ReconciledAmount = iVM.ReconciledAmount + eVM.ReconciledAmount;
            tVM.UnreconciledAmount = iVM.UnreconciledAmount + eVM.UnreconciledAmount;
            tVM.IsTotal = true;

            categoryViewModelList.Add(tVM);

            return View(categoryViewModelList);
        }











        /*
        // GET: Categories
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            @ViewBag.ActiveHousehold = user.Household.Name;
            int? HhId = Convert.ToInt32(User.Identity.GetHouseholdId());
            
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var deals = hh.BankAccts.SelectMany(a => a.Deals).OrderByDescending(a => a.DealDate).ToList();
            var categories = db.Categories.Where(c => c.HouseholdId == HhId).OrderBy(c => c.IsExpense).ThenBy(c => c.Name).ToList();
            var catCol = new CategoryCollection();
            catCol.IncomeCategories = new List<CategoryViewModel>();
            catCol.ExpenseCategories = new List<CategoryViewModel>();
            catCol.IncomeTotal = new CategoryViewModel();
            catCol.ExpenseTotal = new CategoryViewModel();
            catCol.GrandTotal = new CategoryViewModel();
            decimal totalActual = 0;
            decimal totalBudget = 0;
            foreach (var cat in categories)
            {
                if (cat.IsExpense)
                    break;
                var cVM = new CategoryViewModel();
                cVM.category = cat;

                foreach (var d in deals)
                {
                    if (d.CategoryId == cat.Id)
                        cVM.ActualAmount += d.Amount;
                }
                cVM.Variance = cVM.ActualAmount - cat.BudgetAmount;
                catCol.IncomeCategories.Add(cVM);
                totalActual += cVM.ActualAmount;
                totalBudget += cVM.category.BudgetAmount;
            }
            //var iVM = new CategoryViewModel();
            catCol.IncomeTotal.category = new Category();
            catCol.IncomeTotal.category.Name = "Total Income";
            catCol.IncomeTotal.category.BudgetAmount = totalBudget;
            catCol.IncomeTotal.ActualAmount = totalActual;
            catCol.IncomeTotal.Variance = totalActual - totalBudget;
            //catCol.IncomeTotal.IsTotal = true;
            // categoryViewModelList.Add(iVM);

            totalActual = totalBudget = 0;
            foreach (var cat in categories)
            {
                if (!cat.IsExpense)
                    continue;
                var cVM = new CategoryViewModel();
                cVM.category = cat;

                foreach (var d in deals)
                {
                    if (d.CategoryId == cat.Id)
                        cVM.ActualAmount += d.Amount;
                }

                cVM.category.BudgetAmount *= -1;
                cVM.ActualAmount *= -1;
                cVM.Variance = cVM.ActualAmount - cat.BudgetAmount;
                catCol.ExpenseCategories.Add(cVM);
                totalActual += cVM.ActualAmount;
                totalBudget += cVM.category.BudgetAmount;
            }
            // var eVM = new CategoryViewModel();
            catCol.ExpenseTotal.category = new Category();
            catCol.ExpenseTotal.category.Name = "Total Expense";
            catCol.ExpenseTotal.category.BudgetAmount = totalBudget;
            catCol.ExpenseTotal.ActualAmount = totalActual;
            catCol.ExpenseTotal.Variance = totalActual - totalBudget;
            //catCol.ExpenseTotal.IsTotal = true;
            // categoryViewModelList.Add(eVM);

            var tVM = new CategoryViewModel();
            catCol.GrandTotal.category = new Category();
            catCol.GrandTotal.category.Name = "Combined Total";
            catCol.GrandTotal.category.BudgetAmount = catCol.IncomeTotal.category.BudgetAmount + catCol.ExpenseTotal.category.BudgetAmount;
            catCol.GrandTotal.ActualAmount = catCol.IncomeTotal.ActualAmount + catCol.ExpenseTotal.ActualAmount;
            catCol.GrandTotal.Variance = catCol.IncomeTotal.Variance + catCol.ExpenseTotal.Variance;
            //catCol.GrandTotal.IsTotal = true;
            // categoryViewModelList.Add(tVM);

            return View(catCol);
        }
        */



        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            Category category = new Category();
            var user = db.Users.Find(User.Identity.GetUserId());
            category.HouseholdId = user.HouseholdId;
            category.IsExpense = true;

            return View(category);
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,HouseholdId,Name,IsExpense,BudgetAmount")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (db.Categories.Any(c => c.Name == category.Name && c.HouseholdId == category.HouseholdId))
                {
                    ModelState.AddModelError("Name", "This category already exists.  Please enter a unique category name");
                    return View(category);
                }
                // if (category.Name=="Other Expense"|| category.Name == "Other Income")
                if (protectedNameList.Contains(category.Name, StringComparer.OrdinalIgnoreCase))
                {
                    category.IsProtected = true;
                }
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", category.HouseholdId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }
            TempData["OriginalCatName"] = category.Name;
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,Name,IsExpense,IsProtected,BudgetAmount")] Category category)
        {
            if (ModelState.IsValid)
            {
                var originalCategoryName = TempData["OriginalCatName"];
                if (category.Name != (string)originalCategoryName)
                {
                    if (db.Categories.Any(c => c.Name == category.Name && c.HouseholdId == category.HouseholdId))
                    {
                        ModelState.AddModelError("Name", "This category already exists.  Please enter a unique category name");
                        return View(category);
                    }
                }
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Category category = db.Categories.Find(id);
        //    if (category == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    // TempData["OriginalCatName"] = category.Name;
        //    return View(category);
        //}

        // POST: Categories/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category.IsProtected)
            {
                ModelState.AddModelError("Name", "This category is cannot be deleted");
                return View(category);
            }
            //int HhId = Convert.ToInt32(User.Identity.GetHouseholdId());
            //var hh = db.Households.Find(HhId);
            var user = db.Users.Find(User.Identity.GetUserId());
            var hh = db.Households.Find(user.HouseholdId);


            var dealsInHh = hh.BankAccts.SelectMany(a => a.Deals);
            if (category.IsExpense)
            {
                Category otherExpense = db.Categories.FirstOrDefault(c => c.Name == "Other Expense" &&
                                                                    c.HouseholdId == category.HouseholdId);
                if (otherExpense == null)
                {
                    otherExpense = new Category();
                    otherExpense.IsExpense = true;
                    otherExpense.IsProtected = true;
                    otherExpense.Name = "Other Expense";
                    otherExpense.HouseholdId = category.HouseholdId;
                    db.Categories.Add(otherExpense);
                    // db.SaveChanges();
                }
                foreach (var d in dealsInHh)
                {
                    if (d.CategoryId == category.Id)
                        d.CategoryId = otherExpense.Id;
                }
            }
            else
            {
                Category otherIncome = db.Categories.FirstOrDefault(c => c.Name == "Other Income" &&
                                                                    c.HouseholdId == category.HouseholdId);
                if (otherIncome == null)
                {
                    otherIncome = new Category();
                    otherIncome.IsExpense = false;
                    otherIncome.IsProtected = true;
                    otherIncome.Name = "Other Income";
                    otherIncome.HouseholdId = category.HouseholdId;
                    db.Categories.Add(otherIncome);
                    //db.SaveChanges();
                }
                foreach (var d in dealsInHh)
                {
                    if (d.CategoryId == category.Id)
                        d.CategoryId = otherIncome.Id;
                }
            }
            db.Categories.Remove(category);
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
