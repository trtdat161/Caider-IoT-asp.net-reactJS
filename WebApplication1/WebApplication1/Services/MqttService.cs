using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace WebApplication1.Services
{
    public class MqttService
    {
        private IManagedMqttClient _mqttClient;
        private readonly ILogger<MqttService> _logger;

        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger;
        }

        // Kết nối MQTT 
        public async Task ConnectAsync(string broker, int port)
        {
            try
            {
                var messageBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(broker, port) // Kết nối ĐẾN broker.hivemq.com:1883 tại địa chỉ này
                    .WithClientId($"AspNetCore_{Guid.NewGuid()}")
                    .WithCleanSession();

                var options = new ManagedMqttClientOptionsBuilder()
                    .WithClientOptions(messageBuilder.Build())
                    .Build();
                // Điều này tạo MQTT Client(ko phải broker) cho ASP.NET Core backend 
                // tức là ASP.NET Core = MQTT Client 1
                // còn esp32 tạo mqtt client cho nó riêng thì sẽ là mqtt client 2
                // cả hai đều connect 1 broker
                _mqttClient = new MqttFactory().CreateManagedMqttClient();

                _mqttClient.ConnectedAsync += async e =>
                {
                    _logger.LogInformation("ok MQTT Broker");
                };

                _mqttClient.DisconnectedAsync += async e =>
                {
                    _logger.LogWarning("disconnected MQTT");
                };
                // kết nối đên broker
                await _mqttClient.StartAsync(options);
                _logger.LogInformation("connecting MQTT...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error connect MQTT");
                throw;
            }
        }

        // Gửi message
        public async Task PublishAsync(string topic, string payload)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                throw new InvalidOperationException("MQTT not connected!");
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.EnqueueAsync(message);
            _logger.LogInformation(" send: {Payload}", payload);
        }
    }
}