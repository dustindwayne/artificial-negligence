using System;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  const int CampaignEpoch=1;
  static bool RequiresCampaignRestart(int savedEpoch){return savedEpoch<CampaignEpoch;}
  static void ApplyCampaignEpoch(){
   if(!RequiresCampaignRestart(PlayerPrefs.GetInt("campaign-epoch",0)))return;
   if(PlayerPrefs.HasKey("progress-v2")&&!PlayerPrefs.HasKey("progress-before-overhaul"))PlayerPrefs.SetString("progress-before-overhaul",PlayerPrefs.GetString("progress-v2"));
   foreach(string key in new[]{"progress-v2","davey-goats-unlocked","brad-jammin-unlocked","mikey-redd-unlocked","prison-mike-unlocked","adaptive-pressure"})PlayerPrefs.DeleteKey(key);
   PlayerPrefs.SetInt("campaign-epoch",CampaignEpoch);PlayerPrefs.Save();
  }
 }
}
