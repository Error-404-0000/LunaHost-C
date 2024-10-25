using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webHosting.Classes
{
    public class Job
    {
        public string Id { get;} = Guid.NewGuid().ToString();
        public JobType JobType { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public object Do {  get; set; } //HTTPJob /ExcuteJob
        public bool Take_LastResponseOn_Success { get; set; } = false;
        public bool Take_Exit_code { get; set; } = false;
        public bool Capture_Last_Response { get; set; } = false;
        public Job After {  get; set; }
    }
 
    public class HTTPJob
    {
        public string Method { get; set; } // GET, POST, PUT, DELETE, etc.
        public string Url { get; set; } // The target URL for the request
        public string Body { get; set; } // Payload for POST/PUT requests
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(); // Custom headers
        public Dictionary<string, string> QueryParameters { get; set; } = new Dictionary<string, string>(); // Query parameters like ?id=123
        public string ContentType { get; set; } = "application/json"; // Content type (JSON, XML, etc.)
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30); // Request timeout
        public Reply Reply { get; set; } // Response handler or data
    }
    public class Reply
    {
        
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Content { get; set; }
        public bool Replied { get; set; } = false;
    }
    public enum JobStatus 
    {
        Success,
        Pending 
    }
}
