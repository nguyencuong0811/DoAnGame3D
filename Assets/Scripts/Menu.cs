using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayGame()
    {
        // gia su scene game cua ban ten la "nhmap2"
        SceneManager.LoadScene("SampleScene");
    }

    // ham goi khi an nut Quit
    public void QuitGame()
    {
        Debug.Log("Game Closed!");
        Application.Quit();

        // chi de test trong Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
