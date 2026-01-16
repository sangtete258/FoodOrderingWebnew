using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models;
using FoodOrderingWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FoodOrderingWeb.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public CartViewModel GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            string? cartJson = session?.GetString(CartSessionKey);
            var cartItems = !string.IsNullOrEmpty(cartJson)
                ? JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson)
                : new List<CartItemViewModel>();

            return new CartViewModel { Items = cartItems ?? new List<CartItemViewModel>() };
        }

        private void SaveCart(CartViewModel cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = JsonSerializer.Serialize(cart.Items);
            session?.SetString(CartSessionKey, cartJson);
        }

        public void AddToCart(int foodId, int quantity = 1)
        {
            var cart = GetCart();
            var food = _context.Foods.Find(foodId);

            if (food == null || !food.IsAvailable) return;

            var existingItem = cart.Items.FirstOrDefault(i => i.FoodId == foodId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    FoodId = food.FoodId,
                    FoodName = food.Name,
                    Price = food.Price,
                    Quantity = quantity,
                    ImageUrl = food.ImageUrl
                });
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int foodId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.FoodId == foodId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int foodId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.FoodId == foodId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(CartSessionKey);
        }
    }
}