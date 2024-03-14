using System;
using System.Collections.Generic;
using Cheetah.Kafka.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using jsonToAvro.ComponentTest.Models;

namespace jsonToAvro.ComponentTest;

[Trait("TestType", "IntegrationTests")]
public class ComponentTest
{
    [Fact]
    public void Json_To_Avro_Component_Test()
    {
        // Arrange
        // Setup configuration. Configuration from appsettings.json is overridden by environment variables.
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        // Create a KafkaTestClientFactory to create KafkaTestReaders and KafkaTestWriters
        var kafkaClientFactory = KafkaTestClientFactory.Create(configuration);

        // Create writer and reader using a key as "timestamp"
//        var writer = kafkaClientFactory.CreateAvroTestWriter<string, OutputEventAvro>("jsonToAvroOutputTopic", item => item.timestamp.ToString());
        // Create writer and reader with no key
        var writer = kafkaClientFactory.CreateAvroTestWriter<OutputEventAvro>("jsonToAvroOutputTopic");

        var reader = kafkaClientFactory.CreateAvroTestReader<OutputEventAvro>("jsonToAvroOutputTopic");

        // Act
        // Create InputEvent
        var inputEvent = new OutputEventAvro()
        {
            deviceId = "deviceId-1",
            value = 12.34,
            timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            extraField = "ExtraFieldValue"
        };

        writer.WriteAsync(inputEvent);

        // Assert
        var messages = reader.ReadMessages(1, TimeSpan.FromSeconds(10));

        messages.Should().ContainSingle(message =>
            message.deviceId == inputEvent.deviceId &&
            message.value == inputEvent.value &&
            message.timestamp == inputEvent.timestamp &&
            message.extraField == "ExtraFieldValue");
        reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)).Should().BeTrue();
    }
}