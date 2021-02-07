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
        private readonly IBasketApi _basketApi;
        private readonly ICatalogApi _catalogApi;

        public CartModel(IBasketApi basketApi, ICatalogApi catalogApi)
        {
            _basketApi = basketApi ?? throw new ArgumentNullException(nameof(basketApi));
            _catalogApi = catalogApi ?? throw new ArgumentNullException(nameof(basketApi));
        }

        public BasketModel Cart { get; set; } = new BasketModel();

        public async Task<string> GetImageFileAsync(string id)
        {
           return  (await _catalogApi.GetCatalog(id)).ImageFile;

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = "swn";
            Cart = await _basketApi.GetBasket(userName);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(string productId)
        {
            var userName = "swn";
            var basket = await _basketApi.GetBasket(userName);

            var item = basket.Items.Where(x => x.ProductId == productId).FirstOrDefault();
            basket.Items.Remove(item);

            var basketUpdated = await _basketApi.UpdateBasket(basket);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string productId, string qty)
        {
            var userName = "swn";
            var basket = await _basketApi.GetBasket(userName);

            var item =   basket.Items.Where(x => x.ProductId == productId).FirstOrDefault().Quantity=int.Parse(qty);

            var basketUpdated = await _basketApi.UpdateBasket(basket);

            return RedirectToPage();
        }



    }
}