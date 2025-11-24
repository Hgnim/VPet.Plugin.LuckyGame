using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows.Controls;
using VPet.Plugin.LuckyGame.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
        public Fortune fortuneWindow;
        public ArcadeExchange arcadeExchangeWindow;
        public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";
        public override void LoadPlugin()
        {
            
        }

        public override void GameLoaded()
        {
            var TokenExchangeMenu = new MenuItem
            {
                Header = "TokenExchangge".Translate(),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
            TokenExchangeMenu.Click += TokenExchangeMenu_Click;
            MW.Main.ToolBar.MenuFeed.Items.Add(TokenExchangeMenu);
        }

        private void TokenExchangeMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(arcadeExchangeWindow != null && arcadeExchangeWindow.IsVisible)
            {
                arcadeExchangeWindow.Activate();
                return;
            }
            else if(arcadeExchangeWindow != null && !arcadeExchangeWindow.IsVisible)
            {
                arcadeExchangeWindow.Close();
                arcadeExchangeWindow = null;
                arcadeExchangeWindow = new ArcadeExchange(MW);
                arcadeExchangeWindow.Show();
            }
            else
            {
                arcadeExchangeWindow = new ArcadeExchange(MW);
                arcadeExchangeWindow.Show();
            }
        }

        private void Fortune_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(fortuneWindow != null && fortuneWindow.IsVisible)
            {
                fortuneWindow.Activate();
                return;
            }
            else if(fortuneWindow != null && !fortuneWindow.IsVisible)
            {
                fortuneWindow.Close();
                fortuneWindow = null;
                fortuneWindow = new Fortune();
                fortuneWindow.Show();
            }
            else
            {
                fortuneWindow = new Fortune();
                fortuneWindow.Show();
            }
        }

        public override void Setting()
        {
            Fortune_Click(this, null);
        }

        public override void LoadDIY()
        {
            try
            {
                var menu = new MenuItem
                {
                    Header = "Lucky Game".Translate(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                };
                var fortune = new MenuItem
                {
                    Header = "Fortune".Translate(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                fortune.Click += Fortune_Click;
                menu.Items.Add(fortune);
                MW.Main.ToolBar.MenuDIY.Items.Add(menu);
            }
            catch (Exception ex)
            {
                MessageBoxX.Show("DIY列表加载错误{0}".Translate(ex.Message), "错误".Translate());
            }
        }
    }
}
