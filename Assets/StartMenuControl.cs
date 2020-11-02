using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuControl : MonoBehaviour
{
    public static StartMenuControl Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public PlayerProfile _lastProfile;

    public GameObject StartMenuCanvas;

    public Button ButtonResume;
    public Button ButtonNew;
    public Button ButtonLoad;
    public Button ButtonSettings;
    public Button ButtonExit;

    // Start is called before the first frame update
    void Start()
    {
        ButtonResume.gameObject.SetActive(_lastProfile != null);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PressResume()
    {
    }
    public void PressNew()
    {
        Game.Instance.NewGame(GameMode.GameModeType.Demo);
        HideMenu();
    }
    public void PressLoad()
    {

    }
    public void PressSettings()
    {

    }
    public void PressExit()
    {

    }
    public void ShowMenu()
    {
        StartMenuCanvas.gameObject.SetActive(true);
    }

    public void HideMenu()
    {
        StartMenuCanvas.gameObject.SetActive(false);
    }
    public void ToggerMenu()
    {
        StartMenuCanvas.gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
