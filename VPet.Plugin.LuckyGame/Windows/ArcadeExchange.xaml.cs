using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VPet.Plugin.LuckyGame.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Windows
{
    public partial class ArcadeExchange : Window
    {
        private double cashBalance { get => MainWindow.GameSavesData.GameSave.Money; }
        private GameTokenCoin TokenCoin;
        private IMainWindow MainWindow;
        private Point startPoint;
        private bool isDragging = false;
        private bool NeedUpdateUI = false;
        private Timer UIUpdadteTimer;
        internal ArcadeExchange(IMainWindow mainWindow, GameTokenCoin gtc)
        {
            MainWindow = mainWindow;
            UIUpdadteTimer = new Timer(async(e) =>
            {
                if (!NeedUpdateUI)
                    return;
                await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    UpdateDisplay();
                }));
                NeedUpdateUI = false;
            }, null, 0, 100);
           
            TokenCoin = gtc;
            gtc.coin.OnCoinChange += (type, amount, rate, fee) =>
            {
                NeedUpdateUI = true;
            };
            InitializeComponent();
            InitializeArcade();
        }

        private void InitializeArcade()
        {
            foreach(string name in GameTokenCoin.Coin.CoinName)
				CoinTypeSelect.Items.Add(name);
            CoinTypeSelect.SelectedIndex = (int)TokenCoin.coin.DefCoinType;
			UpdateDisplay();
        }

        internal static SolidColorBrush GetCoinColor(GameTokenCoin.Coin.CoinType coinType) {
			System.Drawing.Color dColor = GameTokenCoin.Coin.CoinColor[(int)coinType];
            return new SolidColorBrush(
					Color.FromArgb(
						dColor.A,
						dColor.R,
						dColor.G,
						dColor.B
						)
					);
		}
		private void UpdateDisplay()
        {
            CashBalanceText.Text = "¥ {0}".Translate(FormatWithSmartUnits(cashBalance));
            CoinBalanceText.Text = "🎮 {0}".Translate(TokenCoin.GetCoinAmount().ToString("N0"));
			CoinBalanceText.Foreground = GetCoinColor(TokenCoin.coin.DefCoinType);
		}

        private void TokenExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = TokenAmountText.Value ?? 0;
            if (amount > 0)
            {
                ProcessExchange(amount, true);
            }
            else
            {
                Toast.Show("请输入有效的兑换金额".Translate());
            }
        }

        private void MoneyExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = MoneyText.Value ?? 0;
            if (amount > 0)
            {
                ProcessExchange(amount, false);
            }
            else
            {
                Toast.Show("请输入有效的回收金额".Translate());
			}
        }

        private void ProcessExchange(double amount, bool isAdd)
        {
            var result = TokenCoin.ExchangeCoin(MainWindow,(ulong)amount, isAdd);
            switch(result)
            {
                case 0:
                    NeedUpdateUI = true;
                    Toast.Show("兑换成功！".Translate());
                    break;
                case 1:
                    Toast.Show("兑换失败，发生未知错误".Translate());
                    break;
                case 2:
                    Toast.Show("兑换失败，桌宠钱余额不足".Translate());
					break;
                case 3:
                    Toast.Show("兑换失败，游戏币余额不足".Translate());
					break;
                case 4:
                    Toast.Show("兑换失败，兑换金额必须非零".Translate());
					break;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TokenAmountText_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            var amount = TokenAmountText.Value ?? 0;
            if (amount is <= ulong.MaxValue and > 0) {
                var money = TokenCoin.GetExchangeNeedMoney((ulong)amount);
                TokenBlock.Text = "花费 {0} 金钱".Translate(money.ToString("N2"));
            }
            else
                TokenBlock.Text = "无效输入".Translate();
        }

        private void MoneyText_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            var amount = MoneyText.Value ?? 0;
            if (amount is <= ulong.MaxValue and > 0) {
                var money = TokenCoin.GetExchangeGetMoney((ulong)amount);
                MoneyBlock.Text = "获得 {0} 金钱".Translate(money.ToString("N2"));
            }
            else
				MoneyBlock.Text = "无效输入".Translate();
        }

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


        private string FormatWithSmartUnits(double number)
        {
            if (number == 0) return "0";

            double absValue = Math.Abs(number);
            string sign = number < 0 ? "-" : "";

            if (absValue >= 1000000000) // 十亿
            {
                return $"{sign}{(absValue / 1000000000):F1}B";
            }
            else if (absValue >= 1000000) // 百万
            {
                return $"{sign}{(absValue / 1_000000):F1}M";
            }
            else if (absValue >= 1000) // 千
            {
                return $"{sign}{(absValue / 1000):F1}K";
            }
            else if (absValue >= 100) // 100-999
            {
                return $"{sign}{absValue:F0}";
            }
            else if (absValue >= 1) // 1-99
            {
                return $"{sign}{absValue:F0}";
            }
            else // 小数
            {
                return $"{sign}{absValue:F2}";
            }
        }

		private void CoinTypeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(TokenCoin.coin.DefCoinType == (GameTokenCoin.Coin.CoinType)CoinTypeSelect.SelectedIndex) return;
            TokenCoin.coin.DefCoinType = (GameTokenCoin.Coin.CoinType)CoinTypeSelect.SelectedIndex;
            {
                uint cer = TokenCoin.GetCoinExchangeRate();
                byte cer_N = cer.ToString().Split('.').Length > 1 
                                ? (byte)cer.ToString().Split('.')[1].Length 
                                : (byte)0;
				CoinTypeSelect_Lable.Text = "{0} = {1} 金钱"
                    .Translate(GameTokenCoin.Coin.CoinName[(int)TokenCoin.coin.DefCoinType], cer.ToString($"N{cer_N}"));
				double cef = TokenCoin.GetCoinExchangeFee() * (double)100;
                byte cef_N = cef.ToString().Split('.').Length > 1 
                                ? (byte)cef.ToString().Split('.')[1].Length 
                                : (byte)0;
				CoinTypeSelect_Lable2.Text = "回收 {0} 收取 {1}% 手续费"
                    .Translate(GameTokenCoin.Coin.CoinName[(int)TokenCoin.coin.DefCoinType], cef.ToString($"N{cef_N}"));
            }
            TokenAmountText_ValueChanged(null, null);
			MoneyText_ValueChanged(null, null);
			UpdateDisplay();
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e) => LuckyGame.OpenHelpPage("exchangeCoin.html");
	}
}