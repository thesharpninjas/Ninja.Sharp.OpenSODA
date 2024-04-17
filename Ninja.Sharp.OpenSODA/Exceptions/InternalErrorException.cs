// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Exceptions
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException() : base(string.Empty)
        {
        }

        public InternalErrorException(string? message) : base(message)
        {
        }

        public InternalErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
