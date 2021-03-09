using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Resources.Datatypes
{
    public class Phrase
    {
        public string Question { get; set; }
        public string Reaction { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }

        public Phrase(string question, string reaction, ulong guildId, ulong roleId)
        {
            Question = question;
            Reaction = reaction;
            GuildId = guildId;
            RoleId = roleId;
        }
    }
}
