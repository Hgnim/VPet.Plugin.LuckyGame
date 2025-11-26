using System;

namespace VPet.Plugin.LuckyGame.Core.Game {
	internal class Lottery {
		/// <summary>
		/// 彩票号码类
		/// </summary>
		internal class LotteryNumber {
			private byte[] mainNumber = new byte[6];
			private byte[] deputyNumber = new byte[2];
			/// <summary>
			/// 主号码<br/>
			/// 6位；范围0-30（包括0和30）
			/// </summary>
			internal byte[] MainNumber {
				set {
					if (value.Length == 6) {
						foreach(byte b in value) {
							if (b is not (>= 0 and <= 30))
								throw new ArgumentException("MainNumber中的值必须在0到30之间（包括0和30）");
						}
						mainNumber = value;
					}
					else
						throw new ArgumentException("MainNumber的长度必须为6位");
				}
				get => mainNumber;
			}
			/// <summary>
			/// 副号码<br/>
			/// 2位；范围0-10（包括0但不包括10）
			/// </summary>
			internal byte[] DeputyNumber {
				set {
					if (value.Length == 2) {
						foreach (byte b in value) {
							if (b is not (>= 0 and < 10))
								throw new ArgumentException("DeputyNumber中的值必须在0到10之间（包括0但不包括10）");
						}
						deputyNumber = value;
					}
					else
						throw new ArgumentException("DeputyNumber的长度必须为2位");
				}
				get => deputyNumber;
			}
		}
		/// <summary>
		/// 彩票购买类
		/// </summary>
		internal class LotteryBuy {
			/// <summary>
			/// 购买的号码
			/// </summary>
		 	internal LotteryNumber lotteryNumber;
			/// <summary>
			/// 使用多少代币购买，代币越多中奖越多
			/// </summary>
			internal ulong coin;
			/// <summary>
			/// 使用的代币类型，如果为null则使用默认代币类型
			/// </summary>
			internal GameTokenCoin.Coin.CoinType? coinType=null;
		}
		/// <summary>
		/// 彩票结果类
		/// </summary>
		internal class LotteryResult {
			LotteryBuy buyInfo;
			/// <summary>
			/// 购买信息
			/// </summary>
			internal LotteryBuy BuyInfo => buyInfo;

			LotteryNumber winningNumber;
			/// <summary>
			/// 开奖号码
			/// </summary>
			internal LotteryNumber WinningNumber => winningNumber;

			byte mainWinCount;
			/// <summary>
			/// 主号码中奖个数
			/// </summary>
			internal byte MainWinCount => mainWinCount;

			byte deputyWinCount;
			/// <summary>
			/// 副号码中奖个数
			/// </summary>
			internal byte DeputyWinCount => deputyWinCount;

			bool[] mainWinLoc;
			/// <summary>
			/// 主号码中奖号码位置<br/>
			/// 为true则表示该位置号码中奖
			/// </summary>
			internal bool[] MainWinLoc => mainWinLoc;

			bool[] deputyWinLoc;
			/// <summary>
			/// 副号码中奖号码位置<br/>
			/// 为true则表示该位置号码中奖
			/// </summary>
			internal bool[] DeputyWinLoc => deputyWinLoc;

			ulong winCoin;
			/// <summary>
			/// 赢得的代币数量
			/// </summary>
			internal ulong WinCoin => winCoin;

			internal LotteryResult(
				LotteryBuy buyInf, 
				LotteryNumber winningNum, 
				byte mainWinCou, 
				byte deputyWinCou,
				bool[] mainWinLo,
				bool[] deputyWinLo,
				ulong winCoi
			) {
				buyInfo = buyInf;
				winningNumber = winningNum;
				mainWinCount = mainWinCou;
				deputyWinCount = deputyWinCou;
				mainWinLoc = mainWinLo;
				deputyWinLoc = deputyWinLo;
				winCoin = winCoi;
			}
		}

		/// <summary>
		/// 彩票开始
		/// </summary>
		/// <param name="buy">购买信息</param>
		/// <param name="gtc">
		/// 游戏代币数据<br/>
		/// 提供此参数将根据buy参数自动扣取代币和结束时赢得代币，如果留空或为null则需要手动操作
		/// </param>
		/// <returns>返回结果信息</returns>
		internal static LotteryResult Start(LotteryBuy buy, GameTokenCoin gtc=null) {
			buy.coinType ??= gtc.defCoinType;
			gtc?.ChangeCoin(buy.coin, false, buy.coinType, cel: new() {
					Note = "彩票购买",
					OnlyNote = true
				});
			Random ran;
			{
				Random r = new();
				long seed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				foreach(byte b in buy.lotteryNumber.MainNumber) {
					if (Convert.ToBoolean(r.Next(2)))
						seed += b * buy.lotteryNumber.DeputyNumber[0];
					else
						seed += b * buy.lotteryNumber.DeputyNumber[1];
				}
				ran = new(seed.GetHashCode());
			}

			byte[] winMainNum = new byte[6];
			byte[] winDepuNum = new byte[2];
			{
				byte[] spawnRanNum(byte length, int minVal, int maxVal) {
					byte[] ret = new byte[length];
					for (byte b = 0; b < length; b++) {
						bool pass = true;
						byte num;
						do {
							num = (byte)ran.Next(minVal, maxVal);
							for (byte j = 0; j < b; j++) {//生成的号码不重复
								if (num == winMainNum[j]) {
									pass = false;
									break;
								}
								else
									pass = true;
							}
						} while (!pass);
						ret[b] = num;
					}
					return ret;
				}
				winMainNum = spawnRanNum(6, 0, 30 + 1);
				winDepuNum = spawnRanNum(2, 0, 10);
			}
			byte mainWinCount, deputyWinCount;
			bool[] mainWinLoc, deputyWinLoc;
			{
				void checkWin(
					out byte winCount, 
					out bool[] winLoc, 
					byte legth,
					byte[] winNum,
					byte[] buyNum
				) {
					winCount = 0;
					winLoc = new bool[legth];
					for (byte b=0; b < legth; b++) {
						if (buyNum[b] == winNum[b]) {
							winCount++;
							winLoc[b] = true;
						}
						else
							winLoc[b] = false;
					}
				}
				checkWin(
					out mainWinCount,
					out mainWinLoc,
					6,
					winMainNum,
					buy.lotteryNumber.MainNumber
				);
				checkWin(
					out deputyWinCount,
					out deputyWinLoc,
					2,
					winDepuNum,
					buy.lotteryNumber.DeputyNumber
				);
			}

			ulong winCo;{
				double wc = buy.coin;
				if (mainWinCount != 0)
					wc *= (ulong)(2 ^ mainWinCount);
				if (deputyWinCount != 0)
					wc *= Math.Pow(1.5, deputyWinCount);
				if (mainWinCount == 0 && deputyWinCount == 0)
					wc = 0;
				winCo= (ulong)Math.Round(wc);
			}
			gtc?.ChangeCoin(winCo, true, buy.coinType, cel: new() {
					Note = "彩票获奖",
					OnlyNote = true
				});
			return new(
				buy,
				new() {
					MainNumber = winMainNum,
					DeputyNumber = winDepuNum
				},
				mainWinCount,
				deputyWinCount,
				mainWinLoc,
				deputyWinLoc,
				winCo
			);
		}
	}
}
