using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DiscordBot.Resources.Extensions
{
    public class UserSetupCollection
    {
        private Object FileLock = new Object();
        private readonly string FilePath = Directory.GetCurrentDirectory() + @"\Data\UserSetup.csv";

        public UserSetupCollection(string localFilePath)
        {
            FilePath = Directory.GetCurrentDirectory() + localFilePath;
        }

        public ulong GetGuildID(ulong userId)
        {
            lock (FileLock)
            {
                if (File.Exists(FilePath))
                {
                    using(FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using(StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while((line = reader.ReadLine()) != null)
                            {
                                string[] data = line.Split(';');
                                if (data[0] == userId.ToString())
                                    return ulong.Parse(data[1]);
                            }
                        }
                    }
                }
                return 0;
            }
        }

        public void SetGuildID(ulong userId, ulong guildId)
        {
            lock(FileLock)
            {
                if (GetGuildID(userId) == 0)
                {
                    using (FileStream file = new FileStream(FilePath, FileMode.Append))
                    {
                        using (StreamWriter writer = new StreamWriter(file))
                        {
                            writer.WriteLine($"{userId};{guildId}");
                        }
                    }
                }
                else
                {
                    List<string> lines = new List<string>();
                    using (FileStream file = new FileStream(FilePath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            while((line = reader.ReadLine()) != null)
                            {
                                string[] data = line.Split(';');
                                if (data[0] != userId.ToString())
                                    lines.Add(line);
                            }
                        }
                    }
                    using (FileStream file = new FileStream(FilePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(file))
                        {
                            foreach(string line in lines)
                            {
                                writer.WriteLine(line);
                            }
                            writer.WriteLine($"{userId};{guildId}");
                        }
                    }
                }
            }
        }

        public bool RemoveUserId(ulong userId)
        {
            if(File.Exists(FilePath))
            {
                int linesCount = 0;
                List<string> lines = new List<string>();
                using (FileStream file = new FileStream(FilePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            linesCount++;
                            string[] data = line.Split(';');
                            if (data[0] != userId.ToString())
                                lines.Add(line);
                        }
                    }
                }
                using (FileStream file = new FileStream(FilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        foreach (string line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                if (linesCount > lines.Count)
                    return true;
            }
            return false;
        }
    }
}
