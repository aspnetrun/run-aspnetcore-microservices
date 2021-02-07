using AspnetRunBasics.ApiCollection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.ViewComponents
{
    public class CategoryComponent:ViewComponent
    {
        private readonly ICatalogApi _catalogApi;
        public CategoryComponent(ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("_CategoryPartial", await _catalogApi.GetCatalog());
        }
    }
}
