using UnityEngine.Advertisements;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class VideoAd : MonoBehaviour
{

    string gameId = "4044969";
    string placementId = "Banner_Android";
    bool testMode = true;
    float time = 0f;

    private static VideoAd instance;
    public static VideoAd Instance { get { return instance; } }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    { 
        Advertisement.Initialize(gameId, testMode);
        Advertisement.Banner.SetPosition(BannerPosition.TOP_LEFT);
    }


    // Update is called once per frame
    public void Update()
    {
        time += Time.deltaTime;    
    }

    public void ShowVideoAd()
    {
        AudioManager.Instance.theme.source.Stop();
        AudioManager.Instance.ambient.source.Stop();
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
            StartCoroutine(ResumeTheme());
            Analytics.CustomEvent("Ad", new Dictionary<string, object> { { "TimeSinceLastShown", (int)time} });
            time = 0f;
        }
        else
        {
            Debug.Log("No ad ready");
        }
        
    }

    public void ShowBannerAd()
    {
        StartCoroutine(ShowBanner());
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    IEnumerator ShowBanner()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        Advertisement.Banner.Show(placementId);
    }

    IEnumerator ResumeTheme()
    {
        
        while (Advertisement.isShowing)
        {
            yield return null;
        }
        AudioManager.Instance.PlayTheme();
        yield return null;
    }
}
