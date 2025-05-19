using Amc_theater.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AMC_THEATER_1.Models
{
    [Table("TRN_REGISTRATION", Schema = "AMCTHEATER")]
    public class TheaterViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("\"APPL_ID\"")]
        public int ApplId { get; set; }

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

        [Column("T_STATUS")]
        public string T_STATUS { get; set; }

        [Column("REJECT_REASON")]
        public string REJECT_REASON { get; set; }

        [Column("T_OWNER_NAME")]
        public string T_OWNER_NAME { get; set; }

        [Column("MANAGER_CON_NO")]
        public string T_OWNER_NUMBER { get; set; }

        [Column("T_OWNER_EMAIL")]
        public string T_OWNER_EMAIL { get; set; }

      
        [Column("T_ADDRESS")]
        public string T_ADDRESS { get; set; }

        [Column("T_PEC_NO")]
        public string T_PEC_NO { get; set; }

        [Column("MANAGER_NAME")]
        public string ManagerName { get; set; }

        [Column("T_PRC_NO")]
        public string T_PRC_NO { get; set; }

        [Column("LicenseDate")]
        public DateTime? LicenseDate { get; set; }

        [Column("T_COMMENCEMENT_DATE")]
        public DateTime? T_COMMENCEMENT_DATE { get; set; }
        [Column("CREATE_DATE")]
        public DateTime? CreateDate { get; set; }


        [Column("UPDATE_DATE")]
        public DateTime? UPDATE_DATE { get; set; }

        // Lists of related data
        public List<MST_TT_TYPE> ScreenTypes { get; set; }
        public List<ScreenViewModel> Screens { get; set; }

        // Additional properties
        [NotMapped]
        public string PAYMENT_STATUS { get; set; }

        [NotMapped]
        public string SINCE_MONTH { get; set; }

        [NotMapped]
        public string Display_T_ID => T_STATUS?.Equals("Approved", StringComparison.OrdinalIgnoreCase) == true
                                        ? (T_ID?.ToString() ?? "NOT GENERATED")
                                        : "NOT GENERATED";

        [NotMapped]
        public int SCREEN_COUNT { get; set; }

        [NotMapped]
        public int THEATER_SCREEN_COUNT { get; set; }

        [NotMapped]
        public int VIDEO_THEATER_SCREEN_COUNT { get; set; }

        [NotMapped]
        public bool IsEditMode { get; set; }

        // ✅ Receipt fields (no need for ReceiptFilterViewModel anymore)
        [NotMapped]
        public string RCPT_NO { get; set; } = "N/A";

        [NotMapped]
        public DateTime? RCPT_GEN_DATE { get; set; } = DateTime.Now;

        [NotMapped]
        public string PAY_MODE { get; set; } = "Cash";

        [NotMapped]
        public string STATUS { get; set; } = "Paid";
        //internal List<DocumentViewModel> Documents { get; set; }

        // ===== METHODS =====
        public string GetScreenType()
        {
            return ScreenTypes?.FirstOrDefault()?.ScreenType ?? "N/A";
        }
    }
}



                    