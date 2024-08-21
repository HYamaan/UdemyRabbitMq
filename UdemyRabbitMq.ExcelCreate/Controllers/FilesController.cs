using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMq.ExcelCreate.Hubs;
using UdemyRabbitMq.ExcelCreate.Models;

namespace UdemyRabbitMq.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not {Length:>0}) return BadRequest("Dosya seçilmedi.");

            var findUserFile = await _context.UserFiles.FirstOrDefaultAsync(x => x.Id == fileId);
            if (findUserFile is null) return BadRequest("Kullanıcı dosyası bulunamadı.");
            
            var filePath = findUserFile.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            findUserFile.CreatedAt = DateTime.Now;
            findUserFile.FilePath = filePath;
            findUserFile.Status = FileStatus.Completed;

            await _context.SaveChangesAsync();
            //TODO: SIGNALR notification to client
            _hubContext.Clients.User(findUserFile.UserId).SendAsync("ReceiveMessage", "Dosya oluşturuldu");

            stream.Close();

            return Ok();
        }
    }
}
