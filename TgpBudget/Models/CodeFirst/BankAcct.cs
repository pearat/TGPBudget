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
            this.Transactions = new HashSet<Transaction>();
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

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}