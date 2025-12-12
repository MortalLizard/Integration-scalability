using Shared.Contracts.OrderBook;

namespace Shared.Contracts.Mappers;

public static class MarketplaceOrderlineProcessMapper
{
    extension(MarketplaceOrderlineProcess process)
    {
        public MarketplaceOrderlineProcessed ToMarketplaceOrderlineProcessed()
        {
            return new MarketplaceOrderlineProcessed(
                CorrelationId: process.CorrelationId,
                LineId: process.LineId,
                BookId: process.BookId,
                Price: process.Price
            );
        }

        public MarketplaceOrderlineProcessFailed ToMarketplaceOrderlineProcessFailed()
        {
            return new MarketplaceOrderlineProcessFailed(
                CorrelationId: process.CorrelationId,
                LineId: process.LineId,
                BookId: process.BookId,
                Price: process.Price
            );
        }
    }
}
