using System.IO;
using Microsoft.AspNetCore.Http;

namespace AbiFramework.Extensions
{
    public static class FormFileExtensions
    {
        public static byte[] ReadAllAsByte(this IFormFile formFile)
        {
            using (var ms = new MemoryStream())
            {
                formFile.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
