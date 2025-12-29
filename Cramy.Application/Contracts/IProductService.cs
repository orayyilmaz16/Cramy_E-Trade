using Cramy.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.Contracts
{
    public interface IProductService
    {
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ProductDto>> GetPagedAsync(int page, int pageSize);
        Task<Guid> CreateAsync(CreateProductDto dto);
        Task UpdateAsync(Guid id, UpdateProductDto dto);
        Task ToggleActiveAsync(Guid id, bool isActive);
        Task<bool> AdjustStockAsync(Guid id, int delta, string reason, string? referenceId);
    }

}
