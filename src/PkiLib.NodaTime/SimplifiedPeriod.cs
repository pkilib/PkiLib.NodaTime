using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NodaTime;

namespace Org.PkiLib.NodaTime
{
    public class SimplifiedPeriod : IComparable<SimplifiedPeriod>, IComparable, IEquatable<SimplifiedPeriod>
    {
        public const long NanosecondsPerTick = NodaConstants.NanosecondsPerTick;
        public const long TicksPerMillisecond = NodaConstants.TicksPerMillisecond;
        public const long MillisecondsPerSecond = NodaConstants.MillisecondsPerSecond;
        public const long SecondsPerMinute = NodaConstants.SecondsPerMinute;
        public const long MinutesPerHour = NodaConstants.MinutesPerHour;
        public const long HoursPerDay = NodaConstants.HoursPerDay;
        public const long DaysPerWeek = NodaConstants.DaysPerWeek;
        public const long WeeksPerMonth = 4; // simplification - there are always 4 weeks (28 days) in a month
        public const long MonthsPerYear = 12;
        public const long YearsPerCentury = 100;

        public static readonly long[] Multipliers = new long[]
        {
            // 0
            NanosecondsPerTick,
            // 1
            TicksPerMillisecond,
            // 2
            MillisecondsPerSecond,
            // 3
            SecondsPerMinute,
            // 4
            MinutesPerHour,
            // 5
            HoursPerDay,
            // 6
            DaysPerWeek,
            // 7
            WeeksPerMonth,
            // 8
            MonthsPerYear,
            // 9
            YearsPerCentury
        };

        public Period Value { get; }

        public SimplifiedPeriod([NotNull] Period value, bool allowRound)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (TrySimplified(value, allowRound, out Period simplifiedPeriod))
            {
                Value = simplifiedPeriod;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private static long Simplified(long currentValue, long multiplier, out long nextValueChange)
        {
            if (currentValue == 0)
            {
                nextValueChange = 0;
                return 0;
            }

            if (currentValue < multiplier)
            {
                nextValueChange = 0;
                return currentValue;
            }

#if NETSTANDARD1_0_OR_GREATER && !NETSTANDARD1_4_OR_GREATER
            static long DivRem(long a, long b, out long result)
            {
                long div = a / b;
                result = a - (div * b);
                return div;
            }

            nextValueChange = DivRem(currentValue, multiplier, out long newValue);
#else
            nextValueChange = Math.DivRem(currentValue, multiplier, out long newValue);
#endif
            return newValue;
        }

        public static bool TrySimplified([NotNull] Period value, bool allowRound, out Period simplifiedPeriod)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (IsSimplified(value, out _))
            {
                simplifiedPeriod = value;
                return true;
            }

            long[] valueParts = new[]
            {
                value.Nanoseconds,
                value.Ticks,
                value.Milliseconds,
                value.Seconds,
                value.Minutes,
                value.Hours,
                value.Days,
                value.Weeks,
                value.Months,
                value.Years
            };

            const int weeksIndex = 7;
            int[] roundValuePartIndexes = new int[] { weeksIndex };

            int length = Multipliers.Length;

            long[] result = new long[length];

            long valuePartChange = 0;
            for (int i = 0; i < length; i++)
            {
                long valuePart = valueParts[i];
                long multiplier = Multipliers[i];
                long newValuePart;
                // not simplified biggest type
                if (i == length - 1)
                {
                    newValuePart = valuePart + valuePartChange;
                    valuePartChange = 0;
                }
                // not round value if not allow
                else if (!allowRound && roundValuePartIndexes.Contains(i))
                {
                    newValuePart = valuePart + valuePartChange;
                    valuePartChange = 0;
                }
                else
                {
                    long newValue = Simplified(valuePart + valuePartChange, multiplier, out long nextValueChange);
                    newValuePart = newValue;
                    valuePartChange = nextValueChange;
                }
                result[i] = newValuePart;
            }

            if (!allowRound)
            {
                foreach (int roundValuePartIndex in roundValuePartIndexes)
                {
                    if (result[roundValuePartIndex] > Multipliers[roundValuePartIndex])
                    {
                        simplifiedPeriod = Period.Zero;
                        return false;
                    }
                }
            }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            PeriodBuilder periodBuilder = new PeriodBuilder()
            {
                Years = (int)result[^1],
                Months = (int)result[^2],
                Weeks = (int)result[^3],
                Days = (int)result[^4],
                Hours = result[^5],
                Minutes = result[^6],
                Seconds = result[^7]
            };
#else
            PeriodBuilder periodBuilder = new PeriodBuilder()
            {
                Years = (int)result[result.Length - 1],
                Months = (int)result[result.Length - 2],
                Weeks = (int)result[result.Length - 3],
                Days = (int)result[result.Length - 4],
                Hours = result[result.Length - 5],
                Minutes = result[result.Length - 6],
                Seconds = result[result.Length - 7]
            };
#endif

            simplifiedPeriod = periodBuilder.Build();
            return true;
        }

