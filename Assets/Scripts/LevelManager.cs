using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject joystick;
    public Text solved;
    public Tilemap path;
    private TilemapRenderer solution;
    public bool finished;
    public Player player;
    public EndSquare endSquare;
    public GameObject nextPanel;
    public GameObject exitPanel;
    public Button reset;
    public Button exit;
    public Text sound;
    public GameObject credits;
    public GameObject languages;
    private string currentSceneName;
    private int currentSceneIndex;
    private Canvas canvas;
    private float time;
    private int NUMLEVELS = 50;

    public enum Language
    {
        English,
        Francais,
        Malagasy
    }
    public Language currentLanguage;

    // Start is called before the first frame update
    void Start()
    {
        if (path != null)
            solution = path.GetComponent<TilemapRenderer>();
        FixAspect();
        InitSceneInfo();
        InitLanguage();
        if (currentSceneName == "Menu")
            InitMenuButtons();
        if (currentSceneName.StartsWith("T"))
            InitInstructions(int.Parse(currentSceneName.Substring(1)));
        if (currentSceneName == "More Soon")
            InitMore();
        InitLevel();
        if (currentSceneName.StartsWith("L") && !currentSceneName.Substring(1, 1).Equals("S"))
            InitSolved();
        InitAdCounter();
        ShowBannerAd();

        if (!AudioManager.Instance.theme.source.isPlaying)
            AudioManager.Instance.PlayTheme();

        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
    }

    public void CancelExit()
    {
        AudioManager.Instance.Play("click");
        exitPanel.SetActive(false);
        joystick.SetActive(true);
    }
    public void ChangeLanguage(string lang)
    {
        switch (lang)
        {
            case "English":
                PlayerPrefs.SetInt("Language", 0);
                currentLanguage = LevelManager.Language.English;
                break;
            case "Francais":
                PlayerPrefs.SetInt("Language", 1);
                currentLanguage = LevelManager.Language.Francais;
                break;
            case "Malagasy":
                PlayerPrefs.SetInt("Language", 2);
                currentLanguage = LevelManager.Language.Malagasy;
                break;
        }
        PlayerPrefs.Save();
        Analytics.CustomEvent("Language", new Dictionary<string, object> { { "Language", lang } });
        CloseLanguage();
        SceneManager.LoadScene("Menu");
    }
    public void ClearLevel()
    {
        // Decrease count everytime a level is cleared
        PlayerPrefs.SetInt("Ad", PlayerPrefs.GetInt("Ad") - 1);
        PlayerPrefs.Save();

        // If cleared current level (doesn't execute for replaying old levels)
        if (PlayerPrefs.GetInt("CurrentLevel") == currentSceneIndex)
        {
            PlayerPrefs.SetInt("CurrentLevel", currentSceneIndex + 1);
            PlayerPrefs.Save();
            Analytics.CustomEvent("Clear ", new Dictionary<string, object> { { "Level", currentSceneIndex },
                                                                             { "Time", (int)time} });
        }
        AudioManager.Instance.Play("heaven");
        finished = true;
        joystick.SetActive(false);
        path.color = new Color(1, 1, 1, 0);
        solution.sortingLayerName = "Foreground";
        StartCoroutine(LerpSolutionColor());
        StartCoroutine(ShowNextButton());
    }
    public void CloseCredits()
    {
        AudioManager.Instance.Play("click");
        credits.SetActive(false);
    }
    public void CloseLanguage()
    {
        AudioManager.Instance.Play("click");
        languages.SetActive(false);
    }
    public void ConfirmExit()
    {
        Analytics.CustomEvent("Exit ", new Dictionary<string, object> { { "Time", (int)time },
                                                                        { "Level", currentSceneIndex} });
        AudioManager.Instance.Play("click");
        SceneManager.LoadScene("Menu");
    }
    // Fixing camera aspect ratio
    private void FixAspect()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        float aspect = Camera.main.aspect;
        float targetAspect = 9.0f / 16.0f;
        if (aspect < targetAspect)
        {
            Camera.main.orthographicSize /= aspect / targetAspect;
        }

        if (aspect > targetAspect)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            // This fits the canvas nicely with any aspect ratio. It is ony a problem when the ratio is > 16:9
            if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand)
            {
                // This equation was obtained using simple linear approximation using the following points:
                //.5625 => 1920
                //.6667 => 2280
                //.6914 => 2350
                //.7500 => 2535
                float refX = 3288.4f * aspect + 75.775f;
                scaler.referenceResolution = new Vector2(refX, 1080.0f);
            }
        }
    }
    // Initializing count until next ad
    private void InitAdCounter()
    {
        if (!PlayerPrefs.HasKey("Ad"))
        {
            PlayerPrefs.SetInt("Ad", 5);
            PlayerPrefs.Save();
        }
        if (!PlayerPrefs.HasKey("AdShown"))
        {
            PlayerPrefs.SetInt("AdShown", 0);
            PlayerPrefs.Save();
        }
    }
    private void InitCreditsButton()
    {
        Button credits = GameObject.Find("Credits Button").GetComponent<Button>();
        if (credits != null)
        {
            Text creditsText = credits.GetComponentInChildren<Text>();
            switch (currentLanguage.ToString())
            {
                case "English":
                    creditsText.text = "Credits";
                    break;
                case "Francais":
                    creditsText.text = "Credits";
                    break;
                case "Malagasy":
                    creditsText.text = "Fisaorana";
                    break;
            }

        }
    }
    private void InitInstructions(int i)
    {
        Text instructions = canvas.GetComponentInChildren<Text>();
        if (instructions != null)
        {
            switch (i)
            {
                case 1:
                    switch (currentLanguage.ToString())
                    {
                        case "English":
                            instructions.text = "Reach the exit of the Maze.\nSwipe in any direction to move.";
                            break;
                        case "Francais":
                            instructions.text = "Trouvez la sortie du labyrinthe.\nFaites glisser votre doigt dans n'importe quelle direction pour vous deplacer.";
                            break;
                        case "Malagasy":
                            instructions.text = "Tadiavo ny fivoahan'ny labyrinthe.\nAsosay eo amin'ny efijery ny rantsan-tananao mankany amin'ny lalana tianao aleha.";
                            break;
                    }
                    break;
                case 2:
                    switch (currentLanguage.ToString())
                    {
                        case "English":
                            instructions.text = "As long as you are on a white square, keep moving, there is still a way. Try all four directions.";
                            break;
                        case "Francais":
                            instructions.text = "Tant que vous etes sur un carre blanc, continuez, il y a encore un moyen d'avancer. Essayez les quatre directions.";
                            break;
                        case "Malagasy":
                            instructions.text = "Raha mbola eo amin'ny efa-joro fotsy ianao dia mandrosoa hatrany, mbola misy ny lalana. Andramo ireo lalana efatra.";
                            break;
                    }
                    break;
                case 3:
                    switch (currentLanguage.ToString())
                    {
                        case "English":
                            instructions.text = "This is an intersection.\nChoose which way you will go.";
                            break;
                        case "Francais":
                            instructions.text = "Ceci est une intersection.\nChoisissez dans quelle direction vous irez.";
                            break;
                        case "Malagasy":
                            instructions.text = "Ito dia sampanan-dalana.\nSafidio ny lalana halehanao.";
                            break;
                    }
                    break;
                case 4:
                    switch (currentLanguage.ToString())
                    {
                        case "English":
                            instructions.text = "This is a dead end.\nGo back.";
                            break;
                        case "Francais":
                            instructions.text = "Ceci est une impasse.\nFaites demi-tour.";
                            break;
                        case "Malagasy":
                            instructions.text = "Mifarana eto ny lalana.\nMiverena.";
                            break;
                    }
                    break;
            }

        }
    }
    private void InitLanguage()
    {
        if (!PlayerPrefs.HasKey("Language"))
        {
            PlayerPrefs.SetInt("Language", 0);
            PlayerPrefs.Save();
        }
        currentLanguage = (Language)PlayerPrefs.GetInt("Language");
    }
    private void InitLanguageButton()
    {
        Button language = GameObject.Find("Language").GetComponent<Button>();
        if (language != null)
        {
            Text languageText = language.GetComponentInChildren<Text>();
            switch (currentLanguage.ToString())
            {
                case "English":
                    languageText.text = "Language";
                    break;
                case "Francais":
                    languageText.text = "Language";
                    break;
                case "Malagasy":
                    languageText.text = "Fiteny";
                    break;
            }

        }
    }
    // Initializing Level 1
    private void InitLevel()
    {
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
            PlayerPrefs.Save();
        }
    }
    private void InitMenuButtons()
    {
        InitPlayButton();
        InitSoundButton();
        InitLanguageButton();
        InitCreditsButton();
    }
    private void InitMore()
    {
        Text more = GameObject.Find("More").GetComponent<Text>();
        if (more != null)
        {
            switch (currentLanguage.ToString())
            {
                case "English":
                    more.text = "MORE LEVELS\nCOMING SOON";
                    break;
                case "Francais":
                    more.text = "PLUS DE NIVEAUX\nBIENTOT DISPONIBLE";
                    break;
                case "Malagasy":
                    more.text = "MBOLA MISY MARO\nHO AVY TSY ELA";
                    break;
            }

        }
    }
    private void InitPlayButton()
    {
        Button play = GameObject.Find("Play Button").GetComponent<Button>();
        if (play != null)
        {
            Text playText = play.GetComponentInChildren<Text>();
            switch (currentLanguage.ToString())
            {
                case "English":
                    playText.text = "Play";
                    break;
                case "Francais":
                    playText.text = "Jouer";
                    break;
                case "Malagasy":
                    playText.text = "Hilalao";
                    break;
            }

        }
    }
    // Current Scene Info
    private void InitSceneInfo()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }
    private void InitSoundButton()
    {
        switch (currentLanguage.ToString())
        {
            case "English":
                if (AudioManager.Instance.soundState == AudioManager.soundSetting.FXOn)
                {
                    sound.text = "Sound: No Music";
                }
                else if (AudioManager.Instance.soundState == AudioManager.soundSetting.NoneOn)
                {
                    sound.text = "Sound: Off";
                }
                else
                {
                    sound.text = "Sound: On";
                }
                break;
            case "Francais":
                if (AudioManager.Instance.soundState == AudioManager.soundSetting.FXOn)
                {
                    sound.text = "Son: Pas de Musique";
                }
                else if (AudioManager.Instance.soundState == AudioManager.soundSetting.NoneOn)
                {
                    sound.text = "Son: Pas de Son";
                }
                else
                {
                    sound.text = "Son: Musique et Sons";
                }
                break;
            case "Malagasy":
                if (AudioManager.Instance.soundState == AudioManager.soundSetting.FXOn)
                {
                    sound.text = "Feo: Tsy Misy Feon-kira";
                }
                else if (AudioManager.Instance.soundState == AudioManager.soundSetting.NoneOn)
                {
                    sound.text = "Feo: Tsy Misy Feo";
                }
                else
                {
                    sound.text = "Feo: Feon-kira sy Feo";
                }
                break;
        }
    }
    private void InitSolved()
    {
        Text solved = GameObject.Find("Solved").GetComponent<Text>();
        if (solved != null)
        {
            switch (currentLanguage.ToString())
            {
                case "English":
                    solved.text = "SOLVED";
                    break;
                case "Francais":
                    solved.text = "RESOLU";
                    break;
                case "Malagasy":
                    solved.text = "VITA";
                    break;
            }

        }
    }
    public void LanguageMenu()
    {
        AudioManager.Instance.Play("click");
        Button back = languages.GetComponentInChildren<Button>();
        Text backText = back.GetComponentInChildren<Text>();
        if (backText != null)
        {
            switch (currentLanguage.ToString())
            {
                case "English":
                    backText.text = "Back";
                    break;
                case "Francais":
                    backText.text = "Retour";
                    break;
                case "Malagasy":
                    backText.text = "Hiverina";
                    break;
            }

        }
        languages.SetActive(true);
    }

    public void LevelSelect()
    {
        AudioManager.Instance.Play("click");
        if (PlayerPrefs.GetInt("CurrentLevel") > NUMLEVELS)
            SceneManager.LoadScene("More Soon");
        else
        {
            int levels = ((PlayerPrefs.GetInt("CurrentLevel") - 1) / 10) + 1;
            SceneManager.LoadScene("LS" + levels);
        }
    }
    public void NextLevel()
    {
        AudioManager.Instance.Play("click");
        ShowAd();
        int level = currentSceneIndex + 1;
        Analytics.CustomEvent("Play", new Dictionary<string, object> { { "Level", level } });
        SceneManager.LoadScene(level);
    }
    public void NextLevel(string level)
    {
        AudioManager.Instance.Play("click");
        ShowAd();
        int lvl = int.Parse(level.Substring(1));
        Analytics.CustomEvent("Play", new Dictionary<string, object> { { "Level", lvl } });
        SceneManager.LoadScene(level);
    }
    public void NextLevelMenu()
    {
        AudioManager.Instance.Play("click");
        if (currentSceneName != "LS5")
            SceneManager.LoadScene(currentSceneIndex + 1);
        else
        {
            SceneManager.LoadScene("More Soon");
            Analytics.CustomEvent("More", new Dictionary<string, object> { { "Level", PlayerPrefs.GetInt("CurrentLevel") } });
        }

    }
    public void OnClickSound()
    {
        AudioManager.Instance.Play("click");
        if (AudioManager.Instance.soundState == AudioManager.soundSetting.AllOn)
        {
            AudioManager.Instance.soundState = AudioManager.soundSetting.FXOn;
            AudioManager.Instance.PlayTheme();//Stops the theme
            InitSoundButton();
            PlayerPrefs.SetInt("Sound", 1);
        }
        else if (AudioManager.Instance.soundState == AudioManager.soundSetting.FXOn)
        {
            AudioManager.Instance.soundState = AudioManager.soundSetting.NoneOn;
            InitSoundButton();
            PlayerPrefs.SetInt("Sound", 2);
        }
        else
        {
            AudioManager.Instance.soundState = AudioManager.soundSetting.AllOn;
            AudioManager.Instance.PlayTheme();
            InitSoundButton();
            PlayerPrefs.SetInt("Sound", 0);
        }
        PlayerPrefs.Save();
        Analytics.CustomEvent("Sound", new Dictionary<string, object> { { "Setting", sound.text },
                                                                        { "Level", PlayerPrefs.GetInt("CurrentLevel")} });
    }
    public void PreviousLevelMenu()
    {
        AudioManager.Instance.Play("click");
        if (currentSceneName != "LS1")
            SceneManager.LoadScene(currentSceneIndex - 1);
    }
    public void ResetPosition()
    {
        AudioManager.Instance.Play("click");
        player.movePoint.transform.position = player.initialPosition;
        player.transform.position = player.initialPosition;
        if (finished) // Hide solution again and set finished to false
        {
            finished = false;
            joystick.SetActive(true);
            solved.color = new Color(1, 1, 1, 0);
            solution.sortingLayerName = "Default";
            path.color = Color.white;
            nextPanel.SetActive(false);
        }
        else
            Analytics.CustomEvent("Reset ", new Dictionary<string, object> { { "Level", currentSceneIndex } });

    }
    public void ShowAd()
    {
        if (PlayerPrefs.GetInt("Ad") <= 0)
        {
            PlayerPrefs.SetInt("Ad", 5);
            PlayerPrefs.SetInt("AdShown", PlayerPrefs.GetInt("AdShown") + 1);
            PlayerPrefs.Save();
            VideoAd.Instance.ShowVideoAd();
            // The ShowVideoAd() method above also sends an "Ad" custom event with time since last ad
            Analytics.CustomEvent("Ad", new Dictionary<string, object>{{ "Count", PlayerPrefs.GetInt("AdShown")},
                                                                       { "Scene", currentSceneIndex} });
        }
    }
    // Show banner ad on menus
    private void ShowBannerAd()
    {
        switch (currentSceneName)
        {
            case "Menu":
            case "LS1":
            case "LS2":
            case "LS3":
            case "LS4":
            case "LS5":
            case "More Soon":
                VideoAd.Instance.ShowBannerAd();
                break;
            default:
                VideoAd.Instance.HideBannerAd();
                break;
        }
    }
    public void ShowCredits()
    {
        AudioManager.Instance.Play("click");

        Text thanks = credits.GetComponentInChildren<Text>();
        Button back = credits.GetComponentInChildren<Button>();
        Text backText = back.GetComponentInChildren<Text>();
        if (thanks != null)
        {
            switch (currentLanguage.ToString())
            {
                case "English":
                    thanks.text = "Thanks to";
                    backText.text = "Back";
                    break;
                case "Francais":
                    thanks.text = "Merci a";
                    backText.text = "Retour";
                    break;
                case "Malagasy":
                    thanks.text = "Misaotra an'i";
                    backText.text = "Hiverina";
                    break;
            }

        }
        credits.SetActive(true);
        Analytics.CustomEvent("Credits", new Dictionary<string, object> { { "Level", PlayerPrefs.GetInt("CurrentLevel") } });
    }
    public void ShowExit()
    {
        AudioManager.Instance.Play("exit");
        Text exit = exitPanel.GetComponentInChildren<Text>();
        if (exit != null)
        {
            switch (currentLanguage.ToString())
            {
                case "English":
                    exit.fontSize = 250;
                    exit.text = "Exit?";
                    break;
                case "Francais":
                    exit.fontSize = 225;
                    exit.text = "Quitter?";
                    break;
                case "Malagasy":
                    exit.fontSize = 250;
                    exit.text = "Hiala?";
                    break;
            }
        }
        exitPanel.SetActive(true);
        joystick.SetActive(false);
    }

    IEnumerator LerpSolutionColor()
    {
        float time = 0f;
        while (time < 1000f)
        {
            time += Time.deltaTime;
            path.color = Color.Lerp(path.color, Color.white, time / 1000f);
            yield return null;
        }
    }
    IEnumerator ShowNextButton()
    {
        reset.interactable = false;
        exit.interactable = false;
        yield return new WaitForSeconds(3f);
        solved.color = Color.black;
        reset.interactable = true;
        exit.interactable = true;
        nextPanel.SetActive(true);
        AudioManager.Instance.Play("chime");
    }
}