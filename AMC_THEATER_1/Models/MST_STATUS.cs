using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    [Table("MST_STATUS", Schema = "AMCTHEATER")]
    public class MST_STATUS
    {
        [Key]

        public int StatusId { get; set; }

       
        public string StatusType { get; set; } = "All"; // Default value
    }
}