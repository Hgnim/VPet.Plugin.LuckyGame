using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame
{
    public class LuckyGame : MainPlugin {
		public LuckyGame(IMainWindow mainwin) : base(mainwin) {
		}
		public override string PluginName => "LuckyGame";

    }
}
