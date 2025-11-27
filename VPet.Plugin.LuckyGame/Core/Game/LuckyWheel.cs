using System;
using System.Threading;
using System.Threading.Tasks;
using static VPet.Plugin.LuckyGame.Core.GameTokenCoin;

namespace VPet.Plugin.LuckyGame.Core.Game {
	internal class LuckyWheel {
		internal class LuckyWheelBuy {
			/// <summary>
			/// 押下的代币数量
			/// </summary>
			internal required ulong Coin { get; set; }
			/// <summary>
			/// 使用的代币类型，如果为null则使用默认代币类型
			/// </summary>
			internal GameTokenCoin.Coin.CoinType? CoinType { get; set; } = null;
			/// <summary>
			/// 选择的押币点
			/// </summary>
			internal required ushort Place { get; set; }
			/// <summary>
			/// 总共的可押点数量
			/// </summary>
			internal required ushort AllPlace { get; set; }
		}
		internal class LuckyWheelResult {
			/// <summary>
			/// 购买信息
			/// </summary>
			internal required LuckyWheelBuy BuyInfo { get; set; }
			/// <summary>
			/// 如果赢了，将赢得的代币数量。注意此值始终为假设赢了时的值，如果输了，则不要使用此值
			/// </summary>
			internal required ulong IfWinCoin { get; set; }
			/// <summary>
			/// 最后停止时的角度
			/// </summary>
			internal required float StopAngle { get; set; }
		}
		private class RoundAngle {
			private float angle = 0;
			internal float Angle {
				set {
					angle =
						value > 360
							? value % 360
							: value < 0
								? 360 - (-value % 360)
								: value;
					OnAngelChange?.Invoke(angle);
				}
				get => angle;
			}
			internal Action<float> OnAngelChange;
		}
		private readonly RoundAngle ra=new();
		/// <summary>
		/// 当前角度
		/// </summary>
		internal float NowAngle {
			set{
				if (!IsRunning) ra.Angle = value;
			}
			get => ra.Angle;
		}
		/// <summary>
		/// 当角度变化时触发
		/// </summary>
		internal Action<float> OnAngelChange {
			set => ra.OnAngelChange = value;
			get => ra.OnAngelChange;
		}
		
		private bool isRunning = false;
		/// <summary>
		/// 转盘当前是否在运行
		/// </summary>
		internal bool IsRunning => isRunning;

		internal byte PlaceCoin(LuckyWheelBuy lwb,GameTokenCoin gtc)
		{
            lwb.CoinType ??= gtc.defCoinType;
            return gtc.ChangeCoin(lwb.Coin, false, lwb.CoinType, cel: new()
            {
                Note = "幸运转盘购买",
                OnlyNote = true
            });
        }
		/// <summary>
		/// 开始转盘
		/// </summary>
		/// <param name="lwb">购买信息</param>
		/// <param name="gtc">
		/// 游戏代币数据<br/>
		/// 提供此参数将根据lwb参数自动扣取代币，如果留空或为null则需要手动操作
		/// </param>
		/// <param name="rate">每秒计算次数</param>
		/// <param name="MaxSpeed">最大速度，如果留空则根据rate计算</param>
		/// <returns>返回转盘停下时的角度</returns>
		internal async Task<LuckyWheelResult> StartWheel(LuckyWheelBuy lwb,ushort rate=24,int? MaxSpeed=null) {
			isRunning = true;

			Random ran = new();
			int maxSpeed = MaxSpeed is not null and > 24 && MaxSpeed > rate
								? (int)MaxSpeed 
								: rate * 2;
			float speed= ran.Next(rate, maxSpeed) + ran.NextSingle();
			await Task.Run(() => {
				while (speed >= 0) {
					ra.Angle += speed;
					if (speed > rate) {
						speed -= (ran.Next(90, 100 + 1) * 0.01f) * (maxSpeed / (rate/3.5f));
					}
					else {
						if (speed > 1) {
							speed -= ran.NextSingle() * (speed * 0.01f);
						}
						else {
							speed -= ran.Next(10 + 1) * 0.01f;
						}
					}
						
					Thread.Sleep((int)(1000 / rate));
				}
			});
			LuckyWheelResult lwr = new() {
				BuyInfo = lwb,
				StopAngle = ra.Angle,
				IfWinCoin = (ulong)(lwb.Coin * lwb.AllPlace)
			};
			isRunning = false;
			return lwr;
		}
		/// <summary>
		/// 赢得的代币数量
		/// </summary>
		/// <param name="stopPlace">转盘停止点</param>
		/// <param name="lwr">转盘结果</param>
		/// <param name="gtc">
		/// 游戏代币数据<br/>
		/// 提供此参数将根据lwb参数自动添加赢得的代币，如果留空或为null则需要手动操作
		/// </param>
		/// <returns>返回赢得的代币数</returns>
		internal static ulong WinCoin(ushort stopPlace,LuckyWheelResult lwr, GameTokenCoin gtc = null) {
			if (stopPlace == lwr.BuyInfo.Place) {
				gtc?.ChangeCoin(lwr.IfWinCoin, true, lwr.BuyInfo.CoinType, cel: new() {
					Note = "幸运转盘获奖",
					OnlyNote = true
				});
				return lwr.IfWinCoin;
			}
			else {
				return 0;
			}
		}  
	}
}
