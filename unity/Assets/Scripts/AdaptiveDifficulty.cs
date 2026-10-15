using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  static readonly string[] DifficultyNames={"RELAXED","STANDARD","HARD","ADAPTIVE"};
  int difficultyMode;float adaptivePressure;int stageHits,stageKills;bool difficultyResultRecorded;
  public float EncounterDifficulty {get;private set;}=1;
  public string DifficultyName {get{return DifficultyNames[Mathf.Clamp(PlayerPrefs.GetInt("difficulty-mode",1),0,3)];}}
  public static float NextAdaptivePressure(float pressure,int hits,int kills,float collection,bool died){
   float delta=died?-.18f:hits>=5?-.12f:hits>=3?-.05f:hits<=1&&collection>=.85f&&kills>=3?.10f:hits<=2&&collection>=.6f?.04f:0;
   return Mathf.Clamp(pressure+delta,-.3f,.45f);
  }
  void BeginDifficultyStage(){
   difficultyMode=Mathf.Clamp(PlayerPrefs.GetInt("difficulty-mode",1),0,3);
   adaptivePressure=Mathf.Clamp(PlayerPrefs.GetFloat("adaptive-pressure",0),-.3f,.45f);
   EncounterDifficulty=difficultyMode==0?.8f:difficultyMode==2?1.3f:difficultyMode==3?1+adaptivePressure:1;
   stageHits=stageKills=0;difficultyResultRecorded=false;
  }
  void RecordDifficultyResult(bool died){
   if(qaMode||difficultyResultRecorded)return;difficultyResultRecorded=true;
   if(difficultyMode!=3)return;
   adaptivePressure=NextAdaptivePressure(adaptivePressure,stageHits,stageKills,totalGpus>0?(float)collected/totalGpus:0,died);
   PlayerPrefs.SetFloat("adaptive-pressure",adaptivePressure);PlayerPrefs.Save();
  }
  void CycleDifficulty(){PlayerPrefs.SetInt("difficulty-mode",(PlayerPrefs.GetInt("difficulty-mode",1)+1)%4);PlayerPrefs.Save();BuildScreen();}
 }
}
