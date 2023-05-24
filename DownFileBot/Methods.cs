﻿using IniParser;
using IniParser.Model;
using Plugin.Media.Abstractions;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
abstract public class Methods
{
   
   public static async Task<string> DownFile(string url)
    {
        
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string fileName = Path.GetFileName(url);
        if (fileName.Contains('?')) { fileName = fileName.Split('?')[0]; }
        Console.WriteLine($"Идет загрузка файла: {fileName} ");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            if (url.Contains(":8090/")&& BotTelegram.data.GetKey("YourSettingsTor")!=null)
            {
                string authInfo = BotTelegram.data["Profile0"]["YourSettingsTor"];    //строка для авторизации ,логин и пароль торрсерва чере":"
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));//авторизируемся
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            }

            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(fileName, FileMode.Create))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        if (totalBytes >= 2000000000) //проверка файла больше ли 2000мб
                        {
                            return "";
                        }
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            try
                            {
                                await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                continue;
                            }
                            totalBytesRead += bytesRead;
                            if (totalBytes > 0)
                            {
                                double percentage = Math.Round(((double)totalBytesRead / totalBytes) * 100, 1);
                                double megabytesDownloaded = totalBytesRead / (1024 * 1024);
                                double megabytesRemaining = (totalBytes - totalBytesRead) / (1024 * 1024);

                                Console.Write($"\rИдет загрузка файла :{megabytesDownloaded:F2} MB /{totalBytes / (1024 * 1024)} MB ({percentage}%) - скачается через: {TimeSpan.FromSeconds(stopwatch.Elapsed.TotalSeconds / totalBytesRead * (totalBytes - totalBytesRead)):hh\\:mm\\:ss}    скорость: {Math.Round((bytesRead / 1024) / stopwatch.Elapsed.TotalSeconds, 2)} Mb/s  ");
                            }
                        }
                    }
                }
            }
        }
        Console.WriteLine("\nФайл успешно скачан!");
        return fileName;
    }
    
    public static bool UrlIsValid(string url)
    {
        Uri uriResult;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (result)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                result = response.StatusCode == HttpStatusCode.OK;
                response.Close();
            }
            catch
            {
                result = false;
            }
        }

        return result;
    }
}

