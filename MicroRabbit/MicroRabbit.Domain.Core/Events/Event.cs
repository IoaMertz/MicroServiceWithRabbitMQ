using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Event
    {
        // protected set so only the class itself and subclasses can modify the value 
        public DateTime Timestamp { get; protected set; }
        // protected ctor so it can only be called from within the class or derived classes
        protected Event()
        {
            Timestamp = DateTime.Now;
        }


    }
}
