using System;
using FluentAssertions;
using NUnit.Framework;
using process_runtime_monitor;

namespace Tests
{
    [TestFixture]
    public class ProcessTableEntityTests
    {
        DateTime TenThirty = new DateTime(2012, 1, 1, 10, 30, 0);
        DateTime TenThirtyTwo = new DateTime(2012, 1, 1, 10, 32, 15);
        DateTime TenFifty = new DateTime(2012, 1, 1, 10, 50, 0);
        DateTime TenFiftyFive = new DateTime(2012, 1, 1, 10, 55, 15);

        [Test]
        public void AddNewTimeSlotUpdatesTotalMinutesRun()
        {
            var p = new ProcessTableEntity();
            p.TotalMinutesRun.Should().Be(0);
            p.AddRunTimes(TenFifty, TenFiftyFive);
            p.TotalMinutesRun.Should().Be(5);
        }

        [Test]
        public void AddNewTimeSlotMultipleTimesCorrectlyUpdatesTotalMinutesRun()
        {
            var p = new ProcessTableEntity();
            p.TotalMinutesRun.Should().Be(0);
            p.AddRunTimes(TenThirty, TenThirtyTwo);
            p.AddRunTimes(TenFifty, TenFiftyFive);
            p.TotalMinutesRun.Should().Be(7);
        }

        [Test]
        public void AddNewTimeSlotUpdatesTimesRun()
        {
            var p = new ProcessTableEntity();
            p.TotalMinutesRun.Should().Be(0);
            p.AddRunTimes(TenFifty, TenFiftyFive);
            p.TimesRun.Should().Be("10:50|10:55");
        }

        [Test]
        public void AddNewTimeSlowMultipleTimesCorrectlyAppendsToTimesRun()
        {
            var p = new ProcessTableEntity();
            p.TotalMinutesRun.Should().Be(0);
            p.AddRunTimes(TenThirty, TenThirtyTwo);
            p.AddRunTimes(TenFifty, TenFiftyFive);
            p.TimesRun.Should().Be("10:30|10:32;10:50|10:55");
        }
    }
}
