using LinePutScript.Localization.WPF;
using System;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal class Speak {
		internal enum SayType {
			placeVeryHighWin, placeHighWin, placeMidWin, placeLowWin, placeVeryLowWin,
			placeVeryHighLose, placeHighLose, placeMidLose, placeLowLose, placeVeryLowLose,
		}
		internal readonly ClickText[] placeVeryHighWin;
		internal readonly ClickText[] placeHighWin;
		internal readonly ClickText[] placeMidWin;
		internal readonly ClickText[] placeLowWin;
		internal readonly ClickText[] placeVeryLowWin;

		internal readonly ClickText[] placeVeryHighLose;
		internal readonly ClickText[] placeHighLose;
		internal readonly ClickText[] placeMidLose;
		internal readonly ClickText[] placeLowLose;
		internal readonly ClickText[] placeVeryLowLose;

		internal static string GetRandomSpeakText(ClickText[] speakTexts) =>
			speakTexts.Length > 0 
				? speakTexts[new Random().Next(speakTexts.Length)].TranslateText 
				: null;

		/// <summary>
		/// 各个对话的随机权重，最大值为byte.MaxValue
		/// </summary>
		internal class SpeakRandomWeightOption {
			internal byte placeVeryHighWin = byte.MaxValue;
			internal byte placeHighWin = byte.MaxValue;
			internal byte placeMidWin = 200;
			internal byte placeLowWin = 125;
			internal byte placeVeryLowWin = 60;

			internal byte placeVeryHighLose = byte.MaxValue;
			internal byte placeHighLose = byte.MaxValue;
			internal byte placeMidLose = 200;
			internal byte placeLowLose = 125;
			internal byte placeVeryLowLose = 60;
		}

		/// <summary>
		/// 指定桌宠说指定类型的话
		/// </summary>
		/// <param name="srwo">随机权重</param>
		/// <param name="type">说话的类型</param>
		internal void DoSpeak(IMainWindow MW, SpeakRandomWeightOption srwo, SayType type) {
			void say(string text) {
				if (text != null)
					MW.Main.Say(text.Translate());
			}
			bool ranBack(byte weight) {
				Random ran = new();
				byte v = (byte)ran.Next(0, byte.MaxValue);
				return (weight > v);
			}
			switch (type) {
				case SayType.placeVeryHighWin:
					if(ranBack(srwo.placeVeryHighWin))
						say(GetRandomSpeakText(placeVeryHighWin));
					break;
				case SayType.placeHighWin:
					if (ranBack(srwo.placeHighWin))
						say(GetRandomSpeakText(placeHighWin));
					break;
				case SayType.placeMidWin:
					if (ranBack(srwo.placeMidWin))
						say(GetRandomSpeakText(placeMidWin));
					break;
				case SayType.placeLowWin:
					if (ranBack(srwo.placeLowWin))
						say(GetRandomSpeakText(placeLowWin));
					break;
				case SayType.placeVeryLowWin:
					if (ranBack(srwo.placeVeryLowWin))
						say(GetRandomSpeakText(placeVeryLowWin));
					break;

				case SayType.placeVeryHighLose:
					if (ranBack(srwo.placeVeryHighLose))
						say(GetRandomSpeakText(placeVeryHighLose));
					break;
				case SayType.placeHighLose:
					if (ranBack(srwo.placeHighLose))
						say(GetRandomSpeakText(placeHighLose));
					break;
				case SayType.placeMidLose:
					if (ranBack(srwo.placeMidLose))
						say(GetRandomSpeakText(placeMidLose));
					break;
				case SayType.placeLowLose:
					if (ranBack(srwo.placeLowLose))
						say(GetRandomSpeakText(placeLowLose));
					break;
				case SayType.placeVeryLowLose:
					if (ranBack(srwo.placeVeryLowLose))
						say(GetRandomSpeakText(placeVeryLowLose));
					break;
			}
		}
		/// <summary>
		/// 根据所花费的代币自动计算桌宠说话的类型并说话
		/// </summary>
		/// <param name="gtc">游戏代币数据</param>
		/// <param name="usedCoin">使用的代币数量数据</param>
		/// <param name="isWin">是否赢了</param>
		/// <param name="srwo">随机权重，如果留空或为null则使用默认值</param>
		internal void DoSpeak(IMainWindow MW, GameTokenCoin gtc, GameTokenCoin.CoinGroup usedCoin, bool isWin, SpeakRandomWeightOption srwo = null) {
			srwo ??= new();
			int level = MW.Core.Save.Level;
			ulong allCoin = gtc.GetCoinAmount(usedCoin.CoinType);
			double allMoney = MW.Core.Save.Money;
			//使用的代币值多少桌宠币
			double coinToMoney = gtc.GetExchangeNeedMoney(usedCoin.Value, usedCoin.CoinType);

			//使用的代币占代币总数的百分比
			float coinPerc = (float)usedCoin.Value / (float)(allCoin + usedCoin.Value);
			//使用的代币值的桌宠币与总桌宠币的百分比
			float moneyPerc = (float)coinToMoney / (float)(allMoney + coinToMoney);

			SayType st;
#pragma warning disable IDE0045
			if (coinPerc < moneyPerc) {//更小的值优先
				if(coinToMoney < level) {//如果小于等级，说明投注的代币价值对用户来说并不高
					if (coinPerc > 0.6666) {
						st = isWin 
							? SayType.placeMidWin 
							: SayType.placeMidLose;
					}
					else if (coinPerc > 0.3333) {
						st = isWin
							? SayType.placeLowWin
							: SayType.placeLowLose;
					}
					else {
						st = isWin
							? SayType.placeVeryLowWin
							: SayType.placeVeryLowLose;
					}
				}
				else {
					if (coinPerc > 0.8) {
						st = isWin
							? SayType.placeVeryHighWin
							: SayType.placeVeryHighLose;
					}
					else if (coinPerc > 0.6) {
						st = isWin
							? SayType.placeHighWin
							: SayType.placeHighLose;
					}
					else if (coinPerc > 0.4) {
						st = isWin
							? SayType.placeMidWin
							: SayType.placeMidLose;
					}
					else if (coinPerc > 0.2) {
						st = isWin
							? SayType.placeLowWin
							: SayType.placeLowLose;
					}
					else {
						st = isWin
							? SayType.placeVeryLowWin
							: SayType.placeVeryLowLose;
					}
				}
			}
			else {
				if (coinToMoney < level) {//如果小于等级，说明投注的代币价值对用户来说并不高
					if (moneyPerc > 0.6666) {
						st = isWin
							? SayType.placeMidWin
							: SayType.placeMidLose;
					}
					else if (moneyPerc > 0.3333) {
						st = isWin
							? SayType.placeLowWin
							: SayType.placeLowLose;
					}
					else {
						st = isWin
							? SayType.placeVeryLowWin
							: SayType.placeVeryLowLose;
					}
				}
				else {
					if (moneyPerc > 0.8) {
						st = isWin
							? SayType.placeVeryHighWin
							: SayType.placeVeryHighLose;
					}
					else if (moneyPerc > 0.6) {
						st = isWin
							? SayType.placeHighWin
							: SayType.placeHighLose;
					}
					else if (moneyPerc > 0.4) {
						st = isWin
							? SayType.placeMidWin
							: SayType.placeMidLose;
					}
					else if (moneyPerc > 0.2) {
						st = isWin
							? SayType.placeLowWin
							: SayType.placeLowLose;
					}
					else {
						st = isWin
							? SayType.placeVeryLowWin
							: SayType.placeVeryLowLose;
					}
				}
			}
#pragma warning restore IDE0045
			DoSpeak(MW, srwo, st);
		}
		/// <summary>
		/// 根据所花费的代币与赢得的代币自动计算桌宠说话的类型并说话
		/// </summary>
		/// <param name="gtc">游戏代币数据</param>
		/// <param name="usedCoin">使用的代币数量数据</param>
		/// <param name="wonCoin">赢得的代币数量数据</param>
		/// <param name="srwo">随机权重，如果留空或为null则使用默认值</param>
		internal void DoSpeak(IMainWindow MW, GameTokenCoin gtc, GameTokenCoin.CoinGroup usedCoin, GameTokenCoin.CoinGroup wonCoin, SpeakRandomWeightOption srwo = null) {
			srwo ??= new();
			int level = MW.Core.Save.Level;
			ulong allCoin = gtc.GetCoinAmount(usedCoin.CoinType);
			double allMoney = MW.Core.Save.Money;
			//使用的代币值多少桌宠币
			double coinToMoney = gtc.GetExchangeNeedMoney(usedCoin.Value, usedCoin.CoinType);

			//使用的代币占代币总数的百分比
			float coinPerc = (float)usedCoin.Value / (float)(allCoin + usedCoin.Value);
			//使用的代币值的桌宠币与总桌宠币的百分比
			float moneyPerc = (float)coinToMoney / (float)(allMoney + coinToMoney);

			float winCoinPerc = (float)wonCoin.Value / (float)usedCoin.Value;

			SayType st;
#pragma warning disable IDE0045
			if (coinPerc < moneyPerc) {//更小的值优先
				if (coinToMoney < level) {//如果小于等级，说明投注的代币价值对用户来说并不高
					if (coinPerc > 0.6666) {
						if (winCoinPerc >= 1.6) {
							st = SayType.placeMidWin;
						}
						else if(winCoinPerc >= 1.3) {
							st = SayType.placeLowWin;
						}
						else if(winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.6666) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.3333) {
							st = SayType.placeLowLose;
						}
						else {
							st = SayType.placeMidLose;
						}
					}
					else if (coinPerc > 0.3333) {
						if (winCoinPerc >= 2) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.5) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeVeryLowLose;
						}
						else {
							st = SayType.placeLowLose;
						}
					}
					else {
						if (winCoinPerc >= 3) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else {
							st = SayType.placeVeryLowLose;
						}
					}
				}
				else {
					if (coinPerc > 0.8) {
						if(winCoinPerc >= 1.8) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 1.6) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.4) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.8) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.6) {
							st = SayType.placeLowLose;
						}
						else if (winCoinPerc > 0.4) {
							st = SayType.placeMidLose;
						}
						else if (winCoinPerc > 0.2) {
							st = SayType.placeHighLose;
						}
						else {
							st = SayType.placeVeryHighLose;
						}
					}
					else if (coinPerc > 0.6) {
						if (winCoinPerc >= 2.2) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 1.9) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.6) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.3) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.75) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeLowLose;
						}
						else if (winCoinPerc > 0.25) {
							st = SayType.placeMidLose;
						}
						else {
							st = SayType.placeHighLose;
						}
					}
					else if (coinPerc > 0.4) {
						if (winCoinPerc >= 2.6) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 2.2) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.8) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.4) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.6666) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.3333) {
							st = SayType.placeLowLose;
						}
						else{
							st = SayType.placeMidLose;
						}
					}
					else if (coinPerc > 0.2) {
						if (winCoinPerc >= 3) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 2.5) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.5) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeVeryLowLose;
						}
						else {
							st = SayType.placeLowLose;
						}
					}
					else {
						if (winCoinPerc >= 5) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 4) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 3) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else {
							st = SayType.placeVeryLowLose;
						}
					}
				}
			}
			else {
				if (coinToMoney < level) {//如果小于等级，说明投注的代币价值对用户来说并不高
					if (moneyPerc > 0.6666) {
						if (winCoinPerc >= 1.6) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.3) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.6666) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.3333) {
							st = SayType.placeLowLose;
						}
						else {
							st = SayType.placeMidLose;
						}
					}
					else if (moneyPerc > 0.3333) {
						if (winCoinPerc >= 2) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.5) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeVeryLowLose;
						}
						else {
							st = SayType.placeLowLose;
						}
					}
					else {
						if (winCoinPerc >= 3) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else {
							st = SayType.placeVeryLowLose;
						}
					}
				}
				else {
					if (moneyPerc > 0.8) {
						if (winCoinPerc >= 1.8) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 1.6) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.4) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.8) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.6) {
							st = SayType.placeLowLose;
						}
						else if (winCoinPerc > 0.4) {
							st = SayType.placeMidLose;
						}
						else if (winCoinPerc > 0.2) {
							st = SayType.placeHighLose;
						}
						else {
							st = SayType.placeVeryHighLose;
						}
					}
					else if (moneyPerc > 0.6) {
						if (winCoinPerc >= 2.2) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 1.9) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.6) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.3) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.75) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeLowLose;
						}
						else if (winCoinPerc > 0.25) {
							st = SayType.placeMidLose;
						}
						else {
							st = SayType.placeHighLose;
						}
					}
					else if (moneyPerc > 0.4) {
						if (winCoinPerc >= 2.6) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 2.2) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 1.8) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.4) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.6666) {
							st = SayType.placeVeryLowLose;
						}
						else if (winCoinPerc > 0.3333) {
							st = SayType.placeLowLose;
						}
						else {
							st = SayType.placeMidLose;
						}
					}
					else if (moneyPerc > 0.2) {
						if (winCoinPerc >= 3) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 2.5) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 1.5) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else if (winCoinPerc > 0.5) {
							st = SayType.placeVeryLowLose;
						}
						else {
							st = SayType.placeLowLose;
						}
					}
					else {
						if (winCoinPerc >= 5) {
							st = SayType.placeVeryHighWin;
						}
						else if (winCoinPerc >= 4) {
							st = SayType.placeHighWin;
						}
						else if (winCoinPerc >= 3) {
							st = SayType.placeMidWin;
						}
						else if (winCoinPerc >= 2) {
							st = SayType.placeLowWin;
						}
						else if (winCoinPerc >= 1) {
							st = SayType.placeVeryLowWin;
						}
						else {
							st = SayType.placeVeryLowLose;
						}
					}
				}
			}
#pragma warning restore IDE0045
			DoSpeak(MW, srwo, st);
		}

		internal Speak(IMainWindow MW) {
			placeVeryHighWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryHighWin")];
			placeHighWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeHighWin")];
			placeMidWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeMidWin")];
			placeLowWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeLowWin")];
			placeVeryLowWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryLowWin")];

			placeVeryHighLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryHighLose")];
			placeHighLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeHighLose")];
			placeMidLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeMidLose")];
			placeLowLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeLowLose")];
			placeVeryLowLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryLowLose")];
		}
	}
}
