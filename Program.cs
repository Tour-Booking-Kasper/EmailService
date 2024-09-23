class Program
{
    static void Main(string[] args)
    {
        var emailConsumer = new Consumer(
            hostName: "localhost",
            exchangeName: "tourExchange",
            queueName: "emailServiceQueue",
            routingKey: "tour.booked"
            deadLetterExchange: "deadLetterExchange",
            deadLetterQueue: "deadLetterQueue"
        );

        emailConsumer.Consume();
    }
}
