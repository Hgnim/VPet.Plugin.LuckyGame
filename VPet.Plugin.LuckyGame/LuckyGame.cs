using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
﻿using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Core.Tool;
using VPet.Plugin.LuckyGame.Windows;
using VPet_Simulator.Windows.Interface;
using static VPet.Plugin.LuckyGame.Core.GameTokenCoin;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
		public Fortune fortuneWindow;
        public ArcadeExchange arcadeExchangeWindow;
        public LotteryPage lotteryWindow;
        internal GameTokenCoin gtc;
        public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";
        public override void LoadPlugin()
        {
            try { 
            DataSave.EnsureDatabaseBackup();
            DataSave.Read(
                MW, 
                out DataSave.ReadResult rr, 
                out GameTokenCoin.GameTokenCoin_Args gtcArg, 
                out DataSave.CoinExchangeLog_CheckResult celcr
            );
			gtc = new GameTokenCoin(gtcArg);
            if (!rr.IsFirst) {
                if (celcr.haveDiff) {//是否与数据库存在差异
                    for(byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
                        bool isAdd;
                        if (celcr.coinBack[b] > 0)
                            isAdd = true;
                        else if (celcr.coinBack[b] < 0)
                            isAdd = false;
                        else
                            break;
                        gtc.ChangeCoin(
                            (ulong)Math.Abs(celcr.coinBack[b]),
                            isAdd,
                            (GameTokenCoin.Coin.CoinType)b,
                            new() {
								SaveTag = DataSave.ThisSaveTag,
								CoinKey = Coin.CoinKey[b],
								CoinChange = $"{(isAdd ? '+' : '-')}{Math.Abs(celcr.coinBack[b])}",
								Note = "数据异常，代币回滚",
							},
                            true
                        );
                    }
                    if (celcr.moneyBack != 0) {
                        MW.Core.Save.Money += celcr.moneyBack;
                        DataSave.CoinExchangeLog_Insert(new() {
							SaveTag = DataSave.ThisSaveTag,
							CoinKey = Coin.CoinKey[0],
							CoinChange = "0",
							MoneyChange = $"{(celcr.moneyBack>0 ? '+' : '-')}{Math.Abs(celcr.moneyBack)}",
							Note = "数据异常，金钱回滚",
						});
					}
					MessageBoxX.Show("检测到幸运游戏数据异常", "错误");//用于测试，后期将润色
				}
                else if (!celcr.haveData) {//数据库中是否存在数据
                    /*gtc.coin.CoinBlack = 0;
                    gtc.coin.CoinBlue = 0;
                    gtc.coin.CoinGreen = 0;
                    gtc.coin.CoinRed = 0;
                    gtc.coin.CoinWhite = 0;*/
                    for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
                        ulong clearCoin = gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
                        gtc.ChangeCoin(
                            clearCoin,
                            false,
                            (GameTokenCoin.Coin.CoinType)b,
                            new() {
                                SaveTag = DataSave.ThisSaveTag,
                                CoinKey = Coin.CoinKey[b],
                                CoinChange = $"-{clearCoin}",
                                Note = "数据丢失，代币清除",
                            },
                            true
                        );
                    }
					MessageBoxX.Show("检测到幸运游戏数据丢失", "错误");//用于测试，后期将润色
				}
            }
            }catch(Exception ex) { ErrorHelper.ShowError(ex); }
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

        private void LotteryMenu_Click(object sender, RoutedEventArgs e)
        {
            if(lotteryWindow != null && lotteryWindow.IsVisible)
            {
                lotteryWindow.Activate();
                return;
            }
            else if(lotteryWindow != null && !lotteryWindow.IsVisible)
            {
                lotteryWindow.Close();
                lotteryWindow = null;
                lotteryWindow = new LotteryPage(gtc);
                lotteryWindow.Show();
            }
            else
            {
                lotteryWindow = new LotteryPage(gtc);
                lotteryWindow.Show();
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
                var LotteryMenu = new MenuItem
                {
                    Header = "Lottery".Translate(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                };
                LotteryMenu.Click += LotteryMenu_Click;
                menu.Items.Add(fortune);
                menu.Items.Add(TokenExchangeMenu);
                menu.Items.Add(LotteryMenu);
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
