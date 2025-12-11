using Orchestrator.Gateway.DTOs;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Utils.Enrichers;

public static class OrderlineEnricher
{
    extension(OrderlineDto dto)
    {
        public MarketplaceOrderlineProcess ToMarketplaceOrderlineProcess(Guid correlationId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new MarketplaceOrderlineProcess(
                BookId: dto.BookId,
                CorrelationId: correlationId,
                Price: dto.Price
            );
        }

        public InventoryOrderlineProcess ToInventoryOrderlineProcess(Guid correlationId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            ArgumentNullException.ThrowIfNull(dto.Quantity);

            return new InventoryOrderlineProcess(
                BookId: dto.BookId,
                CorrelationId: correlationId,
                Price: dto.Price,
                Quantity: dto.Quantity.Value
            );
        }
    }
}
