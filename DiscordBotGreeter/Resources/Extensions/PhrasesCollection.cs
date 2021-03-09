using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Discord;
using Discord.WebSocket;
using DiscordBot.Resources.Datatypes;

namespace DiscordBot.Resources.Extensions
{
    public class PhrasesCollection
    {
        private static Object FileLock = new Object();
        private static readonly string FilePath = Directory.GetCurrentDirectory() + @"\Data\Phrases.csv";

        public static readonly string ConfirmEndPhrasesQuestion = "Bitte reagiere auf diese Nachricht mit :white_check_mark:, damit die Rollenzuweisung abgeschlossen wird.";

        public static ulong GetRoleId(string question, string reaction, ulong guildId)
        {
            lock (FileLock)
            {
                if (File.Exists(FilePath))
                {
                    using (FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] data = line.Split(';');
                                if (data[0] == question &&
                                    data[1] == reaction &&
                                    data[2] == guildId.ToString())
                                {
                                    return ulong.Parse(data[3]);
                                }
                            }
                        }
                    }
                }
                return 0;
            }
        }

        public static List<string> GetAllReactions(string question, ulong guildId)
        {
            lock (FileLock)
            {
                List<string> reactions = new List<string>();
                if (File.Exists(FilePath))
                {
                    using (FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] data = line.Split(';');
                                if (data[0] == question && data[2] == guildId.ToString())
                                {
                                    reactions.Add(data[1]);
                                }
                            }
                        }
                    }
                }
                return reactions;
            }
        }

        public static List<string> GetAllQuestions(ulong guildId)
        {
            lock(FileLock)
            {
                List<string> questions = new List<string>();
                if(File.Exists(FilePath))
                {
                    using(FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] data = line.Split(';');
                                if (data[2] == guildId.ToString())
                                {
                                    if (questions.Contains(data[0]) == false)
                                    {
                                        questions.Add(data[0]);
                                    }
                                }
                            }
                        }
                    }
                }
                return questions;
            }
        }

        public static bool SetRoleId(string question, string reaction, ulong guildId, ulong roleId)
        {

            if (GetRoleId(question, reaction, guildId) == 0)
            {
                lock (FileLock)
                {
                    using (FileStream file = new FileStream(FilePath, FileMode.Append))
                    {
                        using (StreamWriter writer = new StreamWriter(file))
                        {
                            writer.WriteLine($"{question};{reaction};{guildId};{roleId}");
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static bool RemoveQuestion(string question)
        {
            lock (FileLock)
            {
                if (File.Exists(FilePath))
                {
                    List<Phrase> phrases = new List<Phrase>();
                    int linesCount = 0;
                    using (FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                linesCount++;
                                string[] data = line.Split(';');
                                if (data[0] != question)
                                    phrases.Add(new Phrase(data[0], data[1], ulong.Parse(data[2]), ulong.Parse(data[3])));
                            }
                        }
                    }
                    using (FileStream file = new FileStream(FilePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(file))
                        {
                            foreach (Phrase phrase in phrases)
                            {
                                writer.WriteLine($"{phrase.Question};{phrase.Reaction};{phrase.GuildId};{phrase.RoleId}");
                            }
                        }
                    }
                    if (linesCount > phrases.Count)
                        return true;
                }
                return false;
            }
        }
    }
}
