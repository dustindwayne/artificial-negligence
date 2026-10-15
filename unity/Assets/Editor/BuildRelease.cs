using UnityEngine;
public static class BuildRelease {
 public static void All(){BuildGame.Desktop();BuildGame.Android();BuildWeb.Build();Debug.Log("AN_RELEASE_ALL_PASS / Windows, Android, Web");}
}
