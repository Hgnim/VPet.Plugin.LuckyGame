using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal struct DataSave {
		const string mainKey = "LuckyGame";
		/// <summary>
		/// 保存数据
		/// </summary>
		internal static void Save(IMainWindow MW,GameTokenCoin gtc) {
			MW.GameSavesData[mainKey][(LinePutScript.gstr)"bd"] = ThisSaveTag_DB;
			for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
				MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]] =
					(long)gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
			MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"] = (int)gtc.defCoinType;
			MW.GameSavesData[mainKey][(LinePutScript.gi64)"SaveTime"] = TimeData;
		}
		internal class ReadResult {
			/// <summary>
			/// 是否为第一次启动
			/// </summary>
			internal bool IsFirst { get; set; }
		}
		/// <summary>
		/// 读取数据
		/// </summary>
		internal static void Read(
			IMainWindow MW, 
			out ReadResult rr,
			out GameTokenCoin.GameTokenCoin_Args gtcArg, 
			out CoinExchangeLog_CheckResult celcr
		) {
			bool first;
			{ 
#nullable enable
				string? birthday = MW.GameSavesData[mainKey][(LinePutScript.gstr)"bd"];
#nullable disable
				first = (birthday is null or "");
				birthday ??= TimeData.ToString();
				thisSaveTag = $"{MW.PrefixSave}:{birthday}";
			}
			gtcArg = new() { 
				coins =new ulong[GameTokenCoin.Coin.CoinKey.Length],
			};
			for(byte b= 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
				try {
					long? c = MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]];
					gtcArg.coins[b] = c is not null
						? (ulong)c
						: 0;
				} catch { gtcArg.coins[b] = 0; }
			}
			try{
				int? dct = MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"];
				gtcArg.defCoiType = dct is not null 
					? (GameTokenCoin.Coin.CoinType)dct 
					: GameTokenCoin.Coin.CoinType.coinBlack;
			} catch { gtcArg.defCoiType = GameTokenCoin.Coin.CoinType.coinBlack; }

			celcr = !first 
				? CoinExchangeLog_Check(MW.GameSavesData[mainKey][(LinePutScript.gi64)"SaveTime"]) 
				: null;

			rr = new() {
				IsFirst = first,
			};
		}

        const string databaseBackupConnectStr = "Data Source=lgbk.db;Version=3;";

        public static void EnsureDatabaseBackup() {
			try
			{
				using (SQLiteConnection sql = new(databaseBackupConnectStr))
				{
					sql.Open();
					using (SQLiteCommand command = new(
						@"
							CREATE TABLE IF NOT EXISTS CoinExchangeLog (
								Id INTEGER PRIMARY KEY AUTOINCREMENT,
								SaveTag TEXT NOT NULL,
								Time LONG NOT NULL,
								CoinKey TEXT NOT NULL,
                                CoinChange TEXT NOT NULL,
								MoneyChange TEXT,
								Note TEXT
							);
						"
					, sql))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (SQLiteException ex)
			{
				MessageBoxX.Show("数据库备份初始化失败！\n{0}".Translate(ex.Message), "错误".Translate());
			}
        }

		/// <summary>
		/// 统一时间数据获取
		/// </summary>
		internal static long TimeData => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
#nullable enable
		private static string? thisSaveTag = null;
#nullable disable
		/// <summary>
		/// 该存档标识
		/// </summary>
		internal static string ThisSaveTag => thisSaveTag ?? throw new Exception("thisSaveTag未初始化！");
		/// <summary>
		/// 获取存档名前缀信息
		/// </summary>
		internal static string ThisSaveTag_PrefixSave {
			get {
				string[] s = ThisSaveTag.Split(':');
				string all="";
				//特意选了windows文件名不支持的':'用于做分隔符。但为了以防万一，用更保险的方法获取前缀名
				for(byte b = 0; b < s.Length - 1; b++) {
					if (b != 0)
						all += ':';
					all += s[b];
				}
				return all;
			}
		}
		/// <summary>
		/// 获取birthday信息
		/// </summary>
		internal static string ThisSaveTag_DB => ThisSaveTag.Split(':')[^1];
		internal class CoinExchangeLog {
			/// <summary>
			/// 为true调用函数时不会写入数据库
			/// </summary>
			internal bool DisThisTime { get; set; } = false;
			internal string SaveTag { get; set; } = ThisSaveTag;
			internal long Time { get; set; } = TimeData;
			internal /*required*/ string CoinKey { get; set; }
			internal /*required*/ string CoinChange { get; set; }
#nullable enable
			internal string? MoneyChange { get; set; } = null;
			internal string? Note { get; set; } = null;
#nullable disable
			/// <summary>
			/// 为true时，其它值将根据参数或默认值补全<br/>
			/// 用于ChangeCoin函数
			/// </summary>
			internal bool OnlyNote { get; set; } = false;

		}
		/// <summary>
		/// 代币更改日志插入
		/// </summary>
		/// <param name="cel">日志信息</param>
		internal static async void CoinExchangeLog_Insert(CoinExchangeLog cel) => await Task.Run(() => {
			if (cel.CoinKey != null && cel.CoinChange != null) {
				if (!cel.DisThisTime)
					using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
						sql.Open();
						using (SQLiteCommand command = new(
							@$"
						INSERT INTO CoinExchangeLog (SaveTag, Time, CoinKey, CoinChange, MoneyChange, Note) 
								VALUES ('{cel.SaveTag}', '{cel.Time}', '{cel.CoinKey}', '{cel.CoinChange}', '{cel.MoneyChange}', '{cel.Note}');
						"
						, sql)) {
							command.ExecuteNonQuery();
						}
					}
			}
			else throw new Exception("CoinExchangeLog_Insert函数中有关键参数为null");
		});
		internal class CoinExchangeLog_CheckResult {
			/// <summary>
			/// 检查是否有差异
			/// </summary>
			internal bool haveDiff;
			/// <summary>
			/// 检查是否有数据<br/>
			/// 如果存档中有数据而数据库没有，则表示数据库被删除或更改
			/// </summary>
			internal bool haveData;
			/// <summary>
			/// 需要回滚的代币数
			/// </summary>
			internal long[] coinBack;
			/// <summary>
			/// 需要回滚的桌宠币数
			/// </summary>
			internal double moneyBack;
		}
		/// <summary>
		/// 代币更改日志检查
		/// </summary>
		/// <param name="saveTime">存档保存时间，从存档中读取</param>
		/// <returns>
		/// 检查结果<br/>
		/// </returns>
		private static CoinExchangeLog_CheckResult CoinExchangeLog_Check(long saveTime) {
			CoinExchangeLog_CheckResult celcr = new() {
				haveDiff = false,//如果存档和数据库的时间对不上，则会被设置为true
#pragma warning disable IDE0300
				coinBack = new long[5] { 0, 0, 0, 0, 0 },
#pragma warning restore IDE0300
				moneyBack = 0,
			};
			using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
				sql.Open();

				using (SQLiteCommand command = new(
					@$"
					SELECT * FROM CoinExchangeLog
					WHERE Time >= {saveTime} AND SaveTag = '{ThisSaveTag}';
					"
				, sql)) {
					using (SQLiteDataReader reader = command.ExecuteReader()) {
						static GameTokenCoin.Coin.CoinType keyToType(string key) {
							for(byte b=0;b<GameTokenCoin.Coin.CoinKey.Length;b++) {
								if (GameTokenCoin.Coin.CoinKey[b] == key) {
									return (GameTokenCoin.Coin.CoinType)b;
								}
							}
							throw new Exception("keyToType参数异常");
						}
						while (reader.Read()) {
							if (!celcr.haveDiff) celcr.haveDiff = true;
							long cc = Convert.ToInt64(reader["CoinChange"]);
							double? mc = mc = double.TryParse(reader["MoneyChange"].ToString(), out double mc_res) ? mc_res : null;
							celcr.coinBack[(int)keyToType(reader["CoinKey"].ToString())] += cc;
							if (mc != null) celcr.moneyBack += (double)mc;
						}
					}
				}
				if (!celcr.haveDiff) {
					using (SQLiteCommand command = new(
					@$"
					SELECT * FROM CoinExchangeLog
					WHERE SaveTag = '{ThisSaveTag}';
					"
					, sql)) {
						using (SQLiteDataReader reader = command.ExecuteReader()) {
							celcr.haveData = reader.Read();
						}
					}
				}
			}
			return celcr;
		}
	}
}
