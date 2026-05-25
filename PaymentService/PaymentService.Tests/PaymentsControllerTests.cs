using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PaymentService.Tests.Fakes;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.UseCases;

namespace PaymentService.Tests;

[TestFixture]
public class PaymentsControllerTests
{
    [Test]
    public async Task CreatePayment_ReturnsCreatedWithPayment()
    {
        var expected = new PaymentDto { Id = 1, OrderId = 15, Price = 300m };
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(expected));
        var controller = new PaymentsController(mediator, NullLogger<PaymentsController>.Instance);
        var command = new CreatePaymentCommand { OrderId = 15, Price = 300m };

        var response = await controller.CreatePayment(command);

        var created = response.Result as CreatedResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Location, Is.EqualTo("/api/payments/get/15"));
        Assert.That(created.Value, Is.SameAs(expected));
        Assert.That(mediator.LastRequest, Is.SameAs(command));
    }

    [Test]
    public async Task UpdatePaymentStatus_SynchronizesRouteValuesAndReturnsOk()
    {
        var expected = new PaymentDto { Id = 9, OrderId = 3, Price = 88m, Status = true };
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(expected));
        var controller = new PaymentsController(mediator, NullLogger<PaymentsController>.Instance);
        var command = new UpdatePaymentStatusCommand();

        var response = await controller.UpdatePaymentStatus(9, true, command);

        var ok = response.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(expected));
        Assert.That(command.PaymentId, Is.EqualTo(9));
        Assert.That(command.Status, Is.True);
        Assert.That(mediator.LastRequest, Is.SameAs(command));
    }

    [Test]
    public async Task UpdatePaymentStatus_WhenPaymentDoesNotExist_ReturnsNotFound()
    {
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(null));
        var controller = new PaymentsController(mediator, NullLogger<PaymentsController>.Instance);

        var response = await controller.UpdatePaymentStatus(100, false, new UpdatePaymentStatusCommand());

        Assert.That(response.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetPayment_WhenPaymentExists_ReturnsOk()
    {
        var expected = new PaymentDto { Id = 5, OrderId = 2, Price = 60m };
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(expected));
        var controller = new PaymentsController(mediator, NullLogger<PaymentsController>.Instance);

        var response = await controller.GetPayment(expected.Id);

        var ok = response.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(expected));
        var query = mediator.LastRequest as GetPaymentQuery;
        Assert.That(query, Is.Not.Null);
        Assert.That(query!.PaymentId, Is.EqualTo(expected.Id));
    }

    [Test]
    public async Task GetPayment_WhenPaymentDoesNotExist_ReturnsNotFound()
    {
        var mediator = new TestMediator((_, _) => Task.FromResult<object?>(null));
        var controller = new PaymentsController(mediator, NullLogger<PaymentsController>.Instance);

        var response = await controller.GetPayment(777);

        Assert.That(response.Result, Is.TypeOf<NotFoundResult>());
    }
}
