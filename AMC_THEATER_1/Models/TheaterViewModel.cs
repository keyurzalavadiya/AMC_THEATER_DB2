using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AMC_THEATER_1.Models
{
    [Table("TRN_REGISTRATION", Schema = "AMCTHEATER")] // ✅ Ensuring correct DB2 schema mapping
    public class TheaterViewModel
    {
        // ===== PRIMARY KEY =====
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Column("\"APPL_ID\"")]
        public int ApplId { get; set; }  // ✅ Primary Key (Matches DB Column)

        // ===== COLUMNS FROM DATABASE =====
        [Column("T_ID")]
        public string T_ID { get; set; }

        [Column("T_NAME")]
        public string T_NAME { get; set; }

        [Column("T_CITY")]
        public string T_CITY { get; set; }

        [Column("T_TENAMENT_NO")]
        public string T_TENAMENT_NO { get; set; }

        [Column("T_WARD")]
        public string T_WARD { get; set; }

        [Column("T_ZONE")]
        public string T_ZONE { get; set; }

        [Column("T_STATUS")] // 🔄 Fixed column name
        public string T_STATUS { get; set; }

        [Column("REJECT_REASON")] // 🔄 Fixed column name
        public string REJECT_REASON { get; set; }

        [Column("T_OWNER_NAME")]
        public string T_OWNER_NAME { get; set; }

        [Column("T_OWNER_NUMBER")]
        public string T_OWNER_NUMBER { get; set; }

        [Column("T_OWNER_EMAIL")]
        public string T_OWNER_EMAIL { get; set; }

        //[Column("REG_ID")]
        //public string REG_ID { get; set; }

        [Column("T_COMMENCEMENT_DATE")]
        public DateTime? T_COMMENCEMENT_DATE { get; set; } // 🔄 Made Nullable (DB2 allows NULL)

        [Column("T_ADDRESS")]
        public string T_ADDRESS { get; set; }

        [Column("UPDATE_DATE")]
        public DateTime? UPDATE_DATE { get; set; } // 🔄 Made Nullable (DB2 allows NULL)

        // ===== FOREIGN KEYS =====
        public List<MST_TT_TYPE> ScreenTypes { get; set; }
        public List<ScreenViewModel> Screens { get; set; }

        // ===== NOT MAPPED (Calculated Fields, Display-Only Fields) =====
        [NotMapped]
        public string PAYMENT_STATUS { get; set; } // ✅ Not in DB, calculated for UI

        [NotMapped]
        public string SINCE_MONTH { get; set; } // ✅ Not in DB, calculated for UI

        [NotMapped]
        public string Display_T_ID => T_STATUS?.Equals("Approved", StringComparison.OrdinalIgnoreCase) == true
                                        ? (T_ID?.ToString() ?? "NOT GENERATED")
                                        : "NOT GENERATED";

        [NotMapped]
        public int SCREEN_COUNT { get; set; } // ✅ Count from `NO_OF_SCREENS`

        [NotMapped]
        public int THEATER_SCREEN_COUNT { get; set; } // ✅ Separate count for Theater Screens

        [NotMapped]
        public int VIDEO_THEATER_SCREEN_COUNT { get; set; } // ✅ Separate count for Video Theater Screens

        [NotMapped]
        public bool IsEditMode { get; set; } // ✅ Used for UI Edit Mode

        // ===== METHODS =====
        public string GetScreenType()
        {
            return ScreenTypes?.FirstOrDefault()?.ScreenType ?? "N/A";
        }
    }
}
