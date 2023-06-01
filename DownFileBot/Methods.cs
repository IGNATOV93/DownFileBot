using IniParser;
using IniParser.Model;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
abstract public class Methods
{
   
   public static async Task<string> DownFile(string url,string idMessage)
    {
        
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string fileName = Path.GetFileName(url);
        if (fileName.Contains('?')) { fileName = fileName.Split('?')[0]; }
        Console.WriteLine($"Идет загрузка файла: {fileName} ");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            if (url.Contains(":8090/")&& BotTelegram.data?.GetKey(idMessage) !=null)
            {
                string authInfo = BotTelegram.data["Profile0"][idMessage];    //строка для авторизации ,логин и пароль торрсерва чере":"
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));//авторизируемся
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            }

            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(fileName, FileMode.Create))
                    {
                        byte[] buffer = new byte[10485760];//1082 было в стандарте .Теперь под файл 2гб ,вроде более менее
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        if (totalBytes >= 2147483648) //проверка файла больше ли 2000мб
                        {
                            return "";
                        }
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        int countDisconnect =0;
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            try
                            {
                                await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            }
                            catch (Exception ex)
                            {
                                if (countDisconnect > 10) { return ""; }//10 попыток докачать файл при ошибке
                                countDisconnect++;
                                Console.WriteLine(ex.Message);
                                continue;
                            }
                            totalBytesRead += bytesRead;
                            if(totalBytes > 0)
                            {
                                double percentage = Math.Round(((double)totalBytesRead / totalBytes) * 100, 1);
                                double megabytesDownloaded = totalBytesRead / (1024 * 1024);
                                double megabytesRemaining = (totalBytes - totalBytesRead) / (1024 * 1024);
                                double speed =Math.Round(megabytesDownloaded/stopwatch.Elapsed.TotalSeconds,1);
                                BotTelegram.DownfileIfo = $"\rЗагрузка на vps:  {megabytesDownloaded:F2} MB /{totalBytes / (1024 * 1024)} MB\r\n ({percentage}%) \r\n осталось : {TimeSpan.FromSeconds(stopwatch.Elapsed.TotalSeconds / totalBytesRead * (totalBytes - totalBytesRead)):hh\\:mm\\:ss} сек. \r\n {speed} Mb/s ";
                              
                                Console.Write(BotTelegram.DownfileIfo);

                            }
                        }
                        stopwatch.Stop();

                        double totalSecondsAll = stopwatch.Elapsed.TotalSeconds;
                        double megabytesDownloadedAll = new FileInfo(fileName).Length / (1024 * 1024);
                        double speedAll = Math.Round(megabytesDownloadedAll / totalSecondsAll, 1);

                        Console.WriteLine($"Загрузка завершена за {totalSecondsAll:F2} секунд. Средняя скорость загрузки: {speedAll:F2} МБ/с.");
                    }
                }
            }
        }
        Console.WriteLine("\nФайл загружен на vps сервер");
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
    public static  bool FileToVideoIsValid (string filePath)
    {
        string fileExtension = filePath.Split('.')[filePath.Split('.').Length - 1];
        Console.WriteLine(filePath);
        if (fileExtension == "mp4" ||
            fileExtension == "avi" ||
            fileExtension == "mov" ||
            fileExtension == "wmv" ||
            fileExtension == "flv" ||
            fileExtension == "mkv")
        {
            Console.WriteLine("Файл является видео-файлом.");
            return true;
        }
        else
        {
            Console.WriteLine("Файл не является видео-файлом.");
            return false;
        }
    }

    public static string VideoInfo(string filePath) 
    {
        var tfile = TagLib.File.Create(filePath);
       
        string title = tfile.Tag.Title;
        string minutes = tfile.Properties.Duration.TotalMinutes.ToString();
        long fileSize = tfile.Length;
        string fileSizeInGB = Math.Round((double)fileSize / (1024 * 1024 * 1024),2).ToString();
        string resultInfoVideoFile = $"{title}\r\n{minutes} min.   {fileSizeInGB} Gb.";
        return resultInfoVideoFile;
    }
}

