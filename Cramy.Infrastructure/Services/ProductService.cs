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
    public class ProductService : IProductService
    {
        private readonly CramyDbContext _db;
        public ProductService(CramyDbContext db) => _db = db;

        public async Task<ProductDto?> GetByIdAsync(Guid id)
            => await _db.Products.Where(p => p.Id == id)
                .Select(p => new ProductDto(p.Id, p.SKU, p.Name, p.Description, p.Price, p.StockQuantity, p.CategoryId, p.IsActive,p.ImageUrl))
                .FirstOrDefaultAsync();

        public async Task<IReadOnlyList<ProductDto>> GetPagedAsync(int page, int pageSize)
            => await _db.Products.OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto(p.Id, p.SKU, p.Name, p.Description, p.Price, p.StockQuantity, p.CategoryId, p.IsActive, p.ImageUrl))
                .ToListAsync();

        public async Task<Guid> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                SKU = dto.SKU,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _db.Products.FindAsync(id) ?? throw new KeyNotFoundException("Ürün bulunamadı");
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(Guid id, bool isActive)
        {
            var product = await _db.Products.FindAsync(id) ?? throw new KeyNotFoundException("Ürün bulunamadı");
            product.IsActive = isActive;
            await _db.SaveChangesAsync();
        }

        public async Task<bool> AdjustStockAsync(Guid id, int delta, string reason, string? referenceId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product is null) return false;

            var newQty = product.StockQuantity + delta;
            if (newQty < 0) return false; // oversell önleme

            product.StockQuantity = newQty;
            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = id,
                Delta = delta,
                Reason = reason,
                ReferenceId = referenceId,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return true;
        }
    }

}
