using QuickFix.Fields;
using QuickFix.FIX44;
using OrderAccumulator.Interfaces;
using Serilog;

namespace OrderAccumulator.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly Dictionary<string, decimal> _exposures = new();
        private readonly decimal _limit;

        public OrderProcessor(decimal limit)
        {
            _limit = limit;
        }

        public bool ProcessOrder(NewOrderSingle order)
        {
            var symbol = order.Symbol.Value;
            var quantity = order.OrderQty.Value;
            var price = order.Price.Value;
            var side = order.Side.Value;

            if (!_exposures.ContainsKey(symbol))
                _exposures[symbol] = 0;

            var impact = side == Side.BUY ? quantity * price : -quantity * price;
            var newExposure = _exposures[symbol] + impact;

            if (Math.Abs(newExposure) <= _limit)
            {
                _exposures[symbol] = newExposure;
                Log.Information("Ordem aceita");
                PrintExposures();
                return true;
            }
            Log.Information("Ordem rejeitada");

            return false;
        }

        private void PrintExposures()
        {
            Log.Information("Exposição por símbolo:");
            foreach (var item in _exposures)
            Log.Information($"{item.Key}: {item.Value}");
        }
    }
}