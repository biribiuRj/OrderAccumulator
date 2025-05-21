using FluentAssertions;
using Moq;
using OrderAccumulator.Fix;
using OrderAccumulator.Interfaces;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Tests.IntegrationTest
{
    public class OrderAccumulatorAppIntegrationTests
    {
        private readonly Mock<IOrderProcessor> _processorMock;
        private readonly FakeMessageSender _fakeSender;
        private readonly OrderAccumulatorApp _app;

        public OrderAccumulatorAppIntegrationTests()
        {
            _processorMock = new Mock<IOrderProcessor>();
            _fakeSender = new FakeMessageSender();
            _app = new OrderAccumulatorApp(_processorMock.Object, _fakeSender);
        }

        [Fact]
        public void Should_Send_ExecutionReport_When_Order_Is_Accepted()
        {
            var sessionID = new SessionID("FIX.4.4", "SENDER", "TARGET");
            _processorMock.Setup(p => p.ProcessOrder(It.IsAny<NewOrderSingle>())).Returns(true);
            var order = CreateOrder();

            _app.OnMessage(order, sessionID);

            _fakeSender.LastMessage.Should().NotBeNull("uma mensagem deve ter sido enviada");

            var report = _fakeSender.LastMessage as ExecutionReport;
            report.Should().NotBeNull("a mensagem enviada deve ser um ExecutionReport");

            report!.OrdStatus.getValue().Should().Be(OrdStatus.NEW);
            report.ExecType.getValue().Should().Be(ExecType.NEW);
        }

        [Fact]
        public void Should_Send_Rejected_ExecutionReport_When_Order_Is_Rejected()
        {
            var sessionID = new SessionID("FIX.4.4", "SENDER", "TARGET");
            _processorMock.Setup(p => p.ProcessOrder(It.IsAny<NewOrderSingle>())).Returns(false);
            var order = CreateOrder();

            _app.OnMessage(order, sessionID);

            _fakeSender.LastMessage.Should().NotBeNull("uma mensagem deve ter sido enviada");

            var report = _fakeSender.LastMessage as ExecutionReport;
            report.Should().NotBeNull("a mensagem enviada deve ser um ExecutionReport");

            report!.OrdStatus.getValue().Should().Be(OrdStatus.REJECTED);
            report.ExecType.getValue().Should().Be(ExecType.REJECTED);
        }

        private NewOrderSingle CreateOrder(char side = Side.BUY, decimal qty = 100, decimal price = 10, string symbol = "VALE3")
        {
            return new NewOrderSingle(
                new ClOrdID(Guid.NewGuid().ToString()),
                new Symbol(symbol),
                new Side(side),
                new TransactTime(DateTime.UtcNow),
                new OrdType(OrdType.MARKET))
            {
                Symbol = new Symbol(symbol),
                OrderQty = new OrderQty(qty),
                Price = new Price(price)
            };
        }

        private class FakeMessageSender : IMessageSender
        {
            public QuickFix.Message? LastMessage { get; private set; }

            public void Send(QuickFix.Message message, SessionID sessionID)
            {
                LastMessage = message;
            }
        }
    }
}
