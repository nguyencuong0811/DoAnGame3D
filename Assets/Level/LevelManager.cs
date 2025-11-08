using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void LoadScene2()
    {
            SceneManager.LoadScene(2);
    }
    public void LoadSceneMenu()
    {
        SceneManager.LoadScene(0);
    }
}
