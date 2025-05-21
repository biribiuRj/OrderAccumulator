using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using OrderAccumulator.Interfaces;
using Serilog;

namespace OrderAccumulator.Fix
{
    public class OrderAccumulatorApp : MessageCracker, IApplication
    {
        private readonly IOrderProcessor _orderProcessor;
        private readonly IMessageSender _messageSender;
        private Session? _session;

        public OrderAccumulatorApp(IOrderProcessor orderProcessor, IMessageSender messageSender)
        {
            _orderProcessor = orderProcessor;
            _messageSender = messageSender;
        }

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
            Log.Information("Sessão FIX criada: {SessionID}", sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            Log.Information("Logon na sessão: {SessionID}", sessionID);
        }

        public void OnLogout(SessionID sessionID)
        {
            Log.Information("Logout da sessão: {SessionID}", sessionID);
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            Log.Information("Mensagem administrativa recebida: {Message}", message.ToString());
        }

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            Log.Information("Mensagem administrativa enviada: {Message}", message.ToString());
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            Log.Information("Mensagem de aplicação enviada: {Message}", message.ToString());
        }

        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            Log.Information("Mensagem de aplicação recebida: {Message}", message.ToString());
            Crack(message, sessionID);
        }

        public void OnMessage(NewOrderSingle order, SessionID sessionID)
        {
            var orderId = order?.ClOrdID?.Value;
            var symbol = order?.Symbol?.Value;
            var side = order?.Side?.Value;
            var qty = order?.OrderQty?.Value;
            var price = order?.Price?.Value;

            Log.Information("Ordem recebida: OrderID={OrderId}, Symbol={Symbol}, Side={Side}, Qty={Qty}, Price={Price}",
                orderId, symbol, side, qty, price);

            try
            {
                var report = new ExecutionReport
                {
                    ClOrdID = order.ClOrdID,
                    OrderID = new OrderID(Guid.NewGuid().ToString()),
                    Symbol = order.Symbol,
                    Side = order.Side,
                    OrderQty = order.OrderQty,
                    Price = order.Price,
                    ExecID = new ExecID(Guid.NewGuid().ToString()),
                    TransactTime = new TransactTime(DateTime.UtcNow)
                };

                bool accepted = _orderProcessor.ProcessOrder(order);

                if (accepted)
                {
                    report.OrdStatus = new OrdStatus(OrdStatus.NEW);
                    report.ExecType = new ExecType(ExecType.NEW);
                    Log.Information("Ordem aceita e confirmada: {OrderId}", orderId);
                }
                else
                {
                    report.OrdStatus = new OrdStatus(OrdStatus.REJECTED);
                    report.ExecType = new ExecType(ExecType.REJECTED);
                    Log.Warning("Ordem rejeitada: {OrderId}", orderId);
                }

                _messageSender.Send(report, sessionID);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao processar a ordem {OrderId}", orderId);
            }
        }
    }
}
