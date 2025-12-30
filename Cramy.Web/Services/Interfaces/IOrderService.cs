using Cramy.Domain.Entities;
using Cramy.Web.Models;

namespace Cramy.Web.Services.Interfaces;

public interface IOrderService
{
    List<OrderViewModel> GetOrdersForUser(string userEmail);

    Task<Order?> GetOrderDetailsForUserAsync(string userId, Guid orderId);
}