using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderAccumulator.Config;
using OrderAccumulator.Fix;
using OrderAccumulator.Interfaces;
using OrderAccumulator.Services;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using Serilog;

namespace OrderAccumulator
{
    public class FixService : IHostedService
    {
        private readonly FixSettings _fixSettings;
        private IAcceptor _acceptor;

        public FixService(IOptions<FixSettings> fixSettings)
        {
            _fixSettings = fixSettings.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Iniciando serviço FIX com config: {File}, Limite: {Limit}", _fixSettings.ConfigFile, _fixSettings.ExposureLimit);
            Log.Information("Limite configurado: {limit}", _fixSettings.ExposureLimit);

            SessionSettings settings = new SessionSettings(_fixSettings.ConfigFile);
            IOrderProcessor processor = new OrderProcessor(_fixSettings.ExposureLimit);
            IMessageSender sender = new MessageSender();
            IApplication app = new OrderAccumulatorApp(processor, sender);

            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            IMessageFactory messageFactory = new DefaultMessageFactory();

            _acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory, messageFactory);
            _acceptor.Start();

            Log.Information("FIX Acceptor iniciado.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Parando serviço FIX...");
            _acceptor?.Stop();
            Log.Information("FIX Acceptor parado.");
            return Task.CompletedTask;
        }
    }
}
