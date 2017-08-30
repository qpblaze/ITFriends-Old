using IronPython.Runtime;

namespace Konan.Classes
{
    public class HackerEarth
    {
        public string errors { get; set; }
        public string id { get; set; }
        public string code_id { get; set; }
        public string message { get; set; }
        public string compile_status { get; set; }
        public string web_link { get; set; }
        public RunStatus run_status { get; set; }


        public HackerEarth()
        {
            errors = "";
            id = "";
            code_id = "";
            message = "";
            compile_status = "";
            web_link = "";
            run_status = new RunStatus();
        }

       
    }
}