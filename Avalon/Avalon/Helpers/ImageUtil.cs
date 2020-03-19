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


        public async Task UploadImageAsync(CurrentUser currentUser, IFormFile photo)
        {
            try
            {
                if (photo.Length > 0 && photo.Length < _fileSizeLimit)
                {
                    // TODO: Find a place for you files!
                    if (!Directory.Exists($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}"))
                    {
                        Directory.CreateDirectory($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}");
                    }

                    // TODO: Scan files for virus!!!!!

                    var randomFileName = Path.GetRandomFileName();
                    var fileName = randomFileName.Split('.');

                    using (var filestream = System.IO.File.Create($"C:/Peter Rose - Private/Photos/{currentUser.ProfileId}/{fileName[0]}.png"))
                    {
                        await photo.CopyToAsync(filestream);
                        filestream.Flush();
                    }
                }

                // TODO: Find på noget bedre end en exception når den fejler fx. pga. file size.
                throw new Exception();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
