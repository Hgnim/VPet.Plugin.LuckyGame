using LinePutScript.Localization.WPF;
using System;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VPet.Plugin.LuckyGame.Controls {
	public partial class Toast : UserControl{
		private bool IsShowing = false;
		private bool AllowShow = true;
		private Timer UIUpdateTimer;
		private CancellationTokenSource cts = new CancellationTokenSource();
        public Toast() {
			InitializeComponent();
			UIUpdateTimer = new Timer((e) =>
			{
				if(AllowShow == false) AllowShow = true;
            },null,0,100);
        }

		/// <summary>
		/// 执行吐司框
		/// </summary>
		/// <param name="message">吐司框中的消息文本</param>
		/// <param name="maxOpacity">淡入时最大不透明度</param>
		/// <param name="time">吐司框持续显示的时间</param>
		/// <param name="fadeTime">淡入淡出动画时间</param>
		public async void Show(string message,double maxOpacity=0.8, TimeSpan? time = null, TimeSpan? fadeTime = null) {
			await Dispatcher.BeginInvoke(async () =>
			{
				if (!AllowShow) return;
				AllowShow = false;
				if (IsShowing == true)
				{
					cts.Cancel();
                }
				IsShowing = true;
				time ??= TimeSpan.FromSeconds(2);
				fadeTime ??= TimeSpan.FromMilliseconds(300);
				MsgText.Text = message.Translate();

				FadeIn((TimeSpan)fadeTime, maxOpacity);
				try
				{
					await Task.Delay((TimeSpan)time, cts.Token);
				}
				catch (TaskCanceledException) { }
                FadeOut((TimeSpan)fadeTime, maxOpacity);
				IsShowing = false;
            });
		}

		private uint showNum = 0;//用于判断多次执行
		internal void FadeIn(TimeSpan time,double maxOpa) {
			this.BeginAnimation(OpacityProperty, new DoubleAnimation(0, maxOpa, time));
			showNum++;
		}
		internal void FadeOut(TimeSpan time,double maxOpa) {
			if (showNum > 1)//如果执行次数大于一，则不播放淡出动画，直到释放次数小于等于1后执行
				showNum--;
			else {
				this.BeginAnimation(OpacityProperty, new DoubleAnimation(maxOpa, 0, time));
				showNum = 0;
			}
		}
	}
}
