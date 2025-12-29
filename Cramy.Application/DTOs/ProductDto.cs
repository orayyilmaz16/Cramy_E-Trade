using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.DTOs
{
    public record ProductDto(Guid Id, string SKU, string Name, string Description, decimal Price, int StockQuantity, Guid CategoryId, bool IsActive);

}
