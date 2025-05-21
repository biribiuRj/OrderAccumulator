using FluentAssertions;
using OrderAccumulator.Services;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Tests.UnitTest
{
    public class OrderProcessorTests
    {
        private readonly OrderProcessor _processor;

        public OrderProcessorTests()
        {
            _processor = new OrderProcessor(100000000);
        }

        private NewOrderSingle CreateOrder(string symbol, decimal qty, decimal price, char side)
        {
            return new NewOrderSingle(
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(symbol),
                new Side(side),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.MARKET))
            {
                Symbol = new Symbol(symbol),
                OrderQty = new OrderQty(qty),
                Price = new Price(price)
            };
        }

        [Fact]
        public void Should_Accept_BuyOrder_When_Exposure_Is_Below_Limit()
        {
            var order = CreateOrder("VIIA4", 100, 5000, Side.BUY);

            var result = _processor.ProcessOrder(order);

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_Reject_BuyOrder_When_Exposure_Exceeds_Limit()
        {
            var order = CreateOrder("VIIA4", 1000, 2000000, Side.BUY);

            var result = _processor.ProcessOrder(order);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Track_Exposure_By_Symbol_Independently()
        {
            var buyApple = CreateOrder("PETR4", 100, 1000, Side.BUY);
            var buyGoogle = CreateOrder("PETR4", 100, 1000, Side.BUY);

            _processor.ProcessOrder(buyApple).Should().BeTrue();
            _processor.ProcessOrder(buyGoogle).Should().BeTrue();
        }

        [Fact]
        public void Should_Accept_SellOrder_When_It_Decreases_Exposure()
        {
            var buy = CreateOrder("PETR4", 100, 9000, Side.BUY);
            var sell = CreateOrder("VALE3", 50, 1000, Side.SELL);

            _processor.ProcessOrder(buy).Should().BeTrue();
            _processor.ProcessOrder(sell).Should().BeTrue();
        }

        [Fact]
        public void Should_Reject_SellOrder_When_It_Makes_Exposure_Too_Negative()
        {
            var sell = CreateOrder("VALE3", 1000, 200000, Side.SELL);

            var result = _processor.ProcessOrder(sell);

            result.Should().BeFalse();
        }
    }
}
