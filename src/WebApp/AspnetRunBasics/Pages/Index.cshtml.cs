using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspnetRunBasics.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogApi _catalogApi;
        private readonly IBasketRepository _basketRepository;

        public IndexModel(ICatalogApi catalogApi, IBasketRepository basketRepository)
        {
            _catalogApi = catalogApi ?? throw new ArgumentNullException(nameof(catalogApi));
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
        }

        public IEnumerable<CatalogModel> ProductList { get; set; } = new List<CatalogModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            ProductList = await _catalogApi.GetCatalog();
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

        public async Task<IActionResult> OnPostAddToCartAsync(string productId)
        {
            var product = await _catalogApi.GetCatalog(productId);

            var basket = _basketRepository.GetAllBasket();
            if (basket.Items.Find(i => i.ProductId == productId) == null)
            {
                basket.Items.Add(new BasketItemRepositoryModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageFile = product.ImageFile
                });
            }
            else
            {
                basket.Items.Find(i => i.ProductId == productId).Quantity++;
            }

            _basketRepository.Update(basket);

            return RedirectToPage();
        }

    }
}
