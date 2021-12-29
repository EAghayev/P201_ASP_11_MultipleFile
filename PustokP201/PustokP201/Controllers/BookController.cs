using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PustokP201.Models;
using PustokP201.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PustokP201.Controllers
{
    public class BookController : Controller
    {
        private readonly PustokContext _context;
        public BookController(PustokContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult SetSession(int id)
        {
            HttpContext.Session.SetString("bookId", id.ToString());

            return Content("added");
        }

        public ActionResult GetSession()
        {
            string idStr = HttpContext.Session.GetString("bookId");

            return Content(idStr);
        }

        public IActionResult SetCookie(int id)
        {
            HttpContext.Response.Cookies.Append("bookId", id.ToString());
            return Content("cookie");
        }

        public IActionResult GetCookie(string key)
        {
            string str = HttpContext.Request.Cookies[key];

            return Content(str);
        }

        public IActionResult AddBasket(int bookId)
        {
            if (!_context.Books.Any(x => x.Id == bookId))
            {
                return NotFound();
            }

            List<CookieBasketItemViewModel> basketItems = new List<CookieBasketItemViewModel>();
            string existBasketItems = HttpContext.Request.Cookies["basketItemList"];

            if (existBasketItems != null)
            {
                basketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(existBasketItems);
            }

            CookieBasketItemViewModel item = basketItems.FirstOrDefault(x => x.BookId == bookId);

            if (item == null)
            {
                item = new CookieBasketItemViewModel
                {
                    BookId = bookId,
                    Count = 1
                };
                basketItems.Add(item);
            }
            else
                item.Count++;

            var bookIdsStr = JsonConvert.SerializeObject(basketItems);

            HttpContext.Response.Cookies.Append("basketItemList", bookIdsStr);

            var data = _getBasketItems(basketItems);

            return Ok(data);
        }

        public IActionResult ShowBasket()
        {
            var bookIdsStr = HttpContext.Request.Cookies["basketItemList"];
            List<CookieBasketItemViewModel> bookIds = new List<CookieBasketItemViewModel>();
            if (bookIdsStr != null)
            {
                bookIds = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(bookIdsStr);
            }
            return Json(bookIds);
        }

        public IActionResult Checkout()
        {
            List<CheckoutItemViewModel> checkoutItems = new List<CheckoutItemViewModel>();

            string basketItemsStr = HttpContext.Request.Cookies["basketItemList"];
            if (basketItemsStr != null)
            {
                List<CookieBasketItemViewModel> basketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketItemsStr);

                foreach (var item in basketItems)
                {
                    CheckoutItemViewModel checkoutItem = new CheckoutItemViewModel
                    {
                        Book = _context.Books.FirstOrDefault(x => x.Id == item.BookId),
                        Count = item.Count
                    };
                    checkoutItems.Add(checkoutItem);
                }
            }

            return View(checkoutItems);
        }

        private BasketViewModel _getBasketItems(List<CookieBasketItemViewModel> cookieBasketItems)
        {
            BasketViewModel basket = new BasketViewModel
            {
                BasketItems = new List<BasketItemViewModel>(),
            };

            foreach (var item in cookieBasketItems)
            {
                Book book = _context.Books.Include(x=>x.BookImages).FirstOrDefault(x => x.Id == item.BookId);
                BasketItemViewModel basketItem = new BasketItemViewModel
                {
                    Name = book.Name,
                    Price = book.DiscountPercent>0?(book.SalePrice*(1-book.DiscountPercent/100)):book.SalePrice,
                    BookId = book.Id,
                    Count = item.Count,
                    PosterImage = book.BookImages.FirstOrDefault(x=>x.PosterStatus==true)?.Image
                };

                basketItem.TotalPrice = basketItem.Count * basketItem.Price;
                basket.TotalAmount += basketItem.TotalPrice;
                basket.BasketItems.Add(basketItem);
            }

            return basket;
        }

    }
}
