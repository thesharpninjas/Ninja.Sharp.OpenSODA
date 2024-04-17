﻿// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Extensions
{
    internal static class GuidExtensions
    {
        private static readonly int[] _guidByteOrder = [15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3];

        public static Guid Decrease(this Guid guid)
        {
            var bytes = guid.ToByteArray();
            bool carry = true;
            for (int i = 0; i < _guidByteOrder.Length && carry; i++)
            {
                int index = _guidByteOrder[i];
                byte oldValue = bytes[index]--;
                carry = oldValue < bytes[index];
            }
            return new Guid(bytes);
        }
    }
}
