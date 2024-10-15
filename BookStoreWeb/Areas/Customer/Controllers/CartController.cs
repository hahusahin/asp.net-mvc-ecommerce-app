using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM? ShoppingCartVM {  get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        // GET: CartController
        public ActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(item => item.ApplicationUserId == userId, includeProperties: "Product")
            };
            foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
            {
                cartItem.Price = GetPriceBasedOnQuantity(cartItem);
                ShoppingCartVM.OrderTotal += (cartItem.Price * cartItem.Count);
            }
            return View(ShoppingCartVM);
        }

        public ActionResult Plus(int cartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            cartFromDB.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDB);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Minus(int cartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if(cartFromDB.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDB);
            } else
            {
                cartFromDB.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Remove(int cartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDB);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Summary()
        {
            return View();
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
