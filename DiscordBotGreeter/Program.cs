using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DiscordBot.Resources.Extensions;
using DiscordBot.Resources.Datatypes;

namespace DiscordBot
{
    public class Program
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private Object lockObj;
        private Settings settings;
        public static UserSetupCollection userSetupCollection;
        public static UserSetupCollection questionWhitelistCollection;


        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            lockObj = new Object();
            settings = LoadSettings.LoadSettingsJson();
            userSetupCollection = new UserSetupCollection(@"\Data\UserSetup.csv");
            questionWhitelistCollection = new UserSetupCollection(@"\Data\QuestionWhitelist.csv");
            Console.Title = settings.Name;

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            });

            client.Ready += Client_Ready;
            client.Log += Client_Log;
            client.UserJoined += Client_UserJoined;
            client.ReactionAdded += Client_ReactionAdded;

            client.MessageReceived += Client_MessageReceived;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            await client.SetGameAsync("I am very friendly");
        }

        private async Task Client_Log(LogMessage message)
        {
            lock (lockObj)
            {
                ConsoleColor bgSeverityColor;
                ConsoleColor severityColor;

                switch (message.Severity)
                {
                    case (LogSeverity.Critical):
                        bgSeverityColor = ConsoleColor.Black;
                        severityColor = ConsoleColor.DarkRed;
                        break;

                    case (LogSeverity.Debug):
                        bgSeverityColor = ConsoleColor.Magenta;
                        severityColor = ConsoleColor.Gray;
                        break;

                    case (LogSeverity.Error):
                        bgSeverityColor = ConsoleColor.Red;
                        severityColor = ConsoleColor.Black;
                        break;

                    case (LogSeverity.Info):
                        bgSeverityColor = ConsoleColor.White;
                        severityColor = ConsoleColor.Black;
                        break;

                    case (LogSeverity.Verbose):
                        bgSeverityColor = ConsoleColor.Blue;
                        severityColor = ConsoleColor.Gray;
                        break;

                    case (LogSeverity.Warning):
                        bgSeverityColor = ConsoleColor.Yellow;
                        severityColor = ConsoleColor.Black;
                        break;

                    default:
                        bgSeverityColor = ConsoleColor.Black;
                        severityColor = ConsoleColor.White;
                        break;

                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[" + DateTime.Now + "] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("(" + message.Source + ")\t");
                Console.BackgroundColor = bgSeverityColor;
                Console.ForegroundColor = severityColor;
                Console.Write(message.Severity);
                Console.ResetColor();
                if (message.Severity == LogSeverity.Verbose)
                    Console.Write(":  " + message.Message + "\n");
                else if (message.Severity == LogSeverity.Warning)
                    Console.Write(":  " + message.Message + "\n");
                else
                    Console.Write(":\t  " + message.Message + "\n");
            }
        }

        private async Task Client_UserJoined(SocketGuildUser user)
        {
            if (user.IsBot == false)
            {
                // Greet
                IDMChannel channel = await user.GetOrCreateDMChannelAsync();
                ulong guildId = user.Guild.Id;
                await channel.SendMessageAsync($"Sei gegrüßt {user.Username}! Willkommen auf unserem Server :grin:\n" +
                    $"Damit dir deine richtigen Server-Rechte zugeteilt werden können, müsstest du einige Fragen beantworten.\n" +
                    $"Dauert auch nicht lange, versprochen ;)");
                userSetupCollection.SetGuildID(user.Id, guildId);

                // Ask Questions saved for this guild
                foreach(string question in PhrasesCollection.GetAllQuestions(guildId))
                {
                    IUserMessage message = await channel.SendMessageAsync(question);
                    foreach(string emote in PhrasesCollection.GetAllReactions(question, guildId))
                    {
                        await message.AddReactionAsync(new Emoji(emote));
                    }
                }

                // End Questioning
                IUserMessage confirmEndMessage = await channel.SendMessageAsync(PhrasesCollection.ConfirmEndPhrasesQuestion);
                await confirmEndMessage.AddReactionAsync(new Emoji("✅"));
            }
        }

        private async Task Client_MessageReceived(SocketMessage messagePar)
        {
            SocketUserMessage message = (SocketUserMessage)messagePar;
            SocketCommandContext context = new SocketCommandContext(client, message);

            if (context.Message == null || context.Message.Content == "")
                return;
            if (context.User.IsBot)
                return;

            int argPos = 0;
            if (!(message.HasStringPrefix(settings.Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
                return;

            var result = await commands.ExecuteAsync(context, argPos, null);
            if(!result.IsSuccess)
            {
                lock(lockObj)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[" + DateTime.Now + "] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("(" + message.Source + ")\t");
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("msgError");
                    Console.ResetColor();
                    Console.Write(": " + context.Message.Content + " | " + result.ErrorReason + "\n");
                }
            }
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            IUser userConf = await channel.GetUserAsync(reaction.UserId);
            if (userConf.IsBot == false)
            {
                IMessage message = await channel.GetMessageAsync(messageCache.Id);
                ulong userId = reaction.UserId;

                // Ende Rollenzuweisung
                if (message.Content == PhrasesCollection.ConfirmEndPhrasesQuestion && reaction.Emote.Name == "✅")
                {
                    userSetupCollection.RemoveUserId(userId);
                    await channel.SendMessageAsync(":white_check_mark: Rollenzuweisung abgeschlossen");
                    return;
                }

                // Frage Rollenzuweisung
                ulong guildId;
                if ((guildId = userSetupCollection.GetGuildID(userId)) != 0)
                {
                    ulong roleId = PhrasesCollection.GetRoleId(message.Content, reaction.Emote.Name, guildId);
                    if (roleId != 0)
                    {
                        SocketGuild guild = client.GetGuild(guildId);
                        SocketGuildUser user = guild.GetUser(userId);
                        await user.AddRoleAsync(guild.GetRole(roleId));
                        return;
                    }
                }
            }
        }
    }
}
