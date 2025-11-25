using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System.Data.SQLite;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal struct DataSave {
		const string mainKey = "LuckyGame";
		/// <summary>
		/// 保存数据
		/// </summary>
		internal static void Save(IMainWindow MW,GameTokenCoin gtc) {
			for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
				MW.GameSavesData[mainKey][(LinePutScript.gi64)GameTokenCoin.Coin.CoinKey[b]] =
					(long)gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b);
			MW.GameSavesData[mainKey][(LinePutScript.gint)"DefCoinType"] = (int)gtc.defCoinType;

			DatabaseBackupSave(MW,gtc);
		}
		/// <summary>
		/// 读取数据
		/// </summary>
		internal static void Read(IMainWindow MW, out GameTokenCoin.GameTokenCoin_Args gtcArg) {
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

			DatabaseBackupRead(ref gtcArg);
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
							CREATE TABLE IF NOT EXISTS Coin (
								KeyName TEXT PRIMARY KEY,
								Value INTEGER
							);
							CREATE TABLE IF NOT EXISTS Other (
								KeyName TEXT PRIMARY KEY,
								Value INTEGER
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

		private static void DatabaseBackupSave(IMainWindow MW, GameTokenCoin gtc) {
			try
			{
				using (SQLiteConnection sql = new(databaseBackupConnectStr))
				{
					sql.Open();
					string moreCommand = "";
					for (byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++)
					{
						moreCommand +=
							@$"INSERT OR REPLACE INTO Coin (KeyName, Value) VALUES ('{GameTokenCoin.Coin.CoinKey[b]}', '{gtc.GetCoinAmount((GameTokenCoin.Coin.CoinType)b)}');";
					}
					using (SQLiteCommand command = new(
						@$"
							{moreCommand}
							INSERT OR REPLACE INTO Other (KeyName, Value) 
								VALUES ('Money','{MW.Core.Save.Money}');
						"
					, sql))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch(SQLiteException ex)
			{
				MessageBoxX.Show("数据库备份保存失败！\n{0}".Translate(ex.Message), "错误".Translate());
            }
		}
		private static void DatabaseBackupRead(ref GameTokenCoin.GameTokenCoin_Args gtcArg) {
			using (SQLiteConnection sql = new(databaseBackupConnectStr)) {
				sql.Open();

				using (SQLiteCommand command = new(
					@$"
					SELECT KeyName, Value FROM Coin
					"
				, sql)) {
					using (SQLiteDataReader reader = command.ExecuteReader()) {
						while (reader.Read()) {
							for(byte b = 0; b < GameTokenCoin.Coin.CoinKey.Length; b++) {
								if(reader["KeyName"].ToString() == GameTokenCoin.Coin.CoinKey[b]) {

								}
							}
						}
					}
				}
			}
		}
	}
}
