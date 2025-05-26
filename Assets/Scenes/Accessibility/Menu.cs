using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject normalPath;
    public GameObject accessPath;
    public GameObject wellLitPath;

    public Toggle access;
    public Toggle wellLit;

    public void LoadAccessibilityScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Accessibility");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Accessibility")
        {
            Debug.Log("Accessibility scene loaded!");

            // Example: Find and modify an object in the new scene
            GameObject go = GameObject.Find("SomeUIElement");
            if (go != null)
            {
                // Do stuff, like change text or toggle state
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Update()
    {
        string targetTag;
        if (access.isOn)
        {
            targetTag = "Access";
        }
        else
        {
            if (wellLit.isOn)
            {
                targetTag = "WellLit";
            }
            else
            {
                targetTag = "Normal";
            }
        }
        SceneData.targetTag = targetTag;
        GameObject[] paths = { normalPath, accessPath, wellLitPath };
        SceneData.changePathColours(paths, true);
    }



}