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
        public int HouseholdId { get; set; }
        [Required]
        public string AccountName { get; set; }
        public string HeldAt { get; set; }
        public string AcctNumber { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Closed { get; set; }
        public decimal BalanceCurrent { get; set; }
        public decimal BalanceReconciled { get; set; }

        public virtual ICollection<Deal> Deals { get; set; }
    }

    public class BankAcctViewModel
    {

        public int HouseholdId { get; set; }
        public string HouseholdName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        [DataType(DataType.Text)]
        [Display(Name = "Display or Screen Name")]
        public string AccountName { get; set; }

        public string HeldAt { get; set; }
        public string AcctNumber { get; set; }
        public decimal BalanceOpening { get; set; }
        public DateTimeOffset Created { get; set; }

    }
}