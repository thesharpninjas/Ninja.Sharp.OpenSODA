// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base(string.Empty)
        {
        }

        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
