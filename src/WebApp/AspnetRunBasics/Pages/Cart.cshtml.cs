using System;
using System.Linq;
using System.Threading.Tasks;
using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspnetRunBasics
{
    public class CartModel : PageModel
    {
        private readonly IBasketRepository _basketRepository;

        public CartModel(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));

        }

        public BasketRepositoryModel Cart { get; set; } = new BasketRepositoryModel();

        //public async Task<string> GetImageFileAsync(string id)
        //{
        //   return  (await _catalogApi.GetCatalog(id)).ImageFile;

        //}

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = _basketRepository.GetAllBasket();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(string productId)
        {
            var basket = _basketRepository.GetAllBasket();

            var item = basket.Items.Where(x => x.ProductId == productId).FirstOrDefault();
            basket.Items.Remove(item);

            _basketRepository.Update(basket);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string productId, string qty)
        {
            var basket = _basketRepository.GetAllBasket();

            var item =   basket.Items.Where(x => x.ProductId == productId).FirstOrDefault().Quantity=int.Parse(qty);

            _basketRepository.Update(basket);

            return RedirectToPage();
        }
    }
}