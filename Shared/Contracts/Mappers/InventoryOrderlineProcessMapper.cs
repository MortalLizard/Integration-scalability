using Shared.Contracts.OrderBook;

namespace Shared.Contracts.Mappers;

public static class InventoryOrderlineProcessMapper
{
    extension(InventoryOrderlineProcess process)
    {
        public InventoryOrderlineProcessed ToInventoryOrderlineProcessed()
        {
            return new InventoryOrderlineProcessed(
                CorrelationId: process.CorrelationId,
                LineId: process.LineId,
                BookId: process.BookId,
                Quantity: process.Quantity,
                Price: process.Price
            );
        }

        public InventoryOrderlineProcessFailed ToInventoryOrderlineProcessFailed()
        {
            return new InventoryOrderlineProcessFailed(
                CorrelationId: process.CorrelationId,
                LineId: process.LineId,
                BookId: process.BookId,
                Quantity: process.Quantity,
                Price: process.Price
            );
        }
    }
}
