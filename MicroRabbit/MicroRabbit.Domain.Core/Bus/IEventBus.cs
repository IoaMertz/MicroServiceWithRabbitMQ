using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventBus
    {
        //we will use mediatR to send commands to various places throught the bus
        Task SendCommand<T>(T command) where T : Command;
        // we want to publish events , @ because event is a reserved keyword
        void Publish<T>(T @event) where T : Event;
        // t -> event type , TH -> eventHandler
        void subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;
    }
}
