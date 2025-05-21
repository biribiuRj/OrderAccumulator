namespace OrderAccumulator.Interfaces
{
    public interface IOrderProcessor
    {
        bool ProcessOrder(QuickFix.FIX44.NewOrderSingle order);
    }
}
