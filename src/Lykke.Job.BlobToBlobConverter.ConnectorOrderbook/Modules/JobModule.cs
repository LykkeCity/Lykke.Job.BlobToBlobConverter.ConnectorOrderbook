using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.PeriodicalHandlers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Settings;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Modules
{
    public class JobModule : Module
    {
        private readonly BlobToBlobConverterConnectorOrderbookSettings _settings;
        private readonly ILog _log;
        private readonly string _instanceTag;

        public JobModule(BlobToBlobConverterConnectorOrderbookSettings settings, ILog log, string instanceTag)
        {
            _settings = settings;
            _log = log;
            _instanceTag = instanceTag;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterResourcesMonitoring(_log);

            builder.RegisterType<BlobReader>()
                .As<IBlobReader>()
                .SingleInstance()
                .WithParameter("container", _settings.InputContainer)
                .WithParameter("blobConnectionString", _settings.InputBlobConnString);

            builder.RegisterType<BlobSaver>()
                .As<IBlobSaver>()
                .SingleInstance()
                .WithParameter("blobConnectionString", _settings.OutputBlobConnString)
                .WithParameter("rootContainer", _settings.InputContainer);

            builder.RegisterType<MessageProcessor>()
                .As<IMessageProcessor>()
                .SingleInstance();

            builder.RegisterType<TypeResolver>()
                .As<IMessageTypeResolver>()
                .SingleInstance();

            builder.RegisterType<StructureBuilder>()
                .As<IStructureBuilder>()
                .As<IDynamicStructure>()
                .SingleInstance()
                .WithParameter("instanceTag", _instanceTag);

            builder.RegisterType<BlobProcessor>()
                .As<IBlobProcessor>()
                .SingleInstance();

            builder.RegisterType<PeriodicalHandler>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.BlobScanPeriod));
        }
    }
}
