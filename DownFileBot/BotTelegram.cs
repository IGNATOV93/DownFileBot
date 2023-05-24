using IniParser;
using IniParser.Model;
using System.Net;
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
    public static Dictionary<string, int> DSettingsWievFiles = new();


public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var message = update.Message;
            var buttontopViewFiles = new KeyboardButton("Настройка отображения файлов");
            var keyboardSettingsViewFiles = new ReplyKeyboardMarkup(new[] { buttontopViewFiles }); keyboardSettingsViewFiles.ResizeKeyboard = true;
            var inlineKeyboardOptionViewFiles = new InlineKeyboardMarkup(new[]
                   {
                new[]
                  {
                    InlineKeyboardButton.WithCallbackData("Как видеофайлы",
                        "setMovies"),
                    InlineKeyboardButton.WithCallbackData("Как документы","setDocuments")

                  },
               });

            if (message?.Text != null)
            {
                IdChats = message?.Chat?.Id.ToString();
                IdMessages = message.MessageId;
                if (message.Text == "/start") { }
                if (message.Text == "Настройка отображения файлов")
                {
                    await botClient.DeleteMessageAsync(IdChats, IdMessages);
                    await botClient.SendTextMessageAsync(IdChats, "Выберите как отображать файлы", replyMarkup: inlineKeyboardOptionViewFiles);
                    return;
                }
                if (Methods.UrlIsValid(message.Text.ToString()))
                {
                    
                    PathDownFile = await Methods.DownFile(message.Text.ToString());
                    if (PathDownFile != "")
                    {
                        var messageDounFile = await botClient.SendTextMessageAsync(IdChats, "Файл загружается ...", replyMarkup: keyboardSettingsViewFiles);
                        var stream = new FileStream(PathDownFile, FileMode.Open);
                        InputOnlineFile file = new InputOnlineFile(stream, PathDownFile);
                        await botClient.DeleteMessageAsync(IdChats, IdMessages);
                       
                        if (DSettingsWievFiles.ContainsKey(IdChats))
                        {
                          
                            if (DSettingsWievFiles[IdChats] == 1)
                            {
                                await botClient.SendVideoAsync(IdChats, file, replyMarkup: keyboardSettingsViewFiles);
                            }
                            if (DSettingsWievFiles[IdChats] == 2)
                            {
                                await botClient.SendDocumentAsync(IdChats, file, replyMarkup: keyboardSettingsViewFiles);
                            }

                        }

                        System.IO.File.Delete(PathDownFile);
                        return;
                    }
                    await botClient.SendTextMessageAsync(IdChats, "Это печально но файл больше 2гб или неверная ссылка");
                    return;
                }
                await botClient.SendTextMessageAsync(IdChats, "Ссылка на файл не корректная");
                return;
            }
            if (update?.CallbackQuery?.Data != null) //ОБРАБОТКА UPDATE (inline)
            {
                string textInline = update.CallbackQuery.Data.ToString();
                IdChats = update.CallbackQuery.Message.Chat.Id.ToString();
                IdMessages = Int32.Parse(update.CallbackQuery.Message.MessageId.ToString());
                if (textInline == "setMovies")
                {
                    if (!DSettingsWievFiles.ContainsKey(IdChats)) { DSettingsWievFiles.Add(IdChats, 1); }
                    if (DSettingsWievFiles.ContainsKey(IdChats)) { DSettingsWievFiles[IdChats] = 1; }
                    await botClient.SendTextMessageAsync(IdChats, "Сохранил ваш выбор");
                    return;
                }
                if (textInline == "setDocuments")
                {
                    if (!DSettingsWievFiles.ContainsKey(IdChats)) { DSettingsWievFiles.Add(IdChats, 2); }
                    if (DSettingsWievFiles.ContainsKey(IdChats)) { DSettingsWievFiles[IdChats] = 2; }
                    await botClient.SendTextMessageAsync(IdChats, "Сохранил ваш выбор");
                    return;
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Что то пошло не так..");
           
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


