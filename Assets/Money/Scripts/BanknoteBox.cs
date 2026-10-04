using UnityEngine;

namespace Money.Scripts
{
    public class BanknoteBox : MonoBehaviour
    {
        public static GameObject Instance;
        void Awake()
        {
            Instance = gameObject;
        }
    }
}
