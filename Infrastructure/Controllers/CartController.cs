﻿using Microsoft.AspNetCore.Mvc;
using UseCases;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Entities.Enums;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartItemManager _cartItemManager;
        private readonly BookManager _bookManager;

        public CartController(CartItemManager cartItemManager, BookManager bookManager)
        {
            _cartItemManager = cartItemManager;
            _bookManager = bookManager;
        }

        //Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }

            var cartItems = await _cartItemManager.GetByUserIdAsync(int.Parse(userId));

            var books = await _bookManager.GetAllAsync();
            var bookLookup = books.ToDictionary(_ => _.Id);

            // Lấy thông tin sách từ CartItem, bao gồm trạng thái và thông báo nếu Suspended
            var cartItemDetails = cartItems.Select(item => new
            {
                Id = item.Id,
                BookId = item.BookId,
                BookTitle = bookLookup[item.BookId].Title,
                BookPrice = bookLookup[item.BookId].Price,
                IsSuspended = bookLookup[item.BookId].Status == EntityStatus.Suspended,
                SuspendedMessage = bookLookup[item.BookId].Status == EntityStatus.Suspended ? "Sản phẩm hiện đang bị tạm ngưng và không thể mua." : null
            });
            return View(cartItemDetails);
        }


        [HttpPost]
        public async Task<IActionResult> AddAsync(int? bookId)
        {
            if (bookId == null)
            {
                return BadRequest("Sản phẩm không hợp lệ.");
            }

            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không tìm thấy người dùng.");
            }

            // Kiểm tra nếu sản phẩm đã có trong giỏ hàng của người dùng
            var cartItems = await _cartItemManager.GetByUserIdAsync(userId);
            if (cartItems.Any(item => item.BookId == bookId))
            {
                // Nếu sản phẩm đã có trong giỏ hàng, không làm gì cả
                return RedirectToAction("Index");
            }

            // Nếu chưa có, thêm sản phẩm vào giỏ hàng
            await _cartItemManager.AddAsync(new Entities.CartItem { BookId = bookId.Value, UserId = userId });

            return RedirectToAction("Index");
        }


        // Xóa sản phẩm khỏi giỏ hàng
        [HttpGet]
        public async Task<IActionResult> DeleteAsync(int? id)
        {
            await _cartItemManager.DeleteAsync(id.Value);

            //return RedirectToAction("Index", "Cart");
            return RedirectToAction("Index");
        }
    }
}