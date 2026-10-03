using UnityEngine;

public class BanknoteBox : MonoBehaviour
{
    public static GameObject Instance;
    void Awake()
    {
        Instance = gameObject;
    }
}
