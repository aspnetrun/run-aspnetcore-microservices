using AspnetRunBasics.ApiCollection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.Controllers
{
    public class BasketController : Controller
    {

        private readonly IBasketApi _basketApi;

        public BasketController(IBasketApi basketApi)
        {
            _basketApi = basketApi ?? throw new ArgumentNullException(nameof(basketApi));
        }

        [HttpGet]
        public int GetDataAsync()
        {
            var userName = "swn";
            var count =  _basketApi.GetBasket(userName).Result.Items.Count;

            return count;
        }

    }
}
