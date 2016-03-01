// <copyright file="TimestampGuid.cs" company="Cyrious">
//     Copyright (c) Cyrious Software.  All rights reserved.
// </copyright>

namespace PubSubHub
{
    using System;

    /// <summary>
    /// A time stamp guid modifies the Guid so that the last 6 bytes (40-bits)
    /// are based on the date and time.  This is helpful in database keys
    /// because the last 32 bits of the Guid are used as the first part of the
    /// index.  Therefore, this gives a "random" guid a chronological order and
    /// makes sure that new records are usually inserted after existing ones.
    /// <para>
    /// The remaining 96 bits are left randomized, so the likelihood of a duplicate
    /// key is still less probable than the canonization of Madonna.
    /// </para>
    /// </summary>
    public static class TimestampGuid
    {
        /// <summary>
        /// A time stamp guid modifies the Guid so that the last 6 bytes (40-bits)
        /// are based on the date and time.  This is helpful in database keys
        /// because the last 32 bits of the Guid are used as the first part of the
        /// index.  Therefore, this gives a "random" guid a chronological order and
        /// makes sure that new records are usually inserted after existing ones.
        /// The remaining 96 bits are left randomized, so the likelihood of a duplicate
        /// key is still less probable than the canonization of Madonna.
        /// </summary>
        /// <returns>A new timestampped Guid.</returns>
        public static Guid Create()
        {
            ulong newSuffix = CreateDateTime64bit(DateTime.UtcNow);

            byte[] byteArray = Guid.NewGuid().ToByteArray();

            // now replace the byte array elements
            //   Note that SQLGuids are sorted in a screwy way ...
            //      Byte[10] -> sorted first
            //      Byte[11] -> sorted next
            //      Byte[12] -> sorted next
            //      Byte[13] -> sorted next
            //      Byte[14] -> sorted next
            //      Byte[15] -> sorted next
            //      Byte[08] -> sorted next
            //      Byte[09] -> sorted next
            //      Byte[07] -> sorted next
            //      Byte[06] -> sorted next
            //      Byte[05] -> sorted next
            //      Byte[04] -> sorted next
            //      Byte[03] -> sorted next
            //      Byte[02] -> sorted next
            //      Byte[01] -> sorted next
            //      Byte[00] -> sorted last
            byteArray[10] = (byte)(newSuffix >> 40); // year
            byteArray[11] = (byte)(newSuffix >> 32); // months and part of day
            byteArray[12] = (byte)(newSuffix >> 24); // day and Most Significant part of time
            byteArray[13] = (byte)(newSuffix >> 16); // next most significant part of time
            byteArray[14] = (byte)(newSuffix >> 08); // next most significant part of time
            byteArray[15] = (byte)newSuffix; // least significant part of time

            // and create the real guid to use
            return new Guid(byteArray);
        }

        /// <summary>
        /// Translate a time into a 64bit integer that can be used as a sorting value.
        /// </summary>
        /// <param name="dt">The datetime to create an integer from.</param>
        /// <returns>An Int64 representation of the DateTime <paramref name="dt"/></returns>
        private static ulong CreateDateTime64bit(DateTime dt)
        {
            // The structure of the last 32 bits is (from right/LSB to left/MSB)
            //   bits 00 - 28 -> time (enough precision for about 1/1000th of a second)
            //   bits 29 - 33 -> day (1-31) (5 bits)
            //   bits 34 - 37 -> month (1-12) (4 bits)
            //   bits 38 - 47 -> number of years since 1900 (10 bits, 0-1025)

            // 86400000 milliseconds in a day -> 28 bits of precision maximum
            // we are only retaining 23, so lose the last 5
            ulong timeofday = ((uint)dt.TimeOfDay.TotalMilliseconds) >> 5;
            ulong days = (ulong)(((((dt.Year - 1900) << 4) | dt.Month) << 5) | dt.Day);

            return (days << 31) | (timeofday & 0x00007FFFFF);
        }
    }
}
