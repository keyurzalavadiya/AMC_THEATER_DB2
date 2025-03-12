using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    [Table("DEPT_LOGIN_DETAILS", Schema = "AMCTHEATER")]
    public class DEPT_LOGIN_DETAILS
    {
        [Key]

        public int DeptId { get; set; }
        [Column("DEPT_USERNAME")]

        public string DeptUsername { get; set; }
        [Column("DEPT_PASS")]


        public string DeptPass { get; set; }
    }
}