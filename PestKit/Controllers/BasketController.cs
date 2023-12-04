using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PestKit.Data;
using PestKit.Models;
using PestKit.ViewModels;

namespace PestKit.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new();
            if (Request.Cookies["Basket"] is not null)
            {
                List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                foreach (BasketCookieItemVM basketCookieItem in basket)
                {
                    Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

                    if (product is not null)
                    {
                        BasketItemVM basketItemVM = new()
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            Count = basketCookieItem.Count,
                            SubTotal = product.Price * basketCookieItem.Count,
                        };
                        basketVM.Add(basketItemVM);

                    }
                }
            }
            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id, string controllerName)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
                var item = basket.FirstOrDefault(b => b.Id == id);
                if (item is null)
                {
                    BasketCookieItemVM itemVm = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };
                    basket.Add(itemVm);
                }
                else
                {
                    item.Count++;
                }
            }
            else
            {
                basket = new();
                BasketCookieItemVM itemVm = new BasketCookieItemVM
                {
                    Id = id,
                    Count = 1
                };
                basket.Add(itemVm);
            }

            string json = JsonConvert.SerializeObject(basket);
            Response.Cookies.Append("Basket", json);

            return RedirectToAction(nameof(Index), controllerName);
        }

        public async Task<IActionResult> DeleteBasket(int id, int? count)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
                var item = basket.FirstOrDefault(b => b.Id == id);
                if (item is not null)
                {
                    if (count is not null)
                    {
                        item.Count = 1;
                    }
                    item.Count--;
                    if (item.Count == 0)
                    {
                        basket.Remove(item);
                    }

                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UpdateCart()
        {
            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = new();


                string json = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("Basket", json);

            }

            return RedirectToAction(nameof(Index));
        }
    }
}
