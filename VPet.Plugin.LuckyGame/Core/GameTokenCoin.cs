using System;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal class GameTokenCoin {
		/// <summary>
		/// 代币类型
		/// </summary>
		internal enum CoinType {
			coinBlack, coinBlue, coinGreen, coinRed, coinWhite
		}
		private ulong coinBlack;
		private ulong coinBlue;
		private ulong coinGreen;
		private ulong coinRed;
		private ulong coinWhite;

		internal ulong CoinBlack => coinBlack;
		/// <summary>
		/// 汇率，1代币兑换多少桌宠钱<br/>
		/// 可动态更改
		/// </summary>
		internal uint er_coinBlack = 1;//exchange rate
		/// <summary>
		/// 兑换桌宠钱时的手续费占比<br/>
		/// 可动态更改
		/// </summary>
		internal float ef_coinBlack = .1f; //exchange fee
		internal ulong CoinBlue => coinBlue;
		/// <inheritdoc cref="er_coinBlack"/>
		internal uint er_coinBlue = 10;
		/// <inheritdoc cref="ef_coinBlack"/>
		internal float ef_coinBlue = .08f;
		internal ulong CoinGreen => coinGreen;
		/// <inheritdoc cref="er_coinBlack"/>
		internal uint er_coinGreen = 100;
		/// <inheritdoc cref="ef_coinBlack"/>
		internal float ef_coinGreen = .05f;
		internal ulong CoinRed => coinRed;
		/// <inheritdoc cref="er_coinBlack"/>
		internal uint er_coinRed = 1000;
		/// <inheritdoc cref="ef_coinBlack"/>
		internal float ef_coinRed = .03f;
		internal ulong CoinWhite => coinWhite;
		/// <inheritdoc cref="er_coinBlack"/>
		internal uint er_coinWhite = 10000;
		/// <inheritdoc cref="ef_coinBlack"/>
		internal float ef_coinWhite = .01f;

		/// <summary>
		/// 在原基础上更变代币的数量，并对桌宠钱进行相应的增减
		/// </summary>
		/// <param name="cType">代币类型</param>
		/// <param name="value">更变的值</param>
		/// <returns>
		/// 返回值：<br/>
		/// 0：成功<br/>
		/// 1：未知错误<br/>
		/// 2：桌宠钱不足购买<br/>
		/// 3：代币不足兑换桌宠钱<br/>
		/// 4：value参数值等于0<br/>
		/// </returns>
		internal byte ChangeCoin(IMainWindow MW, CoinType cType,long value) {
			byte ret = 1;
			void action(ref ulong coin,uint er,float ef,long val) {
				if (val > 0) {
					double moneyMinus = val * er;
					if (!(MW.Core.Save.Money < moneyMinus)) {
						MW.Core.Save.Money -= moneyMinus;
						coin += (ulong)val;
						ret = 0;
					}
					else
						ret = 2;
				}
				else if (val < 0) {
					double moneyAdd = (-val * er) - (-val * er)*ef;
					if (!(coin < (ulong)(-val))) {
						MW.Core.Save.Money += moneyAdd;
						coin -= (ulong)(-val);
						ret = 0;
					}
					else
						ret = 3;
				}
				else
					ret = 4;
			}
			switch (cType) {
				case CoinType.coinBlack:
					action(ref coinBlack, er_coinBlack,ef_coinBlack, value);
					break;
				case CoinType.coinBlue:
					action(ref coinBlue,er_coinBlue, ef_coinBlue, value);
					break;
				case CoinType.coinGreen:
					action(ref coinGreen,er_coinGreen, ef_coinGreen, value);
					break;
				case CoinType.coinRed:
					action(ref coinRed,er_coinRed, ef_coinRed, value);
					break;
				case CoinType.coinWhite:
					action(ref coinWhite,er_coinWhite, ef_coinWhite, value);
					break;
			}
			return ret;
		}

		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="coins">
		/// 各类型代币的初始化值，留空或为null则默认为0<br/>
		/// 按照CoinType枚举顺序排序
		/// </param>
		/// <param name="erCoin">
		/// 各类代币的汇率值，空或null则用默认值<br/>
		/// CoinType排序
		/// </param>
		/// <param name="efCoin">
		/// 各类代币的兑换桌宠钱手续费收取比例，空或null则用默认值<br/>
		/// CoinType排序
		/// </param>
		internal GameTokenCoin(ulong[] coins = null, uint[] erCoin=null,float[] efCoin = null) {
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
			if(efCoin != null) {
				ef_coinBlack = efCoin[(int)CoinType.coinBlack];
				ef_coinBlue = efCoin[(int)CoinType.coinBlue];
				ef_coinGreen = efCoin[(int)CoinType.coinGreen];
				ef_coinRed = efCoin[(int)CoinType.coinRed];
				ef_coinWhite = efCoin[(int)CoinType.coinWhite];
			}
		}
	}
}
