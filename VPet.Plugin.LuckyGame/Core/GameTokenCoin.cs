using LinePutScript.Localization.WPF;
using System.Drawing;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal class GameTokenCoin {
		internal class Coin {
			/// <summary>
			/// 代币信息委托
			/// </summary>
			/// <param name="type">代币类型</param>
			/// <param name="value">代币数量</param>
			/// <param name="rate">该代币汇率</param>
			/// <param name="fee">该代币兑回手续费</param>
			internal delegate void CoinInfo(CoinType type,ulong value,uint rate, double fee);
			internal CoinInfo OnCoinChange;
			/// <summary>
			/// 代币类型
			/// </summary>
			internal enum CoinType {
				coinBlack, coinBlue, coinGreen, coinRed, coinWhite
			}
			/// <summary>
			/// 对应CoinType的代币颜色
			/// </summary>
			internal static readonly Color[] CoinColor =
				[
					Color.Black,
					Color.Blue,
					Color.Green,
					Color.Red,
					Color.White,
				];
			/// <summary>
			/// 每个代币对用户显示的名称
			/// </summary>
			internal static readonly string[] CoinName =
				[
					"黑色代币".Translate(),
					"蓝色代币".Translate(),
					"绿色代币".Translate(),
					"红色代币".Translate(),
					"白色代币".Translate(),
				];
			/// <summary>
			/// 每个代币的键名
			/// </summary>
			internal static readonly string[] CoinKey =
				[
					"BlackCoin",
					"BlueCoin",
					"GreenCoin",
					"RedCoin",
					"WhiteCoin",
				];

			private ulong coinBlack;
			private ulong coinBlue;
			private ulong coinGreen;
			private ulong coinRed;
			private ulong coinWhite;

			/// <summary>
			/// 代币-黑色
			/// </summary>
			internal ulong CoinBlack {
				set {
					if (coinBlack != value) {
						coinBlack = value;
						OnCoinChange?.Invoke(CoinType.coinBlack, CoinBlack, er_coinBlack, ef_coinBlack);
					}
				}
				get => coinBlack;
			}
			/// <summary>
			/// 汇率，1代币兑换多少桌宠钱<br/>
			/// 可动态更改
			/// </summary>
			internal uint er_coinBlack = 1;//exchange rate
			/// <summary>
			/// 兑换桌宠钱时的手续费占比<br/>
			/// 可动态更改
			/// </summary>
			internal double ef_coinBlack = .5; //exchange fee
			/// <summary>
			/// 代币-蓝色
			/// </summary>
			internal ulong CoinBlue {
				set {
					if (coinBlue != value) {
						coinBlue = value;
						OnCoinChange?.Invoke(CoinType.coinBlue, CoinBlue, er_coinBlue, ef_coinBlue);
					}
				}
				get => coinBlue;
			}
			/// <inheritdoc cref="er_coinBlack"/>
			internal uint er_coinBlue = 100;
			/// <inheritdoc cref="ef_coinBlack"/>
			internal double ef_coinBlue = .025;
			/// <summary>
			/// 代币-绿色
			/// </summary>
			internal ulong CoinGreen {
				set {
					if (coinGreen != value) {
						coinGreen = value;
						OnCoinChange?.Invoke(CoinType.coinGreen, CoinGreen, er_coinGreen, ef_coinGreen);
					}
				}
				get => coinGreen;
			}
			/// <inheritdoc cref="er_coinBlack"/>
			internal uint er_coinGreen = 1_0000;
			/// <inheritdoc cref="ef_coinBlack"/>
			internal double ef_coinGreen = .00125;
			/// <summary>
			/// 代币-红色
			/// </summary>
			internal ulong CoinRed {
				set {
					if (coinRed != value) {
						coinRed = value;
						OnCoinChange?.Invoke(CoinType.coinRed, CoinRed, er_coinRed, ef_coinRed);
					}
				}
				get => coinRed;
			}
			/// <inheritdoc cref="er_coinBlack"/>
			internal uint er_coinRed = 100_0000;
			/// <inheritdoc cref="ef_coinBlack"/>
			internal double ef_coinRed = .0000625;
			/// <summary>
			/// 代币-白色
			/// </summary>
			internal ulong CoinWhite {
				set {
					if (coinWhite != value) {
						coinWhite = value;
						OnCoinChange?.Invoke(CoinType.coinWhite, CoinWhite, er_coinWhite, ef_coinWhite);
					}
				}
				get => coinWhite;
			}
			/// <inheritdoc cref="er_coinBlack"/>
			internal uint er_coinWhite = 1_0000_0000;
			/// <inheritdoc cref="ef_coinBlack"/>
			internal double ef_coinWhite = .000003125;

			internal Coin(ulong[] coins, uint[] erCoin, float[] efCoin) {
				if (coins != null) {
					coinBlack = coins[(int)CoinType.coinBlack];
					coinBlue = coins[(int)CoinType.coinBlue];
					coinGreen = coins[(int)CoinType.coinGreen];
					coinRed = coins[(int)CoinType.coinRed];
					coinWhite = coins[(int)CoinType.coinWhite];
				}
				else {
					coinBlack = 0;
					coinBlue = 0;
					coinGreen = 0;
					coinRed = 0;
					coinWhite = 0;
				}
				if (erCoin != null) {
					er_coinBlack = erCoin[(int)CoinType.coinBlack];
					er_coinBlue = erCoin[(int)CoinType.coinBlue];
					er_coinGreen = erCoin[(int)CoinType.coinGreen];
					er_coinRed = erCoin[(int)CoinType.coinRed];
					er_coinWhite = erCoin[(int)CoinType.coinWhite];
				}
				if (efCoin != null) {
					ef_coinBlack = efCoin[(int)CoinType.coinBlack];
					ef_coinBlue = efCoin[(int)CoinType.coinBlue];
					ef_coinGreen = efCoin[(int)CoinType.coinGreen];
					ef_coinRed = efCoin[(int)CoinType.coinRed];
					ef_coinWhite = efCoin[(int)CoinType.coinWhite];
				}
			}
		}
		internal Coin coin;
		/// <summary>
		/// 当前/默认 使用的代币类型
		/// </summary>
		internal Coin.CoinType defCoinType;
		/// <summary>
		/// 代币兑换<br/>
		/// 在原基础上更变代币的数量，并对桌宠钱进行相应的增减
		/// </summary>
		/// <param name="value">更变的值</param>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		/// <returns>
		/// 返回值：<br/>
		/// 0：成功<br/>
		/// 1：未知错误<br/>
		/// 2：桌宠钱不足购买<br/>
		/// 3：代币不足兑换桌宠钱<br/>
		/// 4：value参数值等于0<br/>
		/// </returns>
		internal byte ExchangeCoin(IMainWindow MW, long value, Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			byte ret = 1;
			ulong action(ulong c,uint er, double ef,long val) {
				if (val > 0) {
					double moneyMinus = val * er;
					if (!(MW.Core.Save.Money < moneyMinus)) {
						MW.Core.Save.Money -= moneyMinus;
						c += (ulong)val;
						ret = 0;
					}
					else
						ret = 2;
				}
				else if (val < 0) {
					double moneyAdd = (-val * er) - (-val * er)*ef;
					if (!(c < (ulong)(-val))) {
						MW.Core.Save.Money += moneyAdd;
						c -= (ulong)(-val);
						ret = 0;
					}
					else
						ret = 3;
				}
				else
					ret = 4;
				return c;
			}
			switch (cType) {
				case Coin.CoinType.coinBlack:
					coin.CoinBlack = action(coin.CoinBlack, coin.er_coinBlack, coin.ef_coinBlack, value);
					break;
				case Coin.CoinType.coinBlue:
					coin.CoinBlue = action(coin.CoinBlue, coin.er_coinBlue, coin.ef_coinBlue, value);
					break;
				case Coin.CoinType.coinGreen:
					coin.CoinGreen = action(coin.CoinGreen, coin.er_coinGreen, coin.ef_coinGreen, value);
					break;
				case Coin.CoinType.coinRed:
					coin.CoinRed = action(coin.CoinRed, coin.er_coinRed, coin.ef_coinRed, value);
					break;
				case Coin.CoinType.coinWhite:
					coin.CoinWhite = action(coin.CoinWhite, coin.er_coinWhite, coin.ef_coinWhite, value);
					break;
			}
			return ret;
		}
		/// <summary>
		/// 获取兑换指定数量代币所需桌宠币数
		/// </summary>
		/// <param name="coinTarget">目标数量的代币</param>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		internal double GetExchangeNeedMoney(ulong coinTarget, Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			static ulong action(ulong c,uint er) => c * er;
			return cType switch {
				Coin.CoinType.coinBlack => action(coinTarget, coin.er_coinBlack),
				Coin.CoinType.coinBlue => action(coinTarget, coin.er_coinBlue),
				Coin.CoinType.coinGreen => action(coinTarget, coin.er_coinGreen),
				Coin.CoinType.coinRed => action(coinTarget, coin.er_coinRed),
				Coin.CoinType.coinWhite => action(coinTarget, coin.er_coinWhite),
				_ => -1,
			};
		}
		/// <summary>
		/// 获取将指定数量的代币兑换为桌宠币后可获得的金额
		/// </summary>
		/// <param name="coinTarget">目标数量的代币</param>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		internal double GetExchangeGetMoney(ulong coinTarget, Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			static double action(ulong c, uint er, double ef) => (c * er) - ((c * er) * ef);
			return cType switch {
				Coin.CoinType.coinBlack => action(coinTarget, coin.er_coinBlack, coin.ef_coinBlack),
				Coin.CoinType.coinBlue => action(coinTarget, coin.er_coinBlue, coin.ef_coinBlue),
				Coin.CoinType.coinGreen => action(coinTarget, coin.er_coinGreen, coin.ef_coinGreen),
				Coin.CoinType.coinRed => action(coinTarget, coin.er_coinRed, coin.ef_coinRed),
				Coin.CoinType.coinWhite => action(coinTarget, coin.er_coinWhite, coin.ef_coinWhite),
				_ => -1,
			};
		}

		/// <summary>
		/// 获取指定代币的汇率
		/// </summary>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		internal uint GetCoinExchangeRate(Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			return cType switch {
				Coin.CoinType.coinBlack => coin.er_coinBlack,
				Coin.CoinType.coinBlue => coin.er_coinBlue,
				Coin.CoinType.coinGreen => coin.er_coinGreen,
				Coin.CoinType.coinRed => coin.er_coinRed,
				Coin.CoinType.coinWhite => coin.er_coinWhite,
				_ => 0,
			};
		}
		/// <summary>
		/// 获取指定代币的兑换手续费比例
		/// </summary>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		internal double GetCoinExchangeFee(Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			return cType switch {
				Coin.CoinType.coinBlack => coin.ef_coinBlack,
				Coin.CoinType.coinBlue => coin.ef_coinBlue,
				Coin.CoinType.coinGreen => coin.ef_coinGreen,
				Coin.CoinType.coinRed => coin.ef_coinRed,
				Coin.CoinType.coinWhite => coin.ef_coinWhite,
				_ => -1,
			};
		}

		/// <summary>
		/// 获取指定代币的数量
		/// </summary>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		internal ulong GetCoinAmount(Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			return cType switch {
				Coin.CoinType.coinBlack => coin.CoinBlack,
				Coin.CoinType.coinBlue => coin.CoinBlue,
				Coin.CoinType.coinGreen => coin.CoinGreen,
				Coin.CoinType.coinRed => coin.CoinRed,
				Coin.CoinType.coinWhite => coin.CoinWhite,
				_ => 0,
			};
		}

		/// <summary>
		/// 在原基础上更改代币数量
		/// </summary>
		/// <param name="value">更变的值</param>
		/// <param name="cType">
		/// 代币类型<br/>
		/// 留空或为null则使用默认值
		/// </param>
		/// <returns>
		/// 返回值：<br/>
		/// 0：成功<br/>
		/// 1：未知错误<br/>
		/// 2：value参数值等于0<br/>
		/// 3：代币不足<br/>
		/// </returns>
		internal byte ChangeCoin(long value, Coin.CoinType? cType=null) {
			cType ??= defCoinType;
			byte ret = 1;
			ulong action(ulong c,long v) {
				if (v > 0) {
					c += (ulong)v;
					ret = 0;
				}
				else if (v < 0) {
					if (c - (ulong)(-v) >= 0) {
						c -= (ulong)(-v);
						ret = 0;
					}
					else
						ret = 3;
				}
				else
					ret = 2;
				return c;
			}
			switch (cType) {
				case Coin.CoinType.coinBlack:
					coin.CoinBlack = action(coin.CoinBlack, value);
					break;
				case Coin.CoinType.coinBlue:
					coin.CoinBlue = action(coin.CoinBlue, value);
					break;
				case Coin.CoinType.coinGreen:
					coin.CoinGreen = action(coin.CoinGreen, value);
					break;
				case Coin.CoinType.coinRed:
					coin.CoinRed = action(coin.CoinRed, value);
					break;
				case Coin.CoinType.coinWhite:
					coin.CoinWhite = action(coin.CoinWhite, value);
					break;
			}
			return ret;
		}
		/// <summary>
		/// 构造函数的参数类
		/// </summary>
		internal class GameTokenCoin_Args {
			/// <summary>
			/// 各类型代币的初始化值，为null则默认为0<br/>
			/// 按照CoinType枚举顺序排序
			/// </summary>
			internal ulong[] coins = null;
			/// <summary>
			/// 各类代币的汇率值，null则用默认值<br/>
			/// CoinType排序
			/// </summary>
			internal uint[] erCoin = null;
			/// <summary>
			/// 各类代币的兑换桌宠钱手续费收取比例，null则用默认值<br/>
			/// CoinType排序
			/// </summary>
			internal float[] efCoin = null;
			/// <summary>
			/// 默认使用的代币类型
			/// </summary>
			internal Coin.CoinType defCoiType=Coin.CoinType.coinBlack;
		}
		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="args">
		/// 构造函数参数类，留空或为null则使用默认值
		/// </param>
		internal GameTokenCoin(GameTokenCoin_Args args=null) {
			args ??= new();
			coin = new(args.coins, args.erCoin, args.efCoin);
			defCoinType = args.defCoiType;
		}
	}
}
