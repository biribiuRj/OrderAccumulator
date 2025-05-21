using OrderAccumulator.Interfaces;
using QuickFix;

namespace OrderAccumulator.Fix
{
    public class MessageSender : IMessageSender
    {
        public void Send(Message message, SessionID sessionID)
        {
            Session.SendToTarget(message, sessionID);
        }
    }
}