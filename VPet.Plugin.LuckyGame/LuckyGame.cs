using VPet.Plugin.LuckyGame.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
		public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";
		public override void LoadPlugin() {
			//临时测试使用
			Fortune f = new();
			f.Show();
		}
    }
}
