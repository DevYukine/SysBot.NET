using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord
{
    [Summary("Queues new Link Code trades")]
    public class TradeModule : ModuleBase<SocketCommandContext>
    {
        private static TradeQueueInfo<PK8> Info => SysCordInstance.Self.Hub.Queues.Info;

        [Command("tradeList")]
        [Alias("tl")]
        [Summary("Prints the users in the trade queues.")]
        [RequireSudo]
        public async Task GetTradeListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the provided Pokémon file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsyncAttach([Summary("Trade Code")]int code)
        {
            var sudo = Context.User.GetIsSudo();

            var attachment = Context.Message.Attachments.FirstOrDefault();
            if (attachment == default)
            {
                await ReplyAsync("No attachment provided!").ConfigureAwait(false);
                return;
            }

            var att = await NetUtil.DownloadPKMAsync(attachment).ConfigureAwait(false);
            if (!att.Success || !(att.Data is PK8 pk8))
            {
                await ReplyAsync("No PK8 attachment provided!").ConfigureAwait(false);
                return;
            }

            await AddTradeToQueueAsync(code, Context.User.Username, pk8, sudo).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Trade Code")]int code, [Summary("Showdown Set")][Remainder]string content)
        {
            const int gen = 8;
            content = ReusableActions.StripCodeBlock(content);
            
            ushort? secretId = null;
            uint? trainerId = null;
            string? ot = null;
			
            var showdownRows = new List<string>();
            var invalidExtraRows = new List<string>();

            foreach (var row in content.Split('\n'))
            {
                var arr = row.Split(':');

                var val = arr.Length == 2 ? arr[1].Trim() : "";

                try
                {
                    if (row.Contains("Secret Id:"))
                        secretId = ushort.Parse(val);
                    else if (row.Contains("Trainer Id:"))
                        trainerId = uint.Parse(val);
                    else if (row.Contains("Trainer:")) 
                        ot = val;
                    else
                        showdownRows.Add(row);
                }
                catch (Exception)
                {
                    invalidExtraRows.Add(row);
                }
            }
            
            var set = new ShowdownSet(string.Join("\n", showdownRows));
            var template = AutoLegalityWrapper.GetTemplate(set);
            var invalidLines = set.InvalidLines
                .Concat(invalidExtraRows)
                .ToArray();
            if (invalidLines.Length != 0)
            {
                var msg = $"Unable to parse Showdown Set:\n{string.Join("\n", invalidLines)}";
                await ReplyAsync(msg).ConfigureAwait(false);
                return;
            }

            var sav = AutoLegalityWrapper.GetTrainerInfo(gen);

            var pkm = sav.GetLegal(template, out _);
            if (secretId != null) pkm.SID = (int) secretId;
            if (trainerId != null) pkm.TID = (int) trainerId;
            if (ot != null) pkm.OT_Name = ot;
            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[template.Species];
            var invalid = !(pkm is PK8) || (!la.Valid && SysCordInstance.Self.Hub.Config.Legality.VerifyLegality);
            if (invalid)
            {
                var imsg = $"Oops! I wasn't able to create something from that. Here's my best attempt for that {spec}!";
                await Context.Channel.SendPKMAsync(pkm, imsg).ConfigureAwait(false);
                return;
            }

            pkm.ResetPartyStats();
            var sudo = Context.User.GetIsSudo();
            await AddTradeToQueueAsync(code, Context.User.Username, (PK8) pkm, sudo).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Showdown Set")][Remainder]string content)
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsync(code, content).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the attached file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsyncAttach()
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsyncAttach(code).ConfigureAwait(false);
        }

        private async Task AddTradeToQueueAsync(int code, string trainerName, PK8 pk8, bool sudo)
        {
            if (!pk8.CanBeTraded())
            {
                await ReplyAsync("Provided Pokémon content is blocked from trading!").ConfigureAwait(false);
                return;
            }

            var la = new LegalityAnalysis(pk8);
            if (!la.Valid && SysCordInstance.Self.Hub.Config.Legality.VerifyLegality)
            {
                await ReplyAsync("PK8 attachment is not legal, and cannot be traded!").ConfigureAwait(false);
                return;
            }

            await Context.AddToQueueAsync(code, trainerName, sudo, pk8, PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }
    }
}
