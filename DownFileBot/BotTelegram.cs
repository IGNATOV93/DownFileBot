﻿using IniParser;
using IniParser.Model;
using System.Net;
using System.Reflection.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

public abstract class BotTelegram
{
    private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
    private static FileIniDataParser parser = new FileIniDataParser();
    public static IniData? data = parser.ReadFile(path);
    static private TelegramBotClient client = new TelegramBotClient(new TelegramBotClientOptions(
           data["Profile0"]["YourBotTelegreamToken"], data["Profile0"]["YourLocalServerTelegram"]));
    public static string? IdChats;
    public static int IdMessages;
    public static string PathDownFile = "";
    public static string DownfileIfo = "";


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
           
                if (message.Text == "/start") {
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
                if(message.Text.Contains("&&&"))
                {
                    var authTorArray = message.Text.Split("&&&");
                    string authString = authTorArray[0]+":"+authTorArray[1];
                    data["Profile0"][IdChats] = authString;
                    parser.ReadFile(path);
                    parser.WriteFile("settings.ini", data);
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Хорошо,авторизация сохранена", replyMarkup: KeyboarDeleteMessage);
                    return;
                }
                if (Methods.UrlIsValid(message.Text.ToString()))
                {

                    var messageDownFile =await botClient.SendTextMessageAsync(IdChats, "Файл загружается ...");
                    Task<string> t1 = Methods.DownFile(message.Text.ToString(), IdChats);

                    Task t2 = SendTextMessageInLoopAsync();// Создаем асинхронный метод для отправки текстовых сообщений
                    await Task.WhenAll(t1, t2); // Выполняем задачи параллельно и ждем результат их завершения
                    async Task SendTextMessageInLoopAsync()
                    {
                        while (!t1.IsCompleted) // Цикл будет выполняться, пока задача t1 не завершится
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3)); // Задержка на  секунду
                            await botClient.EditMessageTextAsync(IdChats,messageDownFile.MessageId,DownfileIfo); // Отправляем текстовое сообщение
                            
                        }
                    }
                    PathDownFile = t1.Result;
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.DeleteMessageAsync(IdChats, messageDownFile.MessageId);
                    if (PathDownFile != "")
                    {
                        var stream = new FileStream(PathDownFile, FileMode.Open);
                        InputOnlineFile file = new InputOnlineFile(stream, PathDownFile);
                     var messageDounwFileTelegram=await botClient.SendTextMessageAsync(IdChats, "Зазрузка в телеграм..");

                       var downFileTelegram=await botClient.SendDocumentAsync(IdChats, file, replyMarkup: keyButtonMain);
                        string fileId = downFileTelegram.Document.FileId;
                        long bytesUploaded = 0;
                        var fileInfo = new FileInfo(PathDownFile);
                        long fileSize = fileInfo.Length;
                        DateTime startTime = DateTime.Now;
                        while (bytesUploaded < fileSize)
                        {
                            // Отслеживать статус загрузки файла
                            var File = await botClient.GetFileAsync(fileId);
                            bytesUploaded = (long)File.FileSize;
                            TimeSpan elapsed = DateTime.Now - startTime;
                            double speedInBytesPerSecond = bytesUploaded / elapsed.TotalSeconds;
                            double speedInMegabytesPerSecond = speedInBytesPerSecond / (1024 * 1024);

                            Console.WriteLine("{0} MB / {1} MB - Speed: {2:0.00} MB/s", bytesUploaded / (1024 * 1024), fileSize / (1024 * 1024), speedInMegabytesPerSecond);

                            // Задержка перед следующей проверкой статуса загрузки
                            await Task.Delay(1000);
                        }

                        Console.WriteLine("Upload complete!");

                        Console.WriteLine("Файл загружен в телеграм сервер");
                        System.IO.File.Delete(PathDownFile);
                        return;
                    }
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Это печально но файл больше 1.85 гб или неверная ссылка", replyMarkup: KeyboarDeleteMessage);
                    return;
                }
                await botClient.DeleteMessageAsync(IdChats, IdMessages);
                await botClient.SendTextMessageAsync(IdChats, "Ссылка на файл не корректная", replyMarkup:KeyboarDeleteMessage);
                return;
            }
            if (update?.CallbackQuery?.Data != null) //ОБРАБОТКА UPDATE (inline)
            {
                string textInline = update.CallbackQuery.Data.ToString();
                IdChats = update.CallbackQuery.Message.Chat.Id.ToString();
                IdMessages = Int32.Parse(update.CallbackQuery.Message.MessageId.ToString());
                if(textInline== "deletemessages")
                {
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    return;
                }    
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Что то пошло не так..",replyMarkup:KeyboarDeleteMessage);
           
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


