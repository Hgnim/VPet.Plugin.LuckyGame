using System;
using System.Threading;
using System.Threading.Tasks;

namespace VPet.Plugin.LuckyGame.Core.Game {
	internal class LuckyWheel {
		private class RoundAngle {
			private float angle = 0;
			internal float RoAngle => angle;
			internal float Angle {
				set {
					angle =
						value > 360
							? value % 360
							: value < 0
								? 360 - (-value % 360)
								: value;
					OnAngelChange?.Invoke(RoAngle);
				}
				get => angle;
			}
			internal Action<float> OnAngelChange;
		}
		private readonly RoundAngle ra=new();
		/// <summary>
		/// 当前角度
		/// </summary>
		internal float NowAngle => ra.RoAngle;
		/// <summary>
		/// 当角度变化时触发
		/// </summary>
		internal Action<float> OnAngelChange {
			set => ra.OnAngelChange = value;
			get => ra.OnAngelChange;
		}
		/// <summary>
		/// 开始转盘
		/// </summary>
		/// <param name="rate">每秒计算次数</param>
		/// <param name="MaxSpeed">最大速度，如果留空则根据rate计算</param>
		/// <returns>返回转盘停下时的角度</returns>
		internal async Task<float> StartWheel(ushort rate=24,int? MaxSpeed=null) {
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
			return ra.RoAngle;
		}
		ulong? coin;
		ushort? place,allPlace;
		/// <summary>
		/// 押代币
		/// </summary>
		/// <param name="Coin">代币数量</param>
		/// <param name="Place">押点</param>
		/// <param name="AllPlace">总共押点数量</param>
		internal void PlaceCoin(ulong Coin,ushort Place,ushort AllPlace) {
			coin = Coin;
			place = Place;
			allPlace = AllPlace;
		}
		/// <summary>
		/// 赢得的代币数量
		/// </summary>
		/// <param name="stopPlace">转盘停止点</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		internal ulong WinCoin(ushort stopPlace) => 
			coin is not null && place is not null && allPlace is not null
				? stopPlace == place 
					? (ulong)(coin * allPlace) 
					: 0
				: throw new InvalidOperationException("存在null值，可能是没有执行PlaceCoin函数");
	}
}
