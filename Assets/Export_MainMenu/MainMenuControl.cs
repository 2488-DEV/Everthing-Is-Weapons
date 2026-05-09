using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // เพิ่มตัวนี้เข้ามาเพื่อใช้ Light 2D
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

    [Header("Light Control")]
    public Light2D targetLight;         // ลากดวงไฟ Spot Light มาใส่ตรงนี้
    public float normalIntensity = 5.0f;
    public float hoverIntensity = 0.5f;
    public float lightFadeSpeed = 5.0f;

    private bool isProcessing = false;  // ป้องกันการกดปุ่มซ้ำซ้อน
    private Coroutine lightFadeRoutine;

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
        // โหลดค่าระดับเสียง BGM
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        if (bgmSlider != null)
        {
            bgmSlider.value = savedBgm / maxBgmVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        SetBGMVolume(bgmSlider != null ? bgmSlider.value : 1f);

        // โหลดค่าระดับเสียง SFX
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

    // --- ระบบ Delay 1 วินาทีก่อนทำงาน ---

    public void OpenSetting() => StartCoroutine(DelayAction(() => {
        CloseAllPanels();
        if (settingPanel != null) settingPanel.SetActive(true);
    }));

    public void OpenInformation() => StartCoroutine(DelayAction(() => {
        CloseAllPanels();
        if (informationPanel != null) informationPanel.SetActive(true);
    }));

    public void OpenHowToPlay() => StartCoroutine(DelayAction(() => {
        CloseAllPanels();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }));

    public void StartGame(string sceneName) => StartCoroutine(DelayAction(() => {
        if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
    }));

    public void QuitGame() => StartCoroutine(DelayAction(() => {
        Debug.Log("กวัก! ออกเกมแล้วนะนาย");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }));

    private IEnumerator DelayAction(System.Action action)
    {
        if (isProcessing) yield break;
        isProcessing = true;

        yield return new WaitForSeconds(1.0f); // ดีเลย์ 1 วิ ตามที่ขอ

        action?.Invoke();
        isProcessing = false;
    }

    // --- ระบบแสง (ทำงานทันทีที่เมาส์ชี้) ---

    public void OnHoverStartButton() // เอาไปผูกกับ Event Trigger [Pointer Enter]
    {
        if (lightFadeRoutine != null) StopCoroutine(lightFadeRoutine);
        lightFadeRoutine = StartCoroutine(FadeLight(hoverIntensity));
    }

    public void OnExitStartButton() // เอาไปผูกกับ Event Trigger [Pointer Exit]
    {
        if (lightFadeRoutine != null) StopCoroutine(lightFadeRoutine);
        lightFadeRoutine = StartCoroutine(FadeLight(normalIntensity));
    }

    private IEnumerator FadeLight(float target)
    {
        if (targetLight == null) yield break;
        while (!Mathf.Approximately(targetLight.intensity, target))
        {
            targetLight.intensity = Mathf.MoveTowards(targetLight.intensity, target, lightFadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // --- ระบบพื้นฐานอื่นๆ ---

    public void CloseAllPanels()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        if (informationPanel != null) informationPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
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

    public void PlayCustomSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }
}