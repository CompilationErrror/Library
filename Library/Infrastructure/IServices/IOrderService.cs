using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IOrderService
    {
        Task PlaceBookOrderAsync(int bookId, Guid custId);
        Task<List<OrderedBook>> GetOrderedBooksByUserIdAsync(Guid userId);
        Task DeleteOrderedBooksByIdAsync(List<int> bookIds);
    }
}
