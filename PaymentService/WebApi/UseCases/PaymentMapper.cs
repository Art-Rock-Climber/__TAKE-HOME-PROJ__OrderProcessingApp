using Riok.Mapperly.Abstractions;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

[Mapper]
public static partial class PaymentMapper
{
    public static partial PaymentDto ToDto(Payment payment);
}