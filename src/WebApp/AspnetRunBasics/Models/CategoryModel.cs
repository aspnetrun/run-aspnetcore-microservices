using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.Models
{
    public class CategoryModel
    {
        //Daha Sonra ürün sayısını göstermek istedğinde kullan
        public  string CategoryName { get; set; }
        public decimal ProductCount { get; set; }
    }
}
