using SWH.Attributes;
using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
using SWH.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using webHosting.Classes;

namespace webHosting.Endpoints
{
    public class Jobs : PageContent
    {
        // Get Method to retrieve all jobs
        [Authorization]
        [GetMethod("/list")]
        public IHttpResponse ListJobs([FromHeader] string userId, [FromHeader("Authorization")] string auth)
        {
            if (Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == auth) is Target target && target is not null)
            {
                var response = HttpResponse.OK(JsonSerializer.Serialize(target.Jobs));
                response.Headers["Content-Type"] = "application/json";
                return response;
            }
            return HttpResponse.NotFound();
        }

        // POST Method to create a new job
        [Authorization]
        [PostMethod("/create")]
        public IHttpResponse CreateJob([FromHeader("userid")] string userId, [FromHeader("Authorization")] string auth, [FromBody] Job newJob)
        {
            if (Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == auth) is Target target && target is not null)
            {
                // Deserialize the job and its 'After' chain recursively
                DeserializeJob(newJob);

                newJob.Status = JobStatus.Pending;
                target.Jobs.Add(newJob);

                var response = HttpResponse.OK(JsonSerializer.Serialize(newJob));
                response.Headers["Content-Type"] = "application/json";
                return response;
            }
            return HttpResponse.NotFound();
        }

        // Helper function to recursively deserialize jobs and their 'After' chains
        private void DeserializeJob(Job job)
        {
            // Deserialize the 'Do' object based on the JobType
            if (job.JobType == JobType.Excute)
            {
                job.Do = JsonSerializer.Deserialize<ExcuteJob>(job.Do.ToString()!)!;
                ((ExcuteJob)job.Do).Reply = new Reply(); // Ensure Reply is initialized
            }
            else if (job.JobType == JobType.HTTP)
            {
                job.Do = JsonSerializer.Deserialize<HTTPJob>(job.Do.ToString()!)!;
                ((HTTPJob)job.Do).Reply = new Reply(); // Ensure Reply is initialized
            }

            // Recursively handle the 'After' job if it exists
            if (job.After != null)
            {
                DeserializeJob(job.After); // Recursive call for the 'After' job
            }
            job.Status = JobStatus.Pending;

        }
        // PUT Method to update an existing job
        [Authorization]
        [PutMethod("/update/{jobId}")]
        public IHttpResponse UpdateJob([FromHeader] string userId, [FromHeader("Authorization")] string auth, [FromRoute] string jobId, [FromBody] Job updatedJob)
        {
            if (Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == auth) is Target target && target is not null)
            {
                var job = target.Jobs.FirstOrDefault(j => j.Id == jobId);
                if (job != null)
                {
                    target.Jobs.Remove(job); // Remove the old job
                    target.Jobs.Add(updatedJob); // Add the updated job
                    var response = HttpResponse.OK(JsonSerializer.Serialize(updatedJob));
                    response.Headers["Content-Type"] = "application/json";
                    return response;
                }
                return HttpResponse.NotFound();
            }
            return HttpResponse.NotFound();
        }

        // DELETE Method to remove a job
        [Authorization]
        [DeleteMethod("/delete/{jobId}")]
        public IHttpResponse DeleteJob([FromHeader] string userId, [FromHeader("Authorization")] string auth, [FromRoute] string jobId)
        {
            if (Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == auth) is Target target && target is not null)
            {
                var job = target.Jobs.FirstOrDefault(j => j.Id == jobId);
                if (job != null)
                {
                    target.Jobs.Remove(job); // Remove the job
                    return HttpResponse.OK();
                }
                return HttpResponse.NotFound();
            }
            return HttpResponse.NotFound();
        }
        //[Authorization]
        //[PutMethod("/{id}/complete")]
        //public IHttpResponse Complete([FromRoute] string id)
        //{

        //}
    }
}
