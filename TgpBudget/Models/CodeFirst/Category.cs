using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int? HouseholdId { get; set; }
        [Required]
        [Display(Name = "Category")]
        public string Name { get; set; }
        [Required]
        public bool IsExpense { get; set; }
        [Required]

        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal BudgetAmount { get; set; }
        public bool IsProtected { get; set; }

        public virtual Household Household { get; set; }
    }
}