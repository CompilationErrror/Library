using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IOrderService
    {
        Task PlaceBookOrder(int bookId, Guid custId);
        Task<List<OrderedBook>> GetOrderedBooksByUserId(Guid userId);
        Task DeleteOrderedBooksByIdAsync(List<int> bookIds);
    }
}
