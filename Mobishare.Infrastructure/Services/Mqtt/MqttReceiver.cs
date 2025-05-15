using System.Text;
using MQTTnet;
using MQTTnet.Client;

namespace Mobishare.Infrastructure.Services;

public class MqttReceiver : IDisposable
{
    private readonly string brokerAddress;
    private readonly string topic;
    private readonly MqttFactory factory;
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions options;

    public event Action<string>? TopicMessage; // event that handle the messages received from the broker
    /*Action rappresents a method that requires a string as a parameter and doasn't return nothing (if i want to return something i must use Func<>)*/

    /// <summary>
    /// Constructor for the Receiver class.
    /// Initializes the MQTT client and sets up the broker address and topic.
    /// </summary>
    /// <param name="brokerAddress"></param>
    /// <param name="topic"></param>
    public MqttReceiver(string brokerAddress, string topic)
    {
        this.brokerAddress = brokerAddress;
        this.topic = topic;

        // Creation of an MQTTFactory object, used for creating MQTT clients
        factory = new MqttFactory();

        // Creaton of client using the MQTTFactory
        mqttClient = factory.CreateMqttClient();

        /* Configuration of the client's options using the class MqttClientOptionsBuilder
            - .WithClientId() -> Sets the unique client identifier
            - .WithTcpServer() -> Sets the address of the broker where the client will get the messages
            - .Build() -> Create the MqttClientOptions object
        */
        options = new MqttClientOptionsBuilder().WithClientId("ReceiverClient").WithTcpServer(brokerAddress).Build();
        
        // Set up the message handler
        mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
    }

    // method that get the message's payload and send it to the event MessageReceived
    private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            // get the message's payload
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            //Console.WriteLine($"Received message: {payload}");
            
            // Invoke the event with the received message
            TopicMessage?.Invoke(payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    // method that start the comunication with the broker
    public async Task StartAsync()
    {

        // Connect the client to the broker using the options created created before (CancellationToken.None indicate that no cancellation is requested)
        await mqttClient.ConnectAsync(options, CancellationToken.None); //async because it's a async method
        Console.WriteLine("broker connected");

        /* Subscription to the specified topic
            - .WithTopic() -> Sets the topic to subscribe
            - .Build() -> Create the MqttTopicFilter object
            - .SubscribeAsync() -> Subscribe to the topic using the client
        */
        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
    }

    // method that stops the comunication with the broker
    public async Task StopAsync()
    {
        // Disconnect the client from the broker
        var disconnectOptions = new MqttClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(disconnectOptions, CancellationToken.None);
        Console.WriteLine("broker disconnected");
    }

    // static async Task Main(string[] args)
    // {
    //     var receiver = new MqttReceiver("broker.hivemq.com", "arduino/gps");

    //     receiver.TopicMessage += (msg) =>
    //     {
    //         Console.WriteLine($"[Main] Message received by handler: {msg}");
    //     };

    //     await receiver.StartAsync();

    //     Console.WriteLine("Press any key to exit...");
    //     Console.ReadKey();

    //     await receiver.StopAsync();
    //     Console.WriteLine("Receiver stopped.");
    // }

    public void Dispose()
    {
        // Dispose of the MQTT client
        mqttClient.Dispose();
    }
}

