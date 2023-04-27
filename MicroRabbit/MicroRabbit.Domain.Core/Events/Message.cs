using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Events
{
    //Our Core message object ,  IRequest --> MediatR nugget,
    //Every request send a bool back px message send
    public abstract class Message :IRequest<bool>  
    {
        public string MessageType { get; protected set; }

        protected Message() 
        {
            MessageType = GetType().Name;
        }

    }
}
