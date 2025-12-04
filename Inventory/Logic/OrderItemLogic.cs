using Inventory.Database.Services;
using Inventory.DTOs;

namespace Inventory.Logic;

public class OrderItemLogic : IOrderItemLogic
{
    private readonly IBookService _bookService;

    public OrderItemLogic(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task ProcessOrderItem(OrderItemDto orderItemDto, CancellationToken ct = default)
    {
        var updatedBook = await _bookService.UpdateStockAsync(orderItemDto.ProductId, orderItemDto.QuantityChange);

        if (updatedBook != null)
        {
            var currentPrice = updatedBook.GetTotal();

            // Build a success message and send with producer

        }
        else
        {
            // Build a failure message and send with producer
        }
    }
}
