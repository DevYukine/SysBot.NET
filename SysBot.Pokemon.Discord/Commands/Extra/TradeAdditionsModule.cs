using Discord;
using Discord.Commands;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Generates and queues various silly trade additions")]
    public class TradeAdditionsModule : ModuleBase<SocketCommandContext>
    {
        private static TradeQueueInfo<PK8> Info => SysCordInstance.Self.Hub.Queues.Info;

        [Command("fixOT")]
        [Alias("fix", "f")]
        [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
        [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
        public async Task FixAdOT()
        {
            var code = Info.GetRandomTradeCode();
            var sig = Context.User.GetFavor();
            await Context.AddToQueueAsync(code, Context.User.Username, sig, new PK8(), PokeRoutineType.FixOT, PokeTradeType.FixOT).ConfigureAwait(false);
        }

        [Command("fixOT")]
        [Alias("fix", "f")]
        [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
        [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
        public async Task FixAdOT([Summary("Trade Code")] int code)
        {
            var sig = Context.User.GetFavor();
            await Context.AddToQueueAsync(code, Context.User.Username, sig, new PK8(), PokeRoutineType.FixOT, PokeTradeType.FixOT).ConfigureAwait(false);
        }

        [Command("fixOTList")]
        [Alias("fl", "fq")]
        [Summary("Prints the users in the FixOT queue.")]
        [RequireSudo]
        public async Task GetFixListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.FixOT);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordList")]
        [Alias("tcl", "tcq")]
        [Summary("Prints users in the TradeCord queue.")]
        [RequireSudo]
        public async Task GetTradeCordListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.TradeCord);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending TradeCord Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordCatch")]
        [Alias("k", "catch")]
        [Summary("Catch a random Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCord()
        {
            var user = Context.User.Id.ToString();
            TradeCordParanoiaChecks(Context);
            if (!Info.Hub.Config.Trade.TradeCordChannels.Contains(Context.Channel.Id.ToString()) && !Info.Hub.Config.Trade.TradeCordChannels.Equals(""))
            {
                await ReplyAsync($"You're typing the command in the wrong channel!").ConfigureAwait(false);
                return;
            }
            else if (!TradeCordCanCatch(user, out TimeSpan timeRemaining))
            {
                var embedTime = new EmbedBuilder { Color = Color.DarkBlue };
                embedTime.AddField(x =>
                {
                    x.Name = $"{Context.User.Username}, you're too quick!";
                    x.Value = $"Please try again in {Math.Ceiling((decimal)timeRemaining.Seconds):N0} {(_ = timeRemaining.Seconds > 1 ? "seconds" : "second")}!";
                    x.IsInline = false;
                });

                await Context.Message.Channel.SendMessageAsync(embed: embedTime.Build()).ConfigureAwait(false);
                return;
            }

            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            int.TryParse(content[0], out int catchID);
            var rng = new Random();
            var trainerInfo = string.Empty;
            string shinyType = string.Empty;
            string formHack = string.Empty;
            var formEdgeCaseRng = rng.Next(0, 2);
            var partnerPikachuHeadache = new string[] { "-Original", "-Partner", "-Hoenn", "-Sinnoh", "-Unova", "-Alola", "-Kalos", "-World" };
            var alcremieDeco = (uint)rng.Next(0, 6);
            var catchRng = rng.Next(0, 100);
            var shinyRng = rng.Next(0, 100);
            var gmaxRng = rng.Next(0, 100);
            bool canGmax = false;
            var speciesRng = (int)TradeExtensions.GalarDex.GetValue(rng.Next(0, TradeExtensions.GalarDex.Length - 1));
            var eggRng = rng.Next(0, 100);
            PKM eggPkm = new PK8();
            bool egg = false;
            bool femaleDependent = false;
            bool maleDependent = false;

            if (content[1] != "0" && content[2] != "0")
                egg = CanGenerateEgg(content) && eggRng > 60;

            if (content[3] != "0" && content[4] != "0" && content[5] != "0" && content[6] != "0")
                trainerInfo = $"\nOT: {content[3]}\nOTGender: {content[4]}\nTID: {content[5]}\nSID: {content[6]}";

            if (egg)
            {
                eggPkm = TradeExtensions.EggRngRoutine(content, trainerInfo);
                var laEgg = new LegalityAnalysis(eggPkm);
                var specEgg = GameInfo.Strings.Species[eggPkm.Species];
                var invalidEgg = !(eggPkm is PK8) || (!laEgg.Valid && SysCordInstance.Self.Hub.Config.Legality.VerifyLegality);
                if (invalidEgg)
                {
                    await Context.Channel.SendPKMAsync(eggPkm, $"Something went wrong!\n{ReusableActions.GetFormattedShowdownText(eggPkm)}").ConfigureAwait(false);
                    eggPkm = eggPkm.LegalizePokemon();
                    if (!new LegalityAnalysis(eggPkm).Valid)
                    {
                        await Context.Channel.SendMessageAsync($"Oops, I was unable to legalize the egg!").ConfigureAwait(false);
                        return;
                    }
                    else eggPkm.RefreshChecksum();
                }

                eggPkm.ResetPartyStats();
            }

            if (catchRng > 20)
            {
                if (speciesRng == (int)Species.Meowstic || speciesRng == (int)Species.Indeedee)
                    _ = formEdgeCaseRng == 1 ? formHack = "-M" : formHack = "-F";
                else if (speciesRng == (int)Species.NidoranF || speciesRng == (int)Species.NidoranM)
                    _ = speciesRng == (int)Species.NidoranF ? formHack = "-F" : formHack = "-M";
                else if (speciesRng == (int)Species.Sinistea || speciesRng == (int)Species.Polteageist)
                    _ = formEdgeCaseRng == 1 ? formHack = "" : formHack = "-Antique";
                else if (speciesRng == (int)Species.Magearna)
                    _ = formEdgeCaseRng == 1 ? formHack = "" : formHack = "-Original";
                else if (speciesRng == (int)Species.Pikachu)
                    _ = formEdgeCaseRng == 1 ? formHack = "" : formHack = partnerPikachuHeadache[rng.Next(0, partnerPikachuHeadache.Length - 1)];
                else if (speciesRng == (int)Species.Exeggutor)
                    _ = formEdgeCaseRng == 1 ? formHack = "" : formHack = "-Alola";

                if (TradeExtensions.ShinyLock.Contains(speciesRng) || (speciesRng == (int)Species.Pikachu && formHack != "-Partner" && formHack != ""))
                    shinyRng = 0;

                if (shinyRng > 98)
                    shinyType = "\nShiny: Square";
                else if (shinyRng > 95)
                    shinyType = "\nShiny: Star";

                if (speciesRng == (int)Species.Exeggutor && formHack == "-Alola")
                    shinyType = "\nShiny: Square";

                var set = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration(speciesRng, 2, 8)}{formHack}{shinyType}{trainerInfo}");
                canGmax = set.CanToggleGigantamax(set.Species, set.FormIndex) && gmaxRng > 70 && set.Species != (int)Species.Melmetal;
                if (canGmax)
                    set.CanGigantamax = true;

                var template = AutoLegalityWrapper.GetTemplate(set);
                var sav = AutoLegalityWrapper.GetTrainerInfo(8);
                var pkm = sav.GetLegal(template, out _);

                if (speciesRng == (int)Species.Exeggutor && pkm.AltForm == 1)
                    _ = shinyRng > 80 ? CommonEdits.SetShiny(pkm, Shiny.Random) : pkm.SetIsShiny(false);

                TradeExtensions.RngRoutine(pkm, canGmax, alcremieDeco);
                if (pkm.Species == (int)Species.Pikachu && pkm.AltForm == 0 && shinyRng > 95)
                    CommonEdits.SetShiny(pkm, Shiny.Random);

                if (TradeExtensions.GenderDependent.Contains(pkm.Species) && !set.CanGigantamax && pkm.AltForm == 0)
                {
                    if (pkm.Gender == 0)
                        maleDependent = true;
                    else femaleDependent = true;
                }

                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[pkm.Species];
                var invalid = !(pkm is PK8) || (!la.Valid && SysCordInstance.Self.Hub.Config.Legality.VerifyLegality);
                if (invalid)
                {
                    await Context.Channel.SendPKMAsync(pkm, $"Something went wrong!\n{ReusableActions.GetFormattedShowdownText(pkm)}").ConfigureAwait(false);
                    pkm = pkm.LegalizePokemon();
                    pkm.IsNicknamed = false;
                    if (!new LegalityAnalysis(pkm).Valid)
                    {
                        await Context.Channel.SendMessageAsync($"Oops, I was unable to legalize it!").ConfigureAwait(false);
                        return;
                    }
                    else pkm.RefreshChecksum();
                }

                catchID++;
                content[0] = catchID.ToString();
                File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));

                pkm.ResetPartyStats();
                TradeCordDump("TradeCord", user, pkm, out int index);
                var speciesName = SpeciesName.GetSpeciesNameGeneration(pkm.Species, 2, 8);
                var form = FormOutput(pkm);

                var pokeImg = $"https://projectpokemon.org/images/sprites-models/homeimg/poke_capture_" +
                    (pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : $"0{pkm.Species}") + (pkm.AltForm < 10 ? "_00" : "_0") +
                    pkm.AltForm + "_" + (pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : femaleDependent ? "fd" : maleDependent ? "md" : "mf") + "_" +
                    (canGmax ? "g" : "n") + "_0000000" + (pkm.Species == (int)Species.Alcremie ? alcremieDeco : 0) + "_f_" + (pkm.IsShiny ? "r" : "n") + ".png";

                var ballImg = $"https://serebii.net/itemdex/sprites/pgl/" + $"{(Ball)pkm.Ball}ball".ToLower() + ".png";
                var embed = new EmbedBuilder { Color = pkm.IsShiny && shinyType.Contains("Square") ? Color.Gold : pkm.IsShiny && shinyType.Contains("Star") ? Color.LightOrange : Color.Teal, ImageUrl = pokeImg, ThumbnailUrl = ballImg };
                embed.AddField(x =>
                {
                    x.Name = $"{Context.User.Username}'s Catch [#{catchID}]";
                    x.Value = $"You threw a {(Ball)pkm.Ball} Ball at a {(pkm.IsShiny ? "**shiny** wild **" + speciesName + form + "**" : "wild " + speciesName + form)}...";
                    x.IsInline = false;
                }).AddField(x =>
                {
                    x.Name = "\nResults";
                    x.Value = $"Success! It put up a fight, but you caught {(pkm.IsShiny ? "**" + speciesName + form + $" [ID: {index}]**" : speciesName + form + $" [ID: {index}]")}!";
                    x.IsInline = false;
                });

                if (egg)
                {
                    catchID++;
                    content[0] = catchID.ToString();
                    var eggSpeciesName = SpeciesName.GetSpeciesNameGeneration(eggPkm.Species, 2, 8);
                    var eggForm = FormOutput(eggPkm);
                    File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));
                    TradeCordDump("TradeCord", user, eggPkm, out int indexEgg);
                    embed.AddField(x =>
                    {
                        x.Name = "\nEggs";
                        x.Value = $"You got " + $"{(eggPkm.IsShiny ? "a **shiny egg**" : "an egg")}" +
                        $" from the daycare! Welcome, {(eggPkm.IsShiny ? "**" + eggSpeciesName + eggForm + $" [ID: {indexEgg}]**" : eggSpeciesName + eggForm + $" [ID: {indexEgg}]")}!";
                        x.IsInline = false;
                    });
                }

                TradeCordCooldown(user);
                await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var spookyRng = rng.Next(0, 100);
                var imgRng = rng.Next(1, 3);
                string imgGarf = "https://i.imgur.com/BOb6IbW.png";
                string imgConk = "https://i.imgur.com/oSUQhYv.png";
                var embedFail = new EmbedBuilder { Color = Color.Teal, ImageUrl = spookyRng > 90 && imgRng == 1 ? imgGarf : spookyRng > 90 && imgRng == 2 ? imgConk : string.Empty };
                var ball = (Ball)rng.Next(2, 26);
                embedFail.AddField(x =>
                {
                    x.Name = $"{Context.User.Username}'s Catch";
                    x.Value = $"You threw a {(ball == Ball.Cherish ? Ball.Poke : ball)} Ball at a wild {(spookyRng > 90 && imgRng != 3 ? "...whatever that thing is" : SpeciesName.GetSpeciesNameGeneration(speciesRng, 2, 8))}...";
                    x.IsInline = false;
                }).AddField(x =>
                {
                    x.Name = "Results";
                    x.Value = $"{(spookyRng > 90 && imgRng != 3 ? "One wiggle... Two... It breaks free and stares at you, smiling. You run for dear life." : "...but it managed to escape!")}";
                    x.IsInline = false;
                });

                if (egg)
                {
                    catchID++;
                    content[0] = catchID.ToString();
                    var eggSpeciesName = SpeciesName.GetSpeciesNameGeneration(eggPkm.Species, 2, 8);
                    var eggForm = FormOutput(eggPkm);
                    File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));
                    TradeCordDump("TradeCord", user, eggPkm, out int indexEgg);
                    embedFail.AddField(x =>
                    {
                        x.Name = "\nEggs";
                        x.Value = $"You got " + $"{(eggPkm.IsShiny ? "a **shiny egg**" : "an egg")}" +
                        $" from the daycare! Welcome, {(eggPkm.IsShiny ? "**" + eggSpeciesName + eggForm + $" [ID: {indexEgg}]**" : eggSpeciesName + eggForm + $" [ID: {indexEgg}]")}!";
                        x.IsInline = false;
                    });
                }

                TradeCordCooldown(user);
                await Context.Message.Channel.SendMessageAsync(embed: embedFail.Build()).ConfigureAwait(false);
                return;
            }
        }

        [Command("TradeCord")]
        [Alias("tc")]
        [Summary("Trade a caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeForTradeCord([Summary("Trade Code")] int code, [Summary("Numerical catch ID")] string id)
        {
            var user = Context.User.Id.ToString();
            TradeCordParanoiaChecks(Context);
            if (!Directory.Exists($"TradeCord\\Backup\\{user}"))
                Directory.CreateDirectory($"TradeCord\\Backup\\{user}");

            if (!int.TryParse(id, out _))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }

            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("There is no Pokémon with this ID.").ConfigureAwait(false);
                return;
            }

            var pkm = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(path[0]));
            if (pkm == null)
            {
                await Context.Message.Channel.SendMessageAsync("Oops, something happened when converting your Pokémon!").ConfigureAwait(false);
                return;
            }

            TradeExtensions.TradeCordPath.Add(path[0]);
            var sig = Context.User.GetFavor();
            await Context.AddToQueueAsync(code, Context.User.Username, sig, (PK8)pkm, PokeRoutineType.TradeCord, PokeTradeType.TradeCord).ConfigureAwait(false);
        }

        [Command("TradeCord")]
        [Alias("tc")]
        [Summary("Trade a caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeForTradeCord([Summary("Numerical catch ID")] string id)
        {
            var user = Context.User.Id.ToString();
            TradeCordParanoiaChecks(Context);
            if (!Directory.Exists($"TradeCord\\Backup\\{user}"))
                Directory.CreateDirectory($"TradeCord\\Backup\\{user}");

            if (!int.TryParse(id, out _))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }

            var code = Info.GetRandomTradeCode();
            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("There is no Pokémon with this ID.").ConfigureAwait(false);
                return;
            }

            var pkm = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(path[0]));
            if (pkm == null)
            {
                await Context.Message.Channel.SendMessageAsync("Oops, something happened when converting your Pokémon!").ConfigureAwait(false);
                return;
            }

            TradeExtensions.TradeCordPath.Add(path[0]);
            var sig = Context.User.GetFavor();
            await Context.AddToQueueAsync(code, Context.User.Username, sig, (PK8)pkm, PokeRoutineType.TradeCord, PokeTradeType.TradeCord).ConfigureAwait(false);
        }

        [Command("TradeCordCatchList")]
        [Alias("l", "list")]
        [Summary("List a user's Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task PokeList([Summary("Species name or catch ID of a Pokémon")] [Remainder] string name)
        {
            TradeCordParanoiaChecks(Context);
            name = name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1).ToLower();
            if (name.Contains('-'))
            {
                var split = name.Split('-');
                name = split[0] + "-" + split[1].Substring(0, 1).ToUpper() + split[1].Substring(1, split[1].Length - 1).ToLower();
            }

            if (name.Contains(' '))
            {
                var split = name.Split(' ');
                name = split[0] + " " + split[1].Substring(0, 1).ToUpper() + split[1].Substring(1, split[1].Length - 1).ToLower();
            }

            var species = SpeciesName.GetSpeciesID(name.Split('-')[0].Trim());
            if (species == -1 && !name.Contains("Nidoran") && !name.Contains("Egg") && !name.Contains("Shinies"))
            {
                await Context.Message.Channel.SendMessageAsync("Not a valid Pokémon").ConfigureAwait(false);
                return;
            }

            var user = Context.User.Id.ToString();
            var list = new List<string>();
            List<string> templist = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Contains(".pk8")).ToList();
            foreach (var line in templist)
            {
                var sanitize = line.Split('\\')[2].Trim();
                list.Add(sanitize.Remove(sanitize.IndexOf(".pk8")));
            }

            var countTemp = list.FindAll(x => x.Contains(name));
            var count = new List<string>();
            var countSh = new List<string>();
            var countShAll = new List<string>();
            foreach (var line in countTemp)
            {
                var sort = line.Split('-')[0].Trim();

                if (sort.Contains("★"))
                    countSh.Add(sort);
                count.Add(sort);
            }

            if (name == "Shinies")
            {
                foreach (var line in list)
                {
                    if (line.Contains("★"))
                        countShAll.Add(line.Split('-').Length > 2 ? line.Split('-')[1].Trim() + "-" + line.Split('-')[2].Trim() : line.Split('-')[1].Trim());
                }
            }

            var entry = string.Join(", ", name == "Shinies" ? countShAll.OrderBy(x => x.Substring(0, 1)) : count.OrderBy(x => x.Contains('★') ? int.Parse(x.Split('★')[1]) : int.Parse(x)));
            if (entry == "")
            {
                await Context.Message.Channel.SendMessageAsync("No results found.").ConfigureAwait(false);
                return;
            }

            var msg = $"{Context.User.Username}'s {(name != "Shinies" ? "List" : "Shiny Pokémon")} [Total: {(name == "Shinies" ? $"★{list.Count(x => x.Contains("★"))}" : $"{count.Count}, ★{countSh.Count}")}]";
            if (entry.Length > 1024)
                entry = entry.AsSpan().Slice(0, 1021).ToString() + "...";

            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            embed.AddField(x =>
            {
                x.Name = msg;
                x.Value = entry;
                x.IsInline = false;
            });

            if (entry.Length == 1024)
            {
                embed.AddField(x =>
                {
                    x.Name = "Too many results to display!";
                    x.Value = "Please consider trading or releasing some Pokémon.";
                    x.IsInline = false;
                });
            }

            await Context.Message.Channel.SendMessageAsync(embed : embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordInfo")]
        [Alias("i", "info")]
        [Summary("Displays details for a user's Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordInfo([Summary("Numerical catch ID")] string id)
        {
            TradeCordParanoiaChecks(Context);
            if (!int.TryParse(id, out _))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }

            var user = Context.User.Id.ToString();
            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Could not find this ID.").ConfigureAwait(false);
                return;
            }

            var pkm = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(path[0]));
            if (pkm == null)
            {
                await Context.Message.Channel.SendMessageAsync("Oops, something happened when converting your Pokémon!").ConfigureAwait(false);
                return;
            }

            bool canGmax = new ShowdownSet(ShowdownSet.GetShowdownText(pkm)).CanGigantamax;
            bool maleDependent = false;
            bool femaleDependent = false;
            if (TradeExtensions.GenderDependent.Contains(pkm.Species) && !canGmax && pkm.AltForm == 0)
            {
                if (pkm.Gender == 0)
                    maleDependent = true;
                else femaleDependent = true;
            }

            var pokeImg = $"https://projectpokemon.org/images/sprites-models/homeimg/poke_capture_" +
                (pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : $"0{pkm.Species}") + "_00" +
                pkm.AltForm + "_" + (pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : femaleDependent ? "fd" : maleDependent ? "md" : "mf") + "_" +
                (canGmax ? "g" : "n") + "_0000000" + (pkm.Species == (int)Species.Alcremie ? pkm.SpriteItem : 0) + "_f_" + (pkm.IsShiny ? "r" : "n") + ".png";

            var split = path[0].Split('\\')[2].Split('-');
            var embed = new EmbedBuilder { Color = pkm.IsShiny ? Color.Blue : Color.DarkBlue, ThumbnailUrl = pokeImg };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s {(pkm.IsShiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(pkm.Species, 2, 8)}{(pkm.AltForm > 0 ? "-" + split[2].Split('.')[0].Trim() : "")} [ID: {id}]";
                x.Value = $"\n{ReusableActions.GetFormattedShowdownText(pkm)}";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordMassRelease")]
        [Alias("mr", "massrelease")]
        [Summary("Mass releases every non-shiny and non-Ditto Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task MassRelease()
        {
            TradeCordParanoiaChecks(Context);
            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            var id1 = content[1].Split('_')[content[1].Contains("★") ? 1 : 0];
            var id2 = content[2].Split('_')[content[2].Contains("★") ? 1 : 0];
            List<string> favorites = new List<string>();
            if (content.Count > 7)
            {
                for (int i = 7; i < content.Count; i++)
                    favorites.Add(content[i]);
            }

            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => !x.Split('\\')[2].Contains("★") && !x.Split('\\')[2].Contains("Ditto") &&
            !x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id1) && !x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id2)).ToList();

            foreach (var fav in favorites)
            {
                var match = path.Find(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim() == fav.Replace("★", "").Trim());
                path.Remove(match);
            }

            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Cannot find any more non-shiny, non-Ditto, non-favorite Pokémon to release.").ConfigureAwait(false);
                return;
            }

            foreach (var line in path)
                File.Delete(line);

            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Mass Release";
                x.Value = $"Every non-shiny Pokémon was released, excluding Ditto and those in daycare.";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed : embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordRelease")]
        [Alias("r", "release")]
        [Summary("Releases a user's specific Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Release([Summary("Numerical catch ID")] string id)
        {
            TradeCordParanoiaChecks(Context);
            if (!int.TryParse(id, out _))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }

            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            List<string> favorites = new List<string>();
            if (content.Count > 7)
            {
                for (int i = 7; i < content.Count; i++)
                    favorites.Add(content[i]);
            }

            if (content[1] != "0" || content[2] != "0")
            {
                var shiny1 = content[1].Contains("★");
                var shiny2 = content[2].Contains("★");
                if (content[1].Split('_')[shiny1 ? 1 : 0].Equals(id) || content[2].Split('_')[shiny2 ? 1 : 0].Equals(id))
                {
                    await Context.Message.Channel.SendMessageAsync("Cannot release a Pokémon in daycare.").ConfigureAwait(false);
                    return;
                }
            }

            string? fav = favorites.Count > 0 ? favorites.Find(x => x.Replace("★", "").Trim().Equals(id)) : "";
            fav = fav != null ? fav.Replace("★", "").Trim() : fav;
            if (fav == id)
            {
                await Context.Message.Channel.SendMessageAsync("Cannot release a Pokémon added to favorites.").ConfigureAwait(false);
                return;
            }
            else if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Cannot find this Pokémon.").ConfigureAwait(false);
                return;
            }

            File.Delete(path[0]);
            var sanitize = path[0].Split('-').Length > 2 ? (path[0].Split('-')[1] + "-" + path[0].Split('-')[2].Replace(".pk8", "")).Trim() : path[0].Split('-')[1].Replace(".pk8", "").Trim();
            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Release";
                x.Value = $"You release your {(path[0].Contains("★") ? "★" + sanitize : sanitize)}.";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed : embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordDaycare")]
        [Alias("dc")]
        [Summary("Check what's inside the daycare.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task DaycareInfo()
        {
            var content = File.ReadAllText($"TradeCord\\{Context.User.Id}.txt").Split(',').ToList();
            if (content[1] == "0" || content[2] == "0")
            {
                await Context.Message.Channel.SendMessageAsync("You do not have anything in daycare.").ConfigureAwait(false);
                return;
            }

            var split1 = content[1].Split('_');
            var split2 = content[2].Split('_');
            var shiny1 = split1.Contains("★");
            var shiny2 = split2.Contains("★");
            var speciesID1 = int.Parse(split1[shiny1 ? 2 : 1]);
            var speciesID2 = int.Parse(split2[shiny2 ? 2 : 1]);
            var dcSpecies1 = $"[ID: {split1[shiny1 ? 1 : 0]}] {(shiny1 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(speciesID1, 2, 8)} ({(Ball)int.Parse(split1[shiny1 ? 3 : 2])})";
            var dcSpecies2 = $"[ID: {split2[shiny2 ? 1 : 0]}] {(shiny2 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(speciesID2, 2, 8)} ({(Ball)int.Parse(split2[shiny2 ? 3 : 2])})";

            var embedInfo = new EmbedBuilder { Color = Color.DarkBlue };
            embedInfo.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Daycare Info";
                x.Value = $"{dcSpecies1}\n{dcSpecies2}{(CanGenerateEgg(content) ? "\n\nThey seem to really like each other." : "\n\nThey don't really seem to be fond of each other. Make sure they're base evolution and can be eggs!")}";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embedInfo.Build()).ConfigureAwait(false);
            return;
        }

        [Command("TradeCordDaycare")]
        [Alias("dc")]
        [Summary("Adds (or removes) Pokémon to daycare.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Daycare([Summary("Action to do (withdraw, deposit)")] string action, [Summary("Catch ID or elaborate action (\"All\" if withdrawing")] string id)
        {
            bool numerical = int.TryParse(id, out _);
            TradeCordParanoiaChecks(Context);
            if (!numerical)
            {
                id = id.Substring(0, 1).ToUpper() + id.Substring(1, id.Length - 1);
                if (id != "All")
                {
                    await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                    return;
                }
            }

            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            if (action.ToLower() == "w")
            {
                if (content[1] == "0" && content[2] == "0")
                {
                    await Context.Message.Channel.SendMessageAsync("You do not have anything in daycare.").ConfigureAwait(false);
                    return;
                }

                bool shiny1 = content[1].Contains("★");
                bool shiny2 = content[2].Contains("★");
                var id1 = content[1] != "0" ? content[1].Split('_')[shiny1 ? 1 : 0].Trim() : "";
                var id2 = content[2] != "0" ? content[2].Split('_')[shiny2 ? 1 : 0].Trim() : "";
                List<string> species = new List<string>();
                if (action.ToLower() == "w" && id != "All")
                {
                    if (id1.Equals(id))
                        species.Add(content[1].Split('_')[shiny1 ? 2 : 1].Trim());
                    else if (id2.Equals(id))
                        species.Add(content[2].Split('_')[shiny2 ? 2 : 1].Trim());

                    for (int i = 1; i < 3; i++)
                        content[i] = content[i].Split('_')[content[i].Contains("★") ? 1 : 0].Trim().Equals(id) ? "0" : content[i];

                    File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));
                }
                else if (action.ToLower() == "w" && id == "All")
                {
                    if (content[1] != "0")
                        species.Add(content[1].Split('_')[shiny1 ? 2 : 1].Trim());
                    
                    if (content[2] != "0")
                        species.Add(content[2].Split('_')[shiny2 ? 2 : 1].Trim());

                    content[1] = content[2] = "0";
                    File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));
                }

                if (species.Count < 1)
                {
                    await Context.Message.Channel.SendMessageAsync(id == "All" ? "You do not have anything in daycare." : "You do not have that Pokémon in daycare.").ConfigureAwait(false);
                    return;
                }

                var species1 = SpeciesName.GetSpeciesNameGeneration(int.Parse(species[0]), 2, 8);
                var species2 = species.Count > 1 ? SpeciesName.GetSpeciesNameGeneration(int.Parse(species[1]), 2, 8) : "";
                var embedWithdraw = new EmbedBuilder { Color = Color.DarkBlue };
                embedWithdraw.AddField(x =>
                {
                    x.Name = $"{Context.User.Username}'s Daycare Withdraw";
                    x.Value = $"{(action.ToLower() == "w" && id != "All" ? $"You withdrew your [ID: {id}] {species1} from the daycare." : $"You withdrew your [ID: {(id1 != "" ? id1 : id2)}] {species1}{(species.Count > 1 ? $" and [ID: {id2}] {species2}" : "")} from the daycare.")}";
                    x.IsInline = false;
                });

                await Context.Message.Channel.SendMessageAsync(embed: embedWithdraw.Build()).ConfigureAwait(false);
                return;
            }
            else if (action.ToLower() == "d" || action.ToLower() == "deposit")
            {
                List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
                if (!path[0].Split('\\')[2].Contains(id))
                {
                    await Context.Message.Channel.SendMessageAsync("There is no Pokémon with this ID.").ConfigureAwait(false);
                    return;
                }
                else if (content[1] != "0" && content[2] != "0")
                {
                    await Context.Message.Channel.SendMessageAsync("Daycare full, please withdraw something first.").ConfigureAwait(false);
                    return;
                }

                var pkm = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(path[0]));
                if (pkm == null)
                {
                    await Context.Message.Channel.SendMessageAsync("Oops, something happened when converting your Pokémon!").ConfigureAwait(false);
                    return;
                }

                if (content[1] == "0" && content[2] != "0" && !content[2].Split('_')[1].Equals(id))
                    content[1] = (pkm.IsShiny ? "★_" : "") + id + "_" + pkm.Species + "_" + pkm.Ball;
                else if (content[2] == "0" && content[1] != "0" && !content[1].Split('_')[1].Equals(id))
                    content[2] = (pkm.IsShiny ? "★_" : "") + id + "_" + pkm.Species + "_" + pkm.Ball;
                else if (content[1] == "0" && content[2] == "0")
                    content[1] = (pkm.IsShiny ? "★_" : "") + id + "_" + pkm.Species + "_" + pkm.Ball;
                else
                {
                    await Context.Message.Channel.SendMessageAsync("You've already deposited that Pokémon to daycare.").ConfigureAwait(false);
                    return;
                }

                File.WriteAllText($"TradeCord\\{user}.txt", string.Join(",", content));
                var speciesStr = $"{SpeciesName.GetSpeciesNameGeneration(pkm.Species, 2, 8)}({(Ball)pkm.Ball})";
                var embedDeposit = new EmbedBuilder { Color = Color.DarkBlue };
                embedDeposit.AddField(x =>
                {
                    x.Name = $"{Context.User.Username}'s Daycare Deposit";
                    x.Value = $"Deposited your {(pkm.IsShiny ? "★" + speciesStr : speciesStr)} to daycare!";
                    x.IsInline = false;
                });

                await Context.Message.Channel.SendMessageAsync(embed: embedDeposit.Build()).ConfigureAwait(false);
                return;
            }
            else await Context.Message.Channel.SendMessageAsync("Unrecognized command.").ConfigureAwait(false);
        }

        [Command("TradeCordGift")]
        [Alias("gift", "g")]
        [Summary("Gifts a Pokémon to a mentioned user.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Gift([Summary("Numerical catch ID")] string id, [Summary("User mention")] string _)
        {
            TradeCordParanoiaChecks(Context);
            var user = Context.User.Id.ToString();
            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => !x.Contains("★") ? x.Split('\\')[2].Split('-')[0].Trim().Equals(id) : x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            if (!int.TryParse(id, out int _int))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Please mention a user you're gifting a Pokémon to.").ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.First().Id == Context.User.Id)
            {
                await Context.Message.Channel.SendMessageAsync("...Why?").ConfigureAwait(false);
                return;
            }

            var dir = Path.Combine("TradeCord", Context.Message.MentionedUsers.First().Id.ToString());
            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Cannot find this Pokémon.").ConfigureAwait(false);
                return;
            }
            else if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var split = path[0].Split('\\')[2].Split('-');
            var oldname = split[0].Replace("★", "").Trim().Substring(0, id.Length);
            var newIDparse = Directory.GetFiles(dir).Where(x => x.Contains(".pk8")).Select(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim()).ToArray();
            var newID = newIDparse.OrderBy(x => int.Parse(x)).ToArray();
            File.Move(path[0], $"{dir}\\{path[0].Split('\\')[2].Replace(oldname, Indexing(newID).ToString())}");

            var sanitize = split.Length > 2 ? (split[1] + "-" + split[2]).Split('.')[0].Trim() : split[1].Trim();
            var embed = new EmbedBuilder { Color = Color.Purple };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Gift";
                x.Value = $"You gifted your {(path[0].Contains("★") ? "★**" + sanitize.Split('.')[0] + "**" : sanitize.Split('.')[0])} to {Context.Message.MentionedUsers.First().Username}.";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordTrainerInfoSet")]
        [Alias("tis")]
        [Summary("Sets individual trainer info for caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TrainerInfoSet(string OT, string OTGender, [Summary("6 digit TID")] string TID, [Summary("4 digit SID")] string SID)
        {
            TradeCordParanoiaChecks(Context);
            var user = Context.User.Id.ToString();
            OT = OT.Trim();
            OTGender = (OTGender.Substring(0, 1).ToUpper() + OTGender.Substring(1, OTGender.Length - 1)).Trim();
            TID = TID.Trim();
            SID = SID.Trim();
            if (OT.Length > 12)
            {
                await Context.Message.Channel.SendMessageAsync("OT name too long, has to be 12 characters or fewer!").ConfigureAwait(false);
                return;
            }
            else if (OTGender != "Male" && OTGender != "Female")
            {
                await Context.Message.Channel.SendMessageAsync("Enter your in-game OT gender as \"Male\" or \"Female\"!").ConfigureAwait(false);
                return;
            }
            else if (!int.TryParse(TID, out _) || TID.Length != 6)
            {
                await Context.Message.Channel.SendMessageAsync("TID has to be 6 digits long!").ConfigureAwait(false);
                return;
            }
            else if (!int.TryParse(SID, out _) || SID.Length != 4)
            {
                await Context.Message.Channel.SendMessageAsync("SID has to be 4 digits long!").ConfigureAwait(false);
                return;
            }

            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            content[3] = OT;
            content[4] = OTGender;
            content[5] = TID;
            content[6] = SID;

            File.WriteAllText($"TradeCord\\{Context.User.Id}.txt", string.Join(",", content));
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Trainer Info";
                x.Value = $"\nYou've set your trainer info as the following: \n**OT:** {OT}\n**OT Gender:** {OTGender}\n**TID:** {TID}\n**SID:** {SID}";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordTrainerInfo")]
        [Alias("ti")]
        [Summary("Displays currently set trainer info.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TrainerInfo()
        {
            TradeCordParanoiaChecks(Context);
            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Trainer Info";
                x.Value = $"\n**OT:** {(content[3] == "0" ? "Not set." : content[3])}" +
                $"\n**OT Gender:** {(content[4] == "0" ? "Not set." : content[4])}" +
                $"\n**TID:** {(content[5] == "0" ? "Not set." : content[5])}" +
                $"\n**SID:** {(content[6] == "0" ? "Not set." : content[6])}";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordFavorites")]
        [Alias("fav")]
        [Summary("Display favorites list.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordFavorites()
        {
            TradeCordParanoiaChecks(Context);
            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            List<string> favorites = new List<string>();
            if (content.Count > 7)
            {
                for (int i = 7; i < content.Count; i++)
                    favorites.Add(content[i]);
            }
            else
            {
                await Context.Message.Channel.SendMessageAsync($"You don't have anything in favorites yet!").ConfigureAwait(false);
                return;
            }

            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            embed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}'s Favorites";
                x.Value = $"{string.Join(", ", favorites)}";
                x.IsInline = false;
            });

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordFavorites")]
        [Alias("fav")]
        [Summary("Add/Remove a Pokémon to a favorites list.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordFavorites([Summary("Catch ID")] string id)
        {
            TradeCordParanoiaChecks(Context);
            if (!int.TryParse(id, out int _int))
            {
                await Context.Message.Channel.SendMessageAsync("Please enter a numerical catch ID.").ConfigureAwait(false);
                return;
            }

            var user = Context.User.Id.ToString();
            var content = File.ReadAllText($"TradeCord\\{user}.txt").Split(',').ToList();
            List<string> path = Directory.GetFiles(Path.Combine("TradeCord", user)).Where(x => !x.Contains("★") ? x.Split('\\')[2].Split('-')[0].Trim().Equals(id) : x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim().Equals(id)).ToList();
            if (path.Count == 0)
            {
                await Context.Message.Channel.SendMessageAsync("Cannot find this Pokémon.").ConfigureAwait(false);
                return;
            }

            List<string> favorites = new List<string>();
            if (content.Count > 7)
            {
                for (int i = 7; i < content.Count; i++)
                    favorites.Add(content[i]);
            }

            string? fav = favorites.Count > 0 ? favorites.Find(x => x.Replace("★", "").Trim().Equals(id)) : "";
            fav = fav != null ? fav.Replace("★", "").Trim() : fav;
            var split = path[0].Split('\\')[2];
            var name = split.Split('-')[1].Replace(".pk8", "").Trim();
            if (fav != id)
            {
                content.Add(split.Split('.')[0].Split('-')[0].Trim());
                File.WriteAllText($"TradeCord\\{Context.User.Id}.txt", string.Join(",", content));
                await Context.Message.Channel.SendMessageAsync($"{Context.User.Username}, added your {(split.Contains("★") ? "★" + name : name)} to favorites!").ConfigureAwait(false);
                return;
            }
            else if (fav == id)
            {
                content.Remove(split.Split('.')[0].Split('-')[0].Trim());
                File.WriteAllText($"TradeCord\\{Context.User.Id}.txt", string.Join(",", content));
                await Context.Message.Channel.SendMessageAsync($"{Context.User.Username}, removed your {(split.Contains("★") ? "★" + name : name)} from favorites!").ConfigureAwait(false);
                return;
            }
        }

        private void TradeCordDump(string folder, string subfolder, PKM pkm, out int index)
        {
            var dir = Path.Combine(folder, subfolder);
            Directory.CreateDirectory(dir);
            var split = pkm.FileName.Split('-');
            var speciesName = SpeciesName.GetSpeciesNameGeneration(pkm.Species, 2, 8);
            var form = FormOutput(pkm);
            if (speciesName.Contains("Nidoran"))
            {
                speciesName = speciesName.Remove(speciesName.Length - 1);
                form = pkm.Species == (int)Species.NidoranF ? "-F" : "-M";
            }

            var array = Directory.GetFiles(dir).Where(x => x.Contains(".pk8")).Select(x => x.Split('\\')[2].Split('-')[0].Replace("★", "").Trim()).ToArray();
            array = array.OrderBy(x => int.Parse(x)).ToArray();
            index = Indexing(array);
            var newname = (pkm.IsShiny ? "★" + index.ToString() : index.ToString()) + " - " + speciesName + form  + $"{(pkm.IsEgg ? " (Egg)" : "")}" + ".pk8";
            var fn = Path.Combine(dir, Util.CleanFileName(newname));
            File.WriteAllBytes(fn, pkm.DecryptedPartyData);
        }

        private int Indexing(string[] array)
        {
            var i = 0;
            return array.Where(x => int.Parse(x) > 0).Distinct().OrderBy(x => int.Parse(x)).Any(x => int.Parse(x) != (i += 1)) ? i : i + 1;
        }

        private void TradeCordCooldown(string id)
        {
            if (Info.Hub.Config.Trade.TradeCordCooldown > 0)
            {
                var line = TradeExtensions.TradeCordCooldown.FirstOrDefault(z => z.Contains(id));
                if (line != null)
                    TradeExtensions.TradeCordCooldown.Remove(TradeExtensions.TradeCordCooldown.FirstOrDefault(z => z.Contains(id)));
                TradeExtensions.TradeCordCooldown.Add($"{id},{DateTime.Now}");
            }
        }

        private bool TradeCordCanCatch(string user, out TimeSpan timeRemaining)
        {
            if (Info.Hub.Config.Trade.TradeCordCooldown < 0)
                Info.Hub.Config.Trade.TradeCordCooldown = default;

            var line = TradeExtensions.TradeCordCooldown.FirstOrDefault(z => z.Contains(user));
            DateTime.TryParse(line != null ? line.Split(',')[1] : string.Empty, out DateTime time);
            var timer = time.AddSeconds(Info.Hub.Config.Trade.TradeCordCooldown);
            timeRemaining = timer - DateTime.Now;

            if (DateTime.Now < timer)
                return false;

            return true;
        }

        private void TradeCordParanoiaChecks(SocketCommandContext context)
        {
            var user = context.User.Id.ToString();
            if (!Directory.Exists("TradeCord") || !File.Exists($"TradeCord\\{user}.txt"))
            {
                Directory.CreateDirectory($"TradeCord\\{user}");
                File.Create($"TradeCord\\{user}.txt").Close();
                File.WriteAllText($"TradeCord\\{user}.txt", "0,0,0,0,0,0,0");
            }
        }

        private string FormOutput(PKM pkm)
        {
            var strings = GameInfo.GetStrings(LanguageID.English.GetLanguage2CharName());
            var formString = FormConverter.GetFormList(pkm.Species, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, 8);

            if (formString[pkm.AltForm] == "Normal" || formString[pkm.AltForm].Contains("-") || formString[pkm.AltForm] == "")
                formString[pkm.AltForm] = "";
            else formString[pkm.AltForm] = "-" + formString[pkm.AltForm];

            return formString[pkm.AltForm];
        }

        public static bool CanGenerateEgg(List<string> content)
        {
            var split1 = content[1].Split('_');
            var split2 = content[2].Split('_');
            var shiny1 = split1.Contains("★");
            var shiny2 = split2.Contains("★");
            var speciesID1 = int.Parse(split1[shiny1 ? 2 : 1]);
            var speciesID2 = int.Parse(split2[shiny2 ? 2 : 1]);

            if (speciesID1 == 132 && speciesID2 == 132)
                return true;
            else if (speciesID1 == speciesID2 && TradeExtensions.ValidEgg.Contains(speciesID1))
                return true;
            else if ((speciesID1 == 132 || speciesID2 == 132) && (TradeExtensions.ValidEgg.Contains(speciesID1) || TradeExtensions.ValidEgg.Contains(speciesID2)))
                return true;
            else return false;
        }
    }
}
