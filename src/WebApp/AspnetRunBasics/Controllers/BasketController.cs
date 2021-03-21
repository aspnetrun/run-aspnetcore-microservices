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

        private readonly IBasketRepository _basketRepository;

        public BasketController(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
        }

        [HttpGet]
        public int GetDataAsync()
        {
             return _basketRepository.GetAllBasket().Items.Count;
        }

    }
}
