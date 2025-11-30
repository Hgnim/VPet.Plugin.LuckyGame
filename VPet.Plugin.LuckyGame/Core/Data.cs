using System.Collections.Generic;

namespace VPet.Plugin.LuckyGame.Core {
	internal class Data {
		internal required GameTokenCoin gtc;
		internal class Lottery {
			/// <summary>
			/// 当前持有的彩票
			/// </summary>
			internal required List<Game.Lottery.LotteryBuy> lotteryHave;
		}
		internal required Lottery lottery;

		internal required Speak speak;
	}
}
