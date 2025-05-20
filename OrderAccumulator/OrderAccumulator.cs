using QuickFix;
using QuickFix.Fields;

namespace OrderAccumulator
{
    public class OrderAccumulator : MessageCracker, IApplication
    {
        private readonly Dictionary<string, List<QuickFix.FIX44.NewOrderSingle>> _orders = new Dictionary<string, List<QuickFix.FIX44.NewOrderSingle>>();
        private readonly Dictionary<string, decimal> _exposures = new Dictionary<string, decimal>();
        private const decimal limit = 1000000;
        private Session? _session { get; set; }

        public void OnCreate(SessionID sessionID) => _session = Session.LookupSession(sessionID);

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logou na sessão: " + sessionID);
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void ToApp(Message message, SessionID sessionID)
        {
        }

        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine("deslogou da sessão: " + sessionID);
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle order, SessionID sessionID)
        {
            Console.WriteLine("Ordem recebida: OrderID=" + order.ClOrdID + " Asset=" + order.Symbol.Value + " Lado=" + order.Side.Value + " Quantity=" + order.OrderQty.Value + " Preço=" + order.Price.Value);

            try
            {
                var execution = new QuickFix.FIX44.ExecutionReport();

                execution.Side = order.Side;
                execution.OrderQty = order.OrderQty;
                execution.Price = order.Price;
                execution.Symbol = order.Symbol;

                if (ProcessOrder(order))
                {
                    execution.OrdStatus = new OrdStatus(OrdStatus.NEW);
                    execution.ExecType = new ExecType(ExecType.NEW);
                }
                else
                {
                    execution.OrdStatus = new OrdStatus(OrdStatus.REJECTED);
                    execution.ExecType = new ExecType(ExecType.REJECTED);
                }

                Session.SendToTarget(execution, _session.SessionID);
            }
            catch
            {
                Console.WriteLine("Ocorreu um erro ao processar a ordem.");
            }

        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("Mensagem Enviada OrderGenerator:  " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("Mensagem recebida OrderGenerator:    " + message.ToString());
        }

        private bool ProcessOrder(QuickFix.FIX44.NewOrderSingle order)
        {
            if (order.Side.Value == Side.BUY)
            {
                if (!_exposures.ContainsKey(order.Symbol.ToString()))
                    _exposures.Add(order.Symbol.ToString(), 0);

                if ((_exposures[order.Symbol.Value] + (order.OrderQty.Value * order.Price.Value)) < limit)
                {
                    switch (order.Side.Value)
                    {
                        case Side.BUY:
                            _exposures[order.Symbol.ToString()] += order.OrderQty.Value * order.Price.Value;
                            break;
                        case Side.SELL:
                            _exposures[order.Symbol.ToString()] -= order.OrderQty.Value * order.Price.Value;
                            break;
                    }
                    Console.WriteLine("Ordem aceita");
                    Console.WriteLine("Exposição por símbolo:");
                    foreach (var symbol in _exposures.Keys)
                    {
                        Console.WriteLine(symbol + ": " + _exposures[symbol]);
                    }
                    Console.WriteLine("");
                    return true;
                }
                else
                {
                    Console.WriteLine("Ordem Rejeitada");
                    return false;
                }
            }
            else
            {
                if (!_exposures.ContainsKey(order.Symbol.ToString()))
                    _exposures.Add(order.Symbol.ToString(), 0);

                if ((_exposures[order.Symbol.Value] - (order.OrderQty.Value * order.Price.Value)) < limit)
                {
                    switch (order.Side.Value)
                    {
                        case Side.BUY:
                            _exposures[order.Symbol.ToString()] += order.OrderQty.Value * order.Price.Value;
                            break;
                        case Side.SELL:
                            _exposures[order.Symbol.ToString()] -= order.OrderQty.Value * order.Price.Value;
                            break;
                    }
                    Console.WriteLine("Ordem aceita");
                    Console.WriteLine("Exposição por símbolo:");
                    foreach (var symbol in _exposures.Keys)
                    {
                        Console.WriteLine(symbol + ": " + _exposures[symbol]);
                    }
                    Console.WriteLine("");
                    return true;
                }
                else 
                {
                    Console.WriteLine("Ordem Rejeitada");

                    return false;
                }
            }
        }
    }
}