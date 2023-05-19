using IniParser;
using IniParser.Model;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
    public abstract class BotTelegram
    {
        private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokens.ini");
        private static FileIniDataParser parser = new FileIniDataParser();
        private static IniData? data = parser.ReadFile(path);
        static private TelegramBotClient client = new TelegramBotClient(data["Profile0"]["YourBotTelegreamToken"]);
        private static string? urlInput;
        public static string? IdChats;
        public static string? IdMessages;
        public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var message = update.Message;
            IdChats = message?.Chat?.Id.ToString();
            IdMessages = message.MessageId.ToString();
            if (message.Text != null)
            {
                if (UrlIsValid(message.Text))
                {

                }

            }
            if (update?.CallbackQuery?.Data != null)
            {

            }
        }


        public static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Console.WriteLine(arg2.Message);
            return Task.CompletedTask;
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
        static public void StatsBot()
        {
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }
    }
