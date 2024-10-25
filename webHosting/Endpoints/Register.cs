using Newtonsoft.Json;
using SWH.Attributes;
using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
using SWH.MiddleWares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using webHosting.Classes;

namespace webHosting.Endpoints
{
    public class Register : PageContent
    {
        public static string BaseRoute = "http://10.0.0.71/register";

        public static List<Target> Targets = new();
        [PostMethod]
        public IHttpResponse register()
        {
            Target t = new()
            {
                Joined_Date = DateTime.Now,
                User_Agent = request!.Headers["User-Agent"]
            };
            var r = HttpResponse.OK($@"
{{
 ""userId"": ""{t.Id}"",
 ""Auth"" : ""{t.Auth}""
}}");
            r.Headers["Content-Type"] = "application/json";
            //getting user infomation:DEFAULT
            Dictionary<string,string> auth_headers = new();
            auth_headers["userid"] =  t.Id;
            auth_headers["Authorization"] = t.Auth;

            //t.Jobs.Add(new Job()
            //{
            //    JobType = JobType.HTTP,
            //    Do = new HTTPJob()
            //    {
            //        Method = "POST",
            //        Reply = new Reply(),  // Initialize the reply object for this job
            //        Url = BaseRoute + "/is-active",  // Another target URL for the next job
            //        Headers = auth_headers,  // Authorization headers for the next job
            //        ContentType = "application/json",  // Content type for the next job
            //    },
            //    Capture_Last_Response = false,//Default ;the last respon wont be change or updated ; lr : ""
            //    After = new Job()  // Nested job (After)
            //    {
            //        JobType = JobType.HTTP,  // Second job type is also HTTP
            //        Do = new HTTPJob()
            //        {
            //            Method = "POST",
            //            Reply = new Reply(),  // Initialize the reply object for this job
            //            Url = BaseRoute + "/is-active",  // Another target URL for the next job
            //            Headers = auth_headers,  // Authorization headers for the next job
            //            ContentType = "application/json",  // Content type for the next job
            //        },
            //        After = new Job()
            //        {
            //            JobType = JobType.Excute,
            //            Do = new ExcuteJob
            //            {
            //                Application = "cmd.exe",
            //                Command = "hostname",
            //                Reply = new()

            //            },
            //            Capture_Last_Response = true,//Lr is now update to  what excute retured (example : computer1)

            //            After = new Job()  // Another nested job (After)
            //            {
            //                JobType = JobType.HTTP,
            //                Do = new HTTPJob()
            //                {
            //                    Method = "POST",
            //                    Reply = new Reply(),
            //                    Url = BaseRoute + "/device-name",  // Another URL for the next job
            //                    Headers = auth_headers,
            //                    Body = "{lr}",//last respon
            //                    ContentType = "application/json",
            //                },
            //                Take_LastResponseOn_Success = true,
            //                Capture_Last_Response = false,
            //                After = new Job()
            //                {
            //                    JobType = JobType.Excute,
            //                    Do = new ExcuteJob()
            //                    {
            //                        Application = "cmd.exe",
            //                        Command = "wmic computersystem get PCSystemType",
            //                        Reply = new()
            //                    },
            //                    Capture_Last_Response = true,
            //                    After = new Job()
            //                    {
            //                        JobType = JobType.HTTP,
            //                        Do = new HTTPJob()
            //                        {
            //                            Method = "POST",
            //                            Reply = new Reply(),
            //                            Url = BaseRoute + "/device-type",
            //                            Headers = auth_headers,
            //                            Body = "{lr}",
            //                            ContentType = "application/json",
            //                        },
            //                        Take_LastResponseOn_Success = true
            //                    }
            //                }
            //            }
            //        }
            //    }
            //});
            Targets.Add(t);
            return r;
        }

      
       
        [GetMethod("/user/details/{userid}")]
        public IHttpResponse GetDetails([FromHeader("userId")] string userid, [FromHeader("Authorization")] string Auth)
        {
            if(userid == null || Auth==null)
                return HttpResponse.BadRequest();
            if(!Targets.Any(x=>x.Id == userid))
                return HttpResponse.BadRequest();
            var json = System.Text.Json.JsonSerializer.Serialize(Targets.FirstOrDefault(x => x.Id == userid && x.Auth == Auth));
            var r = HttpResponse.OK(json);
            if (json == "")
                r = HttpResponse.OK("null");
            
            r.Headers["Content-Type"] = "application/json";
            return r;

        }
        [Authorization]
        [GetMethod("/ping")]
        public IHttpResponse Ping([FromHeader]string userid, [FromHeader("Authorization")] string Auth)
        {
            return HttpResponse.OK(IsAuth(userid,Auth).ToString());
        }
        public bool IsAuth(string userid, string Auth)
        {
            if (userid == null || Auth == null)
                return false;
            if (Targets.Any(x => x.Id == userid && x.Auth == Auth))
                return true;
            return false;
        }
        //for testing only
        [GetMethod("/targets")]
        public IHttpResponse GetTargets()
        {
            var r = HttpResponse.OK(JsonConvert.SerializeObject(Targets.ToArray()));
            r.Headers["Content-Type"] = "application/json";
            return r;
        }
        #region J
        //#########################################################################################################################
        private Target AuthMethod(string userId, string authToken)
        {
            return Targets.FirstOrDefault(x => x.Id == userId && x.Auth == authToken);
        }

        // Update User_Agent
     
        // Update Is_Active
        [PostMethod("/is-active")]
        public IHttpResponse UpdateIsActive([FromHeader] string userid, [FromHeader("Authorization")] string auth)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Is_Active = true;

            // Save or update the Is_Active status in your data persistence layer

            var response = HttpResponse.OK("Is_Active status updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Update Device_Name
        [PostMethod("/device-name")]
        public IHttpResponse UpdateDeviceName([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string deviceName)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Device_Name = deviceName;

            // Save or update the Device_Name in your data persistence layer

            var response = HttpResponse.OK("Device name updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Update Operating_System
        [PostMethod("/operating-system")]
        public IHttpResponse UpdateOperatingSystem([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string operatingSystem)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Operating_System = operatingSystem;

            // Save or update the Operating_System in your data persistence layer

            var response = HttpResponse.OK("Operating system updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }
        
        [Authorization]
        [PostMethod("/device-type")]
        public IHttpResponse Update_Device_Type([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string device_info)
        {
           
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }
            string device = device_info.Split("\n")[^1]; // Take the last line of the string.
            string deviceType = "UNKNOW";

            if (int.TryParse(device, out int type))
            {
                deviceType= type switch
                {
                    1 => "Other",
                    2 => "Unknown",
                    3 => "Desktop",
                    4 => "Low Profile Desktop",
                    5 => "Pizza Box",
                    6 => "Mini Tower",
                    7 => "Tower",
                    8 => "Portable",
                    9 => "Laptop",
                    10 => "Notebook",
                    11 => "Handheld",
                    12 => "Docking Station",
                    13 => "All-in-One",
                    14 => "Sub Notebook",
                    15 => "Space-Saving",
                    16 => "Lunch Box",
                    17 => "Main System Chassis",
                    18 => "Expansion Chassis",
                    19 => "SubChassis",
                    20 => "Bus Expansion Chassis",
                    21 => "Peripheral Chassis",
                    22 => "Storage Chassis",
                    23 => "Rack Mount Chassis",
                    24 => "Sealed-Case PC",
                    25 => "Multi-System Chassis",
                    26 => "Compact PCI",
                    27 => "Advanced TCA",
                    28 => "Blade",
                    29 => "Blade Enclosure",
                    30 => "Tablet",
                    _ => "Unknown"
                };
            }
            target.Device_Type = deviceType;

            // Save or update the IP_Address in your data persistence layer

            var response = HttpResponse.OK("IP address updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }
        [PostMethod("/ip-address")]
        public IHttpResponse UpdateIPAddress([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string ipAddress)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.IP_Address = ipAddress;

            // Save or update the IP_Address in your data persistence layer

            var response = HttpResponse.OK("IP address updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }
        // Update Connection_Type
        [PostMethod("/connection-type")]
        public IHttpResponse UpdateConnectionType([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string connectionType)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Connection_Type = connectionType;

            // Save or update the Connection_Type in your data persistence layer

            var response = HttpResponse.OK("Connection type updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Update Device_Name
        [PostMethod("/mac-address")]
        public IHttpResponse UpdateMacAddress([FromHeader] string userid, [FromHeader("Authorization")] string auth, [FromBody] string macAddress)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Mac_Address = macAddress;

            // Save or update the Mac_Address in your data persistence layer

            var response = HttpResponse.OK("MAC address updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }

        // Update Network_Provider
        [PostMethod("/network-provider")]
        public IHttpResponse UpdateNetworkProvider([FromRoute] string userid, [FromHeader("Authorization")] string auth, [FromBody] string networkProvider)
        {
            var target = AuthMethod(userid, auth);
            if (target == null)
            {
                return HttpResponse.Unauthorized("User not authorized");
            }

            target.Network_Provider = networkProvider;

            // Save or update the Network_Provider in your data persistence layer

            var response = HttpResponse.OK("Network provider updated successfully");
            response.Headers["Content-Type"] = "application/json";
            return response;
        }
        #endregion J
    }

}
