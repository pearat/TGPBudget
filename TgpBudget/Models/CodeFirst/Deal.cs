using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBudget.Models
{
    public class Deal
    {
        public int Id { get; set; }
        [Required]
        public int BankAcctId { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public DateTimeOffset Created { get; set; }
        [Required]
        public string Payee { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public bool Reconciled { get; set; }


        public virtual BankAcct BankAcct { get; set; }
        public virtual Category Category { get; set; }

    }
}