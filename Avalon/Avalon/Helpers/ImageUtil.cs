using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Avalon.Helpers
{
    public class ImageUtil : IImageUtil
    {
        private readonly long _fileSizeLimit;

        public ImageUtil(IConfiguration config)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
        }

        // TODO: Check this website for more info on this - https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1


        public async Task UploadImageAsync(CurrentUser currentUser, IFormFile image)
        {
            try
            {
                if (image.Length < 0 || image.Length > _fileSizeLimit)
                {
                    // TODO: Find på noget bedre end en exception når den fejler fx. pga. file size.
                    throw new Exception();
                }

                // TODO: Find a place for you files!
                if (!Directory.Exists($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}"))
                {
                    Directory.CreateDirectory($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}");
                }

                // TODO: Scan files for virus!!!!!

                var randomFileName = Path.GetRandomFileName();
                var fileName = randomFileName.Split('.');

                using (var filestream = File.Create($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}/{fileName[0]}.png"))
                {
                    await image.CopyToAsync(filestream);
                    filestream.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
