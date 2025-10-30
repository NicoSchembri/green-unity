using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private static bool hasReset = false;

    public void PlayGame()
    {
        if (!hasReset)
        {
            SpellBook.ResetAllUnlocks();
            hasReset = true; 
        }

        SceneManager.LoadScene("Prototype2");
    }
}
