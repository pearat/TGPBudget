using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class Household
    {
        public Household()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.BankAcct = new HashSet<BankAcct>();
            this.Categories = new HashSet<Category>();
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string Address { get; set; }
        public string TaxId { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<BankAcct> BankAcct { get; set; }
        public virtual ICollection<Category> Categories { get; set; }


    }
}