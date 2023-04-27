using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    //sealed we dont want any1 to inherit or extend this class == sealed
    public sealed class RabbitMQBus : IEventBus
    {
        public Task SendCommand<T>(T command) where T : Command
        {
            throw new NotImplementedException();
        }

        public void Publish<T>(T @event) where T : Event
        {
            throw new NotImplementedException();
        }

        public void subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}
