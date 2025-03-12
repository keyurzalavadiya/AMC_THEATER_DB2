using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("TRN_REGISTRATION", Schema = "AMCTHEATER")]
    public class TRN_REGISTRATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("APPL_ID")] // ✅ Force DB2 to recognize case-sensitive column names
        public int ApplId { get; set; }  // Primary Key

        [Column("T_ID")]
        public string TId { get; set; }  // Unique Identifier (Not Primary Key)

        [Required]
        [Column("T_STATUS")]
        public string TStatus { get; set; }

        [Required]
        [Column("T_ACTIVE")]
        public short? TActive { get; set; } // SMALLINT

        [Required]
        [Column("T_NAME")]
        public string TName { get; set; }

        [Required]
        [Column("LICENSE_HOLDER_NAME")]
        public string LicenseHolderName { get; set; }

        [Column("MANAGER_CON_NO")]
        public int? ManagerContactNo { get; set; } // Nullable INT

        [Required]
        [Column("MANAGER_NAME")]
        public string ManagerName { get; set; }

        [Required]
        [Column("T_EMAIL")]
        public string TEmail { get; set; }

        [Required, StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Column("T_ADDRESS")]
        public string TAddress { get; set; }

        [Required]
        [Column("T_CITY")]
        public string TCity { get; set; }

        [Column("T_COMMENCEMENT_DATE")]
        [DataType(DataType.Date)]
        public DateTime? TCommencementDate { get; set; } // Nullable DATE

        [Column("LICENSE_DATE")]
        [DataType(DataType.Date)]
        public DateTime? LicenseDate { get; set; } // Nullable DATE

        [Required]
        [Column("T_TENAMENT_NO")]
        public string TTenamentNo { get; set; }

        [Required]
        [Column("T_ZONE")]
        public string TZone { get; set; }

        [Required]
        [Column("T_WARD")]
        public string TWard { get; set; }

        [Required]
        [Column("T_PEC_NO")]
        public string TPecNo { get; set; }

        [Column("T_PRC_NO")]
        public string TPrcNo { get; set; }

        [Required]
        [Column("PROPERTY_TYPE")]
        public string PropertyType { get; set; }

        [Column("T_TAX_PAYING_OFFLINE")]
        public short? TTaxPayingOffline { get; set; } // Nullable SMALLINT

        [Column("OFFLINE_TAX_PAID_MONTH")]
        [DataType(DataType.Date)]
        public DateTime? OfflineTaxPaidMonth { get; set; } // Nullable DATE

        [Column("OFFLINE_TAX_PAID_YEAR")]
        [DataType(DataType.Date)]
        public DateTime? OfflineTaxPaidYear { get; set; } // Nullable DATE

        [Column("OFFLINE_TAX_PAYMENT")]
        public short? OfflineTaxPayment { get; set; } // Nullable SMALLINT

        [Column("REJECT_REASON")]
        public string RejectReason { get; set; }

        [Column("CREATE_DATE")]
        [DataType(DataType.Date)]
        public DateTime? CreateDate { get; set; } // Nullable DATE

        [Column("CREATE_USER")]
        public string CreateUser { get; set; }

        [Column("UPDATE_DATE")]
        [DataType(DataType.Date)]
        public DateTime? UpdateDate { get; set; } // Nullable DATE

        [Column("UPDATE_USER")]
        public string UpdateUser { get; set; }

        [Column("DELETE_DATE")]
        [DataType(DataType.Date)]
        public DateTime? DeleteDate { get; set; } // Nullable DATE

        [Column("DELETE_USER")]
        public string DeleteUser { get; set; }

        // ✅ Foreign Key Navigation Properties
        public virtual ICollection<TRN_THEATRE_DOCS> TRN_THEATRE_DOCS { get; set; } = new List<TRN_THEATRE_DOCS>();

        public List<NO_OF_SCREENS> NO_OF_SCREENS { get; set; } = new List<NO_OF_SCREENS>();

        [NotMapped]
        public bool IsEditMode { get; set; }
    }
}
