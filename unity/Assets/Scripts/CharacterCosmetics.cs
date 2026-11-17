namespace ArtificialNegligence {
 public partial class GameController {
  void SelectStandardOutfit(int choice){if(!stats.OutfitUnlocked(choice))return;stats.outfit=choice;ApplyCharacter();SaveProgress();BuildScreen();}
 }
}
