using StackExchange.Redis;

namespace Basket.API.Data.Interfaces
{
    public interface IBasketContext
    {
        IDatabase Redis { get; }
    }
}
