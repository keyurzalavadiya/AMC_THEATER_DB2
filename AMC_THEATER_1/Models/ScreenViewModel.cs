using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("NO_OF_SCREENS", Schema = "AMCTHEATER")] // ✅ Maps to NO_OF_SCREENS table in DB2
    public class ScreenViewModel
    {
        // ✅ Primary Key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ AUTO_INCREMENT (IDENTITY) column
        [Column("SCREEN_ID")]
        public int ScreenId { get; set; }

        // ✅ Screen Number
        [Required]
        [Column("SCREEN_NO")]
        public int ScreenNo { get; set; }

        // ✅ Foreign Key - Ensure correct mapping
        [Required]
        [Column("\"APPL_ID\"")] // 🔄 Ensure case-sensitive match with DB2
        public int ApplId { get; set; } // ✅ Foreign Key from TRN_REGISTRATION

        //// ✅ Foreign Key Navigation Property
        //[ForeignKey("APPL_ID")] // 🔄 Correct reference (should match property name, not column name)
        //public virtual TheaterViewModel Theater { get; set; }

        // ✅ Other columns
        [Column("AUDIENCE_CAPACITY")]
        public int? AudienceCapacity { get; set; }

        [Column("SCREEN_TYPE")]
        [StringLength(50)]
        public string ScreenType { get; set; }

        // ✅ Fields Not in Database (Ignored in EF)
        [NotMapped]
        public string MonthYear { get; set; }

        [NotMapped]
        public int ScreenPrice { get; set; }  // Store price from TRN_SCREEN_TAX_PRICE

        [NotMapped]
        public int SequentialScreenNo { get; set; }  // Store sequential screen number

        [NotMapped]
        public int ActualShow { get; set; } // Populated from user input

        [NotMapped]
        public int TotalShow { get; set; } // Populated from user input

        [NotMapped]
        public int CancelShow { get; set; } // Populated from user input

        [NotMapped]
        public decimal AmtPerScreen { get; set; } // Calculation logic
    }
}
