using Avalon.Model;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IImageUtil
    {
        Task UploadImageAsync(CurrentUser currentUser, IFormFile photo);
    }
}
