﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TgpBudget.Models
{

    public class Deal :IComparable<Deal>
    {
        [Required]

        public int Id { get; set; }

        public int? BankAcctId { get; set; }
        
        public int? CategoryId { get; set; }

        [Display(Name = "Dollar Amount")]

        [DataType(DataType.Currency)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:C}")]
        public decimal Amount { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset DealDate { get; set; }

        public string Description { get; set; }

        public string Payee { get; set; }

        [Display(Name = "Reconciled?")]
        public bool Reconciled { get; set; }

        public virtual BankAcct BankAcct { get; set; }
        public virtual Category Category { get; set; }

        public int CompareTo(Deal d)
        {
            return Amount.CompareTo((decimal)d.Amount);
        }
    }



        public class DealViewModel
    {
        
        [Required]
        [DataType(DataType.Currency)]
        //[DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:C}")]
        [Display(Name = "Dollar Amount")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Account Name")]
        public int BankAcctId { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Transfer Account")]

        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        //public int XferAcctId { get; set; }


        public int? CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Transaction")]
        public DateTimeOffset DealDate { get; set; }
        public string Description { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Expense")]
        public int? ExpenseId { get; set; }

        public int Id { get; set; }
        public bool IsExpense { get; set; }
        public bool InitialCheckbox { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Income")]
        public int? IncomeId { get; set; }
        public string IncomeToggle { get; set; }

        [Required]
        public string Payee { get; set; }
        
        [Display(Name = "Reconciled?")]
        public bool Reconciled { get; set; }
    }
}