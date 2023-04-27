using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Commands
{
    // when we send a command we send a message across our buss.
    // the message ends up in a different microservice
    public abstract class Command : Message  
    {
        //basic property of every command. It has to be send at a specific time.
        // only those you inherit from this class can se the time stamp (protected)
        public DateTime Timestamp { get; protected set; }  

        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    }
}
