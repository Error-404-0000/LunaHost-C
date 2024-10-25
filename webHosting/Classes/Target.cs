using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace webHosting.Classes
{
    public class Target
    {

            public DateTime Joined_Date { get; set; } = DateTime.UtcNow;
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Auth { get; set; } = Guid.NewGuid().ToString();
            public string Username { get; set; } = "Example1";
            public string User_Agent { get; set; }

            // New properties
            public string Device_Name { get; set; } // e.g., "iPhone 12", "Samsung Galaxy"
            public string Device_Type { get; set; } // e.g., "Mobile", "Desktop"
            public string Operating_System { get; set; } // e.g., "Windows 10", "Android 11"
            public string IP_Address { get; set; } // IP address of the device
            public string Mac_Address { get; set; } // Device MAC Address
            public string Location { get; set; } // Physical location if available (city/country)
            public bool Is_Active { get; set; } = true; // Indicates if the device is currently active
            public DateTime Last_Login { get; set; } = DateTime.UtcNow; // Last time user logged in
            public string Connection_Type { get; set; } // e.g., "WiFi", "4G", "Ethernet"
            public double Battery_Percentage { get; set; } // Battery level of the device (if mobile)
            public bool Is_Charging { get; set; } // Indicates if the device is charging
            public string Network_Provider { get; set; } 
        
            public List<Job> Jobs { get; set; } = new();
        


    }
}
