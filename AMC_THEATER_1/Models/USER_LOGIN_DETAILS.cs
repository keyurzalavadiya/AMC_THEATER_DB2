using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
        [Table("USER_LOGIN_DETAILS", Schema = "AMCTHEATER")]
        public class USER_LOGIN_DETAILS
        {
        [Key]

        public int LogId { get; set; }

        [Column("PHONE_NUMBER")]
        public long PhoneNumber { get; set; } // Changed to long to store full phone numbers
        }
}