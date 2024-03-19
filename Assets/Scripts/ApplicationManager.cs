using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    private (int width, int height) _previousResolution;

    private void Awake()
    {
        _previousResolution.height = Screen.height;
        _previousResolution.width = Screen.width;
    }

    private void Update()
    {
        bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isCtrlPressed == false)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            (int width, int height) resolution;
            FullScreenMode screenMode;

            if (Screen.fullScreen)
            {
                resolution = _previousResolution;
                screenMode = FullScreenMode.Windowed;
            }
            else
            {
                Resolution currentResolution = Screen.currentResolution;
                screenMode = FullScreenMode.FullScreenWindow;

                resolution.height = currentResolution.height;
                resolution.width = currentResolution.width;

                _previousResolution.height = Screen.height;
                _previousResolution.width = Screen.width;
            }

            Screen.SetResolution(resolution.width, resolution.height, screenMode);

            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}