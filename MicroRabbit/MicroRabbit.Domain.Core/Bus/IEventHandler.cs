using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    // in keyword  --> covariance,  it must handle(take in) any generic event, 
    public interface IEventHandler<in TEvent> :IEventHandler 
        where TEvent :Event
    {
        Task Handle(TEvent @event);

    }

    public interface IEventHandler
    {

    }
}
