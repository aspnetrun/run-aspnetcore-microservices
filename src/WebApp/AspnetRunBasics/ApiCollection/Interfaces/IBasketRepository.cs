using AspnetRunBasics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.ApiCollection.Interfaces
{
    public interface IBasketRepository
    {
        void Update(BasketRepositoryModel basketRepositoryModel);
        BasketRepositoryModel GetAllBasket();
        void RemoveAllBasket();

    }
}
