using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICatalogApi _catalogApi;

        public HomeController(ICatalogApi catalogApi, IBasketApi basketApi)
        {
            _catalogApi = catalogApi ?? throw new ArgumentNullException(nameof(catalogApi));
        }

        public async Task<IActionResult> IndexAsync()
        {
            var result = await _catalogApi.GetCatalog();
            
            return View(result);
        }
        public IActionResult Add()
        {
            return View(new AddCatalogModel());
        }

        [HttpPost]
        public IActionResult Add(AddCatalogModel catalog)
        {
            if (ModelState.IsValid)
            {
                CatalogModel catalogModel = new CatalogModel();

                if (catalog.ImageURL != null)
                {
                    var type = Path.GetExtension(catalog.ImageURL.FileName);
                    var newImageName = Guid.NewGuid() + type;

                    var NewImagePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/images/product/" + newImageName);
                    var stream = new FileStream(NewImagePath, FileMode.Create);

                    //resmi NewImagePath adresine kopyaladık
                    catalog.ImageURL.CopyTo(stream);
                    catalogModel.ImageFile = newImageName;
                }

                catalogModel.Name = catalog.Name;
                catalogModel.Description = catalog.Description;
               catalogModel.Price = catalog.Price;
               catalogModel.Category = catalog.Category;
               catalogModel.Summary = catalog.Summary;

                _catalogApi.CreateCatalog(catalogModel);
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return View(catalog);
        }


        public async Task<IActionResult> UpdateAsync(string id)
        {
            var catalogModel  = await _catalogApi.GetProduct(id);


            AddCatalogModel addCatalogModel = new AddCatalogModel
            {
                Id = catalogModel.Id,
                Name = catalogModel.Name,
                Description = catalogModel.Description,
                Price = catalogModel.Price,
                Category = catalogModel.Category,
                Summary = catalogModel.Summary
             };
            return View(addCatalogModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAsync(AddCatalogModel addCatalogModel)
        {
            if (ModelState.IsValid)
            {
                var catalogModel = await _catalogApi.GetProduct(addCatalogModel.Id);
                if (addCatalogModel.ImageURL != null)
                {
                    var type = Path.GetExtension(addCatalogModel.ImageURL.FileName);
                    var newImageName = Guid.NewGuid() + type;

                    var NewImagePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/images/product/" + newImageName);
                    var stream = new FileStream(NewImagePath, FileMode.Create);

                    addCatalogModel.ImageURL.CopyTo(stream);
                    catalogModel.ImageFile = newImageName;

                }
                catalogModel.Id = addCatalogModel.Id;
                catalogModel.Name = addCatalogModel.Name;
                catalogModel.Description = addCatalogModel.Description;
                catalogModel.Price = addCatalogModel.Price;
                catalogModel.Category = addCatalogModel.Category;
                catalogModel.Summary = addCatalogModel.Summary;

                _catalogApi.UpdateCatalog(catalogModel);
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return View(addCatalogModel);
        }

        public IActionResult Delete(string id)
        {
            _catalogApi.DeleteCatalog(id);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

    }
}
