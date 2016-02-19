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
        [Display(Name = "Budget")]
        public decimal BudgetAmount { get; set; }
        public bool IsProtected { get; set; }

        public virtual Household Household { get; set; }
    }
    public class CategoryViewModel 
    {
        public Category category { get; set; }

        [Display(Name="Actual")]
        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal ActualAmount { get; set; }

        [Display(Name = "Variance")]
        [DataType(DataType.Currency)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Variance { get; set; }
        public bool IsTotal { get; set; }

    }
}