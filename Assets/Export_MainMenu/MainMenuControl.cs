using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuControl : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject informationPanel;
    public GameObject howToPlayPanel;

    [Header("Audio Mixer")]
    public AudioMixer myMixer;

    [Header("BGM Settings")]
    public AudioSource bgmSource;
    public Slider bgmSlider;
    private float maxBgmVolume = 0.5f;

    [Header("SFX Settings")]
    public Slider sfxSlider;
    public AudioSource sfxSource;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel != null)
            {
                if (settingPanel.activeSelf)
                    settingPanel.SetActive(false);
                else
                    OpenSetting();
            }
        }
    }

    void Start()
    {
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        if (bgmSlider != null)
        {
            bgmSlider.value = savedBgm / maxBgmVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        SetBGMVolume(bgmSlider != null ? bgmSlider.value : 1f);

        float savedSfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSfx;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        SetSFXVolume(sfxSlider != null ? sfxSlider.value : 0.8f);

        CloseAllPanels();
        AudioListener.volume = 1.0f;
    }

    public void StartWithDelay(string sceneName)
    {
        StartCoroutine(DelaySceneLoad(sceneName));
    }

    private IEnumerator DelaySceneLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("นายลืมพิมพ์ชื่อ Scene กวัก?!");
            yield break;
        }
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadTargetScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
    }

    public void PlayCustomSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    public void SetBGMVolume(float sliderValue)
    {
        float finalVolume = sliderValue * maxBgmVolume;
        if (myMixer != null)
        {
            float dB = Mathf.Log10(Mathf.Clamp(finalVolume, 0.0001f, 1f)) * 20;
            myMixer.SetFloat("MusicVol", dB);
        }
        if (bgmSource != null) bgmSource.volume = finalVolume;
        PlayerPrefs.SetFloat("BGMVolume", finalVolume);
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (myMixer != null)
        {
            float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
            myMixer.SetFloat("SFXVol", dB);
        }
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    public void OpenSetting() { CloseAllPanels(); if (settingPanel != null) settingPanel.SetActive(true); }
    public void OpenInformation() { CloseAllPanels(); if (informationPanel != null) informationPanel.SetActive(true); }
    public void OpenHowToPlay() { CloseAllPanels(); if (howToPlayPanel != null) howToPlayPanel.SetActive(true); }

    public void CloseAllPanels()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        if (informationPanel != null) informationPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // --- ส่วนที่อัปเดตใหม่กวัก! ---
    public void QuitGame()
    {
        StartCoroutine(DelayQuit());
    }

    private IEnumerator DelayQuit()
    {
        Debug.Log("รอ 2 วินาทีก่อนปิดเกมกวัก...");
        yield return new WaitForSeconds(2.0f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        Debug.Log("กวัก! ออกเกมแล้วนะนาย");
    }
}