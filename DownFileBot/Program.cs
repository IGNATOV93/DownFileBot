using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlTypes;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

public class MainClass
{
    //
    static void Main()
    {
        BotTelegram.StatsBot();
        Console.ReadLine();
    }
}