using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (context.Message.TotalPrice > 300)
            {
                //Ödeme başarılı
                PaymentCompletedEvent completedEvent = new()
                {
                    OrderId = context.Message.OrderId
                };

                _publishEndpoint.Publish(completedEvent);

                Console.WriteLine("Ödeme başarılı");
            }
            else
            {
                //Ödeme başarısız
                PaymentFailedEvent failedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    Message = $"{context.Message.OrderId} numaralı siparişin ödemesi başarısız. Bakiye yetersiz."
                };

                _publishEndpoint.Publish(failedEvent);

                Console.WriteLine("Ödeme başarısız");
            }

            return Task.CompletedTask;
        }
    }
}
