using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OrderService.Tests.Fakes;
using OrderService.WebApi.Controllers;
using OrderService.WebApi.UseCases;

namespace OrderService.Tests;

[TestFixture]
public class OrdersControllerTests
{
    [Test]
    public async Task CreateOrder_ReturnsOkWithCreatedOrder()
    {
        var expected = new OrderDto { Id = 10, ProductId = 7, Amount = 2, Price = 150m };
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(expected));
        var controller = new OrdersController(mediator);
        var command = new CreateOrderCommand { ProductId = 7, Amount = 2, Price = 150m };

        var response = await controller.CreateOrder(command);

        var ok = response.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(expected));
        Assert.That(mediator.LastRequest, Is.SameAs(command));
    }

    [Test]
    public async Task GetOrder_WhenOrderExists_ReturnsOk()
    {
        var expected = new OrderDto { Id = 3, ProductId = 9, Amount = 1, Price = 40m };
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(expected));
        var controller = new OrdersController(mediator);

        var response = await controller.GetOrder(expected.Id);

        var ok = response.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(expected));
        var query = mediator.LastRequest as GetOrderQuery;
        Assert.That(query, Is.Not.Null);
        Assert.That(query!.Id, Is.EqualTo(expected.Id));
    }

    [Test]
    public async Task GetOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(null));
        var controller = new OrdersController(mediator);

        var response = await controller.GetOrder(404);

        Assert.That(response.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteOrder_WhenDeleted_ReturnsNoContent()
    {
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(true));
        var controller = new OrdersController(mediator);

        var response = await controller.DeleteOrder(12);

        Assert.That(response, Is.TypeOf<NoContentResult>());
        var command = mediator.LastRequest as DeleteOrderCommand;
        Assert.That(command, Is.Not.Null);
        Assert.That(command!.Id, Is.EqualTo(12));
    }

    [Test]
    public async Task DeleteOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(false));
        var controller = new OrdersController(mediator);

        var response = await controller.DeleteOrder(99);

        Assert.That(response, Is.TypeOf<NotFoundResult>());
    }
}
