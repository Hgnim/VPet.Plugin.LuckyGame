using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VPet.Plugin.LuckyGame.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Windows
{
    public partial class ArcadeExchange : Window
    {
        private double cashBalance = 0.0;
        private int gameCoins = 2500;
        private GameTokenCoin TokenCoin;
        private IMainWindow MainWindow;
        public ArcadeExchange(IMainWindow mainWindow)
        {
            MainWindow = mainWindow;
            InitializeComponent();
            InitializeArcade();
        }

        private void InitializeArcade()
        {
            TokenCoin = new();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            CashBalanceText.Text = $"¥ {cashBalance:N2}";
            CoinBalanceText.Text = $"🎮 {gameCoins:N0} 枚";
        }

        private void ExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && double.TryParse(button.Tag.ToString(), out double amount))
            {
                ProcessExchange(amount);
            }
        }

        private void CustomExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(CustomAmountText.Text, out double amount) && amount > 0)
            {
                ProcessExchange(amount);
            }
            else
            {
                ShowMessage("请输入有效的兑换金额", "warn");
            }
        }

        private void ProcessExchange(double amount)
        {
            var result = TokenCoin.ChangeCoin(MainWindow, GameTokenCoin.CoinType.coinBlack, (long)amount);
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

        private int GetPackageBonus(double amount)
        {
            return amount switch
            {
                10 => 10,
                20 => 20,
                50 => 100,
                100 => 300,
                200 => 800,
                _ => (int)(amount * 1) // 自定义兑换无额外奖励或基础奖励
            };
        }

        private void ShowMessage(string message,string title = "info")
        {
            MessageBoxX.Show(message.Translate(),title.Translate());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}