using UnityEngine;
using TMPro;

public class score : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int food;

    public void addToCounter()
    {
        food++;
        scoreText.SetText("Food Gathered: "+ food);
    }
}