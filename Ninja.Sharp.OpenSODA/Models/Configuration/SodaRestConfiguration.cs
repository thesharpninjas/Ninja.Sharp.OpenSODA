// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Models.Configuration
{
    public class SodaRestConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public bool IsSecure { get; set; } = false;
        public int Port { get; set; } = 1521;
        public bool BypassCertificate { get; set; } = true;
    }
}