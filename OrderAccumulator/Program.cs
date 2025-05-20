using System;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace OrderAccumulator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("------------------------");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Recebendo conexões FIX");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("------------------------");
            Console.WriteLine();
            try
            {
                SessionSettings settings = new SessionSettings(@"config.cfg");
                IApplication app = new OrderAccumulator();
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new FileLogFactory(settings);
                DefaultMessageFactory messageFactory = new DefaultMessageFactory();
                IAcceptor acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory, messageFactory);

                acceptor.Start();
                Console.WriteLine("aperte <enter> para sair");
                Console.Read();
                acceptor.Stop();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("-- Erro --");
                Console.WriteLine(e.ToString());
            }
        }
    }
}