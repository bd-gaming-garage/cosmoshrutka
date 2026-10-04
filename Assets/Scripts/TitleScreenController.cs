using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string gameScene = "SampleScene";

    private Button _startButton;
    private bool _loading;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _startButton = GetComponent<Button>();
        _startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        if (_loading) return;

        if (!Application.CanStreamedLevelBeLoaded(gameScene))
        {
            Debug.LogError($"Scene '{gameScene}' is not available in the build scene list.", this);
            return;
        }

        _loading = true;
        _startButton.interactable = false;
        Cursor.visible = false;
        SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(StartGame);
    }
}
