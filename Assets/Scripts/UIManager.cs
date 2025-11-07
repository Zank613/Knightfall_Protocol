using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image[] livesImages;

    public void UpdateLives(int lives)
    {
        int indexToShow = 9 - lives;

        for (int i = 0; i < livesImages.Length; i++)
        {
            if (i == indexToShow)
            {
                livesImages[i].enabled = true; // Show this one
            }
            else
            {
                livesImages[i].enabled = false; // Hide all others
            }
        }
    }
    
    public void HideLivesUI()
    {
        foreach (Image img in livesImages)
        {
            if (img != null)
            {
                img.enabled = false;
            }
        }
    }
}