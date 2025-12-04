using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VPet.Plugin.LuckyGame.Controls;
using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Core.Game;
using VPet_Simulator.Windows.Interface;
using Point = System.Windows.Point;

namespace VPet.Plugin.LuckyGame.Windows
{
    public partial class LotteryPage : Window
    {
        private readonly Data _data;
        private readonly IMainWindow MW;
        private ulong coinNum = 2; 
        public int CoinNumValue { set => coinNum = Convert.ToUInt64(value); get => Convert.ToInt32(coinNum); }
        private List<ulong> resluts = [];
        private List<TextBlock> PurchaseHistory = [];
        internal LotteryPage(Data data,IMainWindow MW)
        {
            this._data = data;
            this.MW = MW;
            InitializeComponent();
            InitializeNumbers();
        }

        private void InitializeNumbers()
        {
            Reminder.IsChecked = _data.IsShowResult;
            ShowResultAnimation();
        }

        public void ShowResultAnimation()
        {
            if (_data.lotteryResult.lotteryResults.Count > 0)
            {
                ReslutBorder.Visibility = Visibility.Visible;
                NumberRoller.RollingCompleted += OnNumberRollingCompleted;
                NumberRoller.NumberStopped += OnNumberStopped;
                NumberRoller.MainMinValue = 0;
                NumberRoller.MainMaxValue = 30;
                NumberRoller.SpecialMinValue = 0;
                NumberRoller.SpecialMaxValue = 9;
                resluts = Lottery.ResultList2WinCoinDetail(_data.lotteryResult.lotteryResults);
                NumberRoller.SetFinalNumbers(_data.lotteryResult.lotteryResults.First().WinningNumber.MainNumber, _data.lotteryResult.lotteryResults.First().WinningNumber.DeputyNumber);
                foreach(var item in PurchaseHistory)
                {
                    BuyHistoryPanel.Children.Add(item);
                }
                NumberRoller.StartRollingAnimation(2.0);
            }
        }

        private void OnNumberStopped(object sender, NumberStoppedEventArgs e)
        {
            PrizeText.Text = "您的中奖金额为： {0} 代币".Translate(resluts.ElementAt(e.NumberIndex));
        }

        private async void OnNumberRollingCompleted(object sender, EventArgs e)
        {
            var finalCoins = 0.0;
            ulong useCoin = 0;
            GameTokenCoin.Coin.CoinType? ct = null;
            var resultString = "";
            await Task.Run(() =>
            {
                foreach (var item in _data.lotteryResult.lotteryResults)
                {
                    finalCoins += item.WinCoin;
                    useCoin += item.BuyInfo.coin;
                    ct = item.BuyInfo.coinType;
                    item.WinCoinPay(_data.gtc);
                    resultString = item.WinningNumber.ToString();
                }
            });
            MessageBoxX.Show("开奖结果已公布！\n本次中奖号码为：\n{1}\n您本次共获得代币数为：{0}".Translate(finalCoins, resultString), "提示".Translate());
			_data.speak.DoSpeak(MW, _data.gtc,
						new() {
							Value = useCoin,
							CoinType = ct ?? _data.gtc.coin.DefCoinType,
						},
						new GameTokenCoin.CoinGroup() {
							Value = (ulong)finalCoins,
							CoinType = ct ?? _data.gtc.coin.DefCoinType,
						}
					);
			_data.lotteryResult.lotteryResults.Clear();
            PurchaseHistory.Clear();
            ReslutBorder.Visibility = Visibility.Collapsed;
            _data.IsShowing = false;
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
            var buy = new Lottery.LotteryBuy
            {
                lotteryNumber = userNumbers,
                coinType = _data.gtc.coin.DefCoinType,
                coin = coinNum
            };
            var result = buy.Pay(_data.gtc);
            switch (result)
            {
                case 3:
					Toast.Show("代币不足，无法购买彩票！");
                    return;
                case 2:
                case 1:
					Toast.Show("未知错误，请联系mod作者处理！");
                    return;
            }
            _data.lottery.lotteryHave.Add(buy);
            PurchaseHistory.Add(FormatPurchaseBlock(buy));
            Toast.Show("购买彩票成功！\n您的下注代币数为{0}\n您下注的号码为:{1}".Translate(coinNum,userNumbers.ToString()));
            if (AutoClearByBuy.IsChecked == true) ClearAllNumbers_Click(null, null);
        }
        private void RandomBuyButton_Click(object sender, RoutedEventArgs e) {
            RandomButton_Click(null, null);
            BuyButton_Click(null, null);
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

        private void Reminder_Click(object sender, RoutedEventArgs e)
        {
            _data.IsShowResult = Reminder.IsChecked.HasValue ? Reminder.IsChecked.Value : false;
        }
		private void HelpButton_Click(object sender, RoutedEventArgs e) => LuckyGame.OpenHelpPage("lottery.html");

        private TextBlock FormatPurchaseBlock(Lottery.LotteryBuy lotteryBuy)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Inlines.Add(new Run("号码: ".Translate()) { Foreground = Brushes.White , FontWeight = FontWeights.Bold ,FontSize = 12 });
            textBlock.Inlines.Add(new Run(lotteryBuy.lotteryNumber.ToString()) { Foreground = Brushes.LightBlue, FontSize = 12 });
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new Run("下注代币数: ".Translate()) { Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 });
            textBlock.Inlines.Add(new Run(lotteryBuy.coin.ToString()) { Foreground = Brushes.LightGreen , FontSize = 12 });
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new Run("代币类型: ".Translate()) { Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 });
            textBlock.Inlines.Add(new Run(lotteryBuy.coinType.ToString()) { Foreground = Brushes.LightYellow , FontSize = 12 });
            return textBlock;
        }
	}
}