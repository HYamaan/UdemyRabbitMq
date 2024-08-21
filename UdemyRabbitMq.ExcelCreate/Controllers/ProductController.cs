using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using UdemyRabbitMq.ExcelCreate.Models;
using UdemyRabbitMq.ExcelCreate.Services;

namespace UdemyRabbitMq.ExcelCreate.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProductController> _logger;
        private readonly RabbitMqPublisher _rabbitMqPublisher;

        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager, RabbitMqPublisher rabbitMqPublisher, ILogger<ProductController> logger)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            if(User is null || User.Identity is null || User?.Identity?.Name is null)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByNameAsync(User.Identity?.Name);

            if (user is null)
                return RedirectToAction("Login", "Account");

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FilePath = $"/files/{fileName}.xlsx",
                Status = FileStatus.Creating,
            };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();

            _rabbitMqPublisher.Publish(new CreateExcelMessage()
            {
                FileId = userFile.Id.ToString()
            });

            TempData["CreateProductExcel"] = true;
            return RedirectToAction(nameof(DownloadProductExcel));
        }

        public async Task<IActionResult> DownloadProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userFiles = await _context.UserFiles.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).ToListAsync();
            return View(userFiles);
        }

    }
}
