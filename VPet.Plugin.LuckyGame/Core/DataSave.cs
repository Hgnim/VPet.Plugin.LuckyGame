using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.LuckyGame.Core.Game;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal struct DataSave {
		/// <summary>
		/// 插件所在目录
		/// </summary>
		internal static readonly string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		const string mainKey = "LuckyGame";
		/// <summary>
		/// 保存数据
		/// </summary>
		internal static void Save(IMainWindow MW, Data dat) {
			for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
				MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]] =
					(long)dat.gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
			MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"] = (int)dat.gtc.coin.DefCoinType;
			LotteryHave_Save(MW,[.. dat.lottery.lotteryHave]);
		}
		/// <summary>
		/// 读取数据
		/// </summary>
		internal static void Read(
			IMainWindow MW,
			out GameTokenCoin.GameTokenCoin_Args gtcArg,
			out List<Lottery.LotteryBuy> lllb
		) {
			gtcArg = new() { 
				coins =new ulong[GameTokenCoin.Coin.CoinKey.Length],
			};
			for(byte b= 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
				try {
					long? c = MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]];
					gtcArg.coins[b] = c is not null
						? (ulong)c
						: 0;
				} catch { gtcArg.coins[b] = 0; }
			}
			try{
				int? dct = MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"];
				gtcArg.defCoiType = dct is not null 
					? (GameTokenCoin.Coin.CoinType)dct 
					: GameTokenCoin.Coin.CoinType.coinBlack;
			} catch { gtcArg.defCoiType = GameTokenCoin.Coin.CoinType.coinBlack; }

			lllb = LotteryHave_Get(MW);
		}

		private static void LotteryHave_Save(IMainWindow MW, Lottery.LotteryBuy[] lBuy) {
			string saveData=null;
			foreach (Lottery.LotteryBuy lb in lBuy) {
				if (saveData == null)
					saveData = "";
				else
					saveData += '!';

				string lotNum = "";
				foreach (byte n in lb.lotteryNumber.MainNumber) {
					lotNum += $"{n},";
				}
				lotNum = lotNum[..^1];//去掉末尾间隔符
				lotNum += ';';
				foreach (byte n in lb.lotteryNumber.DeputyNumber) {
					lotNum += $"{n},";
				}
				lotNum = lotNum[..^1];

				saveData += $"{lotNum}&{lb.coin}&{(int)lb.coinType}";
			}
			MW.GameSavesData[mainKey]["LotteryHave"].SetString(saveData);
		}
		private static List<Lottery.LotteryBuy> LotteryHave_Get(IMainWindow MW) {
			List<Lottery.LotteryBuy> buys = [];
			string[] strDatas = MW.GameSavesData[mainKey]["LotteryHave"].GetString().Split('!');
			foreach(string sData in strDatas) {
				string[] lbData = sData.Split('&');

				if (lbData.Length == 3) {
					Lottery.LotteryBuy buy = new();
					{
						List<byte> mainNum = [];
						List<byte> depuNum = [];
						string ln = lbData[0];
						foreach (string n in ln.Split(';')[0].Split(',')) {
							mainNum.Add(Convert.ToByte(n));
						}
						foreach (string n in ln.Split(';')[1].Split(',')) {
							depuNum.Add(Convert.ToByte(n));
						}
						buy.lotteryNumber = new() {
							MainNumber = [.. mainNum],
							DeputyNumber = [.. depuNum]
						};
					}
					buy.coin = Convert.ToUInt64(lbData[1]);
					buy.coinType = (GameTokenCoin.Coin.CoinType)Convert.ToInt32(lbData[2]);

					buys.Add(buy);
				}
			}
			return buys;
		}
	}
}
