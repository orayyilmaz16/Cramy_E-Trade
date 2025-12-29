using Cramy.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.Contracts
{
    public interface ICardService
    {
        Task<CardDto> GetCardAsync(string userId);
        Task AddItemAsync(string userId, Guid productId, int quantity);
        Task RemoveItemAsync(string userId, Guid productId);
        Task<OrderConfirmationDto> CheckoutAsync(string userId);
    }

}
