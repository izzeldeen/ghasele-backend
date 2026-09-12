using System;

namespace Ghasele.Application.Scheduling
{
    /// <summary>
    /// The operator's local clock. Everything else in this codebase stores and compares UTC,
    /// which is right for timestamps but wrong for scheduling: a delivery window is "10:00 to
    /// 12:00" in Amman, not in UTC, and "which slots are still bookable today" has to be asked
    /// in the customer's own day.
    /// </summary>
    /// <remarks>
    /// Jordan abolished daylight saving in 2022 and now sits on a fixed UTC+03, so a constant
    /// offset is exact rather than an approximation - and it avoids depending on a tz database
    /// whose id differs between Windows ("Jordan Standard Time") and Linux ("Asia/Amman"), which
    /// would work locally and throw on the deployed server.
    /// </remarks>
    public static class JordanTime
    {
        /// <summary>Jordan's fixed offset from UTC.</summary>
        public static readonly TimeSpan Offset = TimeSpan.FromHours(3);

        /// <summary>The current local date and time in Amman.</summary>
        public static DateTime Now => DateTime.UtcNow + Offset;

        /// <summary>Today's date in Amman.</summary>
        public static DateOnly Today => DateOnly.FromDateTime(Now);

        /// <summary>The current local time of day in Amman.</summary>
        public static TimeOnly TimeOfDay => TimeOnly.FromDateTime(Now);

        /// <summary>
        /// True when a window on <paramref name="date"/> starting at <paramref name="start"/>
        /// is still in the future locally. A window is dropped the moment it starts, not when
        /// it ends: a customer booking a 10:00-12:00 collection at 11:30 would be promised a
        /// visit the driver has already made.
        /// </summary>
        public static bool IsUpcoming(DateOnly date, TimeOnly start)
        {
            if (date > Today) return true;
            if (date < Today) return false;
            return start > TimeOfDay;
        }

        /// <summary>
        /// A local date and window start expressed as the UTC instant it occurs at, for storing
        /// next to the codebase's other UTC timestamps.
        /// </summary>
        public static DateTime ToUtc(DateOnly date, TimeOnly time) =>
            DateTime.SpecifyKind(date.ToDateTime(time) - Offset, DateTimeKind.Utc);
    }
}
