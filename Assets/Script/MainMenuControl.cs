using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class MainMenuControl : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject informationPanel;
    public GameObject howToPlayPanel;

    [Header("Audio Mixer & AudioSource")]
    public AudioMixer myMixer;
    public AudioSource sfxSource;

    [Header("BGM Settings")]
    public AudioSource bgmSource;
    public Slider bgmSlider;
    private float maxBgmVolume = 0.5f;

    [Header("SFX Settings")]
    public Slider sfxSlider;

    [Header("Light Control")]
    public Light2D targetLight;
    public Light2D globalLight;

    [Space(10)]
    public float spotNormal = 5.0f;
    public float spotHover = 0.5f;
    public float globalNormal = 0.7f;
    public float globalHover = 0.1f;

    [Tooltip("ความเร็วการหรี่ไฟ (วินาที)")]
    public float fadeDuration = 1.0f;

    private Coroutine lightFadeRoutine;
    private bool isStarting = false;

    void Start()
    {
        // [อัปเดตเพิ่ม] ถ้ากลับมาหน้า MainMenu ให้สั่งทำลายเพลงต่อเนื่อง (GlobalBGM) ทิ้งทันที
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            GameObject oldBGM = GameObject.Find("GlobalBGM");
            if (oldBGM != null) Destroy(oldBGM);
        }

        // [ส่วนเดิม] ค้นหา Object "GlobalBGM" เพื่อให้ Slider ยังคุมเสียงต่อเนื่องได้
        if (bgmSource == null)
        {
            GameObject bgmObj = GameObject.Find("GlobalBGM");
            if (bgmObj != null) bgmSource = bgmObj.GetComponent<AudioSource>();
        }

        SetupAudio();
        CloseAllPanels();
        AudioListener.volume = 1.0f;

        if (targetLight != null) targetLight.intensity = spotNormal;
        if (globalLight != null) globalLight.intensity = globalNormal;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    // --- [LOGIC พิเศษสำหรับปุ่ม ESC] ---
    private void HandleEscapeKey()
    {
        if (settingPanel != null && settingPanel.activeSelf)
        {
            CloseSetting();
            return;
        }

        if (informationPanel != null && informationPanel.activeSelf)
        {
            informationPanel.SetActive(false);
            return;
        }

        if (howToPlayPanel != null && howToPlayPanel.activeSelf)
        {
            howToPlayPanel.SetActive(false);
            return;
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            OpenSetting();
        }
    }

    // --- [SOUND SYSTEM] ---
    public void PlayClickSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayHoverSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayHoverSoundStart(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.Play();
    }

    public void StopHoverSound()
    {
        if (sfxSource != null) sfxSource.Stop();
    }

    // --- [LIGHT SYSTEM] ---
    public void OnHoverStartButton() => StartFade(spotHover, globalHover);
    public void OnExitStartButton() => StartFade(spotNormal, globalNormal);

    private void StartFade(float targetSpot, float targetGlobal)
    {
        if (lightFadeRoutine != null) StopCoroutine(lightFadeRoutine);
        lightFadeRoutine = StartCoroutine(FadeLightCoroutine(targetSpot, targetGlobal));
    }

    IEnumerator FadeLightCoroutine(float targetSpot, float targetGlobal)
    {
        float time = 0;
        float startSpot = targetLight != null ? targetLight.intensity : 0;
        float startGlobal = globalLight != null ? globalLight.intensity : 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, time / fadeDuration);
            if (targetLight != null) targetLight.intensity = Mathf.Lerp(startSpot, targetSpot, t);
            if (globalLight != null) globalLight.intensity = Mathf.Lerp(startGlobal, targetGlobal, t);
            yield return null;
        }
    }

    // --- [BUTTON FUNCTIONS & SCENE CONTROL] ---
    public void StartGame(string sceneName)
    {
        if (!isStarting && !string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(DelaySceneLoad(sceneName));
        }
    }

    private IEnumerator DelaySceneLoad(string sceneName)
    {
        isStarting = true;
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(2.0f);
        SceneManager.LoadScene(sceneName);
    }

    public void OpenSetting()
    {
        CloseAllPanels();
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSetting()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenInformation() { CloseAllPanels(); if (informationPanel != null) informationPanel.SetActive(true); }
    public void OpenHowToPlay() { CloseAllPanels(); if (howToPlayPanel != null) howToPlayPanel.SetActive(true); }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- [AUDIO SETUP & MIXER] ---
    private void SetupAudio()
    {
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        if (bgmSlider != null) { bgmSlider.value = savedBgm / maxBgmVolume; bgmSlider.onValueChanged.AddListener(SetBGMVolume); }
        ApplyBGMVolume(bgmSlider != null ? bgmSlider.value : (savedBgm / maxBgmVolume));

        float savedSfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        if (sfxSlider != null) { sfxSlider.value = savedSfx; sfxSlider.onValueChanged.AddListener(SetSFXVolume); }
    }

    public void SetBGMVolume(float sliderValue) => ApplyBGMVolume(sliderValue);
    private void ApplyBGMVolume(float sliderValue)
    {
        float finalVolume = sliderValue * maxBgmVolume;
        if (myMixer != null) myMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(finalVolume, 0.0001f, 1f)) * 20);
        if (bgmSource != null) bgmSource.volume = finalVolume;
        PlayerPrefs.SetFloat("BGMVolume", finalVolume);
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (myMixer != null) myMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    public void CloseAllPanels()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        if (informationPanel != null) informationPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}