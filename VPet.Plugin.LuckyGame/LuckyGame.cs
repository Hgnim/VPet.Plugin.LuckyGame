using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows.Controls;
﻿using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
        public Fortune fortuneWindow;
        public ArcadeExchange arcadeExchangeWindow;
        internal GameTokenCoin gtc;
        public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";
        public override void LoadPlugin()
        {
            DataSave.EnsureDatabaseBackup();
            DataSave.Read(MW, out GameTokenCoin.GameTokenCoin_Args gtcArg);
			gtc = new GameTokenCoin(gtcArg);
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
                arcadeExchangeWindow = new ArcadeExchange(MW, gtc);
                arcadeExchangeWindow.Show();
            }
            else
            {
                arcadeExchangeWindow = new ArcadeExchange(MW, gtc);
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
                fortuneWindow = new Fortune(gtc);
                fortuneWindow.Show();
            }
            else
            {
                fortuneWindow = new Fortune(gtc);
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
                var TokenExchangeMenu = new MenuItem
                {
                    Header = "TokenExchangge".Translate(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                };
                TokenExchangeMenu.Click += TokenExchangeMenu_Click;
                menu.Items.Add(fortune);
                menu.Items.Add(TokenExchangeMenu);
                MW.Main.ToolBar.MenuDIY.Items.Add(menu);
            }
            catch (Exception ex)
            {
                MessageBoxX.Show("DIY列表加载错误{0}".Translate(ex.Message), "错误".Translate());
            }
        }

		public override void Save() {
            DataSave.Save(MW, gtc);
			base.Save();
		}
		public override void EndGame() {
            DataSave.Save(MW, gtc);
			base.EndGame();
		}
    }
}
