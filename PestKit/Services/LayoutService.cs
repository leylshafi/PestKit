using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PestKit.Data;
using PestKit.Models;
using PestKit.ViewModels;

namespace PestKit.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _accessor;

        public LayoutService(AppDbContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task<List<BasketItemVM>> InvokeAsync()
        {
            List<BasketItemVM> basketVM = new();
            if (_accessor.HttpContext.Request.Cookies["Basket"] is not null)
            {
                List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(_accessor.HttpContext.Request.Cookies["Basket"]);

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
            return basketVM;
        }

    }
}
