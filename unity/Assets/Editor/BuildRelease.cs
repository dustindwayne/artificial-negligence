using UnityEngine;
public static class BuildRelease {
 public static void All(){BuildGame.Desktop();BuildGame.Linux();BuildGame.Android();BuildWeb.Build();Debug.Log("AN_RELEASE_ALL_PASS / Windows, Linux, Android, Web");}
}
