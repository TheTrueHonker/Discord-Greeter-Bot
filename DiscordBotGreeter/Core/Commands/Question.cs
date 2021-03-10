using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Resources.Extensions;
using DiscordBot.Resources.Datatypes;

namespace DiscordBot.Core.Commands
{
    public class Question : ModuleBase<SocketCommandContext>
    {
        [Command("addQuestion"), Summary("Adds a question, which the bot will ask a new user")]
        public async Task AddQuestionCmd(string question, string emote, string roleId)
        {
            if (Program.questionWhitelistCollection.GetGuildID(Context.User.Id) == Context.Guild.Id)
            {
                ulong guildId = Context.Guild.Id;
                bool questionAdded = PhrasesCollection.SetRoleId(question, emote, guildId, ulong.Parse(roleId));
                if (questionAdded)
                    await Context.Channel.SendMessageAsync(":white_check_mark: Question added");
                else
                    await Context.Channel.SendMessageAsync(":x: Question already added");
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: YOU ARE NOT ALLOWED TO USE THIS COMMAND");
            }
        }

        [Command("removeQuestion"), Summary("Removes a question")]
        public async Task RemoveQuestionCmd(string question)
        {
            if (Program.questionWhitelistCollection.GetGuildID(Context.User.Id) == Context.Guild.Id)
            {
                if (PhrasesCollection.RemoveQuestion(question))
                    await Context.Channel.SendMessageAsync(":white_check_mark: Question deleted");
                else
                    await Context.Channel.SendMessageAsync(":x: Question not found");
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: YOU ARE NOT ALLOWED TO USE THIS COMMAND");
            }
        }

        [Command("whitelistUserQst"), Summary("Whitelists a user to use the question command")]
        public async Task WhitelistUserQst(ulong userId)
        {
            if(Context.Guild.GetUser(userId) != null)
            {
                if(Context.Guild.GetUser(Context.User.Id).GuildPermissions.Administrator || 
                    Program.questionWhitelistCollection.GetGuildID(userId) == Context.Guild.Id)
                {
                    Program.questionWhitelistCollection.SetGuildID(userId, Context.Guild.Id);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(":x: YOU ARE NOT ALLOWED TO USE THIS COMMAND");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: USER NOT FOUND");
            }
        }
    }
}
