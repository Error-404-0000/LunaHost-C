using SWH.Attributes;
using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
using System;
using System.Linq;
using System.Text.Json;
using webHosting.Classes;

namespace webHosting.Endpoints
{
    /// <summary>
    /// Path: /replies
    /// </summary>
    public class Replies : PageContent
    {



        [GetMethod("/{id}")]
        public IHttpResponse GetReply([FromRoute] string id, [FromHeader] string userId, [FromHeader("Authorization")] string userAuth)
        {
            var target = Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == userAuth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            // Perform a deep search for the reply within the job and its 'After' chain
            var job = target.Jobs.FirstOrDefault(j => FindReplyInJob(j, id) != null);

            if (job == null)
            {
                return HttpResponse.NotFound("Reply not found");
            }

            // Extract the reply
            Reply reply = FindReplyInJob(job, id);

            if (reply == null)
            {
                return HttpResponse.NotFound("Reply not found or already fetched");
            }

            // Serialize and return the reply
            reply!.Content = reply.Content?.ToString();  // Ensure content is a string
            var response = HttpResponse.OK(JsonSerializer.Serialize(reply));
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Recursive function to search for a reply in the job and its 'After' chain
        private Reply FindReplyInJob(Job job, string replyId)
        {
            if (job.Do is ExcuteJob execJob && execJob.Reply != null && execJob.Reply.Id == replyId)
            {
                return execJob.Reply;
            }
            else if (job.Do is HTTPJob httpJob && httpJob.Reply != null && httpJob.Reply.Id == replyId)
            {
                return httpJob.Reply;
            }

            // If there is an 'After' job, search in the next job recursively
            if (job.After != null)
            {
                return FindReplyInJob(job.After, replyId);
            }

            return null; // No reply found
        }

        [PutMethod("/{id}/update")]
        public IHttpResponse UpdateReplyMessage([FromRoute] string id, [FromHeader] string userId, [FromHeader("Authorization")] string userAuth, [FromBody] string content)
        {
            var target = Register.Targets.FirstOrDefault(x => x.Id == userId && x.Auth == userAuth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            
            var updatedReply = UpdateReplyInJob(target.Jobs, id, content);

            if (updatedReply == null)
            {
                return HttpResponse.NotFound("Reply not found");
            }

            var response = HttpResponse.OK(JsonSerializer.Serialize(updatedReply));
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Recursive function to update reply content within the job chain
        private Reply UpdateReplyInJob(List<Job> jobs, string replyId, string newContent)
        {
            foreach (var job in jobs)
            {
                Reply reply = FindAndUpdateReply(job, replyId, newContent);
                if (reply != null)
                {
                    job.Status = JobStatus.Success;
                    reply.Replied = true;
                    return reply;
                }

                // If job has nested 'After' jobs, recursively update them
                if (job.After != null)
                {
                    Reply afterReply = UpdateReplyInJob(new List<Job> { job.After }, replyId, newContent);
                    afterReply.Replied = true;
                    if (afterReply != null)
                    {
                        job.Status = JobStatus.Success;
                        
                        return afterReply;
                    }
                }
            }

            return null; // No reply found
        }

        // Recursive function to find and update the reply in a single job
        private Reply FindAndUpdateReply(Job job, string replyId, string newContent)
        {
            if (job.Do is ExcuteJob execJob && execJob.Reply != null && execJob.Reply.Id == replyId)
            {
                execJob.Reply.Content = newContent; // Update reply content
                return execJob.Reply;
            }
            else if (job.Do is HTTPJob httpJob && httpJob.Reply != null && httpJob.Reply.Id == replyId)
            {
                httpJob.Reply.Content = newContent; // Update reply content
                return httpJob.Reply;
            }

            return null; // No reply found in this job
        }

    }
}

