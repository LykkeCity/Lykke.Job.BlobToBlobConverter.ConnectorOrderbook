﻿using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Settings;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.PeriodicalHandlers;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Modules
{
    public class JobModule : Module
    {
        private readonly BlobToBlobConverterConnectorOrderbookSettings _settings;
        private readonly ILog _log;

        public JobModule(BlobToBlobConverterConnectorOrderbookSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
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
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterResourcesMonitoring(_log);

            builder.RegisterType<BlobReader<InConnectorOrderbook>>()
                .As<IBlobReader<InConnectorOrderbook>>()
                .SingleInstance()
                .WithParameter("container", _settings.InputContainer)
                .WithParameter("blobConnectionString", _settings.InputBlobConnString);

            builder.RegisterType<BlobSaver>()
                .As<IBlobSaver>()
                .SingleInstance()
                .WithParameter("blobConnectionString", _settings.OutputBlobConnString)
                .WithParameter("rootContainer", _settings.InputContainer);

            builder.RegisterType<MessageProcessor>()
                .As<IMessageProcessor<InConnectorOrderbook>>()
                .SingleInstance();

            builder.RegisterType<BlobProcessor<InConnectorOrderbook>>()
                .As<IBlobProcessor>()
                .SingleInstance();

            builder.RegisterType<PeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.BlobScanPeriod));
        }
    }
}
