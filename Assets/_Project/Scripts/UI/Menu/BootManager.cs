using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace EdgeAbyss.UI.Menu
{
    /// <summary>
    /// Simple boot manager that handles initial loading and transitions to main menu.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Boot Settings")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private float minimumDisplayTime = 1.5f;
        [SerializeField] private bool skipToGameScene = false;
        [SerializeField] private string gameSceneName = "TestTrack";

        private void Start()
        {
            StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            // Show loading screen for minimum time
            yield return new WaitForSeconds(minimumDisplayTime);

            // Load next scene
            string targetScene = skipToGameScene ? gameSceneName : mainMenuSceneName;
            SceneManager.LoadScene(targetScene);
        }
    }
}
