using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Azure.Storage.Blobs;

namespace Adrians.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            
            return View();
        }

        private string[] permittedExtensions = { ".txt", ".pdf" };
        private long maxFileSize = 1024 * 15;
        private async void LoadFile(InputFileChangeEventArgs e)
        {
            if (e.File != null)
            {
                if (e.File.Size == 0 || e.File.Size > maxFileSize)
                {
                    // log error
                }
                else
                {
                    var ext = Path.GetExtension(e.File.Name).ToLowerInvariant();

                    if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                    {
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        await new BlobContainerClient("connection", "blob").UploadBlobAsync(trustedFileNameForFileStorage, e.File.OpenReadStream());
                    }
                }
            }
        }
    }
}
