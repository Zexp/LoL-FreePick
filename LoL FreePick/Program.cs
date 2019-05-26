using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Threading;

namespace LoL_FreePick
{
    class Program
    {
        static ITelegramBotClient botClient;
        public static List<string> champions = new List<string>();
        public static List<string> prRoles = new List<string>();
        public static List<string> scRoles = new List<string>();

        public static string GetSiteToString(string Site)
        {
            Uri LoLSite = new Uri(Site);

            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LoLSite);
            request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buf, 0, count));
                }
            }
            while (count > 0);

            return sb.ToString();
        }

        public static string ReadInfoFromA(string whereSearch, string whatFind, int index)
        {
            int whatFindIndex = whereSearch.IndexOf(">",whereSearch.IndexOf(whatFind, index) + 11+ whatFind.Length);
            return whereSearch.Substring(whatFindIndex + 1,whereSearch.IndexOf("<", whatFindIndex) - whatFindIndex - 1);
        }
        public static string ReadRole(string whereSearch, string whatFind, int index)
        {
            string wtf = ReadInfoFromA(whereSearch, whatFind, index);
            int whatFindIndex = whereSearch.IndexOf(">", whereSearch.IndexOf(wtf, index) + 11 + wtf.Length);

            return whereSearch.Substring(whatFindIndex + 1, whereSearch.IndexOf("<", whatFindIndex) - whatFindIndex - 1);
        }
        static void Main(string[] args)
        {
            string LolSite = "https://ru.leagueoflegends.com/ru/news/champions-skins/free-rotation";


            string allStr = GetSiteToString(LolSite);

            int indexOfDefault = allStr.IndexOf("default-2-3");
            string neededPage = allStr.Substring(allStr.IndexOf("a href=\"", indexOfDefault) + 8,
                                                allStr.IndexOf("\"", allStr.IndexOf("a href=\"", indexOfDefault) + 8) - allStr.IndexOf("a href=\"", indexOfDefault) - 8);
            Console.WriteLine("-------------------------------------------------------");

            string uriNeededPage = "https://ru.leagueoflegends.com" + neededPage;

            Console.WriteLine(neededPage);

            string freePickPage = GetSiteToString(uriNeededPage);

            List<int> indexexOfChamps = new List<int>();

            int index = 0;
            while ((index = freePickPage.IndexOf("champion-info", index)) != -1)
            {
                indexexOfChamps.Add(index);
                index += "champion-info".Length;
            }

            foreach (var item in indexexOfChamps)
            {
                champions.Add(ReadInfoFromA(freePickPage, "champion-name", item));
                prRoles.Add(ReadRole(freePickPage, "primary-role", item));
                scRoles.Add(ReadRole(freePickPage, "secondary-role", item));
            }

            Console.WriteLine(champions.Capacity);
            int i = 0;
            foreach (var item in champions)
            {
                Console.WriteLine(item + "|||     Основная роль = " + prRoles[i] + "|||     Второстепенная роль = " + scRoles[i]);
                i++;
            }

            botClient = new Telegram.Bot.TelegramBotClient("883680996:AAEIvOUurjoJ_0UwGh07nDP6TzAgKslxxK8");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
            Console.Read();
        }

            static async void Bot_OnMessage(object sender, MessageEventArgs e)
            {
                string myText = "Фрипик на " + DateTime.Today.ToShortDateString() + "\n";

                int i = 0;

                foreach (var item in champions)
                {
                    myText += item + " (" + prRoles[i] + "," + scRoles[i] + ")\n";
                    i++;
                }

            e.Message.Text = myText;


        
            if (e.Message.Text != null)
                {
                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      text: myText
                    );

                foreach (var item in champions)
                {
                    string tempName = item.Replace(" ", "%20");
                    string Guids = item+": https://yandex.ru/search/?text="+ tempName + "%20гайд";
                    await botClient.SendTextMessageAsync(
                          chatId: e.Message.Chat,
                          text: Guids,
                          parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                        );
                }
                
            }
            }
    }
}
