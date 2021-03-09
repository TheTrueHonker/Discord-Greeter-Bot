using System.Collections.Generic;

namespace DiscordBot.Resources.Datatypes
{
    public class Settings
    {
        public string Token { get; set; }
        public ulong Owner { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
    }
}
