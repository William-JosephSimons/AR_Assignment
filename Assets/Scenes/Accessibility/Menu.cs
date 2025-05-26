using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadAccessibilityScene()
    {
        SceneManager.LoadScene("Accessibility");
    }
}