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
		private RoundAngle ra=new();
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
		/// <returns></returns>
		internal async Task<float> StartWheel(ushort rate=24) {
			Random ran = new();
			float speed = ran.Next(50,100)+ran.NextSingle();
			await Task.Run(() => {
				while (speed >= 0) {
					ra.Angle += speed;
					speed -= ran.NextSingle();
					Thread.Sleep((int)(1000 / rate));
				}
			});
			return ra.RoAngle;
		}
	}
}
