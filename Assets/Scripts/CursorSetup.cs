using UnityEngine;

namespace Player.Scripts
{
    public static class CursorSetup {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	
        static void Init() {
            Cursor.visible = false;
        }
    }
}
