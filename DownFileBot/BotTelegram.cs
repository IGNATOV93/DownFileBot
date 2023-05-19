using IniParser;
using IniParser.Model;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
public abstract class BotTelegram
{
    private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
    private static FileIniDataParser parser = new FileIniDataParser();
    public static IniData? data = parser.ReadFile(path);
    static private TelegramBotClient client = new TelegramBotClient(data["Profile0"]["YourBotTelegreamToken"]);
    private static string? urlInputs;
    public static string? IdChats;
    public static string? IdMessages;
   public static string PathDownFile = "";
    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
     
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        var message = update.Message;
        IdChats = message?.Chat?.Id.ToString();
        IdMessages = message.MessageId.ToString();
        if (message.Text != null)
        {

            if (Methods.UrlIsValid(message.Text.ToString()))
            {
                
                PathDownFile = await Methods.DownFile(message.Text.ToString());
                if (PathDownFile != "")
                {
                    var stream = new FileStream(PathDownFile, FileMode.Open);
                    InputOnlineFile file = new InputOnlineFile(stream, PathDownFile);

                    await botClient.SendDocumentAsync(IdChats, file);
                    return;
                }
                await botClient.SendTextMessageAsync(IdChats,"Это печально но файл больше 50мб");
                return;
            }


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
