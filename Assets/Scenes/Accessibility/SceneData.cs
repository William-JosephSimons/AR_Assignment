using UnityEngine;

public static class SceneData
{
  public static string targetTag;

  
    public static void changePathColours(GameObject[] paths, bool changeY = false, float downf = 9.0f, float upf = 12.0f)
    {
        foreach (var path in paths)
        {
            var lr = path.GetComponent<LineRenderer>();
            lr.startColor = lr.endColor = path.CompareTag(targetTag) ? Color.blue : Color.grey;
            if (changeY)
            {
                Vector3 localPos = path.transform.localPosition;
                localPos.y = path.CompareTag(targetTag) ? upf : downf;
                path.transform.localPosition = localPos;
            }
        }
    }
}
