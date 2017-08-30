using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Konan.Classes
{
    public class RunStatus
    {
        public string error { get; set; }
        public string langId { get; set; }
        public string langName { get; set; }
        public string time { get; set; }
        public string memory { get; set; }
        public string output { get; set; }
        public string result { get; set; }
        public string cmpinfo { get; set; }
        public string stderr { get; set; }
    }
}