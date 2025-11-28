using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
﻿using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Core.Game;
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
        internal Data data;
        public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";
        public override void LoadPlugin()
        {
            //try { 
            DataSave.EnsureDatabaseBackup();
            DataSave.Read(
                MW, 
                out DataSave.ReadResult rr, 
                out GameTokenCoin.GameTokenCoin_Args gtcArg, 
                out DataSave.CoinExchangeLog_CheckResult celcr,
                out List<Lottery.LotteryBuy> lllb
            );
            data = new() {
                gtc = new GameTokenCoin(gtcArg),
                lottery = new() { lotteryHave = lllb },
            };
            if (!rr.IsFirst) {
                void ClearCoin(string note) {
					for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
						ulong clearCoin = data.gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
						data.gtc.ChangeCoin(
							clearCoin,
							false,
							(GameTokenCoin.Coin.CoinType)b,
							new() {
								SaveTag = DataSave.ThisSaveTag,
								CoinKey = Coin.CoinKey[b],
								CoinChange = $"-{clearCoin}",
								Note = note,
							},
							true
						);
					}
				}
                if (celcr.haveDiff) {//是否与数据库存在差异
                    for(byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
                        bool isAdd;
                        if (celcr.coinBack[b] > 0)
                            isAdd = true;
                        else if (celcr.coinBack[b] < 0)
                            isAdd = false;
                        else
                            break;
                        data.gtc.ChangeCoin(
                            (ulong)Math.Abs(celcr.coinBack[b]),
                            isAdd,
                            (GameTokenCoin.Coin.CoinType)b,
                            new() {
								SaveTag = DataSave.ThisSaveTag,
								CoinKey = Coin.CoinKey[b],
								CoinChange = $"{(isAdd ? '+' : '-')}{Math.Abs(celcr.coinBack[b])}",
								Note = "数据异常，代币回滚",//注意此行不要翻译，在别处有对其文本的判断
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
							Note = "数据异常，金钱回滚",//注意此行不要翻译，在别处有对其文本的判断
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
                    ClearCoin("数据丢失，代币清除");//注意此行不要翻译，在别处有对其文本的判断
					MessageBoxX.Show("检测到幸运游戏数据丢失", "错误");//用于测试，后期将润色
				}
                if (rr.DbHashPass == false) {
					ClearCoin("数据篡改，代币清除");//注意此行不要翻译，在别处有对其文本的判断
					MessageBoxX.Show("检测到幸运游戏数据被篡改", "错误");//用于测试，后期将润色
				}
            }
            //}catch(Exception ex) { ErrorHelper.ShowError(ex); }
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
                arcadeExchangeWindow = new ArcadeExchange(MW, data.gtc);
                arcadeExchangeWindow.Show();
            }
            else
            {
                arcadeExchangeWindow = new ArcadeExchange(MW, data.gtc);
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
                fortuneWindow = new Fortune(data.gtc);
                fortuneWindow.Show();
            }
            else
            {
                fortuneWindow = new Fortune(data.gtc);
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
                lotteryWindow = new LotteryPage(data.gtc);
                lotteryWindow.Show();
            }
            else
            {
                lotteryWindow = new LotteryPage(data.gtc);
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
            DataSave.Save(MW, data);
			base.Save();
		}
		public override void EndGame() {
            DataSave.Save(MW, data);
			base.EndGame();
		}
    }
}
