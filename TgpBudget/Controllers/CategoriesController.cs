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
using Newtonsoft.Json;

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
            avg90Days,
            currentMonthPartial,
            priorMonthPartial,
            last30DaysPartial,
            avg90DaysPartial
        };


        public startEndDates GetDateRange(DateRange? period)
        {

            DateRange actualDates = period ?? DateRange.currentMonth;

            // ViewBag.Period = actualDates;
            DateTime endDate = System.DateTime.Now;
            var startDate = new DateTime();
            var firstOfMonth = new DateTime(endDate.Year, endDate.Month, 1);
            switch (actualDates)
            {
                case DateRange.currentMonth:
                case DateRange.currentMonthPartial:
                    startDate = firstOfMonth;
                    ViewBag.Period = "Current Month";
                    break;
                case DateRange.priorMonth:
                case DateRange.priorMonthPartial:
                    startDate = firstOfMonth.AddMonths(-1);
                    endDate = firstOfMonth.AddDays(-1);
                    ViewBag.Period = "Prior Month";
                    break;
                case DateRange.last30Days:
                case DateRange.last30DaysPartial:
                    startDate = endDate.AddDays(-30);
                    ViewBag.Period = "Last 30 Days";
                    break;
                case DateRange.avg90Days:
                case DateRange.avg90DaysPartial:
                    startDate = endDate.AddDays(-90);
                    ViewBag.Period = "90 day Average";
                    break;
                default:
                    startDate = firstOfMonth;
                    break;
            }
            var range = new startEndDates();
            range.start = startDate;
            range.end = endDate;
            return range;
        }



        public CatDisplayVM CategoryBudget(DateTime startDate, DateTime endDate)
        {
            int? HhId = Convert.ToInt32(User.Identity.GetHouseholdId());
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var deals = hh.BankAccts.SelectMany(a => a.Deals).OrderByDescending(a => a.DealDate).ToList();
            var categories = db.Categories.Where(c => c.HouseholdId == HhId).OrderBy(c => c.IsExpense).ThenBy(c => c.Name).ToList();
            var catDVM = new CatDisplayVM();
            decimal totalActual = 0;
            decimal totalBudget = 0;
            decimal totalReconciled = 0;
            foreach (var cat in categories)
            {
                if (cat.IsExpense)
                    break;
                var cVM = new CatDisplay();

                cVM.Id = cat.Id;
                cVM.Name = cat.Name;
                cVM.Budget = cat.BudgetAmount;

                foreach (var d in deals)
                {
                    if (d.DealDate >= startDate && d.DealDate <= endDate)
                    {
                        if (d.CategoryId == cat.Id)
                        {
                            cVM.Actual += d.Amount;
                            if (d.Reconciled)
                                cVM.Reconciled += d.Amount;
                        }
                    }
                }
                if ((endDate - startDate).TotalDays > 80)
                {
                    cVM.Actual /= 3;
                    cVM.Reconciled /= 3;
                }
                cVM.Unreconciled = cVM.Actual - cVM.Reconciled;
                cVM.Variance = cVM.Actual - cat.BudgetAmount;
                catDVM.IncCats.Add(cVM);
                totalActual += cVM.Actual;
                totalBudget += cVM.Budget;
                totalReconciled += cVM.Reconciled;
            }
            catDVM.IncTotal.Name = "Total Income";
            catDVM.IncTotal.Reconciled = totalReconciled;
            catDVM.IncTotal.Unreconciled = totalActual - totalReconciled;
            catDVM.IncTotal.Actual = totalActual;
            catDVM.IncTotal.Budget = totalBudget;
            catDVM.IncTotal.Variance = totalActual - totalBudget;
            catDVM.IncTotal.IsTotal = true;
            totalActual = totalBudget = totalReconciled = 0;
            foreach (var cat in categories)
            {
                if (!cat.IsExpense)
                    continue;
                var cVM = new CatDisplay();
                cVM.Id = cat.Id;
                cVM.Name = cat.Name;
                cVM.Budget = cat.BudgetAmount;

                foreach (var d in deals)
                {
                    if (d.DealDate >= startDate && d.DealDate <= endDate)
                    {
                        if (d.CategoryId == cat.Id)
                        {
                            cVM.Actual -= d.Amount;
                            if (d.Reconciled)
                                cVM.Reconciled -= d.Amount;
                        }
                    }
                }
                if ((endDate - startDate).TotalDays > 80)
                {
                    cVM.Actual /= 3;
                    cVM.Reconciled /= 3;
                }
                cVM.Budget *= -1;
                cVM.Unreconciled = cVM.Actual - cVM.Reconciled;
                cVM.Variance = cVM.Actual - cat.BudgetAmount;
                catDVM.ExpCats.Add(cVM);
                totalActual += cVM.Actual;
                totalBudget += cVM.Budget;
                totalReconciled += cVM.Reconciled;
            }
            catDVM.ExpTotal.Name = "Total Expense";
            catDVM.ExpTotal.Reconciled = totalReconciled;
            catDVM.ExpTotal.Unreconciled = totalActual - totalReconciled;
            catDVM.ExpTotal.Actual = totalActual;
            catDVM.ExpTotal.Budget = totalBudget;
            catDVM.ExpTotal.Variance = totalActual - totalBudget;
            catDVM.ExpTotal.IsTotal = true;
            catDVM.NetTotal.Name = "Combined Total";
            catDVM.NetTotal.Reconciled = catDVM.IncTotal.Reconciled + catDVM.ExpTotal.Reconciled;
            catDVM.NetTotal.Unreconciled = catDVM.IncTotal.Unreconciled + catDVM.ExpTotal.Unreconciled;
            catDVM.NetTotal.Actual = catDVM.IncTotal.Actual + catDVM.ExpTotal.Actual;
            catDVM.NetTotal.Budget = catDVM.IncTotal.Budget + catDVM.ExpTotal.Budget;
            catDVM.NetTotal.Variance = catDVM.IncTotal.Variance + catDVM.ExpTotal.Variance;
            catDVM.NetTotal.IsTotal = true;
            return catDVM;
        }



        public ActionResult Index(DateRange? period)
        {

            DateRange actualDates = period ?? DateRange.currentMonth;
            var range = GetDateRange(actualDates);
            ViewBag.Period = actualDates;

            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            @ViewBag.ActiveHousehold = hh.Name;
            var catDVM = CategoryBudget(range.start, range.end);
            if (actualDates > DateRange.avg90Days)
            {
                return PartialView("_Index", catDVM);
            }
            else {
                return View("Index", "Categories", catDVM);
            }
        }


        public ActionResult Details(DateRange? period)
        {
            if (period == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DateRange actualDates = period ?? DateRange.currentMonth;
            var range = GetDateRange(actualDates);
            ViewBag.Period = actualDates;

            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            @ViewBag.ActiveHousehold = hh.Name;
            var catDVM = CategoryBudget(range.start, range.end);

            Category category = db.Categories.Find(32);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(catDVM);
        }


        //// GET: Categories/Details/5
        //public ActionResult Details(int? id)
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
        //    return View(category);
        //}

        public ActionResult List(DateRange? period)
        {

            DateRange actualDates = period ?? DateRange.currentMonth;
            var range = GetDateRange(actualDates);
            ViewBag.Period = actualDates;

            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            @ViewBag.ActiveHousehold = hh.Name;
            var catDVM = CategoryBudget(range.start, range.end);
            if (actualDates > DateRange.avg90Days)
            {
                return PartialView("_Index", catDVM);
            }
            else {
                return View("List", "Categories", catDVM);
            }
        }

        // GET: CategoryPieChart
        public ActionResult GetCategoryPieChart()
        {
            var range = GetDateRange(DateRange.avg90Days);

            var catDVM = CategoryBudget(range.start, range.end);
            var incomeCategories = catDVM.IncCats.OrderByDescending(a => a.Actual);
            var IEPie = new IncExpPieChart();
            var numCats = catDVM.IncCats.Count();
            IEPie.income.labels = new string[numCats];
            IEPie.income.series = new int[numCats];
            IEPie.income.seriesCount = numCats;

            decimal sum = 0;
            foreach (var item in incomeCategories)
            {
                sum += item.Actual;     // calculate sum in order to normalize values
            }
            int i = 0;
            foreach (var item in incomeCategories)
            {
                IEPie.income.series[i] = Decimal.ToInt32(Math.Round(100 * item.Actual / sum)); // normalize to 100
                IEPie.income.labels[i] = item.Name;
                i++;
            }

            var expenseCategories= catDVM.ExpCats.OrderBy(a => a.Actual);
            numCats = catDVM.ExpCats.Count();
            IEPie.expense.labels = new string[numCats];
            IEPie.expense.series = new int[numCats];
            IEPie.expense.seriesCount = numCats;
            sum = 0;
            foreach (var item in expenseCategories)
            {
                sum += item.Actual;     // calculate sum in order to normalize values
            }
            i = 0;
            foreach (var item in expenseCategories)
            {
                IEPie.expense.series[i] = Decimal.ToInt32(Math.Round(100 * item.Actual / sum)); // normalize to 100
                IEPie.expense.labels[i] = item.Name;
                i++;
            }
            return Content(JsonConvert.SerializeObject(IEPie), "application/json");
        }


        public enum ActVar
        {
            IncBudget,
            IncActual,
            ExpBudget,
            ExpActual,
            NetVariance
        };

        // GET: IncomeExpense
        public ActionResult GetIncExpBarChart()
        {
            int numPeriods = 4;
            int numSeries = 5;
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var deals = hh.BankAccts.SelectMany(a => a.Deals).OrderBy(a => a.DealDate);
            var startingMonth = new DateTime(System.DateTime.Today.Year - 1, System.DateTime.Today.Month, 1);
            var currentMonth = startingMonth.AddMonths(10).AddDays(-1); // end of month
            var trailingMonths = new List<DateTime>();
            for (int i = 0; i < numPeriods; i++)
            {
                trailingMonths.Add(currentMonth);
                currentMonth = currentMonth.AddMonths(1);
            }
            decimal inflows = 0;
            decimal outflows = 0;
            var barChartData = (from m in trailingMonths
                                let aSum = -(from d in deals
                                             where d.DealDate.Month == m.Month && d.Category.IsExpense == true
                                             select d.Amount).DefaultIfEmpty().Sum()
                                let bSum = (from d in deals
                                            where d.DealDate.Month == m.Month && d.Category.IsExpense == false
                                            select d.Amount).DefaultIfEmpty().Sum()
                                let _ = outflows += aSum
                                let ___ = inflows += bSum

                                select new
                                {
                                    Month = m,
                                    Outflows = aSum,
                                    Inflows = bSum,
                                }).ToArray();

            decimal totalBudgetExpense = 0;
            decimal totalBudgetIncome = 0;

            foreach (var cat in hh.Categories)
            {
                if (cat.IsExpense)
                    totalBudgetExpense -= cat.BudgetAmount;
                else
                    totalBudgetIncome += cat.BudgetAmount;
            }
            var barChart = new BarChartWithLegend();
            barChart.seriesCount = barChartData.Count();
            barChart.legend = new string[barChart.seriesCount];  // not being used at present   

            barChart.data.labels = new string[numPeriods];
            barChart.data.series = new int[numSeries, numPeriods];
            var catCount = hh.Categories.Count();
            for (int i = 0; i < numPeriods; i++)
            {

                barChart.data.labels[i] = barChartData[i].Month.ToString("MMM/yy");
                barChart.data.series[(int)ActVar.IncActual, i] = Decimal.ToInt32(Math.Round(barChartData[i].Inflows));

                barChart.data.series[(int)ActVar.IncBudget, i] = Decimal.ToInt32(Math.Round(totalBudgetIncome));
                barChart.data.series[(int)ActVar.ExpActual, i] = Decimal.ToInt32(Math.Round(barChartData[i].Outflows));

                barChart.data.series[(int)ActVar.ExpBudget, i] = Decimal.ToInt32(Math.Round(totalBudgetExpense));
                barChart.data.series[(int)ActVar.NetVariance, i] = barChart.data.series[(int)ActVar.IncActual, i] - barChart.data.series[(int)ActVar.IncBudget, i] +
                                                             barChart.data.series[(int)ActVar.ExpActual, i] - barChart.data.series[(int)ActVar.ExpBudget, i];
            }
            return Content(JsonConvert.SerializeObject(barChart), "application/json");
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
                return RedirectToAction("Details",new { period = "currentMonth" });
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
                return RedirectToAction("Details",new { period = "currentMonth" });
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
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
            // TempData["OriginalCatName"] = category.Name;
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)

        //public ActionResult Delete(int? id)
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
            return RedirectToAction("Details",new { period="currentMonth" });
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
