// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Exceptions
{
    public class SodaConfigurationException : Exception
    {
        public SodaConfigurationException() : base(string.Empty)
        {
        }

        public SodaConfigurationException(string? message) : base(message)
        {
        }

        public SodaConfigurationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
