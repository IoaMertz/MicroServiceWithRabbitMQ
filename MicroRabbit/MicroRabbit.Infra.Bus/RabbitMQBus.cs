using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
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
        private readonly IMediator _mediator;

        // this Dict ll hold all the handlers for the different events, it ll hold the Types of handlers
        // The Dictionary and List are like a subscription that knows which subscription is tied to which handlers and event
        private readonly Dictionary<string,List<Type>> _handlers;

        // list of event types
        private readonly List<Type> _eventTypes;

        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }
        public Task SendCommand<T>(T command) where T : Command
        { 
            // command : message , message:IRequest<bool>, _mediator.Send() expects a IRequest<bool>
            return _mediator.Send(command);
        }

        //Is used By Different Microservices to Publish Events To the RabbitMq Server
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                { 
                    //the method sees the abstract variable event and then with generics .GetType().Name
                    //we can see the name of the implementation clas!!
                    var eventName = @event.GetType().Name;

                    // declaring a queue in the rabbitmq server with the same name as the event
                    channel.QueueDeclare(eventName, false, false, false, null);

                    // we use NewtonSoft nugget to serialize @event object to a message - event 
                    var message = JsonConvert.SerializeObject(@event);

                    // putting the message int the body (rabbitMq)
                    var body = Encoding.UTF8.GetBytes(message);

                    //publish the message
                    channel.BasicPublish("",eventName,null,body);



                }
            }

        }

        // It needs the type of event and a handler that handles the specific type of the event
        //we use the local variable Dictionary, List, to store all the hanlders and eventTypes,
        // so we have the unique handlers for the events
        
        public void subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            //if the type of event isnt in the List add it
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            //if there is no key with the event add it to dictionary
            //(the List<Type> in the Dict contains all the different handlers that can handle the event)
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            // basic validation _handlers arleady has the handler registered dont add it and throw exception
            if (_handlers[eventName].Any(s => s.GetType()== handlerType))
            {
                // nameof() returns the name of the variable itself  == handlerType,
                // typeOf().Name OR .GetType().Name returns the name of the type that the variable holds
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} arleady registered for '{eventName}'", nameof(handlerType));
            }

            // if the Dictionary arleady has a key with the eventName and the handler isnt arleady in the List<> add it
            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
            

        }


    }
}
