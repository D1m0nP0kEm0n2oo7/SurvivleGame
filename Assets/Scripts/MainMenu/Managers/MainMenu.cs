using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _continueBtn;
    [SerializeField] private Button _newGameBtn;
    [SerializeField] private Button _savesBtn;
    [SerializeField] private Button _optionsBtn;
    [SerializeField] private Button _exitBtn;

    [Header("Managers")]
    [SerializeField] private WindowsManager _windowsManager;
    [SerializeField] private SceneLoadManager _sceneLoadManager;

    [SerializeField] private string _sceneName = "GameScene";

    private void Start()
    {
        _continueBtn.onClick.AddListener(() => _sceneLoadManager.OpenScene(_sceneName));
        _newGameBtn.onClick.AddListener(() => _windowsManager.OpenWindow(WindowName.NewGame));
        _savesBtn.onClick.AddListener(() => _windowsManager.OpenWindow(WindowName.Saves));
        _optionsBtn.onClick.AddListener(() => _windowsManager.OpenWindow(WindowName.Options));
        _exitBtn.onClick.AddListener(() => OnExit());
    }
    private void OnDestroy()
    {
        _continueBtn.onClick.RemoveAllListeners();
        _newGameBtn.onClick.RemoveAllListeners();
        _savesBtn.onClick.RemoveAllListeners();
        _optionsBtn.onClick.RemoveAllListeners();
        _exitBtn.onClick.RemoveAllListeners();
    }
    private void OnExit()
    {
        Application.Quit();
    }
}
