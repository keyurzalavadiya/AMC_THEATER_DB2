using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("TRN_THEATRE_DOCS", Schema = "AMCTHEATER")]
    public class TRN_THEATRE_DOCS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("TH_DOC_ID")]
        public int ThDocId { get; set; }  // Primary Key ✅ Matches DB2 schema

        [Column("DOC_FILEPATH")]
        public string DocFilePath { get; set; } // NVARCHAR(255) ✅

        [ForeignKey("TRN_REGISTRATION")]
        [Column("APPL_ID")]
        public int ApplId { get; set; }  // ✅ Foreign Key referencing TRN_REGISTRATION

        [ForeignKey("MST_DOCS")]
        [Column("DOC_ID")]
        public int DocId { get; set; }  // ✅ Foreign Key referencing MST_DOCS (NOT NULL)

        [Column("CREATE_DATE")]
        public DateTime? CreateDate { get; set; } // Nullable DATE ✅

        [Column("CREATE_USER")]
        public string CreateUser { get; set; } // VARCHAR(50) ✅

        [Column("UPDATE_DATE")]
        public DateTime? UpdateDate { get; set; } // Nullable DATE ✅

        [Column("UPDATE_USER")]
        public string UpdateUser { get; set; } // VARCHAR(50) ✅

        [Column("DELETE_DATE")]
        public DateTime? DeleteDate { get; set; } // Nullable DATE ✅

        [Column("DELETE_USER")]
        public string DeleteUser { get; set; } // VARCHAR(50) ✅

        [Required]
        [Column("ACTIVE")]
        public short Active { get; set; } // SMALLINT NOT NULL DEFAULT 1 ✅

        // ✅ Navigation Properties
        public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
        public virtual MST_DOCS MST_DOCS { get; set; } // ✅ Navigation for DOC_ID
    }
}
