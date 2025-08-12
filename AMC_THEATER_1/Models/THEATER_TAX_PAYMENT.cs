using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("THEATER_TAX_PAYMENT", Schema = "AMCTHEATER")]
    public class THEATER_TAX_PAYMENT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("TAX_ID")]
        public int TaxId { get; set; }  // Primary Key ✅ Matches DB2 schema

        [ForeignKey("TRN_REGISTRATION")]
        [Column("APPL_ID")]
        public int? ApplId { get; set; } // ✅ Foreign Key referencing TRN_REGISTRATION (Nullable)

        [Column("T_ID")]
        public string TId { get; set; } // VARCHAR(50) ✅

        [Required]
        [Column("PAYMENT_MONTH")]
        public string PaymentMonth { get; set; } // VARCHAR(20) ✅

        [Required]
        [Column("PAYMENT_YEAR")]
        public int PaymentYear { get; set; } // INT ✅

        [Required]
        [Column("TAX_AMOUNT")]
        public decimal TaxAmount { get; set; } // DECIMAL(18,2) ✅

        [Column("SHOW_STATEMENT")]
        public string ShowStatement { get; set; } // VARCHAR(32672) ✅

        [Required]
        [Column("CREATE_USER")]
        public string CreateUser { get; set; } // VARCHAR(50) ✅

        [Required]
        [Column("CREATE_DATE")]
        public DateTime CreateDate { get; set; } = DateTime.Now; // TIMESTAMP DEFAULT CURRENT_TIMESTAMP ✅

        // ✅ Navigation Properties
        public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
    }
}
