using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.DTOs
{
    public record CardDto(Guid Id, string UserId, IReadOnlyList<CartItemDto> Items);

}
