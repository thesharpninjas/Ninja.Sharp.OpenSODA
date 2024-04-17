// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Exceptions
{
    public class InvalidDataException : Exception
    {
        public InvalidDataException() : base(string.Empty)
        {
        }

        public InvalidDataException(string? message) : base(message)
        {
        }

        public InvalidDataException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
