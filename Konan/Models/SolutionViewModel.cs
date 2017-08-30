using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Konan.Models
{
    public class Solution
    {
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Id_P { get; set; }
        public string Code { get; set; }
        public string Language { get; set; }
        public DateTime Date { get; set; }
        public string Output { get; set; }

        [ForeignKey("Id_A")]
        public virtual Account Account { get; set; }
        [ForeignKey("Id_P")]
        public virtual Post Post { get; set; }
    }
}