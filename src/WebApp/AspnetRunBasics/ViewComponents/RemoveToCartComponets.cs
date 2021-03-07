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
        private readonly IBasketRepository _basketRepository;

        public RemoveToCartComponets(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("_RemoteToCartPartial", _basketRepository.GetAllBasket());
        }
    }
}
