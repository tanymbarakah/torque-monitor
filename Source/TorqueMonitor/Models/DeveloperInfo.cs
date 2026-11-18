namespace TorqueMonitor.Models
{
    public class DeveloperInfo
    {
        public static DeveloperInfo Current { get; } = new DeveloperInfo();

        // Developer Details
        public string DeveloperName { get; set; } = "Tanym Barakah";
        public string ProfessionalTitle { get; set; } = "Software & App Developer";
        public string Designation { get; set; } = "Founder & CEO @ NIHAR TECHNOLOGIES";
        public string Country { get; set; } = "Bangladesh";

        // Company Information
        public string Company { get; set; } = "NIHAR TECHNOLOGIES";
        public string Website { get; set; } = "https://nihartechnologies.com";
        public string Email { get; set; } = "tanymbarakah@nihartechnologies.com";

        // Professional Links
        public string GitHubUrl { get; set; } = "https://github.com/tanymbarakah";
        public string LinkedInUrl { get; set; } = "https://www.linkedin.com/in/tanym-barakah/";
        public string FacebookUrl { get; set; } = "https://www.facebook.com/tanymbarakah.me";

        // Application Information
        public string ApplicationName { get; set; } = "TORQUE MONITOR";
        public string Version { get; set; } = "1.0.0";
        public string ReleaseDate { get; set; } = "19-Nov-2026";
        public string Copyright { get; set; } = "© 2026 Tanym Barakah. NIHAR TECHNOLOGIES. All rights reserved.";
        public string Licence { get; set; } = "Personal Use License (Non-Commercial Freeware)";
    }
}
