using AspnetRunBasics.ApiCollection.Infrastructure;
using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.Settings;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspnetRunBasics.ApiCollection
{
    public class OrderApi : BaseHttpClientWithFactory, IOrderApi
    {
        private readonly IApiSettings _settings;

        public OrderApi(IHttpClientFactory factory, IApiSettings settings)
            : base(factory)
        {
            _settings = settings;
        }

        public async Task<OrderModel> GetOrder(string userName)
        {
            var message = new HttpRequestBuilder(_settings.BaseAddress)
                               .SetPath(_settings.OrderPath + "//" + userName)
                               .HttpMethod(HttpMethod.Get)
                               .GetHttpMessage();

            return await SendRequest<OrderModel>(message);
        }
    }
}
