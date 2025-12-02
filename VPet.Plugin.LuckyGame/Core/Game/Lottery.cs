using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

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

			/// <summary>
			/// 获取随机彩票数类型
			/// </summary>
			/// <param name="seed">可选种子，留空或为null则使用默认值</param>
			/// <returns></returns>
			internal static LotteryNumber GetRandomNumber(int? seed=null){
				Random ran = 
					seed == null 
					? new() 
					: new((int)seed);
				byte[] mn = new byte[6];
				byte[] dn = new byte[2];
				void action(ref byte[] num,int min,int max) {
					for (byte b = 0; b < num.Length; b++) {
						bool pass;
						byte ranNum;
						do {
							pass = true;
							ranNum = (byte)ran.Next(min, max);
							for (byte b2 = 0; b2 < b; b2++) {
								if (num[b2] == ranNum) {//生成的号码不重复
									pass = false;
									break;
								}
							}
						} while (!pass);
						num[b] = ranNum;
					}
				}
				action(ref mn, 0, 30 + 1);
				action(ref dn, 0, 10);
				return new() {
					mainNumber = mn,
					deputyNumber = dn
				};
			}
            public override string ToString()
            {
                string mainNumStr = string.Join(" ", MainNumber);
				string deputyNumStr = string.Join(" ", DeputyNumber);
				return $"主号码: [{mainNumStr}] 副号码: [{deputyNumStr}]".Translate();
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

			private bool bought = false;
			/// <summary>
			/// 表示当前彩票是否是已购买状态<br/>
			/// 该值仅可在当下使用，不会持久化保存，并且在后续开奖等函数中不会对其进行判断。
			/// </summary>
			internal bool Bought => bought;

			/// <summary>
			/// 购买彩票
			/// </summary>
			/// <param name="gtc">游戏代币数据</param>
			/// <returns>返回值与gtc.ChangeCoin函数一致</returns>
			internal byte Pay(GameTokenCoin gtc) {
				coinType ??= gtc.coin.DefCoinType;
				byte res = gtc.ChangeCoin(coin, false, coinType, cel: new() {
					Note = "彩票购买",
					OnlyNote = true
				});
				bought = res == 0;
				return res;
			}
		}
		/// <summary>
		/// 彩票结果类
		/// </summary>
		internal class LotteryResult {
			/// <summary>
			/// 购买信息
			/// </summary>
			internal required LotteryBuy BuyInfo { get; set; }

			/// <summary>
			/// 开奖号码
			/// </summary>
			internal required LotteryNumber WinningNumber { get; set; }

			/// <summary>
			/// 主号码中奖个数
			/// </summary>
			internal required byte MainWinCount { get; set; }

			/// <summary>
			/// 副号码中奖个数
			/// </summary>
			internal required byte DeputyWinCount { get; set; }

			/// <summary>
			/// 主号码中奖号码位置<br/>
			/// 为true则表示该位置号码中奖
			/// </summary>
			internal required bool[] MainWinLoc { get; set; } 

			/// <summary>
			/// 副号码中奖号码位置<br/>
			/// 为true则表示该位置号码中奖
			/// </summary>
			internal required bool[] DeputyWinLoc { get; set; }

			/// <summary>
			/// 赢得的代币数量
			/// </summary>
			internal ulong WinCoin => WinCoin_Detail[^1];

			/// <summary>
			/// 赢得代币的细节，长度为主号码长度加副号码长度<br/>
			/// 用于兼容开奖动画，每一个索引代表者开出一个号码后赢得代币数量的变化
			/// </summary>
			internal required ulong[] WinCoin_Detail { get; set; }

			/// <summary>
			/// 将当前实例中赢得的代币数据给予玩家
			/// </summary>
			/// <param name="gtc">游戏代币数据</param>
			/// <returns>返回值与gtc.ChangeCoin函数一致</returns>
			internal byte WinCoinPay(GameTokenCoin gtc) => 
				gtc.ChangeCoin(WinCoin, true, BuyInfo.coinType, cel: new() {
					Note = "彩票获奖",
					OnlyNote = true
				});
		}

		/// <summary>
		/// 彩票开始
		/// </summary>
		/// <param name="buy">购买信息组</param>
		/// <returns>返回结果信息组</returns>
		internal static List<LotteryResult> Start(List<LotteryBuy> buy) {
			byte[] winMainNum, winDepuNum;
			{
				long seed = DataSave.TimeData;
				Random ran = new();
				void action(byte num) {
					if (Convert.ToBoolean(ran.Next(0, 2))) {
						if (long.MaxValue - num > seed) seed += num;
					}
					else {
						if (long.MinValue + num < seed) seed -= num;
					}
				}
				foreach (LotteryBuy bu in buy) {
					foreach(byte num in bu.lotteryNumber.MainNumber) {
						action(num);
					}
					foreach(byte num in bu.lotteryNumber.DeputyNumber) {
						action(num);
					}
				}
				TakeWinNumber(out winMainNum, out winDepuNum, 123);
			}
			List<LotteryResult> lr = new();
			foreach(var item in buy) {
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
						for (byte b = 0; b < legth; b++) {
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
						item.lotteryNumber.MainNumber
					);
					checkWin(
						out deputyWinCount,
						out deputyWinLoc,
						2,
						winDepuNum,
						item.lotteryNumber.DeputyNumber
					);
				}


				ulong[] winCoDet = new ulong[8];
				{
					double[] winCoDet_doub = new double[8];
					for (byte b = 0; b < 8; b++) {
						byte winN = 0;
						if (b < 6) {
							for (byte b2 = 0; b2 < b+1; b2++) {
								if (b2 < 6) 
									if (mainWinLoc[b2]) winN++;
							}
							winCoDet_doub[b] = Math.Pow(winN, item.coin);
						}
						else {
							for(byte b2 = 0; b2 < b-6+1; b2++) {
								if (b2 < 2)
									if (deputyWinLoc[b2]) winN++;
							}
							winCoDet_doub[b] = winCoDet_doub[6-1] + Math.Pow(1.5, winN);
							if (b == 8 - 1 && winN == 0) winCoDet_doub[b] = 0;//如果副号码没有赢，则主号码赢得的不算数
						}
					}
					for(byte b = 0; b < 8; b++) {
						winCoDet[b] = (ulong)Math.Round(winCoDet_doub[b]);
					}
				}
				lr.Add(new() {
					BuyInfo = item,
					WinningNumber = new() {
						MainNumber = winMainNum,
						DeputyNumber = winDepuNum
					},
					MainWinCount = mainWinCount,
					DeputyWinCount = deputyWinCount,
					MainWinLoc = mainWinLoc,
					DeputyWinLoc = deputyWinLoc,
					WinCoin_Detail = winCoDet
				});
			}
			return lr;
		}

		private static void TakeWinNumber(out byte[] winMainNum,out byte[] winDepuNum,long seed) {
			LotteryNumber ln = LotteryNumber.GetRandomNumber(seed.GetHashCode());
			winMainNum = ln.MainNumber;
			winDepuNum = ln.DeputyNumber;
		}
	}
}
