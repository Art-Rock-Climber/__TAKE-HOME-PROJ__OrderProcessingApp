using Riok.Mapperly.Abstractions;
using OrderService.DataAccess.Postgres.Models;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

[Mapper]
public static partial class OrderMapper
{
    public static partial OrderDto ToDto(Order order);
}