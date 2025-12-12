using Orchestrator.Gateway.DTOs;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Utils.Enrichers;

public static class OrderlineEnricher
{
    extension(OrderlineDto dto)
    {
        public MarketplaceOrderlineProcess ToMarketplaceOrderlineProcess(Guid correlationId, Guid lineId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new MarketplaceOrderlineProcess(
                BookId: dto.BookId,
                CorrelationId: correlationId,
                LineId: lineId,
                Price: dto.Price
            );
        }

        public InventoryOrderlineProcess ToInventoryOrderlineProcess(Guid correlationId, Guid lineId)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(dto.Quantity);

            return new InventoryOrderlineProcess(
                BookId: dto.BookId,
                CorrelationId: correlationId,
                LineId: lineId,
                Price: dto.Price,
                Quantity: dto.Quantity.Value
            );
        }
    }
}
