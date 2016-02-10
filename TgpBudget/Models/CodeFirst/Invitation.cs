using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBudget.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string UserEmail { get; set; }
        [Required]
        public string InvitationCode { get; set; }
        [Required]
        public DateTimeOffset IssuedOn { get; set; }
        [Required]
        public DateTimeOffset InvalidAfter { get; set; }

        public virtual Household Household { get; set; }
    }
}