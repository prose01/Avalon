using Avalon.Model;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Interfaces
{
    public interface IImageUtil
    {
        Task AddImageToCurrentUser(CurrentUser currentUser, IFormFile photo, string title);
        Task RemoveImageFromCurrentUser(CurrentUser currentUser, string imageId);
        Task<List<byte[]>> GetImagesAsync(string profileId);
    }
}
