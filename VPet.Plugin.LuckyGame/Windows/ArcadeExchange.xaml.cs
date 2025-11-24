using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        internal ArcadeExchange(IMainWindow mainWindow, GameTokenCoin gtc)
        {
            MainWindow = mainWindow;
            TokenCoin = gtc;
            InitializeComponent();
            InitializeArcade();
        }

        private void InitializeArcade()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            CashBalanceText.Text = "¥ {0}".Translate(FormatWithSmartUnits(cashBalance));
            CoinBalanceText.Text = "🎮 {0}".Translate(TokenCoin.coin.CoinBlack);
        }

        private void TokenExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = TokenAmountText.Value ?? 0;
            if (amount > 0)
            {
                ProcessExchange(amount);
            }
            else
            {
                ShowMessage("请输入有效的兑换金额", "warn");
            }
        }

        private void MoneyExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = MoneyText.Value ?? 0;
            if (amount > 0)
            {
                ProcessExchange(-amount);
            }
            else
            {
                ShowMessage("请输入有效的回收金额", "warn");
            }
        }

        private void ProcessExchange(double amount)
        {
            var result = TokenCoin.ExchangeCoin(MainWindow,GameTokenCoin.Coin.CoinType.coinBlack, (long)amount);
            switch(result)
            {
                case 0:
                    UpdateDisplay();
                    ShowMessage($"兑换成功！");
                    break;
                case 1:
                    ShowMessage("兑换失败，发生未知错误","error");
                    break;
                case 2:
                    ShowMessage("兑换失败，桌宠钱余额不足","warn");
                    break;
                case 3:
                    ShowMessage("兑换失败，游戏币余额不足","warn");
                    break;
                case 4:
                    ShowMessage("兑换失败，兑换金额必须非零","warn");
                    break;
            }
        }

        private void ShowMessage(string message,string title = "info")
        {
            MessageBoxX.Show(message.Translate(),title.Translate());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TokenAmountText_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            var amount = TokenAmountText.Value ?? 0;
            if (amount is <= ulong.MaxValue and > 0) {
                var money = TokenCoin.GetExchangeNeedMoney(GameTokenCoin.Coin.CoinType.coinBlack, (ulong)amount);
                TokenBlock.Text = "花费 {0:F2} 金钱".Translate(money);
            }
            else
                TokenBlock.Text = "无效输入".Translate();
        }

        private void MoneyText_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            var amount = MoneyText.Value ?? 0;
            if (amount is <= ulong.MaxValue and > 0) {
                var money = TokenCoin.GetExchangeGetMoney(GameTokenCoin.Coin.CoinType.coinBlack, (ulong)amount);
                MoneyBlock.Text = "获得 {0:F2} 金钱".Translate(money);
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
            else if (absValue >= 10000) // 万（中文习惯）
            {
                return $"{sign}{(absValue / 10000):F1}万";
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
    }
}