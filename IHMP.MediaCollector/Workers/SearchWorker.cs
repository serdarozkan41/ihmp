using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace IHMP.MediaCollector.Workers
{
    public static class SearchWorker
    {
        private static IInstaApi InstaApi;
        private static bool IsLogin;

        public static async Task LoginInstagran()
        {
            var _user = new UserSessionData
            {
                UserName = "jeffbezosorj",
                Password = "lolgift"
            };

            var delay = RequestDelay.FromSeconds(2, 2);
            InstaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(_user)
                .UseLogger(new DebugLogger(LogLevel.Info))
                .SetRequestDelay(delay)
                .Build();

            var _logresult = await InstaApi.LoginAsync();
            IsLogin = _logresult.Succeeded;
            Console.WriteLine("Logged in...");
        }

        public static async Task<bool> Sync()
        {
            Console.WriteLine("Start collector worker...");
            if (!IsLogin)
            {
                Console.WriteLine("Must be logged in...");
                await LoginInstagran();
                return false;
            }
            try
            {
                Console.WriteLine("Searching hastag medias..");
                var medias = await InstaApi.HashtagProcessor.GetRecentHashtagMediaListAsync(Constants.TargetHastag, PaginationParameters.Empty);
                Console.WriteLine($"Number of media found: {medias.Value.Medias.Count}");
                int syncCount = 0;

                foreach (InstaMedia media in medias.Value.Medias)
                {
                    string _mediaFolder = $"{Constants.MediasPath}\\{media.InstaIdentifier}";
                    if (Directory.Exists(_mediaFolder))
                        continue;
                    else
                        Directory.CreateDirectory(_mediaFolder);

                    if (media.Images.Count == 0)
                        continue;

                    SaveImage($"{_mediaFolder}\\image_0.jpeg", media.Images[0].Uri, ImageFormat.Jpeg);
                    syncCount++;
                }

                Console.WriteLine($"Sync media count: {syncCount}");
            }
            catch (Exception ex)
            {
                IsLogin = false;
                Console.WriteLine($"Error detail: {ex.Message}");
            }
            Console.WriteLine("Finish collector worker...");
            await Task.Delay(2500);
            return true;
        }

        public static void CheckFolder()
        {
            if (!Directory.Exists(Constants.MediasPath))
            {
                Directory.CreateDirectory(Constants.MediasPath);
            }
        }

        private static void SaveImage(string filename, string imageUrl, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }

            stream.Flush();
            stream.Close();
            client.Dispose();
        }
    }
}
