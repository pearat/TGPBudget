using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class Category:IComparable<Category>
    {
        public int Id { get; set; }
        public int? HouseholdId { get; set; }
        public Category()
        {
            this.Deals = new HashSet<Deal>();
        }

        [Required]
        [Display(Name = "Category")]
        public string Name { get; set; }
        [Required]
        public bool IsExpense { get; set; }
        [Required]

        [DataType(DataType.Currency)]
//      [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        [Display(Name = "Budget")]
        public decimal BudgetAmount { get; set; }
        public bool IsProtected { get; set; }

        public virtual Household Household { get; set; }

        public virtual ICollection<Deal> Deals { get; set; }

        public int CompareTo(Category c)
        {
            return Name.CompareTo(c.Name);
        }
    }
    public class CategoryViewModel 
    {
        public Category category { get; set; }

        [Display(Name="Actual")]
        [DataType(DataType.Currency)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal ActualAmount { get; set; }

        [Display(Name = "Variance")]
        [DataType(DataType.Currency)]
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Variance { get; set; }

        [Display(Name = "Reconciled")]
        [DataType(DataType.Currency)]
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal ReconciledAmount { get; set; }

        [Display(Name = "Unreconciled")]
        [DataType(DataType.Currency)]
        public decimal UnreconciledAmount { get; set; }

        public int Count { get; set; }

        public bool IsTotal { get; set; }
    }


    public class CatDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public bool IsExpense { get; set; }
        
        [DataType(DataType.Currency)]
        public decimal Actual { get; set; }
        
        [DataType(DataType.Currency)]
        public decimal Budget { get; set; }
        [DataType(DataType.Currency)]
        public decimal Reconciled { get; set; }
        [DataType(DataType.Currency)]
        public decimal Unreconciled { get; set; }
        [DataType(DataType.Currency)]
        public decimal Variance { get; set; }
        public bool IsTotal { get; set; }
    }

    public class CatDisplayVM
    {
        public CatDisplayVM()
        {
            IncCats = new List<CatDisplay>();
            ExpCats = new List<CatDisplay>();
            IncTotal = new CatDisplay();
            ExpTotal = new CatDisplay();
            NetTotal = new CatDisplay();
        }
        public List<CatDisplay> IncCats { get; set; }
        public List<CatDisplay> ExpCats { get; set; }
        public CatDisplay IncTotal { get; set; }
        public CatDisplay ExpTotal { get; set; }
        public CatDisplay NetTotal { get; set; }
    }

    public class PieChart
    {
        public string[] labels { get; set; }
        public int[] series { get; set; }
        public int seriesCount { get; set; }
    }

    public class IncExpPieChart
    {
        public IncExpPieChart()
        {
            income = new PieChart();
            expense = new PieChart();
        }
        public PieChart income { get; set; }
        public PieChart expense { get; set; }
    }

    public class BarChart
    {
        public string[] labels { get; set; }
        public int[,] series { get; set; }
    }

    public class BarChartWithLegend
    {
        public BarChartWithLegend()
        {
            data = new BarChart();
        }
        public BarChart data { get; set; }
        public string[] legend { get; set; }
        public int seriesCount { get; set; }
    }

    public class startEndDates
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

}