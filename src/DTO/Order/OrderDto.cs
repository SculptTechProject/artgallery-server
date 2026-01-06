using System;
using System.Collections.Generic;

namespace artgallery_server.DTO.Order
{
    public sealed record OrderDto(
        string Email,
        List<int> ArtIds
    );
}
