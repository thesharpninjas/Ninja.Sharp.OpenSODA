// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Models.Configuration
{
    public class SodaSqlConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Schema { get; set; } = "https";
        public int Port { get; set; } = 1521;
        public string ServiceName { get; set; } = string.Empty;
    }

}