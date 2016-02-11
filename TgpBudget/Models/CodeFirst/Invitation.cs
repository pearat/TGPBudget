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
        public string HouseholdName { get; set; }

        public string IssuedBy { get; set; }
        [Required]
        public string GuestEmail { get; set; }


        public string InvitationCode { get; set; }
   
        public DateTimeOffset IssuedOn { get; set; }

        public DateTimeOffset InvalidAfter { get; set; }

    }
}