using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        public static Random rnd = new Random();
        public static string query = "";
        static TelegramBotClient botClient = new TelegramBotClient(""); // your telegram bot key here

        public static ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
               new[]{ new KeyboardButton("Следующее изображение")}
            }
        }; 
        static void Main(string[] args)
        {
            var me = botClient.GetMeAsync().Result;
            rkm.OneTimeKeyboard = true;
            rkm.ResizeKeyboard = true;
            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();
            Console.WriteLine("Бот запущен");
            Console.ReadKey();
        }

        private static void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            var msg = e.Message.Text;
            if (msg == null)
                return;
            if (msg.StartsWith("/find"))
            {
                msg = msg.Replace("/find ", "");
                string newMsg = msg.Replace(" ", "+");
                query = newMsg;
                SendPhoto(newMsg, e);
            }
            if (msg.StartsWith("Следующее изображение"))
            {
                if (query == "")
                    botClient.SendTextMessageAsync(e.Message.Chat.Id, "Для начала найдите изображение при помощи команды - /find");
                else
                    SendPhoto(query, e);
            }
        }
        static string GetJson(string tag)
        {
            string url = $"https://pixabay.com/api/?key=14716131-19818461aa6055415a7165bad&q={tag}&image_type=photo";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }
        private static void SendPhoto(string msg, MessageEventArgs e)
        {
            var json = GetJson(msg);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var hits = ((JArray)data["hits"]).ToObject<List<Dictionary<string, object>>>();
            if (hits.Count > 0)
            {
                botClient.SendPhotoAsync(e.Message.Chat.Id, hits[rnd.Next(0, hits.Count)]["webformatURL"].ToString(), $"Изображение по запросу: {msg}",
                      replyToMessageId: e.Message.MessageId, replyMarkup: rkm);

            }
            else
            {
                botClient.SendTextMessageAsync(e.Message.Chat.Id, "Извините, по Вашему запросу ничего не найдено.", replyToMessageId: e.Message.MessageId);
            }
        }
    }
}
