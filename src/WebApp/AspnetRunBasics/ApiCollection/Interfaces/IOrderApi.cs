using System.Threading.Tasks;

namespace AspnetRunBasics.ApiCollection.Interfaces
{
    public interface IOrderApi
    {
        Task<OrderModel> GetOrder(string userName);
    }
}
