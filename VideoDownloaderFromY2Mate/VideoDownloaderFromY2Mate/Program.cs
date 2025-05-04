using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using VideoDownloaderFromY2Mate.Models;

namespace VideoDownloaderFromY2Mate
{
    internal class Program
    {
        private static readonly string getKeyAPI = "https://api.mp3youtube.cc/v2/sanity/key";
        private static readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0";

        static async Task Main(string[] args)
        {
            // thumbnail: https://i.ytimg.com/vi/<video_id>/0.jpg

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  _____                          _                    _  __     __ ___                     _        \r\n |  __ \\                        | |                  | | \\ \\   / /|__ \\                   | |       \r\n | |  | |  ___ __      __ _ __  | |  ___    __ _   __| |  \\ \\_/ /    ) | _ __ ___    __ _ | |_  ___ \r\n | |  | | / _ \\\\ \\ /\\ / /| '_ \\ | | / _ \\  / _` | / _` |   \\   /    / / | '_ ` _ \\  / _` || __|/ _ \\\r\n | |__| || (_) |\\ V  V / | | | || || (_) || (_| || (_| |    | |    / /_ | | | | | || (_| || |_|  __/\r\n |_____/  \\___/  \\_/\\_/  |_| |_||_| \\___/  \\__,_| \\__,_|    |_|   |____||_| |_| |_| \\__,_| \\__|\\___|\r\n                                                                                                    \r\n                                                                                                    ");
            Console.ResetColor();

            bool isStop = false;

            while (isStop == false)
            {
                await StartDownLoad();

                Console.Write("Do you want to download other video (Y) of (N): ");
                string? answer = Console.ReadLine();
                Console.WriteLine("=====================================");
                if (answer == null || answer.Equals("n", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
        }

        static async Task StartDownLoad()
        {
            try
            {
                string? link = null;
                string? videoId = null;
                while (true)
                {
                    Console.Write("Enter Youtube Video Url: ");
                    link = Console.ReadLine();
                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }

                    try
                    {
                        videoId = ExtractYouTubeVideoId(link);
                        if (videoId != null)
                        {
                            break;
                        }

                        Console.WriteLine("Video not found !!!");
                    }
                    catch
                    {
                        Console.Write("Youtube Video Url is invalid !!!");
                    }
                }

                GetKeyResponse key = await GetKey();
                ConverterResponse downloadUrl = await GetDownloadUrl(key.Key, videoId);


                var cts = new CancellationTokenSource();
                var loadingTask = ShowLoading("Downloading", cts.Token);

                await DownLoadVideo(downloadUrl.Url, downloadUrl.FileName);

                cts.Cancel();
                await loadingTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static string? ExtractYouTubeVideoId(string url)
        {
            Uri uri = new Uri(url);

            if (uri.Host.Contains("youtu.be"))
            {
                // Dạng rút gọn: https://youtu.be/{id}
                return uri.AbsolutePath.Trim('/');
            }
            else if (uri.Host.Contains("youtube.com"))
            {
                // Dạng đầy đủ: https://www.youtube.com/watch?v={id}
                var query = HttpUtility.ParseQueryString(uri.Query);
                return query["v"];
            }

            return null;
        }

        static async Task<GetKeyResponse> GetKey()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9,vi;q=0.8,es;q=0.7");
            client.DefaultRequestHeaders.Add("cache-control", "no-cache");
            client.DefaultRequestHeaders.Add("origin", "https://iframe.y2meta-uk.com");
            client.DefaultRequestHeaders.Add("pragma", "no-cache");
            client.DefaultRequestHeaders.Add("priority", "u=1, i");
            client.DefaultRequestHeaders.Add("referer", "https://iframe.y2meta-uk.com/");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Microsoft Edge\";v=\"136\", \"Not.A/Brand\";v=\"99\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");


            var response = await client.GetAsync(getKeyAPI);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"StatusCode: {response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync();
            GetKeyResponse? model = JsonSerializer.Deserialize<GetKeyResponse>(content);

            if (model == null || string.IsNullOrEmpty(model.Key) == true)
            {
                throw new Exception($"Unexpected results");
            }

            return model;
        }

        static async Task<ConverterResponse> GetDownloadUrl(string key, string videoId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9,vi;q=0.8,es;q=0.7");
            client.DefaultRequestHeaders.Add("cache-control", "no-cache");
            client.DefaultRequestHeaders.Add("key", key);
            client.DefaultRequestHeaders.Add("origin", "https://iframe.y2meta-uk.com");
            client.DefaultRequestHeaders.Add("pragma", "no-cache");
            client.DefaultRequestHeaders.Add("priority", "u=1, i");
            client.DefaultRequestHeaders.Add("referer", "https://iframe.y2meta-uk.com/");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Microsoft Edge\";v=\"136\", \"Not.A/Brand\";v=\"99\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");
            client.DefaultRequestHeaders.Add("user-agent", userAgent);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("link", $"https://youtu.be/{videoId}"),
                new KeyValuePair<string, string>("format", "mp4"),
                new KeyValuePair<string, string>("audioBitrate", "128"),
                new KeyValuePair<string, string>("videoQuality", "1080"),
                new KeyValuePair<string, string>("filenameStyle", "pretty"),
                new KeyValuePair<string, string>("vCodec", "h264")
            });

            var response = await client.PostAsync("https://api.mp3youtube.cc/v2/converter", formData);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"StatusCode: {response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync();
            ConverterResponse? model = JsonSerializer.Deserialize<ConverterResponse>(content);

            if (model == null ||
                string.IsNullOrEmpty(model.Url) ||
                string.IsNullOrEmpty(model.Status) ||
                string.IsNullOrEmpty(model.FileName))
            {
                throw new Exception($"Unexpected results");
            }
            Console.WriteLine($"Video Name: {model.FileName}");
            return model;
        }

        static async Task DownLoadVideo(string url, string fileName)
        {
            string downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string filePath = Path.Combine(downloadFolder, fileName);
            using HttpClient client = new HttpClient();
            byte[] videoBytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(filePath, videoBytes);
            Console.WriteLine();
            Console.WriteLine($"Video Downloaded: {filePath}");
        }

        static async Task ShowLoading(string message, CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            char[] spinner = new[] { '|', '/', '-', '\\' };
            int counter = 0;
            while (!token.IsCancellationRequested)
            {
                Console.Write($"\r{message}... {spinner[counter++ % spinner.Length]}");
                await Task.Delay(100);
            }
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            Console.ResetColor();
        }
    }
}
