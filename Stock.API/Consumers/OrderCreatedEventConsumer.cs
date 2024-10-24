using MassTransit;
using Shared;
using Shared.Events;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            Console.WriteLine(context.Message.OrderId + " - " + context.Message.BuyerId);

            if(context.Message.TotalPrice > 100)
            {
                StockReservedEvent reservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice
                };

                ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
                await endpoint.Send(reservedEvent);

                Console.WriteLine("Stok işlemleri başarılı");
            }
            else
            {
                StockNotReservedEvent notReservedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    Message = $"{context.Message.OrderId} numaralı siparişteki ürünlerin stokları bitmiştir."
                };

                await _publishEndpoint.Publish(notReservedEvent);

                Console.WriteLine("Stok işlemleri başarısız");
            }
        }
    }
}
