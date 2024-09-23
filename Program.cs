class Program
{
    static void Main(string[] args)
    {
        //Opretter en ny consumer, med de nødvendige parametre
        var emailConsumer = new Consumer(
            hostName: "localhost",
            exchangeName: "tourExchange",
            queueName: "emailServiceQueue",
            routingKey: "tour.booked",
            deadLetterExchange: "deadLetterExchange",
            deadLetterQueue: "deadLetterQueue"
        );

        emailConsumer.Consume();
    }
}
