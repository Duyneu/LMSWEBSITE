// File: Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller điều hướng trang chủ và xử lý trang thông báo lỗi chung.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Thesis");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
