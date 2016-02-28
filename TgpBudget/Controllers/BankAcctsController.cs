using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
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
        public ActionResult Index(DateTime? AsOfDate)
        {
            DateTime today = System.DateTime.Today;
            DateTime ReportAsOf = AsOfDate ?? today;
            ViewBag.AsOfDate = ReportAsOf;
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var bankAcctList = new BankAcctList();
            bankAcctList.BAL = hh.BankAccts.OrderBy(b => b.AccountName).ToList();
            foreach (var item in bankAcctList.BAL)
            {
                bankAcctList.totalAccts.BalanceCurrent += item.BalanceCurrent;
                bankAcctList.totalAccts.BalanceReconciled += item.BalanceReconciled;
            }
            return View(bankAcctList);
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
        public ActionResult GetBankChartData()
        {
            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            var monthStart = new DateTime(System.DateTime.Today.Year - 1, System.DateTime.Today.Month, 1);
            var monthEnd = monthStart.AddMonths(2).AddDays(-1); // end of month
            var currentPeriod = new StartEndDates();
            var trailing12Months = new List<StartEndDates>();
            currentPeriod.start = monthStart.AddYears(-10);
            currentPeriod.end = monthEnd;
            for (int i = 0; i < 12; i++)
            {
                trailing12Months.Add(currentPeriod);
                monthStart = monthEnd.AddDays(1);
                monthEnd = monthStart.AddMonths(1).AddDays(-1);
                currentPeriod = new StartEndDates();
                currentPeriod.start = monthStart;
                currentPeriod.end = monthEnd;
            }
            decimal inflows = 0;
            decimal outflows = 0;
            var bankChartData = (from m in trailing12Months
                                 from b in hh.BankAccts
                                 let aSum = -(from d in b.Deals
                                              where m.start.Date <= d.DealDate.Date &&
                                              m.end.Date >= d.DealDate.Date &&
                                              d.Category.IsExpense == true
                                              select d.Amount).DefaultIfEmpty().Sum()
                                 let bSum = (from d in b.Deals
                                             where m.start.Date <= d.DealDate.Date &&
                                             m.end.Date >= d.DealDate.Date &&
                                            d.Category.IsExpense == false
                                             select d.Amount).DefaultIfEmpty().Sum()
                                 let _ = outflows += aSum
                                 let ___ = inflows += bSum
                                 select new
                                 {
                                     AcctId = b.Id,
                                     AcctName = b.AccountName,
                                     Month = m.end,
                                     Outflows = aSum,
                                     Inflows = bSum
                                 }).ToArray();
            int numAccts = bankChartData.Count() / 12;
            var lineChart = new LineChartWithLegend();
            lineChart.seriesCount = numAccts;
            lineChart.legend = new string[numAccts];
            lineChart.data.labels = new string[12];
            lineChart.data.series = new int[numAccts, 12];
            int x; int plusSum; int minusSum; int total;
            for (int i = 0; i < 12; i++)
            {
                lineChart.data.labels[i] = bankChartData[i * numAccts].Month.ToString("MMM/yy");
                x = 0; plusSum = 0; minusSum = 0; total = 0;
                for (int j = 0; j < numAccts; j++)      // initial assignments of bank balances
                {
                    x = Decimal.ToInt32(Math.Round(bankChartData[i * numAccts + j].Inflows + bankChartData[i * numAccts + j].Outflows));

                    lineChart.data.series[j, i] = x;
                    total += x;
                    if (x > 0)
                        plusSum += x;
                    else
                        minusSum += x;
                    if (i == 0)
                        lineChart.legend[j] = bankChartData[j].AcctName;
                    else
                        lineChart.data.series[j, i] += lineChart.data.series[j, i - 1];
                }
                /* if (minusSum != 0 && plusSum != 0)      // need to net out -balance(s) with +balance(s)
                 {
                     for (int j = 0; j < numAccts; j++)
                     {
                         if (Math.Sign(lineChart.data.series[j, i]) != Math.Sign(total)) // minority value
                             lineChart.data.series[j, i] = 0;  // clear balance of an account in the minority
                         else
                         {
                             if (Math.Sign(total) == 1)        // majority is positive
                             {
                                 if (lineChart.data.series[j, i] > -minusSum)  // able to offset with this balance
                                 {
                                     lineChart.data.series[j, i] += minusSum;
                                     minusSum = 0;
                                     break;
                                 }
                                 else        // partial offset only
                                 {
                                     minusSum += lineChart.data.series[j, i];
                                     lineChart.data.series[j, i] = 0;
                                 }
                             }
                             else        // majority is negative
                             {
                                 if (lineChart.data.series[j, i] < -plusSum)  // able to offset with this balance
                                 {
                                     lineChart.data.series[j, i] += plusSum;
                                     plusSum = 0;
                                     break;
                                 }
                                 else      // partial offset only
                                 {
                                     plusSum += lineChart.data.series[j, i];
                                     lineChart.data.series[j, i] = 0;
                                 }
                             }
                         }
                     }
                 }
             */
            }
            return Content(JsonConvert.SerializeObject(lineChart), "application/json");
        }


        // GET: BankAccts
        public ActionResult Recalc(DateTime? AsOfDate)
        {
            DateTime today = System.DateTime.Today;
            
            AsOfDate = AsOfDate ?? today;
            ViewBag.AsOfDate = AsOfDate;
            //var user = db.Users.Find(User.Identity.GetUserId());

            var hh = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            decimal inflows = 0;
            decimal outflows = 0;
            decimal inflowsReconciled = 0;
            decimal outflowsReconciled = 0;

            var bankBalanceData = (from b in hh.BankAccts
                                   let aSum = -(from x in b.Deals
                                                where x.DealDate <= AsOfDate &&
                                                x.Category.IsExpense
                                                select x.Amount).DefaultIfEmpty().Sum()
                                   let bSum = (from x in b.Deals
                                               where x.DealDate <= AsOfDate &&
                                                !x.Category.IsExpense
                                               select x.Amount).DefaultIfEmpty().Sum()
                                   let cSum = -(from x in b.Deals
                                                where x.DealDate <= AsOfDate &&
                                                x.Reconciled &&
                                                x.Category.IsExpense
                                                select x.Amount).DefaultIfEmpty().Sum()
                                   let dSum = (from x in b.Deals
                                               where x.DealDate <= AsOfDate &&
                                                x.Reconciled &&
                                                !x.Category.IsExpense
                                               select x.Amount).DefaultIfEmpty().Sum()
                                   let __ = outflows += aSum
                                   let ___ = inflows += bSum
                                   let _____ = outflowsReconciled += cSum
                                   let ______ = inflowsReconciled += dSum
                                   select new
                                   {
                                       AcctId = b.Id,
                                       AcctName = b.AccountName,
                                       Outflows = aSum,
                                       Inflows = bSum,
                                       OutflowsRec = cSum,
                                       InflowsRec = dSum
                                   }).ToArray();
            int numAccts = bankBalanceData.Count();
            int i = 0;
            foreach (var bank in hh.BankAccts)
            {
                bank.BalanceCurrent = bankBalanceData[i].Inflows + bankBalanceData[i].Outflows;
                bank.BalanceReconciled = bankBalanceData[i].InflowsRec + bankBalanceData[i].OutflowsRec;
                i++;
            }
            db.SaveChanges();


            //foreach (var bank in hh.BankAccts)
            //{
            //    bank.BalanceCurrent = bank.BalanceReconciled = 0;
            //    foreach (var deal in bank.Deals)
            //    {
            //        bank.BalanceCurrent += (deal.Category.IsExpense ? -1 : 1) * deal.Amount;
            //        if (deal.Reconciled)
            //            bank.BalanceReconciled += (deal.Category.IsExpense ? -1 : 1) * deal.Amount;
            //    }
            //}

            return RedirectToAction("Index","BankAccts", new { AsOfDate = AsOfDate});
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
            if (user == null)
            {
                return RedirectToAction("JoinCreate", "Households");
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
                if (bankAcct.OpeningBalance != 0)
                {
                    // generate opening transaction
                }
                var newBankAcct = new BankAcct();

                newBankAcct.AccountNumber = bankAcct.AccountNumber;
                newBankAcct.HeldAt = bankAcct.HeldAt;
                newBankAcct.AccountName = bankAcct.AccountName;
                newBankAcct.HouseholdId = bankAcct.HouseholdId;
                newBankAcct.Opened = bankAcct.OpeningDate;
                newBankAcct.BalanceCurrent = bankAcct.OpeningBalance;
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
