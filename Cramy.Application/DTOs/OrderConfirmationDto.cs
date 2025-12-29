using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Application.DTOs
{
    public record OrderConfirmationDto(Guid OrderId, decimal Total);

}
