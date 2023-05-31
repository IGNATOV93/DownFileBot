﻿using IniParser;
using IniParser.Model;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public abstract class BotTelegram
{
    private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
    private static FileIniDataParser parser = new FileIniDataParser();
    public static IniData? data = parser.ReadFile(path);
    private static HttpClient httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(300)
    };
    static private TelegramBotClient client = new TelegramBotClient(new TelegramBotClientOptions(
           data["Profile0"]["YourBotTelegreamToken"], data["Profile0"]["YourLocalServerTelegram"]), httpClient);
    public static string? IdChats;
    public static int IdMessages;
    public static string PathDownFile = "";
    public static string DownfileIfo = "";
    public event EventHandler<int> FileSizeUpdated;

    private static double size;

    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var buttonAuthTorrserver = new KeyboardButton("Настройка torrserver");
        var keyButtonMain = new ReplyKeyboardMarkup(buttonAuthTorrserver); keyButtonMain.ResizeKeyboard = true;
        var KeyboarDeleteMessage = new InlineKeyboardMarkup(new[]
            {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Скрыть сообщение \U0001F5D1", "deletemessages")
                    }
                });
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var message = update.Message;


            if (message?.Text != null)
            {
                IdChats = message?.Chat?.Id.ToString();
                IdMessages = message.MessageId;

                if (message.Text == "/start")
                {
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Привет,пришли мне ссылку на файл который хотите скачать \r\n а я его скачаю (до 1.85 гб размером)", replyMarkup: keyButtonMain);
                    return;
                }
                if (message.Text == "Настройка torrserver")
                {
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Для настройки авторизации torrserver,напишите в чате логин пароль через \"&&&\"\r\n" +
                        "пример : ivanov&&&1933abcd", replyMarkup: keyButtonMain);
                    return;
                }
                if (message.Text.Contains("&&&"))
                {
                    var authTorArray = message.Text.Split("&&&");
                    string authString = authTorArray[0] + ":" + authTorArray[1];
                    data["Profile0"][IdChats] = authString;
                    parser.ReadFile(path);
                    parser.WriteFile("settings.ini", data);
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Хорошо,авторизация сохранена", replyMarkup: KeyboarDeleteMessage);
                    return;
                }
                if (Methods.UrlIsValid(message.Text.ToString()))
                {

                    var messageDownFile = await botClient.SendTextMessageAsync(IdChats, "Файл загружается на vps...");
                    Task<string> t1 = Methods.DownFile(message.Text.ToString(), IdChats);

                    Task t2 = SendTextMessageInLoopAsync();// Создаем асинхронный метод для отправки текстовых сообщений
                    await Task.WhenAll(t1, t2); // Выполняем задачи параллельно и ждем результат их завершения
                    async Task SendTextMessageInLoopAsync()
                    {
                        await Task.Delay(1000);
                        while (!t1.IsCompleted) // Цикл будет выполняться, пока задача t1 не завершится
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1)); // Задержка на  секунду
                            await botClient.EditMessageTextAsync(IdChats, messageDownFile.MessageId, DownfileIfo); // Отправляем текстовое сообщение

                        }
                    }
                    PathDownFile = t1.Result;
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.DeleteMessageAsync(IdChats, messageDownFile.MessageId);
                    if (PathDownFile != "")
                    {
                        try
                        {
                            int idsendMes = 0;
                            const int bufferSize = 4096;
                            const int timerInterval = 1000; // интервал вывода скорости отправки файла, мс
                            var stopwatch = new Stopwatch();
                            var httpClient = new HttpClient();
                            
                           
                            using (var stream = new FileStream(PathDownFile, FileMode.Open))
                            {
                                var content = new StreamContent(stream, bufferSize);
                                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "document",
                                    FileName = Path.GetFileName(PathDownFile)
                                };
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                                content.Headers.ContentLength = stream.Length;

                                // Вычисляем ориентировочное время загрузки
                                var sizeFileInBytes = new FileInfo(PathDownFile).Length;
                                var sizeFileInMb = Math.Round(((double)sizeFileInBytes / 1048576) / 9 / 60, 1);
                                var messageDounwFileTelegram = await botClient.SendTextMessageAsync(IdChats, $"Загрузка в телеграм займет примерно: {sizeFileInMb} мин.");
                                idsendMes = messageDounwFileTelegram.MessageId;

                                long bytesSent = 0L; double speed = 0.0; double sizeNowFile = 0.0; double allSizeFile = 0.0;
                                double progress = 0.0; long bytesLeft = 0L; double timeLeft = 0.0; long fileLength = stream.Length;
                                stopwatch.Start();

                                using (var formData = new MultipartFormDataContent())
                                {
                                    formData.Add(new StringContent(IdChats), "chat_id");
                                    formData.Add(content, "document");
                                    Timer timer = new Timer(state =>
                                    {
                                       
                                        bytesSent = stream.Position;
                                        double elapsedSecs = stopwatch.ElapsedMilliseconds / 1000.0;
                                        if (elapsedSecs != 0) speed = bytesSent / elapsedSecs / (1024 * 1024.0);

                                        sizeNowFile = Math.Round(bytesSent / (1024 * 1024.0), 2);
                                        allSizeFile = Math.Round(stream.Length / (1024 * 1024.0), 2);
                                        progress = Math.Round((double)bytesSent / fileLength * 100, 2);
                                        bytesLeft = stream.Length - stream.Position;

                                        double remainingBytes = stream.Length - bytesSent;
                                        long remainingTicks = (long)(remainingBytes / (speed * 1024 * 1024) * TimeSpan.TicksPerSecond);
                                        TimeSpan timeLeft = TimeSpan.FromTicks(remainingTicks);

                                        string timeLeftStr = (timeLeft.TotalSeconds < 60) ? $"{timeLeft.TotalSeconds:F0} sec." : $"{timeLeft.TotalMinutes:F0}:{timeLeft.Seconds:D2} min.";

                                        string inputInfoToSendTelegram = $"\rЗагрузка в телеграм: {sizeNowFile}/{allSizeFile} MB \r({progress}%) \r{speed:F2} MB/s \rОсталось : {timeLeftStr}.";
                                        if (progress >= 90.0)
                                        {
                                            inputInfoToSendTelegram = $"\rЗагрузка в телеграм: осталось несколько секунд..";
                                        }
                                       // Console.Write(inputInfoToSendTelegram);
                                        botClient.EditMessageTextAsync(IdChats, messageDounwFileTelegram.MessageId, inputInfoToSendTelegram);
                                    }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(timerInterval));
                                    
                                    var response = await httpClient.SendAsync(new HttpRequestMessage
                                    {
                                        Method = HttpMethod.Post,
                                        RequestUri = new Uri($"{data["Profile0"]["YourLocalServerTelegram"]}/bot{data["Profile0"]["YourBotTelegreamToken"]}/sendDocument"),
                                        Content = formData
                                    });

                                    timer.Dispose();
                                    stopwatch.Stop();
                                    bytesSent = stream.Length;

                                     var result = await response.Content.ReadAsStringAsync();
                                     Console.WriteLine(result);

                                    await botClient.DeleteMessageAsync(IdChats, idsendMes);
                                    Console.WriteLine("Файл загружен в телеграм сервер");
                                    System.IO.File.Delete(PathDownFile);
                                    PathDownFile = "";
                                    DownfileIfo = "";
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                System.IO.File.Delete(PathDownFile);
                            }
                            catch
                            {
                                Console.WriteLine(e);
                                return;
                            }
                            return;
                        }
                    }
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Это печально но файл больше 1.85 гб или неверная ссылка", replyMarkup: KeyboarDeleteMessage);
                    return;

                }
                await botClient.DeleteMessageAsync(IdChats, IdMessages);
                await botClient.SendTextMessageAsync(IdChats, "Ссылка на файл не корректная", replyMarkup: KeyboarDeleteMessage);
                return;
            }
            if (update?.CallbackQuery?.Data != null) //ОБРАБОТКА UPDATE (inline)
            {
                string textInline = update.CallbackQuery.Data.ToString();
                IdChats = update.CallbackQuery.Message.Chat.Id.ToString();
                IdMessages = Int32.Parse(update.CallbackQuery.Message.MessageId.ToString());
                if (textInline == "deletemessages")
                {
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Что то пошло не так..", replyMarkup: KeyboarDeleteMessage);
            return;
        }
    }
    public static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        Console.WriteLine(arg2.Message);
        return Task.CompletedTask;
    }

    static public void StatsBot()
    {
        client.StartReceiving(Update, Error);
        Console.ReadLine();
    }
}


