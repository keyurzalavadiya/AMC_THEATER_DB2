using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    [Table("MST_DOCS", Schema = "AMCTHEATER")] // ✅ Ensure Correct Schema Name
    public class MST_DOCS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("DOC_ID")] // ✅ Ensure This Matches DB2 Column Name
        public int DocId { get; set; }

        [Column("DOC_NAME")]
        public string DocName { get; set; }

        [Column("FILE_SIZE_MB")]
        public int FILE_SIZE_MB { get; set; }

        [Column("DOC_ACTIVE")]
        public short? DocActive { get; set; } // ✅ FIXED: SMALLINT maps to short?


        [Column("FILE_TYPE")]
        public string FILE_TYPE { get; set; }
    }

}