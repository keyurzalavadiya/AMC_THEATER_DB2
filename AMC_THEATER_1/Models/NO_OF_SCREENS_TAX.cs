using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    [Table("NO_OF_SCREENS_TAX", Schema = "AMCTHEATER")]
    public class NO_OF_SCREENS_TAX
    {
        [Key]

        public int STaxId { get; set; }

       
        public int? ApplId { get; set; } // Foreign key to TRN_REGISTRATION

        
        public string TId { get; set; }

       
        public int TaxId { get; set; } // Foreign key to THEATER_TAX_PAYMENT

        public int? RatePerScreen { get; set; } // Nullable INT

        
        public string ScreenType { get; set; }

       
        public int AudienceCapacity { get; set; }

       
        public int TotalShow { get; set; }

       
        public int CancelShow { get; set; }

      
        public int ActualShow { get; set; }

        
        public decimal AmountPerScreen { get; set; }

        // Navigation Properties
        public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
        public virtual THEATER_TAX_PAYMENT THEATER_TAX_PAYMENT { get; set; }
    }
}