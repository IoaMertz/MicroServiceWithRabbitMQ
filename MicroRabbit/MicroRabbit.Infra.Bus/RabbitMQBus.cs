using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory() 
            {
                HostName ="localhost",
                //asynchronous consumer ??
                DispatchConsumersAsync = true,
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // in the publish method we make the name of the event similar to the type
            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            //We give a delegate(we ll make it ) that ll help as with the events.
            //as soon as a message comes in to our queue this method will be called 
            //and use the correct handler
            consumer.Received += Consumer_Recieved;

            // this consumes all the available messages ???
            // and here is where the delegate methos the consumer has ic called ???
            channel.BasicConsume(eventName, true, consumer);
        }


        // When we see an event this method will call the correct handler
        private async Task Consumer_Recieved(object sender, BasicDeliverEventArgs e)
        {
            // This comes from the e
            var eventName = e.RoutingKey;
            // We do the opossite procedure from when we publish the event. From bytes we get a string 
            var message = Encoding.UTF8.GetString(e.Body);

            try
            {
                // The ProccesEvent will know which handler is subscribed to this type of event.
                // And ll do all the work in the background
                await ProccesEvent(eventName , message).ConfigureAwait(false); //ConfigureAwait ????? something with multithreading
            }
            catch(Exception ex)
            {

            }
        }
        // this is where we look throw the dictionary of handlers 
        // The ProccesEvent will know which handler is subscribed to this type of event.
        // And ll do all the work in the background
        private async Task ProccesEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    // Generics just like using new to create an instance. Requires a parametereles ctor 
                    var handler = Activator.CreateInstance(subscription);

                    //why we need this?. If Handler is null continue looking
                    if (handler == null) continue;

                    // Now that we have the Handler. We look in the variable List that hold the event types 
                    //for the event Type based on the eventName
                    //(remember : SingleOrDefault throws an error in more that one is in the collection)
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);

                    //this make an object type (eventType), with the message. Maybe 
                    var @event = JsonConvert.DeserializeObject(message, eventType);

                    // All our event handlers implement
                    //typeof(IEventHandler<>) return a Type. <> indicates and open generic type.
                    //The MakeGenericType method takes one or more Type objects as parameters
                    //and returns a new Type object that represents a closed constructed type based on the original open generic type.
                    //In this case, the eventType parameter is used to create a closed constructed type of IEventHandler<T>,
                    //where T is the type represented by eventType.
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                    //this uses generic to take the method named Handle
                    //(Remember we have make the signature for this method in IEventHandler Interface we made so it has to be there)
                    //from the specific handler and use it passing the event object
                    //We ll create different event handlers for different use cases in our microservices.
                    //They will be inviked from our service buss over here
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
