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
		internal class LotteryResult
		{
			internal required List<Game.Lottery.LotteryResult> lotteryResults;
        }

		internal required Lottery lottery;

		internal required LotteryResult lotteryResult;

        internal required Speak speak;

		internal bool IsShowResult = false;

		internal bool IsShowing = false;
	}
}
