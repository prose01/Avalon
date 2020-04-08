using Avalon.Model;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IImageUtil
    {
        Task UploadImageAsync(CurrentUser currentUser, IFormFile photo);
        Task<List<byte[]>> GetImagesAsync(string profileId);
    }
}
