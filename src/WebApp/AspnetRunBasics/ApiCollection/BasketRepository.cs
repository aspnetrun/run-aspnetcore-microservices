using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.CustomExtensions;
using AspnetRunBasics.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.ApiCollection
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BasketRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void Update(BasketRepositoryModel basketRepositoryModel)
        {
            _httpContextAccessor.HttpContext.Session.SetObject("basket", basketRepositoryModel);
        }


        public BasketRepositoryModel GetAllBasket()
        {
            var result = _httpContextAccessor.HttpContext.Session.GetObject<BasketRepositoryModel>("basket");
            if (result == null)
            {
                return new BasketRepositoryModel();
            }
            return result;
        }

        public void RemoveAllBasket()
        {
            _httpContextAccessor.HttpContext.Session.Remove("basket");
        }

    }
}
