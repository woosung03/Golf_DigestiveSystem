using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 관리자 - DontDestroyOnLoad로 유지
/// 씬 전환 시 각 Manager 참조 갱신 담당
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneController] Scene loaded: {scene.name}");

        // 각 Manager 새 씬 참조 갱신
        HealthManager.Instance?.ResetForNewScene();
        GameManager.Instance?.OnNewSceneLoaded();
    }

    public void LoadNextStage()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(0);
    }

    public void LoadStage(int index) => SceneManager.LoadScene(index);
    public void ReloadCurrentStage() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void LoadMainMenu() => SceneManager.LoadScene(0);
}
