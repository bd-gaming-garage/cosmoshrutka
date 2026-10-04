using UnityEngine;
using UnityEngine.UI;

public class ScreenController : MonoBehaviour
{
    [SerializeField] private Sprite screenEmpty;
    [SerializeField] private Sprite screen5;
    [SerializeField] private Sprite screen10;
    [SerializeField] private Sprite screen17;
    [SerializeField] private Sprite screen24;
    [SerializeField] private Sprite screen30;

    private Image im;

    private void Awake()
    {
        im = GetComponent<Image>();
    }

    private void Update()
    {
        uint p = PassengerSpawner.Instance.AllPassengers();

        if (p == 0) {
            im.sprite = screenEmpty;
            return;
        }


        if (p < 4)
        {
            im.sprite = screen5;
            return;
        }

        if (p < 10)
        {
            im.sprite = screen10;
            return;
        }

        if (p < 24)
        {
            im.sprite = screen17;
            return;
        }

        if (p < 30)
        {
            im.sprite = screen24;
            return;
        }

        if (p == 30)
        {
            im.sprite = screen30;
            return;
        }
    }
}
