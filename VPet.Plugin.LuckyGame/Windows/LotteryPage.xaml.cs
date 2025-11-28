using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Core.Game;

namespace VPet.Plugin.LuckyGame.Windows
{
    public partial class LotteryPage : Window
    {
        private GameTokenCoin gtc;
        private ulong coinNum = 1; 
        public int CoinNumValue { set => coinNum = Convert.ToUInt64(value); get => Convert.ToInt32(coinNum); }
        internal LotteryPage(GameTokenCoin gtc)
        {
            InitializeComponent();
            InitializeNumbers();
            this.gtc = gtc;
        }

        private void InitializeNumbers()
        {
            
        }

		// 随机生成号码
		private void RandomMainButton_Click(object sender, RoutedEventArgs e) {
			Lottery.LotteryNumber numbers = Lottery.LotteryNumber.GetRandomNumber();
			MainNumber1.Value = numbers.MainNumber[0];
			MainNumber2.Value = numbers.MainNumber[1];
			MainNumber3.Value = numbers.MainNumber[2];
			MainNumber4.Value = numbers.MainNumber[3];
			MainNumber5.Value = numbers.MainNumber[4];
			MainNumber6.Value = numbers.MainNumber[5];
		}
		private void RandomSpecialButton_Click(object sender, RoutedEventArgs e) {
			Lottery.LotteryNumber numbers = Lottery.LotteryNumber.GetRandomNumber();
			SpecialNumber1.Value = numbers.DeputyNumber[0];
			SpecialNumber2.Value = numbers.DeputyNumber[1];
		}
		private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Lottery.LotteryNumber numbers = Lottery.LotteryNumber.GetRandomNumber();
            MainNumber1.Value = numbers.MainNumber[0];
            MainNumber2.Value = numbers.MainNumber[1];
            MainNumber3.Value = numbers.MainNumber[2];
            MainNumber4.Value = numbers.MainNumber[3];
            MainNumber5.Value = numbers.MainNumber[4];
            MainNumber6.Value = numbers.MainNumber[5];

            SpecialNumber1.Value = numbers.DeputyNumber[0];
            SpecialNumber2.Value = numbers.DeputyNumber[1];
        }



        // 清空主号码
        private void ClearMainNumbers_Click(object sender, RoutedEventArgs e)
        {
            MainNumber1.Value = 0;
            MainNumber2.Value = 0;
            MainNumber3.Value = 0;
            MainNumber4.Value = 0;
            MainNumber5.Value = 0;
            MainNumber6.Value = 0;
        }
        // 清空副号码
        private void ClearSpecialNumbers_Click(object sender, RoutedEventArgs e)
        {
            SpecialNumber1.Value = 0;
            SpecialNumber2.Value = 0;
        }
		private void ClearAllNumbers_Click(object sender, RoutedEventArgs e) {
            ClearMainNumbers_Click(null,null);
            ClearSpecialNumbers_Click(null, null);
		}

        /// <summary>
        /// 购买彩票按钮按下
        /// </summary>
		private void BuyButton_Click(object sender, RoutedEventArgs e) {

		}

		// 开始开奖
		private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            // 开奖逻辑
            Lottery.LotteryNumber userNumbers = new Lottery.LotteryNumber
            {
                MainNumber = new byte[]
                {
                    (byte)MainNumber1.Value,
                    (byte)MainNumber2.Value,
                    (byte)MainNumber3.Value,
                    (byte)MainNumber4.Value,
                    (byte)MainNumber5.Value,
                    (byte)MainNumber6.Value
                },
                DeputyNumber = new byte[]
                {
                    (byte)SpecialNumber1.Value,
                    (byte)SpecialNumber2.Value
                }
            };
            Lottery.LotteryBuy lotteryBuy = new Lottery.LotteryBuy
            {
                lotteryNumber = userNumbers,
                coinType = gtc.defCoinType,
                coin = coinNum
            };
            var result = Lottery.Start(lotteryBuy, gtc);
            ResultText.Text = "本期中奖号码为：{0}，您的中奖金额为{1}代币".Translate(result.WinningNumber.ToString(),result.WinCoin);
        }

        #region 窗口拖动

        private Point startPoint;
        private bool isDragging = false;

        private void DragArea_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void DragArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);

                // 计算移动距离
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                // 移动窗口
                this.Left += deltaX;
                this.Top += deltaY;
            }
        }

        private void DragArea_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }
        #endregion

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
	}
}