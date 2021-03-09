using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using DiscordBot.Resources.Datatypes;

namespace DiscordBot.Resources.Extensions
{
    public static class LoadSettings
    {
        public static Settings LoadSettingsJson()
        {
            string jsonLocation = Directory.GetCurrentDirectory() + @"\Data\Settings.json";
            //if(File.Exists(jsonLocation) == false)
            //{
            //    Console.BackgroundColor = ConsoleColor.Red;
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.WriteLine("FILE Settings.json NOT FOUND!\nTERMINATING PROGRAM NOW");
            //    System.Threading.Thread.Sleep(3_000);
            //    return null;
            //}

            string json = "";
            using (FileStream stream = new FileStream(jsonLocation, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
            }
            if(json != "")
            {
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                string token = Environment.GetEnvironmentVariable("BotToken");
                ulong owner = ulong.Parse(Environment.GetEnvironmentVariable("BotAuthor"));
                string version = Environment.GetEnvironmentVariable("BotVersion");
                string name = Environment.GetEnvironmentVariable("BotName");
                string prefix = Environment.GetEnvironmentVariable("BotPrefix");
                return new Settings()
                {
                    Token = token,
                    Owner = owner,
                    Version = version,
                    Name = name,
                    Prefix = prefix
                };
            }
        }
    }
}
