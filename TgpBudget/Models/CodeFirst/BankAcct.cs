using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class BankAcct:IComparable<BankAcct>
    {
        public BankAcct()
        {
            this.Deals = new HashSet<Deal>();
        }
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string HeldAt { get; set; }
        public DateTimeOffset? Opened { get; set; }
        public DateTimeOffset? Closed { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal BalanceCurrent { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal BalanceReconciled { get; set; }

        [Required]
        public virtual int HouseholdId { get; set; }

        public virtual ICollection<Deal> Deals { get; set; }

        public int CompareTo(BankAcct b)
        {
            return AccountName.CompareTo(b.AccountName);
        }
    }

    public class BankAcctViewModel
    {

        public int HouseholdId { get; set; }
        [Display(Name = "Household Name / Number")]
        public string HouseholdName { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "Account Name (optional)")]
        public string AccountName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [DataType(DataType.Text)]
        [Display(Name = "Bank or Financial Institution")]
        public string HeldAt { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Account Number (or unique identifier)")]
        public string AccountNumber { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:c}")]
        [Display(Name = "Opening Balance (optional)")]
        public decimal OpeningBalance { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date for Opening Balance")]
        public DateTimeOffset? OpeningDate { get; set; }

    }

    public class BankStmt
    {
        
        public DateTime ReportMonth { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal InFlows { get; set; }
        public decimal OutFlows { get; set; }
        public decimal Xfers { get; set; }
        public decimal EndingBalance { get; set; }
    }

    public class AnnualBankStmt
    {
        public int BankAcctId { get; set; }
        public string AccountName { get; set; }
        public List<BankStmt> ABS { get; set; }
    }

    public class LineChart
    {
        public string[] labels { get; set; }
        public int[,] series { get; set; }
    }

    public class LineChartWithLegend
    {
        public LineChartWithLegend()
        {
            data = new LineChart();
        }
        public LineChart data { get; set; }
        public string[] legend { get; set; }
        public int seriesCount { get; set; }
    }
}