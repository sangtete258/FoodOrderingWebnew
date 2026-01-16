using FoodOrderingWeb.Models;
using FoodOrderingWeb.Models.ViewModels;

namespace FoodOrderingWeb.Services
{
    public interface ICartService
    {
        CartViewModel GetCart();
        void AddToCart(int foodId, int quantity = 1);
        void UpdateQuantity(int foodId, int quantity);
        void RemoveFromCart(int foodId);
        void ClearCart();
    }
}