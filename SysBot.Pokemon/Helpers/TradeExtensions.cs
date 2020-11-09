using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon
{
    public class TradeExtensions
    {
        public PokeTradeHub<PK8> Hub;
        public static int XCoordStart = 0;
        public static int YCoordStart = 0;
        public static List<string> TradeCordPath = new List<string>();
        public static List<string> TradeCordCooldown = new List<string>();
        public static PKM TradeCordPKM = AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet("Zigzagoon")), out _);
        public static byte[] Data = TradeCordPKM.Data;

        public TradeExtensions(PokeTradeHub<PK8> hub)
        {
            Hub = hub;
        }

        public static uint AlcremieDecoration { get => BitConverter.ToUInt32(Data, 0xE4); set => BitConverter.GetBytes(value).CopyTo(Data, 0xE4); }

        public static int[] ValidEgg =
                { 1, 4, 7, 10, 27, 29, 32, 37, 41, 43, 50, 52, 54, 58, 60, 63, 66, 72,
                  77, 79, 81, 83, 90, 92, 95, 98, 102, 104, 108, 109, 111, 114, 115, 116,
                  118, 120, 122, 123, 127, 128, 129, 131, 133, 137, 138, 140, 142, 147, 163,
                  170, 172, 173, 174, 175, 177, 194, 206, 211, 213, 214, 215, 220, 222, 223,
                  225, 227, 236, 238, 239, 240, 241, 246, 252, 255, 258, 263, 270, 273, 278,
                  280, 290, 293, 298, 302, 303, 304, 309, 318, 320, 324, 328, 333, 337, 338,
                  339, 341, 343, 345, 347, 349, 355, 359, 360, 361, 363, 369, 371, 374, 403,
                  406, 408, 410, 415, 420, 422, 425, 427, 434, 436, 438, 439, 440, 442, 443,
                  446, 447, 449, 451, 453, 458, 459, 479, 506, 509, 517, 519, 524, 527, 529,
                  531, 532, 535, 538, 539, 543, 546, 548, 550, 551, 554, 556, 557, 559, 561,
                  562, 564, 566, 568, 570, 572, 574, 577, 582, 587, 588, 590, 592, 595, 597,
                  599, 605, 607, 610, 613, 615, 616, 618, 619, 621, 622, 624, 626, 627, 629,
                  631, 632, 633, 636, 659, 661, 674, 677, 679, 682, 684, 686, 688, 690, 692,
                  694, 696, 698, 701, 702, 703, 704, 707, 708, 710, 712, 714, 722, 725, 728,
                  736, 742, 744, 746, 747, 749, 751, 753, 755, 757, 759, 761, 764, 765, 766,
                  767, 769, 771, 776, 777, 778, 780, 781, 782, 810, 813, 816, 819, 821, 824,
                  827, 829, 831, 833, 835, 837, 840, 843, 845, 846, 848, 850, 852, 854, 856,
                  859, 868, 870, 871, 872, 874, 875, 876, 877, 878, 884, 885 };

        public static int[] GalarDex =
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 27, 28, 29, 30, 31, 32, 33, 34, 37, 38, 41, 42, 43, 44, 45, 50, 51, 52, 53, 54, 55, 58, 59, 60, 61, 62,
                  63, 64, 65, 66, 67, 68, 72, 73, 77, 78, 79, 80, 81, 82, 83, 90, 91, 92, 93, 94, 95, 98, 99, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
                  112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140,
                  141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 163, 164, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 182, 183, 184, 185, 186, 194,
                  195, 196, 197, 199, 202, 206, 208, 211, 212, 213, 214, 215, 220, 221, 222, 223, 224, 225, 226, 227, 230, 233, 236, 237, 238, 239, 240, 241, 242,
                  243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 263, 264, 270, 271, 272, 273, 274, 275, 278, 279, 280,
                  281, 282, 290, 291, 292, 293, 294, 295, 298, 302, 303, 304, 305, 306, 309, 310, 315, 318, 319, 320, 321, 324, 328, 329, 330, 333, 334, 345, 346,
                  347, 348, 337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 350, 355, 356, 359, 360, 361, 362, 363, 364, 365, 369, 371, 372, 373,
                  374, 375, 376, 377, 378, 379, 380, 381, 382, 383, 384, 385, 403, 404, 405, 406, 407, 415, 416, 420, 421, 422, 423, 425, 426, 427, 428, 434, 435,
                  436, 437, 438, 439, 440, 442, 443, 444, 445, 446, 447, 448, 449, 450, 451, 452, 453, 454, 458, 459, 460, 461, 462, 463, 464, 465, 466, 467, 468,
                  470, 471, 473, 474, 475, 477, 478, 479, 480, 481, 482, 483, 484, 485, 486, 487, 488, 494, 506, 507, 508, 509, 520, 517, 518, 519, 520, 521, 524,
                  525, 526, 527, 528, 529, 530, 531, 532, 533, 534, 535, 536, 537, 538, 539, 543, 544, 545, 546, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556,
                  557, 558, 559, 560, 561, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 582, 583, 584, 587, 588, 589,
                  590, 591, 592, 593, 595, 596, 597, 598, 599, 600, 601, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 618, 619, 620, 621, 622,
                  623, 624, 625, 626, 627, 628, 629, 630, 631, 632, 633, 634, 635, 636, 637, 638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 649, 659, 660, 661,
                  662, 663, 674, 675, 677, 678, 679, 680, 681, 682, 683, 684, 685, 686, 687, 688, 689, 690, 691, 692, 693, 694, 695, 696, 697, 698, 699, 700, 701,
                  702, 703, 704, 705, 706, 707, 708, 709, 710, 711, 712, 713, 714, 715, 716, 717, 718, 719, 721, 722, 723, 724, 725, 726, 727, 728, 729, 730, 736,
                  737, 738, 742, 743, 744, 745, 746, 747, 748, 749, 750, 751, 752, 753, 754, 755, 756, 757, 758, 759, 760, 761, 762, 763, 764, 765, 766, 767, 768,
                  769, 770, 771, 772, 773, 776, 777, 778, 780, 781, 782, 783, 784, 785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795, 796, 797, 798, 799, 800,
                  801, 802, 803, 804, 805, 806, 807, 808, 809, 810, 811, 812, 813, 814, 815, 816, 817, 818, 819, 820, 821, 822, 823, 824, 825, 826, 827, 828, 829,
                  830, 831, 832, 833, 834, 835, 836, 837, 838, 839, 840, 841, 842, 843, 844, 845, 846, 847, 848, 849, 850, 851, 852, 853, 854, 855, 856, 857, 858,
                  859, 860, 861, 862, 863, 864, 865, 866, 867, 868, 870, 871, 872, 873, 874, 875, 876, 877, 878, 879, 880, 881, 882, 883, 884, 885, 886, 887, 888,
                  889, 890, 891, 892, 893, 894, 895, 896, 897, 898 };

        public static int[] ShinyLock = { (int)Species.Victini, (int)Species.Keldeo, (int)Species.Volcanion, (int)Species.Cosmog, (int)Species.Cosmoem, (int)Species.Magearna, 
                                          (int)Species.Marshadow, (int)Species.Zacian, (int)Species.Zamazenta, (int)Species.Eternatus, (int)Species.Kubfu, (int)Species.Urshifu,
                                          (int)Species.Zarude, (int)Species.Glastrier, (int)Species.Spectrier, (int)Species.Calyrex };

        public static int[] GenderDependent = { 3, 12, 19, 20, 25, 26, 41, 42, 44, 45, 64, 65, 84, 85, 97, 111, 112, 118, 119, 123, 129, 130, 133,
                                                178, 185, 186, 194, 195, 202, 208, 212, 214, 215, 221, 224,
                                                255, 256, 257, 272, 274, 275, 315, 350, 369,
                                                403, 404, 405, 407, 415, 443, 444, 445, 449, 450, 453, 454, 459, 460, 461, 464, 465, 473,
                                                521, 592, 593,
                                                668 };

        public static int[] Legends = { 144, 145, 146, 150, 151, 243, 244, 245, 249, 250, 251, 377, 378, 379, 380, 381,
                                        382, 383, 384, 385, 480, 481, 482, 483, 484, 485, 486, 487, 488, 494, 638, 639,
                                        640, 641, 642, 643, 644, 645, 646, 647, 649, 716, 717, 718, 719, 721, 772, 773,
                                        785, 786, 787, 788, 789, 790, 791, 792, 800, 801, 802, 807, 808, 809, 888, 889,
                                        890, 891, 892, 893, 894, 895, 896, 897, 898 };

        public static int[] UBs = { 793, 794, 795, 796, 797, 798, 799, 803, 804, 805, 806 };

        public static int[] GalarFossils = { 880, 881, 882, 883 };

        public static int[] SilvallyMemory = { 0, 904, 905, 906, 907, 908, 909, 910, 911, 912, 913, 914, 915, 916, 917, 918, 919, 920 };

        public static int[] GenesectDrives = { 0, 116, 117, 118, 119 };

        public static int[] Amped = { (int)Nature.Adamant, (int)Nature.Brave, (int)Nature.Docile, (int)Nature.Hardy, (int)Nature.Hasty, (int)Nature.Impish, (int)Nature.Jolly,
                                      (int)Nature.Lax, (int)Nature.Naive, (int)Nature.Naughty, (int)Nature.Rash, (int)Nature.Quirky, (int)Nature.Sassy };

        public static int[] LowKey = { (int)Nature.Bashful, (int)Nature.Bold, (int)Nature.Calm, (int)Nature.Careful, (int)Nature.Gentle, (int)Nature.Lonely,
                                       (int)Nature.Mild, (int)Nature.Modest, (int)Nature.Quiet, (int)Nature.Relaxed, (int)Nature.Serious, (int)Nature.Timid };

        public static void RngRoutine(PKM pkm, bool canGmax, uint alcremieDeco)
        {
            var rng = new Random();
            var troublesomeAltForms = pkm.Species == (int)Species.Articuno || pkm.Species == (int)Species.Zapdos || pkm.Species == (int)Species.Moltres || pkm.Species == (int)Species.Raichu ||
                                      pkm.Species == (int)Species.Marowak || pkm.Species == (int)Species.Giratina || pkm.Species == (int)Species.Silvally || pkm.Species == (int)Species.Genesect;

            if (pkm.PersonalInfo.HasFormes)
            {
                pkm.AltForm = (pkm.FatefulEncounter && pkm.Species != (int)Species.Genesect) || (canGmax && (pkm.Species == (int)Species.Meowth)) ||
                    pkm.Species == (int)Species.Indeedee || pkm.Species == (int)Species.Sinistea || pkm.Species == (int)Species.Polteageist || pkm.Species == (int)Species.Pikachu || 
                    pkm.Species == (int)Species.Exeggutor || pkm.Species == (int)Species.Meowstic ?
                    pkm.AltForm : rng.Next(0, pkm.PersonalInfo.FormeCount);

                if (AltFormInfo.IsBattleOnlyForm(pkm.Species, pkm.AltForm, pkm.Format))
                    pkm.AltForm = AltFormInfo.GetOutOfBattleForm(pkm.Species, pkm.AltForm, pkm.Format);
                else if (AltFormInfo.IsFusedForm(pkm.Species, pkm.AltForm, pkm.Format))
                    pkm.AltForm = 0;

                if (pkm.Species == (int)Species.Alcremie)
                {
                    Data = pkm.Data;
                    AlcremieDecoration = alcremieDeco;
                    var tempPkm = PKMConverter.GetPKMfromBytes(Data);
                    pkm = tempPkm ?? pkm;
                }
            }

            if (pkm.AltForm > 0 && troublesomeAltForms)
            {
                switch (pkm.Species)
                {
                    case 26: pkm.Met_Location = 162; pkm.Met_Level = 25; pkm.EggMetDate = null; pkm.Egg_Day = 0; pkm.Egg_Location = 0; pkm.Egg_Month = 0; pkm.Egg_Year = 0; pkm.EncounterType = 0; break;
                    case 105: pkm.Met_Location = 244; pkm.Met_Level = 65; pkm.EggMetDate = null; pkm.Egg_Day = 0; pkm.Egg_Location = 0; pkm.Egg_Month = 0; pkm.Egg_Year = 0; pkm.EncounterType = 0; break;
                    case 144: pkm.Met_Location = 208; pkm.SetIsShiny(false); break;
                    case 145: pkm.Met_Location = 122; pkm.SetIsShiny(false); break;
                    case 146: pkm.Met_Location = 164; pkm.SetIsShiny(false); break;
                    case 487: pkm.HeldItem = 112; break;
                    case 649: pkm.HeldItem = GenesectDrives[pkm.AltForm]; break;
                    case 773: pkm.HeldItem = SilvallyMemory[pkm.AltForm]; break;
                };
            }

            if (pkm.IsShiny && pkm.Met_Location == 244)
                CommonEdits.SetShiny(pkm, Shiny.AlwaysStar);

            pkm.Nature = pkm.FatefulEncounter ? pkm.Nature : 
                pkm.Species == (int)Species.Toxtricity && pkm.AltForm > 0 ? LowKey[rng.Next(0, LowKey.Length)] : 
                pkm.Species == (int)Species.Toxtricity && pkm.AltForm == 0 ? Amped[rng.Next(0, Amped.Length)] : rng.Next(0, 24);
            pkm.StatNature = pkm.Nature;
            pkm.MetDate = DateTime.Parse("2020/10/20");
            _ = pkm.FatefulEncounter || (pkm.Species == (int)Species.Exeggutor && pkm.AltForm == 1) ? pkm.Ball : BallApplicator.ApplyBallLegalRandom(pkm);
            pkm.IVs = pkm.FatefulEncounter ? pkm.IVs : pkm.SetRandomIVs(5);
            pkm.ClearHyperTraining();
            pkm.SetAbilityIndex(Legends.Contains(pkm.Species) || UBs.Contains(pkm.Species) || pkm.FatefulEncounter ? 0 : pkm.Met_Location == 244 || GalarFossils.Contains(pkm.Species) ? 2 : rng.Next(0, 2));
            pkm.SetSuggestedMoves(false);
            pkm.RelearnMoves = (int[])pkm.GetSuggestedRelearnMoves();
            pkm.SetMaximumPPUps(pkm.Moves);
            pkm.FixMoves();

            if (pkm.Met_Location == 244 && (pkm.Species == (int)Species.Poipole || pkm.Species == (int)Species.Naganadel))
                pkm.Ball = (int)Ball.Beast;

            if ((!pkm.FatefulEncounter && pkm.Ball == 16) || (pkm.WasBredEgg && (pkm.Ball == 1 || pkm.Ball == 16)))
                BallApplicator.ApplyBallLegalRandom(pkm);
        }

        public static PKM EggRngRoutine(List<string> content, string trainerInfo)
        {
            var rng = new Random();
            var ball1 = int.Parse(content[1].Split('_')[content[1].Contains("★") ? 3 : 2].Trim());
            var ball2 = int.Parse(content[2].Split('_')[content[2].Contains("★") ? 3 : 2].Trim());
            var species1 = int.Parse(content[1].Split('_')[content[1].Contains("★") ? 2 : 1].Trim());
            var species2 = int.Parse(content[2].Split('_')[content[2].Contains("★") ? 2 : 1].Trim());
            bool specificEgg = (species1 == species2 && ValidEgg.Contains(species1)) || ((species1 == 132 || species2 == 132) && (ValidEgg.Contains(species1) || ValidEgg.Contains(species2)));
            var shinyRng = rng.Next(content[1].Contains("★") && content[2].Contains("★") ? 15 : 0, 150);
            var ballRng = rng.Next(1, 2);
            var speciesRng = specificEgg ? SpeciesName.GetSpeciesNameGeneration(species1 == 132 && species2 != 132 ? species2 : species1, 2, 8) : SpeciesName.GetSpeciesNameGeneration((int)ValidEgg.GetValue(rng.Next(0, ValidEgg.Length - 1)), 2, 8);

            if (speciesRng.Contains("Nidoran"))
                speciesRng = speciesRng.Remove(speciesRng.Length - 1);

            var genderHelper = speciesRng == "Indeedee" || speciesRng == "Nidoran" ? rng.Next(1, 3).ToString() : "";

            var set = new ShowdownSet($"Egg({speciesRng}{(genderHelper == "1" ? "-M" : "-F")}){trainerInfo}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo(8);
            var pkm = sav.GetLegal(template, out _);

            if (pkm.PersonalInfo.HasFormes && pkm.Species != (int)Species.Sinistea && pkm.Species != (int)Species.Indeedee)
                pkm.AltForm = rng.Next(0, pkm.PersonalInfo.FormeCount);

            if (pkm.Species == (int)Species.Rotom)
                pkm.AltForm = 0;

            if (AltFormInfo.IsBattleOnlyForm(pkm.Species, pkm.AltForm, pkm.Format))
                pkm.AltForm = AltFormInfo.GetOutOfBattleForm(pkm.Species, pkm.AltForm, pkm.Format);

            var legalBalls = BallApplicator.GetLegalBalls(pkm);
            if (ballRng == 1 && legalBalls.Contains((Ball)ball1))
                pkm.Ball = ball1;
            else if (ballRng == 2 && legalBalls.Contains((Ball)ball2))
                pkm.Ball = ball2;

            EggTrade((PK8)pkm);
            pkm.SetAbilityIndex(rng.Next(0, 2));
            pkm.Nature = rng.Next(0, 24);
            pkm.StatNature = pkm.Nature;
            pkm.IVs = pkm.SetRandomIVs(4);
            pkm.ClearHyperTraining();

            if (!pkm.ValidBall())
                BallApplicator.ApplyBallLegalRandom(pkm);

            if (shinyRng > 148)
                CommonEdits.SetShiny(pkm, Shiny.AlwaysSquare);
            else if (shinyRng > 145)
                CommonEdits.SetShiny(pkm, Shiny.AlwaysStar);

            return pkm;
        }

        public bool IsItemMule(PK8 pk8)
        {
            if (Hub.Config.Trade.ItemMuleSpecies == Species.None || Hub.Config.Trade.DittoTrade && pk8.Species == 132 || Hub.Config.Trade.EggTrade && pk8.Nickname == "Egg")
                return true;
            return !(pk8.Species != SpeciesName.GetSpeciesID(Hub.Config.Trade.ItemMuleSpecies.ToString()) || pk8.IsShiny);
        }

        public static void DittoTrade(PKM pk8)
        {
            if (pk8.IsNicknamed == false)
                return;

            var dittoStats = new string[] { "ATK", "SPE", "SPA" };
            pk8.MetDate = DateTime.Parse("2020/10/20");
            pk8.StatNature = pk8.Nature;
            pk8.SetAbility(7);
            pk8.SetAbilityIndex(1);
            pk8.Met_Level = 60;
            pk8.Move1 = 144;
            pk8.Move1_PP = 0;
            pk8.Met_Location = 154;
            pk8.Ball = 21;
            pk8.IVs = new int[] { 31, pk8.Nickname.Contains(dittoStats[0]) ? 0 : 31, 31, pk8.Nickname.Contains(dittoStats[1]) ? 0 : 31, pk8.Nickname.Contains(dittoStats[2]) ? 0 : 31, 31 };
            pk8.ClearNickname();
            pk8.SetSuggestedHyperTrainingData();
        }

        public static void EggTrade(PK8 pk8)
        {
            pk8.IsEgg = true;
            pk8.Egg_Location = 60002;
            pk8.EggMetDate = pk8.MetDate = DateTime.Parse("2020/10/20");
            pk8.HeldItem = 0;
            pk8.CurrentLevel = 1;
            pk8.EXP = 0;
            pk8.DynamaxLevel = 0;
            pk8.Met_Level = 1;
            pk8.Met_Location = 0;
            pk8.CurrentHandler = 0;
            pk8.OT_Friendship = 1;
            pk8.HT_Name = "";
            pk8.HT_Friendship = 0;
            pk8.HT_Language = 0;
            pk8.HT_Gender = 0;
            pk8.HT_Memory = 0;
            pk8.HT_Feeling = 0;
            pk8.HT_Intensity = 0;
            pk8.StatNature = pk8.Nature;
            pk8.EVs = new int[] { 0, 0, 0, 0, 0, 0 };
            pk8.Markings = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            pk8.ClearRecordFlags();
            pk8.ClearRelearnMoves();
            pk8.Moves = new int[] { 0, 0, 0, 0 };
            var moveLa = new LegalityAnalysis(pk8);
            pk8.SetRelearnMoves(MoveSetApplicator.GetSuggestedRelearnMoves(moveLa));
            pk8.Moves = pk8.RelearnMoves;
            pk8.Move1_PPUps = pk8.Move2_PPUps = pk8.Move3_PPUps = pk8.Move4_PPUps = 0;
            pk8.SetMaximumPPCurrent(pk8.Moves);
            pk8.SetSuggestedHyperTrainingData();
            pk8.SetSuggestedRibbons();
        }
    }
}
