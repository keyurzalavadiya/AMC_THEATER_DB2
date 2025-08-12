using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("NO_OF_SCREENS_TAX", Schema = "AMCTHEATER")]
    public class NO_OF_SCREENS_TAX
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("S_TAX_ID")]
        public int STaxId { get; set; }  // Primary Key ✅ Matches DB2 schema

        [ForeignKey("TRN_REGISTRATION")]
        [Column("APPL_ID")]
        public int? ApplId { get; set; } // ✅ Foreign Key referencing TRN_REGISTRATION (Nullable)

        [Column("T_ID")]
        public string TId { get; set; } // VARCHAR(50) ✅

        [ForeignKey("THEATER_TAX_PAYMENT")]
        [Column("TAX_ID")]
        public int TaxId { get; set; } // ✅ Foreign Key referencing THEATER_TAX_PAYMENT (NOT NULL)

        [Column("RATE_PER_SCREEN")]
        public int? RatePerScreen { get; set; } // Nullable INT ✅

        [Required]
        [Column("SCREEN_TYPE")]
        public string ScreenType { get; set; } // VARCHAR(50) ✅

        [Required]
        [Column("AUDIENCE_CAPACITY")]
        public int AudienceCapacity { get; set; } // INT ✅

        [Required]
        [Column("TOTAL_SHOW")]
        public int TotalShow { get; set; } // INT ✅

        [Required]
        [Column("CANCEL_SHOW")]
        public int CancelShow { get; set; } // INT ✅

        [Required]
        [Column("ACTUAL_SHOW")]
        public int ActualShow { get; set; } // INT ✅

        [Required]
        [Column("AMT_PER_SCREEN")]
        public decimal AmountPerScreen { get; set; } // DECIMAL(18,2) ✅

        // ✅ Navigation Properties
        public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
        public virtual THEATER_TAX_PAYMENT THEATER_TAX_PAYMENT { get; set; }
    }
}
