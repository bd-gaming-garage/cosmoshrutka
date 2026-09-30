using UnityEngine;

public static class CursorSetup {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	
    static void Init() {
        Cursor.visible = false;
    }
}
