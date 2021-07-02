using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class LevelSelect : MonoBehaviour
{
    public Button button;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = button.GetComponentInChildren<Text>();
        if (PlayerPrefs.GetInt("CurrentLevel") >= int.Parse(text.text))
        {
            button.interactable=true;
        }
        else
        {
            button.interactable=false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadLevel()
    {
        AudioManager.Instance.Play("click");
        SceneManager.LoadScene("L"+text.text);
        Analytics.CustomEvent("Play L" + text.text);
    }
}
