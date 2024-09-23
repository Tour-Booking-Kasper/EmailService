using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            string exchangeName = "tourExchange";

            // Declare the topic exchange
            channel.ExchangeDeclare(exchange: exchangeName, type: "topic");

            // Declare a queue for the email service
            var queueName = "emailServiceQueue";
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Bind the queue to the exchange with routing key 'tour.booked'
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "tour.booked");

            Console.WriteLine(" [*] Waiting for booking messages for Email Service...");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Email Service Received booking: {message}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
