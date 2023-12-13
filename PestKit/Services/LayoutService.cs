using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PestKit.Data;
using PestKit.Models;
using PestKit.ViewModels;
using System.Security.Claims;

namespace PestKit.Services
{
    public class LayoutService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;
        private HttpRequest _request;
        private readonly UserManager<AppUser> _userManager;

        public LayoutService(IHttpContextAccessor contextAccessor, AppDbContext context, UserManager<AppUser> userManager)
        {
            _contextAccessor = contextAccessor;
            _context = context;
            _request = _contextAccessor.HttpContext.Request;
            _userManager = userManager;
        }

        public async Task<List<BasketItemVM>> GetBasketItemsAsync()
        {
            List<BasketItemVM> biList = new();
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).FirstOrDefaultAsync(u => u.Id ==_contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                foreach (BasketItem item in user.BasketItems)
                {
                    biList.Add(new BasketItemVM()
                    {
                        Count = item.Count,
                        Id = item.Id,
                        Name = item.Product.Name,
                        Price = item.Price,
                        SubTotal = item.Count * item.Product.Price
                    });
                }
                return biList;
            }
            else
            {
                if (_request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> bciList = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(_request.Cookies["Basket"]);

                    foreach (BasketCookieItemVM basketCookieItem in bciList)
                    {
                        Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

                        if (product is not null)
                        {
                            BasketItemVM basketItem = new()
                            {
                                Id = basketCookieItem.Id,
                                Count = basketCookieItem.Count,
                                Name = product.Name,
                                Price = product.Price,
                                SubTotal = product.Price * basketCookieItem.Count
                            };

                            biList.Add(basketItem);
                        }

                    }
                }
                else
                {
                    biList = new();
                }
                return biList;
            }
            
        }

        public async Task<List<BasketItemVM>> GetBasketItemsAsync(List<BasketCookieItemVM> bciList)
        {
            List<BasketItemVM> biList=new();

            foreach (BasketCookieItemVM basketCookieItem in bciList)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

                if (product is not null)
                {
                    BasketItemVM basketItem = new()
                    {
                        Id = basketCookieItem.Id,
                        Count = basketCookieItem.Count,
                        Name = product.Name,
                        Price = product.Price,
                        SubTotal = product.Price * basketCookieItem.Count
                    };

                    biList.Add(basketItem);
                }

            }


            return biList;
        }

    }
}
