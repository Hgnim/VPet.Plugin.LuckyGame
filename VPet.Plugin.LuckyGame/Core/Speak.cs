using System;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.LuckyGame.Core {
	internal class Speak {
		internal enum SayType {
			placeVeryHighWin, placeHighWin, placeMidWin, placeLowWin, placeVeryLowWin,
			placeVeryHighLose, placeHighLose, placeMidLose, placeLowLose, placeVeryLowLose,
		}
		internal readonly ClickText[] placeVeryHighWin;
		internal readonly ClickText[] placeHighWin;
		internal readonly ClickText[] placeMidWin;
		internal readonly ClickText[] placeLowWin;
		internal readonly ClickText[] placeVeryLowWin;

		internal readonly ClickText[] placeVeryHighLose;
		internal readonly ClickText[] placeHighLose;
		internal readonly ClickText[] placeMidLose;
		internal readonly ClickText[] placeLowLose;
		internal readonly ClickText[] placeVeryLowLose;

		internal static string GetRandomSpeakText(ClickText[] speakTexts) =>
			speakTexts[new Random().Next(speakTexts.Length)].TranslateText;

		internal void DoSpeak(SayType type, IMainWindow MW) {
			switch (type) {
				case SayType.placeVeryHighWin:
					MW.Main.Say(GetRandomSpeakText(placeVeryHighWin));
					break;
				case SayType.placeHighWin:
					MW.Main.Say(GetRandomSpeakText(placeHighWin));
					break;
				case SayType.placeMidWin:
					MW.Main.Say(GetRandomSpeakText(placeMidWin));
					break;
				case SayType.placeLowWin:
					MW.Main.Say(GetRandomSpeakText(placeLowWin));
					break;
				case SayType.placeVeryLowWin:
					MW.Main.Say(GetRandomSpeakText(placeVeryLowWin));
					break;

				case SayType.placeVeryHighLose:
					MW.Main.Say(GetRandomSpeakText(placeVeryHighLose));
					break;
				case SayType.placeHighLose:
					MW.Main.Say(GetRandomSpeakText(placeHighLose));
					break;
				case SayType.placeMidLose:
					MW.Main.Say(GetRandomSpeakText(placeMidLose));
					break;
				case SayType.placeLowLose:
					MW.Main.Say(GetRandomSpeakText(placeLowLose));
					break;
				case SayType.placeVeryLowLose:
					MW.Main.Say(GetRandomSpeakText(placeVeryLowLose));
					break;
			}
		}

		internal Speak(IMainWindow MW) {
			placeVeryHighWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryHighWin")];
			placeHighWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeHighWin")];
			placeMidWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeMidWin")];
			placeLowWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeLowWin")];
			placeVeryLowWin = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryLowWin")];

			placeVeryHighLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryHighLose")];
			placeHighLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeHighLose")];
			placeMidLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeMidLose")];
			placeLowLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeLowLose")];
			placeVeryLowLose = [.. MW.ClickTexts.FindAll(x => x.Working == "luckyGame_sepak_placeVeryLowLose")];
		}
	}
}
