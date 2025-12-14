using Orchestrator.OrderSaga.Database.Entities;

using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Utils.Mappers;

public static class OrderlineEnricher
{
    extension(OrderSagaLine line)
    {
        public MarketplaceOrderlineProcess ToMarketplaceOrderlineProcess()
        {
            ArgumentNullException.ThrowIfNull(line);

            return new MarketplaceOrderlineProcess(
                BookId: line.BookId,
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                Price: line.Price
            );
        }

        public InventoryOrderlineProcess ToInventoryOrderlineProcess()
        {
            ArgumentNullException.ThrowIfNull(line);
            ArgumentNullException.ThrowIfNull(line.Quantity);

            return new InventoryOrderlineProcess(
                BookId: line.BookId,
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                Price: line.Price,
                Quantity: line.Quantity.Value
            );
        }
    }
}
