using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

public class Consumer
{
    //Opretter properties, for de nødvenige parametre for RabbitMQ
    public string HostName { get; set; }
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public string RoutingKey { get; set; }
    public string DeadLetterExchange { get; set; }
    public string DeadLetterQueue { get; set; }

    // Constructor til at initialisere properties
    public Consumer(string hostName, string exchangeName, string queueName, string routingKey)
    {
        HostName = hostName;
        ExchangeName = exchangeName;
        QueueName = queueName;
        RoutingKey = routingKey;
        DeadLetterExchange
        DeadLetterQueue
    }

    //Consume metode til at modtage beskeder fra RabbitMQ, samt at validere beskederne
    //Her ved jeg godt, at det kunne splittes op yderligere, beklager jeg ikke helt følger SRP her Morten :(
    public void Consume()
    {
        var factory = new ConnectionFactory() { HostName = this.HostName };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Opretter en dead letter exchange og queue.
            channel.ExchangeDeclare(exchange: DeadLetterExchange, type: "direct");
            channel.QueueDeclare(queue: DeadLetterQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare(exchange: this.ExchangeName, type: "topic");
            var queueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DeadLetterExchange }
            };
            channel.QueueDeclare(queue: this.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: queueArguments);
            channel.QueueBind(queue: this.QueueName, exchange: this.ExchangeName, routingKey: this.RoutingKey);

            Console.WriteLine(" [*] Waiting for all tour messages for Back Office...");

            //Opretter en consumer, som lytter efter beskeder på den angivne kø, i dette tilfælde, backOfficeQueue
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                var validation = IsMessageInvalid(messageObject);
                
                //Hvis den ikke er valid, skriver vi det til konsollen, og sender beskeden til dead letter queue.
                if (!validation.Valid)
                {
                    var invalidMessage = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: DeadLetterExchange, routingKey: "", body: invalidMessage);
                    Console.WriteLine(" [!] Invalid message received. Sent to dead letter queue.");
                    return;
                }

                //Hvis beskeden er valid, skriver vi den til konsollen.
                Console.WriteLine($" [x] Processed message: {message}");
            };

            channel.BasicConsume(queue: this.QueueName, autoAck: true, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }

    //Metode, som returnerer et MessageValidation objekt, som indeholder en bool og en string
    private MessageValidation IsMessageInvalid(Message message)
    {
        Console.WriteLine("Validating message...");

        //Hvis navn er email er tomme, returnerer vi valid = false, og en besked.
        if (string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Email))
        {
            {
                return new MessageValidation(false, "Name and Email are required.");
            }
        }

        //Evt. kan der tilføjes yderligere validering her

        //Hvis message er valid, returnerer vi true
        return new MessageValidation(true, "Success");
    }
}
