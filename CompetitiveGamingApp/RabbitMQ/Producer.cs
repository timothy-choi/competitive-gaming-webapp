using System.Text;
using RabbitMQ.Client;


namespace CompetitiveGamingApp.RabbitMQ;


public interface IProducer {
    public void SendOnboardingMessage<T>(T message);
}

public class Producer : IProducer {
    private readonly ConnectionFactory _factory;
    public Producer() {
        _factory = new ConnectionFactory {
            HostName = "localhost"
        };
    }

    public void SendMerchantCreationMessage()

}