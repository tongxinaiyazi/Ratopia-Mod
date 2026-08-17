using System;
using System.Collections.Generic;
using RatopiaMod.YunQing.All.Core;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class ExchangeTicketSelectorTests
    {
        [Theory]
        [InlineData(0, "positive-first")]
        [InlineData(1, "positive-max")]
        [InlineData(3, "negative-first")]
        [InlineData(4, "negative-max")]
        public void OverrideModesSelectTheExpectedTicket(int modeValue, string expectedName)
        {
            var original = new Ticket("original", 1f);
            var positiveFirst = new Ticket("positive-first", 2f);
            var positiveMax = new Ticket("positive-max", 8f);
            var negativeFirst = new Ticket("negative-first", -1f);
            var negativeMax = new Ticket("negative-max", -5f);
            var zero = new Ticket("zero", 0f);
            var tickets = new List<Ticket>
            {
                negativeMax,
                zero,
                positiveMax,
                negativeFirst,
                positiveFirst
            };

            var selected = ExchangeTicketSelector.SelectOrOriginal(
                original,
                tickets,
                (ExchangeRateMode)modeValue,
                ticket => ticket.Rate,
                values => Reorder(values, positiveFirst, negativeFirst, zero, positiveMax, negativeMax),
                error => throw new Xunit.Sdk.XunitException(error.ToString()));

            Assert.Equal(expectedName, selected.Name);
        }

        [Fact]
        public void CommonModeReturnsOriginalAndPreservesShuffleSideEffect()
        {
            var original = new Ticket("original", 1f);
            var first = new Ticket("first", -1f);
            var second = new Ticket("second", 2f);
            var tickets = new List<Ticket> { first, second };
            var shuffleCalls = 0;

            var selected = ExchangeTicketSelector.SelectOrOriginal(
                original,
                tickets,
                ExchangeRateMode.COMMON,
                ticket => ticket.Rate,
                values =>
                {
                    shuffleCalls++;
                    values.Reverse();
                },
                error => throw new Xunit.Sdk.XunitException(error.ToString()));

            Assert.Same(original, selected);
            Assert.Equal(1, shuffleCalls);
            Assert.Same(second, tickets[0]);
        }

        [Fact]
        public void UnknownModeReturnsOriginalAfterRunningOriginalSelectionPreparation()
        {
            var original = new Ticket("original", 1f);
            var tickets = new List<Ticket>
            {
                new Ticket("negative", -1f),
                new Ticket("positive", 1f)
            };
            var shuffleCalls = 0;

            var selected = ExchangeTicketSelector.SelectOrOriginal(
                original,
                tickets,
                (ExchangeRateMode)999,
                ticket => ticket.Rate,
                values => shuffleCalls++,
                error => throw new Xunit.Sdk.XunitException(error.ToString()));

            Assert.Same(original, selected);
            Assert.Equal(1, shuffleCalls);
        }

        [Fact]
        public void InvalidTicketSetFallsBackToOriginalAndReportsTheError()
        {
            var original = new Ticket("original", 1f);
            var tickets = new List<Ticket>
            {
                new Ticket("negative-a", -1f),
                new Ticket("negative-b", -2f)
            };
            Exception reported = null;

            var selected = ExchangeTicketSelector.SelectOrOriginal(
                original,
                tickets,
                ExchangeRateMode.POSITIVE,
                ticket => ticket.Rate,
                values => values.Reverse(),
                error => reported = error);

            Assert.Same(original, selected);
            Assert.IsType<InvalidOperationException>(reported);
        }

        private static void Reorder(List<Ticket> values, params Ticket[] ordered)
        {
            values.Clear();
            values.AddRange(ordered);
        }

        private sealed class Ticket
        {
            internal Ticket(string name, float rate)
            {
                Name = name;
                Rate = rate;
            }

            internal string Name { get; }

            internal float Rate { get; }
        }
    }
}
