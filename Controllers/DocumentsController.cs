using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FEENALOoFINALE.Services;

namespace FEENALOoFINALE.Controllers
{
    [Route("documents")]
    public class DocumentsController : Controller
    {
        private readonly IFormRecognizerService _formService;

        public DocumentsController(IFormRecognizerService formService)
        {
            _formService = formService;
        }

        // GET /documents
        [HttpGet]
        public IActionResult Upload()
        {
            return View();  // You can create Views/Documents/Upload.cshtml for this
        }

        // POST /documents/extract-tables
        [HttpPost("extract-tables")]
        public async Task<IActionResult> ExtractTables(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var tables = await _formService.ExtractTablesAsync(stream);
            return Json(tables);
        }
    }
}
