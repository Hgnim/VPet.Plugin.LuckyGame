using System;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal struct DataSave {
		const string mainKey = "LuckyGame";
		/// <summary>
		/// 保存数据
		/// </summary>
		internal static void Save(IMainWindow MW,GameTokenCoin gtc) {
			for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
				MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]] =
					(long)gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
			MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"] = (int)gtc.defCoinType;
		}
		/// <summary>
		/// 读取数据
		/// </summary>
		internal static void Read(IMainWindow MW, out GameTokenCoin.GameTokenCoin_Args gtcArg) {
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
		}
	}
}
