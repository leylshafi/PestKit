using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PestKit.Data;
using PestKit.Interfaces;
using PestKit.Models;
using PestKit.Services;
using PestKit.Utilities.Exceptions;
using PestKit.ViewModels;
using System.Security.Claims;

namespace PestKit.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly LayoutService _layoutService;
        private readonly IEmailService _emailService;
        public BasketController(AppDbContext context, LayoutService layoutService, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _layoutService = layoutService;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new();
            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null) throw new NotFoundException("User not found");
                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new()
                    {
                        Count = item.Count,
                        Id = item.Id,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        SubTotal = item.Price * item.Count,

                    });
                }

            }
            else
            {

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
            }

            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id)
        {
            if (id <= 0) throw new WrongRequestException("Your request is wrong");
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) throw new NotFoundException("Product not found");

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null) return NotFound();
                var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (basketItem is null)
                {
                    basketItem = new()
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                        OrderId = null
                    };
                    user.BasketItems.Add(basketItem);
                }
                else
                {
                    basketItem.Count++;
                }
                await _context.SaveChangesAsync();

                List<BasketItemVM> basketVMs = new List<BasketItemVM>();
                foreach (BasketItem item in user.BasketItems)
                {
                    basketVMs.Add(new()
                    {
                        Count = item.Count,
                        Id = item.Id,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        SubTotal = item.Price * item.Count,

                    });
                }
                return PartialView("_BasketItemPartial", basketVMs);
            }
            else
            {
                List<BasketCookieItemVM> basket;
                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                    if (item is null)
                    {
                        BasketCookieItemVM itemVM = new()
                        {
                            Id = product.Id,
                            Count = 1
                        };
                        basket.Add(itemVM);
                    }
                    else
                    {
                        item.Count++;
                    }
                }
                else
                {
                    basket = new();
                    BasketCookieItemVM itemVM = new()
                    {
                        Id = product.Id,
                        Count = 1
                    };
                    basket.Add(itemVM);
                }

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("Basket", json);

                List<BasketItemVM> basketVM = await _layoutService.GetBasketItemsAsync(basket);



                return PartialView("_BasketItemPartial", basketVM);
            }
        }

        public async Task<IActionResult> DeleteBasket(int id, int? count)
        {
            if (id <= 0) throw new WrongRequestException("Your request is wrong");
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) throw new NotFoundException("Product not found");
            List<BasketCookieItemVM> basket;
            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null) return NotFound();
                var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (basketItem is null)
                {
                    return NotFound();
                }
                else
                {
                    user.BasketItems.Remove(basketItem);
                }
                await _context.SaveChangesAsync();

                List<BasketItemVM> basketVMs = new List<BasketItemVM>();
                foreach (BasketItem item in user.BasketItems)
                {
                    basketVMs.Add(new()
                    {
                        Count = item.Count,
                        Id = item.Id,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        SubTotal = item.Price * item.Count,

                    });
                }
                return PartialView("_BasketItemPartial", basketVMs);
            }
            else
            {
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

                        List<BasketItemVM> basketVM = await _layoutService.GetBasketItemsAsync(basket);
                        return PartialView("_BasketItemPartial", basketVM);
                    }
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

        public async Task<IActionResult> Checkout()
        {
            AppUser user = await _userManager.Users
                 .Include(u => u.BasketItems
                 .Where(bi => bi.OrderId == null))
                 .ThenInclude(pi => pi.Product)
                 .FirstOrDefaultAsync(u => u.Id == User
                 .FindFirstValue(ClaimTypes.NameIdentifier));

            OrderVM orderVM = new OrderVM()
            {
                BasketItems = user.BasketItems
            };
            return View(orderVM);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {
            AppUser user = await _userManager.Users
                 .Include(u => u.BasketItems
                 .Where(bi => bi.OrderId == null))
                 .ThenInclude(pi => pi.Product)
                 .FirstOrDefaultAsync(u => u.Id == User
                 .FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }
            decimal total = 0;
            orderVM.BasketItems = user.BasketItems;
            foreach (BasketItem item in orderVM.BasketItems)
            {
                item.Price = item.Product.Price;
                total += item.Count * item.Price;
            }
            Order order = new Order()
            {
                Status = null,
                BasketItems = user.BasketItems,
                Address = orderVM.Address,
                AppUserId = user.Id,
                PurchaseAt = DateTime.Now,
                TotalPrice = total,
            };
            await _context.Orders.AddAsync(order);
            
            await _context.SaveChangesAsync();
            string body = $@"<table class=""table"">
  <thead>
    <tr>
      <th scope=""col"">#</th>
      <th scope=""col"">Name</th>
      <th scope=""col"">Price</th>
      <th scope=""col"">Count</th>
    </tr>
  </thead>
  <tbody>";
            foreach (BasketItem item in order.BasketItems)
            {
                body += $@"<tr>
      <th scope=""row"">${item.Id}</th>
      <td>${item.Product.Name}</td>
      <td>${item.Price}</td>
      <td>{item.Count}</td>
    </tr>
";
            };
            body += @"</tbody>
</table>";

            await _emailService.SendEmailAsync(user.Email, "Your Order", body, true);
            user.BasketItems.Clear();
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
