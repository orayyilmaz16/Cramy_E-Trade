using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.DTOs
{
    public record UpdateProductDto(string Name, string Description, decimal Price, int StockQuantity, Guid CategoryId, bool IsActive);

}
