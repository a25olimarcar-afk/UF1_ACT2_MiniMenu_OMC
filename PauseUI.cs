using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private CanvasFader canvasFader;
    [SerializeField] private CanvasGroup warningCanvasGroup;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource ballAudio;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Buttons")]
    [SerializeField] private Button soundButton;
    [SerializeField] private Button defaultButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button exitButton;

    [Header("Warning")]
    [SerializeField] private Button buttonYes;
    [SerializeField] private Button buttonNo;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Toggles")]
    [SerializeField] private Toggle toggleScreen;

    private bool isOpen = false;
    private List<Resolution> availableResolutions = new List<Resolution>();

    //variables temporales que representan los valores que el usuario está modificando en el menú
    private float tempMusicVolume;
    private float tempSFXVolume;
    private int tempResolutionWidth;
    private int tempResolutionHeight;
    private bool tempFullscreen;

    void Start()
    {
        Time.timeScale = 1.0f;

        //evitamos que los elementos del canvas sean interactuables (porque el menu no esta abierto)
        canvasFader.GetCanvasGroup().interactable = false;

        //evitar que el canvas de warning sea visible/interactuable al principio
        warningCanvasGroup.alpha = 0.0f;
        warningCanvasGroup.interactable = false;
        warningCanvasGroup.blocksRaycasts = false; //bloquear raycasts aqui para poder usar la otra pantalla

        ObtainAndUpdateSliders();
        ObtainResolutions();
        ObtainScreenMode();

        musicSlider.onValueChanged.AddListener(MusicVolume);
        sfxSlider.onValueChanged.AddListener(SFXVolume);
        //probar el sonido de los SFX (funcion anónima)
        soundButton.onClick.AddListener(() => { sfxAudioSource.Play(); });
        //al cambiar la resolución, llamamos a ChangeResolution
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        //al cambiar el modo de pantalla, llamamos a ChangeScreenMode
        toggleScreen.onValueChanged.AddListener(ChangeScreenMode);
        defaultButton.onClick.AddListener(DefaultValues);
        applyButton.onClick.AddListener(ApplySettings);
        //botones del canvas de warning
        buttonYes.onClick.AddListener(ConfirmExit);
        buttonNo.onClick.AddListener(CancelExit);
        //boton para salirse del juego
        //Application.Quit() nos saca del juego, pero solo funciona en la build
        exitButton.onClick.AddListener(() => { Application.Quit(); });
    }

    void Update()
    {
        OnPauseMenu();
    }

    private void OnPauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen == false) //si el menu de pausa no esta abierto, lo abrimos
            {
                canvasFader.FadeIn(0.5f);
                Time.timeScale = 0.0f;

                //al poder ver el menu, lo hacemos interactuable
                canvasFader.GetCanvasGroup().interactable = true;

                isOpen = true; //luego marcamos que esta abierto
            }
            else //si el menu de pausa esta abierto, lo cerramos
            {
                //pero, antes de cerrarlo, comprobamos si hay cambios sin guardar
                if (HasUnsavedChanges())
                {
                    ShowWarning();
                }
                else
                {
                    canvasFader.FadeOut(0.5f);
                    Time.timeScale = 1.0f;
                    //al cerrar el menu, lo hacemos no interactuable
                    canvasFader.GetCanvasGroup().interactable = false;
                    isOpen = false; //ahora marcamos que esta cerrado
                }
            }
        }
    }

    private void MusicVolume(float vol)
    {
        musicAudioSource.volume = vol;
        musicVolumeText.text = vol.ToString("0.00"); //actualizamos el texto del volumen de la musica

        //guardamos el valor temporalmente, pero todavía no en PlayerPrefs
        tempMusicVolume = vol;

        Debug.Log($"Music volume set to: {vol}");
    }

    private void SFXVolume(float vol)
    {
        sfxAudioSource.volume = vol;
        ballAudio.volume = vol;
        sfxVolumeText.text = vol.ToString("0.00");

        tempSFXVolume = vol;

        Debug.Log($"SFX volume set to: {vol}");
    }

    private void ObtainAndUpdateSliders()
    {
        //Music volume
        //obtenemos el valor guardado (si no hay ninguno usamos 1.0 predeterminado)
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        tempMusicVolume = savedMusicVolume;

        //actualizar el volumen de la musica, slider y texto con el valor guardado
        musicSlider.value = savedMusicVolume;
        musicAudioSource.volume = savedMusicVolume;
        musicVolumeText.text = savedMusicVolume.ToString("0.00");

        //SFX volume
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        tempSFXVolume = savedSFXVolume;

        sfxSlider.value = savedSFXVolume;
        sfxAudioSource.volume = savedSFXVolume;
        ballAudio.volume = savedSFXVolume;
        sfxVolumeText.text = savedSFXVolume.ToString("0.00");
    }

    private void ObtainResolutions()
    {
        //limpiar las opciones predeterminadas del dropdown
        resolutionDropdown.ClearOptions();

        //obtenemos todas las resoluciones disponibles
        Resolution[] resolutions = Screen.resolutions;

        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

        tempResolutionWidth = savedWidth;
        tempResolutionHeight = savedHeight;

        //para evitar que se repitan resoluciones, usamos un HashSet
        //guardamos las resoluciones que ya hemos añadido al dropdown
        HashSet<string> addedResolutions = new HashSet<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            string resolutionName = res.width + " x " + res.height;

            //si la resolución todavía no ha sido añadida
            if (!addedResolutions.Contains(resolutionName))
            {
                //la añadimos al conjunto de resoluciones utilizadas
                addedResolutions.Add(resolutionName);
                //la añadimos a la lista de resoluciones disponibles
                availableResolutions.Add(res);
                //y la añadimos al dropdown
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutionName));
            }

        }

        //buscamos la resolución guardada y la seleccionamos en el dropdown
        for (int i = 0; i < resolutionDropdown.options.Count; i++)
        {
            if (resolutionDropdown.options[i].text == savedWidth + " x " + savedHeight)
            {
                resolutionDropdown.value = i; //aplicar la resolución guardada en el dropdown
                break; //salimos del bucle una vez que encontramos la resolución guardada
            }
        }

        //actualizamos el valor mostrado en el dropdown
        resolutionDropdown.RefreshShownValue();
    }

    private void ChangeResolution(int index)
    {
        //obtenemos la resolución porque Unity nos devuelve un index
        //Resolution resolution = Screen.resolutions[index];

        //obtenemos la resolución de nuestra lista filtrada
        Resolution resolution = availableResolutions[index];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );

        //guardamos la resolución solamente de forma temporal
        tempResolutionWidth = resolution.width;
        tempResolutionHeight = resolution.height;
    }

    private void ChangeScreenMode(bool isFullscreen)
    {
        if (isFullscreen) //si el toggle esta activado, ponemos pantalla completa
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else //si el toggle esta desactivado, ponemos ventana
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        //guardamos el modo de forma temporal
        tempFullscreen = isFullscreen;
    }

    private void ObtainScreenMode()
    {
        //obtenemos el estado del toggle guardado en PlayerPrefs
        int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1);
        bool isFullscreen = savedFullscreen == 1; //convertimos el int a bool

        tempFullscreen = isFullscreen;

        toggleScreen.isOn = isFullscreen;

        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    private void ApplySettings()
    {
        //al aplicar los cambios, guardamos los valores temporales en PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVolume);

        //PlayerPrefs.SetInt("ResolutionIndex", index);
        //en vez de guardar el index, es mejor guardar la resolución en sí
        //por si cambian las resoluciones disponibles entre dispositivos
        PlayerPrefs.SetInt("ResolutionWidth", tempResolutionWidth);
        PlayerPrefs.SetInt("ResolutionHeight", tempResolutionHeight);

        //guardamos el estado del toggle en PlayerPrefs
        //tenemos que utilizar un int porque PlayerPrefs no soporta bools
        PlayerPrefs.SetInt("IsFullscreen", tempFullscreen ? 1 : 0); //1 = true, 0 = false

        PlayerPrefs.Save(); //nos aseguramos de guardar los cambios (por si acaso)

        Debug.Log("Settings applied correctly");
    }

    private void DefaultValues()
    {
        //valores predterminados (default)
        tempMusicVolume = 1.0f;
        tempSFXVolume = 1.0f;
        tempFullscreen = true;

        //actualizar sliders con los valores predeterminados
        musicSlider.value = tempMusicVolume;
        sfxSlider.value = tempSFXVolume;

        //actualizar audio
        musicAudioSource.volume = tempMusicVolume;
        sfxAudioSource.volume = tempSFXVolume;
        ballAudio.volume = tempSFXVolume;

        //actualizar textos
        musicVolumeText.text = tempMusicVolume.ToString("0.00");
        sfxVolumeText.text = tempSFXVolume.ToString("0.00");

        //actualizar fullscreen
        toggleScreen.isOn = tempFullscreen;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

        //obtenemos la resolución predeterminada del dispositivo
        Resolution defaultResolution = Screen.currentResolution;
        //actualizamos el dropdown con la resolución predeterminada
        tempResolutionWidth = defaultResolution.width;
        tempResolutionHeight = defaultResolution.height;
        //ahora si, aplicamos la resolución predeterminada
        Screen.SetResolution(tempResolutionWidth, tempResolutionHeight, Screen.fullScreen);

        for (int i = 0; i < resolutionDropdown.options.Count; i++)
        {
            if (resolutionDropdown.options[i].text ==
                tempResolutionWidth + " x " + tempResolutionHeight)
            {
                resolutionDropdown.value = i; //aplicar la resolución
                break; //salir del bucle al encontrarla
            }
        }
        resolutionDropdown.RefreshShownValue(); //actualizar el valor mostrado en el dropdown

        Debug.Log("Default values applied");
    }

    private bool HasUnsavedChanges()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

        int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1);
        bool savedFullscreenValue = savedFullscreen == 1;

        //aqui comprobamos si los valores temporales son diferentes a los guardados en PlayerPrefs
        if (tempMusicVolume != savedMusicVolume)
            return true; //si el valor temporal es diferente al guardado, hay cambios sin guardar

        if (tempSFXVolume != savedSFXVolume)
            return true;

        if (tempResolutionWidth != savedWidth ||
            tempResolutionHeight != savedHeight)
            return true;

        if (tempFullscreen != savedFullscreenValue)
            return true;

        return false; //si no hay diferencias, no hay cambios sin guardar
    }

    private void ShowWarning() //mostramos y permitimos interactuar con el canvas de warning
    {
        //evitamos poder interactuar con el canvas de pausa mientras el warning esta activo
        canvasFader.GetCanvasGroup().interactable = false;

        warningCanvasGroup.alpha = 1.0f;
        warningCanvasGroup.interactable = true;
        warningCanvasGroup.blocksRaycasts = true;
    }

    private void HideWarning() //volvemos a ocultar y desactivar el canvas de warning
    {
        warningCanvasGroup.alpha = 0.0f;
        warningCanvasGroup.interactable = false;
        warningCanvasGroup.blocksRaycasts = false;

        //volvemos a permitir interactuar con el canvas de pausa
        canvasFader.GetCanvasGroup().interactable = true;
    }

    //si cancelamos, entonces se cierra el warning y nos quedamos aun en el menu de pausa
    private void CancelExit()
    {
        HideWarning();
    }
    
    //si confirmamos, entonces se cierra el warning y salimos del menu de pausa
    private void ConfirmExit()
    {
        HideWarning();

        canvasFader.FadeOut(0.5f);
        Time.timeScale = 1.0f;
        canvasFader.GetCanvasGroup().interactable = false;
        isOpen = false;
    }

}