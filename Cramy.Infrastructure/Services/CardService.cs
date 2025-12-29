using Cramy.Application.Contracts;
using Cramy.Application.DTOs;
using Cramy.Domain.Entities;
using Cramy.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Infrastructure.Services
{
    public class CardService : ICardService
    {
        private readonly CramyDbContext _db;
        public CardService(CramyDbContext db) => _db = db;

        public async Task<CardDto> GetCardAsync(string userId)
        {
            var card = await _db.Cards.Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Card { UserId = userId };

            if (card.Id == default) _db.Cards.Add(card);
            await _db.SaveChangesAsync();

            return new CardDto(card.Id, card.UserId,
                card.Items.Select(i => new CartItemDto(i.ProductId, i.Product!.Name, i.Quantity, i.UnitPrice)).ToList());
        }

        public async Task AddItemAsync(string userId, Guid productId, int quantity)
        {
            var product = await _db.Products.FirstAsync(p => p.Id == productId && p.IsActive);
            if (product.StockQuantity < quantity) throw new InvalidOperationException("Yetersiz stok");

            var card = await _db.Cards.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Card { UserId = userId };
            if (card.Id == default) _db.Cards.Add(card);

            var existing = card.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing is null)
                card.Items.Add(new CartItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price });
            else
                existing.Quantity += quantity;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(string userId, Guid productId)
        {
            var card = await _db.Cards.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? throw new InvalidOperationException("Sepet bulunamadı");

            var item = card.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is null) return;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task<OrderConfirmationDto> CheckoutAsync(string userId)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var card = await _db.Cards.Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? throw new InvalidOperationException("Sepet bulunamadı");

            foreach (var item in card.Items)
            {
                if (!item.Product!.IsActive)
                    throw new InvalidOperationException($"Ürün aktif değil: {item.Product.Name}");

                if (item.Product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Yetersiz stok: {item.Product.Name}");

                item.Product.StockQuantity -= item.Quantity;
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = item.ProductId,
                    Delta = -item.Quantity,
                    Reason = "Checkout",
                    ReferenceId = null,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Paid,
                Items = card.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(card.Items);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return new OrderConfirmationDto(order.Id, order.Items.Sum(i => i.UnitPrice * i.Quantity));
        }
    }

}
