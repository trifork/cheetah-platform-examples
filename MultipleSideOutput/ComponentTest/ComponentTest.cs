using System;
using System.Collections.Generic;
using Cheetah.ComponentTest.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using MultipleSideOutputExample.ComponentTest.Models;

namespace MultipleSideOutputExample.ComponentTest;

[Trait("TestType", "IntegrationTests")]
public class ComponentTest
{
    readonly IConfiguration _configuration;

    public ComponentTest()
    {
        // These will be overriden by environment variables from compose
        var conf = new Dictionary<string, string>()
        {
            {"KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token"},
            {"KAFKA:CLIENTID", "ClientId" },
            {"KAFKA:CLIENTSECRET", "1234" },
            {"KAFKA:URL", "localhost:9092"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(conf)
            .AddEnvironmentVariables()
            .Build();
    }

    [Fact]
    public void Should_BeImplemented_When_ServiceIsCreated()
    {
        // Arrange
        // Here you'll set up one or more writers and readers, which connect to the topic(s) that your job consumes
        // from and publishes to. 
        var writer = KafkaWriterBuilder.Create<string, MultipleSideOutputExampleInputEvent>(_configuration)
            .WithTopic("MultipleSideOutputExampleInputTopic") // The topic to consume from
            .WithKeyFunction(model => model.DeviceId) // Optional function to retrieve the message key.
                                                          // If no key is desired, use KafkaWriterBuilder.Create<Null, InputModel>
                                                          // and make this function return null
            .Build();

        var reader = KafkaReaderBuilder.Create<string, MultipleSideOutputExampleOutputEvent>(_configuration)
            .WithTopic("MultipleSideOutputExampleOutputTopic")
            .WithGroupId("MyGroup")
            .Build();
        
        // Act
        // Write one or more messages to the writer
        var inputEvent = new MultipleSideOutputExampleInputEvent()
        {
            DeviceId = "deviceId-1",
            ValueA = 12.34,
            ValueB = 22.34,
            ValueC = 32.34,
            ValueD = 42.34,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };
        
        writer.Write(inputEvent);
        
        // Assert
        // Then consume using the reader, supplying how many output messages your input messages expected to generate
        // as well as the maximum duration it is allowed to take for those messages to be produced
        var messages = reader.ReadMessages(1, TimeSpan.FromSeconds(20));
        
        // Then evaluate whether your messages are as expected, and that there are only as many as you expected 
        messages.Should().ContainSingle(message => 
            message.DeviceId == inputEvent.DeviceId && 
            message.Value == inputEvent.ValueA &&
            message.Timestamp == inputEvent.Timestamp &&
            message.ExtraField == "ExtraFieldValue");
        reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)).Should().BeTrue();

        Assert.True(false, "This is really just here to make the test fail and ensure that you either decide to implement a component test or actively decide not to");
    }
}