using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.DTOs
{
    public record CartItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

}
