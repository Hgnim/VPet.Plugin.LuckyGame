using VPet.Plugin.LuckyGame.Core;
using VPet.Plugin.LuckyGame.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
		public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";

		GameTokenCoin gtc;

		public override void LoadPlugin() {
			gtc = new();//此处需要将保存的数据传入，目前暂时使用默认值
			//临时测试使用
			Fortune f = new(ref gtc);
			f.Show();
		}
    }
}
