using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlTypes;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using searchOfTorrents_bot.JackettSearch;
using searchOfTorrents_bot.JackettSearch.models;
using static System.Net.Mime.MediaTypeNames;
using searchOfTorrents_bot.BotTelegram;

public class MainClass
{
    //
    static void Main()
    {

        BotTelegram.StartsBot();
        Console.ReadLine();

    }
}