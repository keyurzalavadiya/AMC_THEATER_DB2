using AMC_THEATER_1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("NO_OF_SCREENS", Schema = "AMCTHEATER")]
public class NO_OF_SCREENS
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("SCREEN_ID")]
    public int ScreenId { get; set; }  // Primary Key ✅ Matches DB2 schema

    [Required]
    [Column("SCREEN_NO")]
    public int ScreenNo { get; set; }  // Not nullable ✅

    [ForeignKey("TRN_REGISTRATION")]
    [Column("APPL_ID")]
    public int ApplId { get; set; }  // ✅ Foreign Key referencing TRN_REGISTRATION (Not nullable)

    [Column("AUDIENCE_CAPACITY")]
    public int? AudienceCapacity { get; set; } // Nullable ✅

    [Column("SCREEN_TYPE")]
    public string ScreenType { get; set; } // VARCHAR(50) ✅

    // ✅ Navigation Property
    public virtual TRN_REGISTRATION TRN_REGISTRATION { get; set; }
}
