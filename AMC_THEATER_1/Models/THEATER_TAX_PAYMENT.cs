using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    [Table("THEATER_TAX_PAYMENT", Schema = "AMCTHEATER")]
    public class THEATER_TAX_PAYMENT
    {
        [Key]

        public int TaxId { get; set; }

       
        public int? ApplId { get; set; } // Foreign key to TRN_REGISTRATION

        
        public string TId { get; set; }

        
        public string PaymentMonth { get; set; }

       
        public int PaymentYear { get; set; }

        
        public decimal TaxAmount { get; set; }

       
        public string ShowStatement { get; set; }

       
        public string CreateUser { get; set; }

       
        public DateTime CreateDate { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
    }
}