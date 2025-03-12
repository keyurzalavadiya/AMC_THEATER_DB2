using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMC_THEATER_1.Models
{
    [Table("MST_TT_TYPE", Schema = "AMCTHEATER")] // ✅ Ensuring correct schema mapping
    public class MST_TT_TYPE
    {
        // ===== PRIMARY KEY =====
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ If DB2 uses an auto-incremented column
        [Column("TYPE_ID")] // ✅ Ensure this matches the actual DB column name
        public int TypeId { get; set; }

        // ===== DATABASE COLUMNS =====
        [Required]
        [Column("SCREEN_TYPE")] // ✅ Match the exact column name in DB2
        [StringLength(50)] // ✅ Assuming max length is 50 (adjust as needed)
        public string ScreenType { get; set; }

        [Required]
        [Column("SCREEN_PRICE")] // ✅ Ensure this matches DB column
        [StringLength(20)] // ✅ Assuming price is stored as VARCHAR (adjust length as needed)
        public string ScreenPrice { get; set; }
    }
}
