using Inventory.Consumers;
using Inventory.Contracts.Commands;
using Inventory.Logic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared;
using Xunit;

namespace Inventory.Tests.Consumers;

public class OrderItemConsumerTests
{
    private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> serviceScopeMock;
    private readonly Mock<IServiceProvider> serviceProviderMock;
    private readonly Mock<IConsumer> consumerMock;
    private readonly Mock<IOrderItemLogic> orderItemLogicMock;
    private readonly OrderItemConsumer sut;

    public OrderItemConsumerTests()
    {
        serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeMock = new Mock<IServiceScope>();
        serviceProviderMock = new Mock<IServiceProvider>();
        consumerMock = new Mock<IConsumer>();
        orderItemLogicMock = new Mock<IOrderItemLogic>();

        serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IOrderItemLogic)))
            .Returns(orderItemLogicMock.Object);

        sut = new OrderItemConsumer(serviceScopeFactoryMock.Object, consumerMock.Object);
    }

    [Fact]
    public void QueueName_ShouldReturnCorrectValue()
    {
        // Arrange
        var expectedQueueName = "inventory.order-item.process";

        // Act
        var queueName = sut.GetType()
            .GetProperty("QueueName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(sut) as string;

        // Assert
        Assert.Equal(expectedQueueName, queueName);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldCallProcessOrderItem()
    {
        // Arrange
        var command = new OrderItemProcess();
        var cancellationToken = CancellationToken.None;

        // Act
        var handleMethod = sut.GetType()
            .GetMethod("HandleMessageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        await (Task)handleMethod!.Invoke(sut, new object[] { command, serviceProviderMock.Object, cancellationToken })!;

        // Assert
        orderItemLogicMock.Verify(x => x.ProcessOrderItem(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldRetrieveOrderItemLogicFromServiceProvider()
    {
        // Arrange
        var command = new OrderItemProcess();
        var cancellationToken = CancellationToken.None;

        // Act
        var handleMethod = sut.GetType()
            .GetMethod("HandleMessageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        await (Task)handleMethod!.Invoke(sut, new object[] { command, serviceProviderMock.Object, cancellationToken })!;

        // Assert
        serviceProviderMock.Verify(x => x.GetService(typeof(IOrderItemLogic)), Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_WithCancellationToken_ShouldPassTokenToLogic()
    {
        // Arrange
        var command = new OrderItemProcess();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var handleMethod = sut.GetType()
            .GetMethod("HandleMessageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        await (Task)handleMethod!.Invoke(sut, new object[] { command, serviceProviderMock.Object, cancellationToken })!;

        // Assert
        orderItemLogicMock.Verify(x => x.ProcessOrderItem(command, cancellationToken), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithRequiredDependencies()
    {
        // Act
        var consumer = new OrderItemConsumer(serviceScopeFactoryMock.Object, consumerMock.Object);

        // Assert
        Assert.NotNull(consumer);
    }
}