        public static bool IsSimplified(Period value, out Exception? exception)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // nanoseconds, ticks and milliseconds not allowed
            const long zero = 0;
            if (value.Nanoseconds != zero)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Nanoseconds)} not {zero}");
                return false;
            }

            if (value.Ticks != zero)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Ticks)} not {zero}");
                return false;
            }

            if (value.Milliseconds != zero)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Milliseconds)} not {zero}");
                return false;
            }

            // seconds
            if (value.Seconds >= SecondsPerMinute)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Seconds)} greater or equals {SecondsPerMinute}");
                return false;
            }

            // minutes
            if (value.Minutes >= MinutesPerHour)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Minutes)} greater or equals {MinutesPerHour}");
                return false;
            }

            // hours
            if (value.Hours >= HoursPerDay)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Hours)} greater or equals {HoursPerDay}");
                return false;
            }

            // days
            if (value.Days >= DaysPerWeek)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Days)} greater or equals {DaysPerWeek}");
                return false;
            }

            // weeks
            if (value.Weeks >= WeeksPerMonth)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Weeks)} greater or equals {WeeksPerMonth}");
                return false;
            }

            // months
            if (value.Months >= MonthsPerYear)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Months)} greater or equals {MonthsPerYear}");
                return false;
            }

            // years
            if (value.Years >= YearsPerCentury)
            {
                exception = new ArgumentOutOfRangeException(nameof(value), $"{nameof(value.Years)} greater or equals {YearsPerCentury}");
                return false;
            }

            exception = null;
            return true;
        }

        public static IEnumerable<SimplifiedPeriod> Split(SimplifiedPeriod from, SimplifiedPeriod to, SimplifiedPeriod splitter)
        {
            if (from >= to)
            {
                throw new ArgumentOutOfRangeException(nameof(to));
            }

            throw new NotImplementedException();
        }

        public int CompareTo(SimplifiedPeriod? other)
        {
            if (ReferenceEquals(null, other)) return 1;

            Period thisValue = this.Value;
            Period otherValue = other.Value;

            // years
            if (thisValue.Years > otherValue.Years)
            {
                return 1;
            }

            if (thisValue.Years < otherValue.Years)
            {
                return -1;
            }

            // months
            if (thisValue.Months > otherValue.Months)
            {
                return 1;
            }

            if (thisValue.Months < otherValue.Months)
            {
                return -1;
            }

            // weeks
            if (thisValue.Weeks > otherValue.Weeks)
            {
                return 1;
            }

            if (thisValue.Weeks < otherValue.Weeks)
            {
                return -1;
            }

            // days
            if (thisValue.Days > otherValue.Days)
            {
                return 1;
            }

            if (thisValue.Days < otherValue.Days)
            {
                return -1;
            }

            // hours
            if (thisValue.Hours > otherValue.Hours)
            {
                return 1;
            }

            if (thisValue.Hours < otherValue.Hours)
            {
                return -1;
            }

            // minutes
            if (thisValue.Minutes > otherValue.Minutes)
            {
                return 1;
            }

            if (thisValue.Minutes < otherValue.Minutes)
            {
                return -1;
            }

            // seconds
            if (thisValue.Seconds > otherValue.Seconds)
            {
                return 1;
            }

            if (thisValue.Seconds < otherValue.Seconds)
            {
                return -1;
            }

            // nanoseconds, ticks and milliseconds not allowed and must be zero
            return 0;
        }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is SimplifiedPeriod other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(SimplifiedPeriod)}");
        }

        public static bool operator <(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return Comparer<SimplifiedPeriod?>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return Comparer<SimplifiedPeriod?>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return Comparer<SimplifiedPeriod?>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return Comparer<SimplifiedPeriod?>.Default.Compare(left, right) >= 0;
        }

        public bool Equals(SimplifiedPeriod? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is SimplifiedPeriod other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimplifiedPeriod? left, SimplifiedPeriod? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
