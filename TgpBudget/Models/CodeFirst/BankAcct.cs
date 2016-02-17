using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class BankAcct
    {
        public BankAcct()
        {
            this.Deals = new HashSet<Deal>();
        }
        public int Id { get; set; }

        [Required]
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string HeldAt { get; set; }
        public DateTimeOffset? Opened { get; set; }
        public DateTimeOffset? Closed { get; set; }
        public decimal BalanceCurrent { get; set; }
        public decimal BalanceReconciled { get; set; }

        [Required]
        public virtual int HouseholdId { get; set; }

        public virtual ICollection<Deal> Deals { get; set; }

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
        [Display(Name = "Opening Balance (can be entered later)")]
        public decimal BalanceOpening { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date for Opening Balance")]
        public DateTimeOffset? OpeningDate { get; set; }

    }
}