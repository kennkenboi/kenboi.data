using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kenboi.Data
{
    public class MyResponse
    {
        public dynamic Response { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; }
    }
}
