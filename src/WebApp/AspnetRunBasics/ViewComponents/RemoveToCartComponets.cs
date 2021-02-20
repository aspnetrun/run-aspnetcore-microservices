using AspnetRunBasics.ApiCollection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.ViewComponents
{
    public class RemoveToCartComponets : ViewComponent
    {
        private readonly IBasketApi _basketApi;

        public RemoveToCartComponets(IBasketApi basketApi, ICatalogApi catalogApi)
        {
            _basketApi = basketApi ?? throw new ArgumentNullException(nameof(basketApi));
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("_RemoteToCartPartial", await _basketApi.GetBasket("swn"));
        }
    }
}
