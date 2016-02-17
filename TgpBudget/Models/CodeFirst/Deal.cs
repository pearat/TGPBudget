using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBudget.Models
{
    public class Deal
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Account Name")]
        public int Id { get; set; }

        public int? BankAcctId { get; set; }
        
        public int? CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Transaction")]
        public DateTimeOffset DealDate { get; set; }
        
        public string Payee { get; set; }
        public string Description { get; set; }

        [Display(Name = "Dollar Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Reconciled?")]
        public bool Reconciled { get; set; }

        public virtual BankAcct BankAcct { get; set; }
        public virtual Category Category { get; set; }
    }

    public class DealViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Account Name")]
        public string BankAcctName { get; set; }

        public bool IsExpense { get; set; }
        public int? CategoryId { get; set; }
        [Display(Name = "Expense")]
        public int? ExpenseId { get; set; }
        [Display(Name = "Income")]
        public int? IncomeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Transaction")]
        public DateTimeOffset DealDate { get; set; }
        
        [Required]
        public string Payee { get; set; }
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Dollar Amount")]
        public decimal Amount { get; set; }
        
        [Display(Name = "Reconciled?")]
        public bool Reconciled { get; set; }

    }
}