using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.LuckyGame.Core.Game;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal struct DataSave {
		/// <summary>
		/// 插件所在目录
		/// </summary>
		internal static readonly string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		const string mainKey = "LuckyGame";
		/// <summary>
		/// 保存数据
		/// </summary>
		internal static void Save(IMainWindow MW, Data dat) {
			MW.GameSavesData[mainKey][(LinePutScript.gstr)"bd"] = ThisSaveTag_BD;
			for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
				MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]] =
					(long)dat.gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
			MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"] = (int)dat.gtc.coin.DefCoinType;
			MW.GameSavesData[mainKey][(LinePutScript.gi64)"SaveTime"] = TimeData;
			LotteryHave_Save([.. dat.lottery.lotteryHave]);

			DatabaseHash_Save(true);//保存哈希值，最后执行
		}
		internal class ReadResult {
			/// <summary>
			/// 是否为第一次启动
			/// </summary>
			internal required bool IsFirst { get; set; }

			/// <summary>
			/// 数据库哈希检查是否通过
			/// </summary>
			internal required bool? DbHashPass {  get; set; }
			/// <summary>
			/// 是否包含数据库相关文件
			/// </summary>
			internal required bool HaveDbFile { get; set; }
		}
		/// <summary>
		/// 读取数据
		/// </summary>
		internal static void Read(
			IMainWindow MW,
			out ReadResult rr,
			out GameTokenCoin.GameTokenCoin_Args gtcArg,
			out CoinExchangeLog_CheckResult celcr,
			out List<Lottery.LotteryBuy> lllb
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
			bool? dbHashCheck = null;
			bool haveDbFile = File.Exists(databaseBackupFileName) && File.Exists(databaseBackupHashFileName);
			//if (first)//不对EnsureDatabaseBackup();函数进行判断，避免后续新增表的时候不执行
			EnsureDatabaseBackup();
			DatabaseHash_StreamInit();
			if (!first && haveDbFile) 
				dbHashCheck = DatabaseHash_Check();

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
				DbHashPass = dbHashCheck,
				HaveDbFile = haveDbFile,
			};

			lllb = LotteryHave_Get();
		}

		const string databaseBackupFileName = "lgbk.db";
		const string databaseBackupHashFileName = "lgbk-hash.bin";

		const string databaseBackupConnectStr = $"Data Source={databaseBackupFileName};Version=3;";

        private static void EnsureDatabaseBackup() {
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
							CREATE TABLE IF NOT EXISTS LotteryHave (
								Id INTEGER PRIMARY KEY AUTOINCREMENT,
								LotteryNumber TEXT NOT NULL,
								Coin TEXT NOT NULL,
								CoinType INTEGER NOT NULL
							);
						"/*将哈希值存入数据库的方案，失败。暂时保留备用
							CREATE TABLE IF NOT EXISTS Data_TextValue (
								Id TEXT PRIMARY KEY,
								Value TEXT
							)  WITHOUT ROWID;
						"*/
					, sql))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (SQLiteException ex)
			{
				MessageBoxX.Show("数据初始化失败！\n{0}".Translate(ex.Message), "错误".Translate());
			}
        }
		/*将哈希值存入数据库的方案，失败。暂时保留备用
		private struct DatabaseHash {
			/// <summary>
			/// 获取数据库的哈希值
			/// </summary>
			/// <returns></returns>
			private static string GetDatabaseBackupHash() {
				using (SHA256 sha256 = SHA256.Create()) {
					using (FileStream stream = File.OpenRead(databaseBackupFileName)) {
						byte[] hash = sha256.ComputeHash(stream);
						return BitConverter.ToString(hash).Replace("-", "");
					}
				}
			}
			private static void WriteDb(string value = "ReadAndWrite") {
				using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
					sql.Open();
					using (var transaction = sql.BeginTransaction()) {
						using (SQLiteCommand command = new(
						@$"
						INSERT INTO Data_TextValue (Id, Value)
							VALUES ('Hash', '{value}')
						ON CONFLICT(Id) DO 
							UPDATE SET Value = '{value}';
						",
						sql, transaction)) {
							command.ExecuteNonQuery();
						}
						transaction.Commit();//等待其确保写入完成
					}
				}
			}
			private static string ReadDb() {
				using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
					sql.Open();

					using (SQLiteCommand command = new(
						@$"
					SELECT Value FROM Data_TextValue
						WHERE Id = 'Hash';
					"
					, sql)) {
						using (SQLiteDataReader reader = command.ExecuteReader()) {
							return reader.Read() 
								? reader["Value"].ToString() 
								: null;
						}
					}
				}
			}
			internal static void DatabaseHash_Save() {
				string hash;
				WriteDb();//空写以写入可控的哈希值
				Thread.Sleep(1000);
				hash = GetDatabaseBackupHash();
				Thread.Sleep(1000);
				WriteDb(hash);
			}
			internal static bool DatabaseHash_Check() {
				string readHash, hash;
				readHash = ReadDb();
				WriteDb();//空写以还原至获取哈希值时的状态
				hash = GetDatabaseBackupHash();
				MessageBox.Show(readHash + '\n' + hash);
				return readHash == hash;
			}
		}
		*/
		/// <summary>
		/// 获取数据库的哈希值
		/// </summary>
		/// <returns></returns>
		private static byte[] GetDatabaseBackupHash() {
			while (true) {
				try {
					using (SHA256 sha256 = SHA256.Create()) {
						using (FileStream stream = new(databaseBackupFileName, FileMode.Open, FileAccess.Read)) {
							return sha256.ComputeHash(stream);
						}
					}
				} catch { }
			}
		}
		static FileStream databaseHashFs;
		private static void DatabaseHash_Save(bool lastSave = false) {
			if (databaseHashFs == null || !databaseHashFs.CanRead || !databaseHashFs.CanWrite) {//如果流不可访问则重新初始化
				databaseHashFs?.Dispose();
				DatabaseHash_StreamInit();
			}
			databaseHashFs.Seek(0, SeekOrigin.Begin);//重置位置，避免出现不可读或不可写的报错
			databaseHashFs.SetLength(0);//清理旧数据

			using (BinaryWriter bw = new(databaseHashFs, Encoding.UTF8, true)) {
				bw.Write(GetDatabaseBackupHash());
				bw.Flush();
			}
			databaseHashFs.Flush(true);

			if (lastSave) databaseHashFs.Close();
		}
		private static bool DatabaseHash_Check() {
			if (databaseHashFs == null || !databaseHashFs.CanRead || !databaseHashFs.CanWrite) {//如果流不可访问则重新初始化
				databaseHashFs?.Dispose();
				DatabaseHash_StreamInit();
			}
			databaseHashFs.Seek(0, SeekOrigin.Begin);//重置位置，避免出现不可读或不可写的报错

			if (databaseHashFs.Length == 0)//如果为空直接返回
				return false;

			using (BinaryReader br = new(databaseHashFs, Encoding.UTF8, true)) {
				return br.ReadBytes((int)databaseHashFs.Length).SequenceEqual(GetDatabaseBackupHash());
			}
		}
		private static void DatabaseHash_StreamInit() =>
			databaseHashFs = new FileStream(
		databaseBackupHashFileName,
		FileMode.OpenOrCreate,
		FileAccess.ReadWrite,
		FileShare.ReadWrite | FileShare.Delete,//允许多次访问和删除
		bufferSize: 4096,
		FileOptions.RandomAccess//优化随机读写
				);

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
		internal static string ThisSaveTag_BD => ThisSaveTag.Split(':')[^1];
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
				if (!cel.DisThisTime) {
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
					DatabaseHash_Save();
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
							if (reader["Note"].ToString() is not "数据异常，代币回滚" and not "数据异常，金钱回滚") {//避免重复读取修复日志
								if (!celcr.haveDiff) celcr.haveDiff = true;
								long cc = Convert.ToInt64(reader["CoinChange"]);
								double? mc = mc = double.TryParse(reader["MoneyChange"].ToString(), out double mc_res) ? mc_res : null;
								celcr.coinBack[(int)keyToType(reader["CoinKey"].ToString())] += cc;
								if (mc != null) celcr.moneyBack += (double)mc;
							}
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

		private static void LotteryHave_Save(Lottery.LotteryBuy[] lBuy) {
			using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
				sql.Open();
				using (SQLiteCommand command = new(
					@$"
						DELETE FROM LotteryHave;
						"
				, sql)) {
					command.ExecuteNonQuery();
				}

				foreach (Lottery.LotteryBuy lb in lBuy) {
					string lotNum = "";
					foreach(byte n in lb.lotteryNumber.MainNumber) {
						lotNum += $"{n},";
					}
					lotNum = lotNum[..^1];//去掉末尾间隔符
					lotNum += ';';
					foreach(byte n in lb.lotteryNumber.DeputyNumber) {
						lotNum += $"{n},";
					}
					lotNum = lotNum[..^1];
					using (SQLiteCommand command = new(
						@$"
						INSERT INTO LotteryHave (LotteryNumber, Coin, CoinType) 
								VALUES ('{lotNum}', '{lb.coin}', '{lb.coinType}');
						"
					, sql)) {
						command.ExecuteNonQuery();
					}
				}
			}
		}
		private static List<Lottery.LotteryBuy> LotteryHave_Get() {
			List<Lottery.LotteryBuy> buys = [];
			using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
				sql.Open();

				using (SQLiteCommand command = new(
					@$"
					SELECT * FROM LotteryHave;
					"
				, sql)) {
					using (SQLiteDataReader reader = command.ExecuteReader()) {
						while (reader.Read()) {
							Lottery.LotteryBuy buy=new();
							{
								List<byte> mainNum = [];
								List<byte> depuNum = [];
								string ln = reader["LotteryNumber"].ToString();
								foreach(string n in ln.Split(';')[0].Split(',')) {
									mainNum.Add(Convert.ToByte(n));
								}
								foreach (string n in ln.Split(';')[1].Split(',')) {
									depuNum.Add(Convert.ToByte(n));
								}
								buy.lotteryNumber = new() {
									MainNumber= [.. mainNum],
									DeputyNumber= [.. depuNum]
								};
							}
							buy.coin = Convert.ToUInt64(reader["Coin"]);
							buy.coinType = (GameTokenCoin.Coin.CoinType)Convert.ToInt32(reader["CoinType"]);
							buys.Add(buy);
						}
					}
				}
			}
			return buys;
		}
	}
}
