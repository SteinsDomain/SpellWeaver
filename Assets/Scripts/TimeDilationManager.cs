using UnityEngine;
using System.Collections;

public class TimeDilationManager : MonoBehaviour {
    public static TimeDilationManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SetTimeDilation(float scale, float duration) {
        StartCoroutine(ApplyTimeDilation(scale, duration));
    }

    private IEnumerator ApplyTimeDilation(float scale, float duration) {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void StartTimeDilation(float scale) {
        Time.timeScale = scale;
    }

    public void StopTimeDilation() {
        Time.timeScale = 1f;
    }
}