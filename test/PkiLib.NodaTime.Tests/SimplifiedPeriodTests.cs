using NodaTime;
using NUnit.Framework;
using System;

namespace Org.PkiLib.NodaTime.Tests
{

    public class SimplifiedPeriodTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Years()
        {
            const int value = 100;
            const int years = 100;

            SimplifiedPeriod yearsWithoutRound = new SimplifiedPeriod(Period.FromYears(value), false);
            Assert.AreEqual(years, yearsWithoutRound.Value.Years);
            Assert.AreEqual(0, yearsWithoutRound.Value.Months);
            Assert.AreEqual(0, yearsWithoutRound.Value.Weeks);
            Assert.AreEqual(0, yearsWithoutRound.Value.Days);
            Assert.AreEqual(0, yearsWithoutRound.Value.Hours);
            Assert.AreEqual(0, yearsWithoutRound.Value.Minutes);
            Assert.AreEqual(0, yearsWithoutRound.Value.Seconds);
            Assert.AreEqual(0, yearsWithoutRound.Value.Milliseconds);
            Assert.AreEqual(0, yearsWithoutRound.Value.Ticks);
            Assert.AreEqual(0, yearsWithoutRound.Value.Nanoseconds);

            SimplifiedPeriod yearsWithRound = new SimplifiedPeriod(Period.FromYears(value), true);
            Assert.AreEqual(years, yearsWithRound.Value.Years);
            Assert.AreEqual(0, yearsWithRound.Value.Months);
            Assert.AreEqual(0, yearsWithRound.Value.Weeks);
            Assert.AreEqual(0, yearsWithRound.Value.Days);
            Assert.AreEqual(0, yearsWithRound.Value.Hours);
            Assert.AreEqual(0, yearsWithRound.Value.Minutes);
            Assert.AreEqual(0, yearsWithRound.Value.Seconds);
            Assert.AreEqual(0, yearsWithRound.Value.Milliseconds);
            Assert.AreEqual(0, yearsWithRound.Value.Ticks);
            Assert.AreEqual(0, yearsWithRound.Value.Nanoseconds);

            Assert.Pass();
        }

        [Test]
        public void Months()
        {
            const int value = 100;
            const int years = 8;
            const int months = 4;

            SimplifiedPeriod monthsWithoutRound = new SimplifiedPeriod(Period.FromMonths(value), false);
            Assert.AreEqual(years, monthsWithoutRound.Value.Years);
            Assert.AreEqual(months, monthsWithoutRound.Value.Months);
            Assert.AreEqual(0, monthsWithoutRound.Value.Weeks);
            Assert.AreEqual(0, monthsWithoutRound.Value.Days);
            Assert.AreEqual(0, monthsWithoutRound.Value.Hours);
            Assert.AreEqual(0, monthsWithoutRound.Value.Minutes);
            Assert.AreEqual(0, monthsWithoutRound.Value.Seconds);
            Assert.AreEqual(0, monthsWithoutRound.Value.Milliseconds);
            Assert.AreEqual(0, monthsWithoutRound.Value.Ticks);
            Assert.AreEqual(0, monthsWithoutRound.Value.Nanoseconds);

            SimplifiedPeriod monthsWithRound = new SimplifiedPeriod(Period.FromMonths(value), true);
            Assert.AreEqual(years, monthsWithRound.Value.Years);
            Assert.AreEqual(months, monthsWithRound.Value.Months);
            Assert.AreEqual(0, monthsWithRound.Value.Weeks);
            Assert.AreEqual(0, monthsWithRound.Value.Days);
            Assert.AreEqual(0, monthsWithRound.Value.Hours);
            Assert.AreEqual(0, monthsWithRound.Value.Minutes);
            Assert.AreEqual(0, monthsWithRound.Value.Seconds);
            Assert.AreEqual(0, monthsWithRound.Value.Milliseconds);
            Assert.AreEqual(0, monthsWithRound.Value.Ticks);
            Assert.AreEqual(0, monthsWithRound.Value.Nanoseconds);

            Assert.Pass();
        }

        [Test]
        public void Weeks()
        {
            const int value = 101;
            const int years = 2;
            const int months = 1;
            const int weeks = 1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SimplifiedPeriod weeksWithoutRound = new SimplifiedPeriod(Period.FromWeeks(value), false);
            });

            SimplifiedPeriod weeksWithRound = new SimplifiedPeriod(Period.FromWeeks(value), true);
            Assert.AreEqual(years, weeksWithRound.Value.Years);
            Assert.AreEqual(months, weeksWithRound.Value.Months);
            Assert.AreEqual(weeks, weeksWithRound.Value.Weeks);
            Assert.AreEqual(0, weeksWithRound.Value.Days);
            Assert.AreEqual(0, weeksWithRound.Value.Hours);
            Assert.AreEqual(0, weeksWithRound.Value.Minutes);
            Assert.AreEqual(0, weeksWithRound.Value.Seconds);
            Assert.AreEqual(0, weeksWithRound.Value.Milliseconds);
            Assert.AreEqual(0, weeksWithRound.Value.Ticks);
            Assert.AreEqual(0, weeksWithRound.Value.Nanoseconds);

            Assert.Pass();
        }
    }
}