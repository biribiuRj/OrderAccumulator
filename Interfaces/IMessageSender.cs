using QuickFix;

namespace OrderAccumulator.Interfaces
{
    public interface IMessageSender
    {
        void Send(Message message, SessionID sessionID);
    }
}