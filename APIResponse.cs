using System.Net;

namespace BackTasks
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode {  get; set; }
        public bool isExitoso { get; set; } = true;
        public List<string> Errors { get; set; }
        public Object Resultado { get; set; }
    }
}
