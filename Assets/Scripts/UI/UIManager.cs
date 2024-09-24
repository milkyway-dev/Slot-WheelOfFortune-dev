using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{



    [SerializeField] private Button About_Button;

    [Header("Popus UI")]
    [SerializeField] private GameObject PopUpPanel;
    [SerializeField] private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField] private GameObject AboutPopup_Object;
    [SerializeField] private Button AboutExit_Button;
    [Header("Paytable Texts")]
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text Scatter_Text;
    [SerializeField] private TMP_Text Wild_Text;

    [Header("Settings Popup")]
    [SerializeField] private GameObject SettingsPopup_Object;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;
    [SerializeField] private Button SoundOn_Button;
    [SerializeField] private Button SoundOff_Button;
    [SerializeField] private Button MusicOn_Button;
    [SerializeField] private Button MusicOff_Button;

    [Header("all Win Popup")]
    [SerializeField] private Sprite BigWin_Sprite;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;
    [SerializeField] private TMP_Text Win_text_title;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;


    [Header("jackpot Win Popup")]
    [SerializeField] private TMP_Text jackpot_Text;
    [SerializeField] private GameObject jackpot_Object;


    [Header("low balance popup")]
    [SerializeField] private GameObject LowBalancePopup_Object;
    [SerializeField] private Button Close_Button;

    //[Header("FreeSpins Popup")]
    //[SerializeField]
    //private GameObject FreeSpinPopup_Object;
    //[SerializeField]
    //private TMP_Text Free_Text;
    //[SerializeField]
    //private Button FreeSpin_Button;

    [Header("Scripts")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private SocketIOManager socketManager;

    [Header("disconnection popup")]
    [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;

    [Header("Quit Popup")]
    [SerializeField] private GameObject quitPopupObject;
    [SerializeField] private Button yes_Button;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button no_Button;


    [Header("Splash Screen")]
    [SerializeField] private GameObject spalsh_screen;
    [SerializeField] private Image progressbar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;
    //private int FreeSpins;
    [Header("Pagination")]
    int CurrentIndex = 0;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private Button RightBtn;
    [SerializeField] private Button LeftBtn;

    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    public GameObject ActivePopup = null;

    private void Awake()
    {
        // if (spalsh_screen) spalsh_screen.SetActive(true);
        // StartCoroutine(LoadingRoutine());
    }

    private void Start()
    {

        SetButton(yes_Button, CallOnExitFunction);
        SetButton(no_Button, () => ClosePopup());
        SetButton(GameExit_Button, () => OpenPopup(quitPopupObject));
        SetButton(About_Button, () => OpenPopup(AboutPopup_Object));
        SetButton(AboutExit_Button, () => ClosePopup());
        SetButton(Settings_Button, () => OpenPopup(SettingsPopup_Object));
        SetButton(SettingsExit_Button, () => ClosePopup());
        SetButton(MusicOn_Button, ToggleMusic);
        SetButton(MusicOff_Button, ToggleMusic);
        SetButton(SoundOn_Button, ToggleSound);
        SetButton(SoundOff_Button, ToggleSound);
        SetButton(LeftBtn, () => Slide(-1));
        SetButton(RightBtn, () => Slide(1));
        SetButton(CloseDisconnect_Button, CallOnExitFunction);
        SetButton(Close_Button, () => ClosePopup());
        SetButton(QuitSplash_button, () => OpenPopup(quitPopupObject));
        isMusic = false;
        isSound = false;
        ToggleMusic();
        ToggleSound();


    }

    void SetButton(Button button, Action action)
    {
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }
    }
    internal void PopulateWin(int value, double amount,Action<bool> isDone)
    {   
        switch (value)
        {
            case 1:
                if (Win_text_title) Win_text_title.text = "Big Win";
                break;
            case 2:
                if (Win_text_title) Win_text_title.text = "Huge Win";
                break;
            case 3:
                if (Win_text_title) Win_text_title.text = "Mega WIn";
                break;

        }

        double initAmount = 0;

        if (WinPopup_Object) WinPopup_Object.SetActive(true);

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 5f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString("f2");
        });

        DOVirtual.DelayedCall(6.5f, () =>
        {

            ClosePopup();
            slotManager.CheckPopups = false;
            isDone(false);
        });
        // StartPopupAnim(amount);

    }

    private IEnumerator LoadingRoutine()
    {
        StartCoroutine(LoadingTextAnimate());
        float fillAmount = 0.7f;
        progressbar.DOFillAmount(fillAmount, 2f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(2f);
        yield return new WaitUntil(() => !socketManager.isLoading);
        progressbar.DOFillAmount(1, 1f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(1f);
        if (spalsh_screen) spalsh_screen.SetActive(false);
        StopCoroutine(LoadingTextAnimate());
    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (loadingText) loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }
    }

    //private void StartFreeSpins(int spins)
    //{
    //    if (MainPopup_Object) MainPopup_Object.SetActive(false);
    //  //  if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
    //    //slotManager.FreeSpin(spins);
    //}

    //internal void FreeSpinProcess(int spins)
    //{
    //    FreeSpins = spins;
    //    //if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(true);
    //    //if (Free_Text) Free_Text.text = spins.ToString();
    //    if (MainPopup_Object) MainPopup_Object.SetActive(true);
    //}

    private void StartPopupAnim(double amount)
    {
        double initAmount = 0;

        if (WinPopup_Object) WinPopup_Object.SetActive(true);

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 5f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString("f2");
        });

        DOVirtual.DelayedCall(6.5f, () =>
        {

            ClosePopup();
            slotManager.CheckPopups = false;
        });
    }

    internal void LowBalPopup()
    {

        OpenPopup(LowBalancePopup_Object);
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        //if (Support_Button) Support_Button.onClick.RemoveAllListeners();
        //if (Support_Button) Support_Button.onClick.AddListener(delegate { UrlButtons(SupportUrl); });

        //if (Terms_Button) Terms_Button.onClick.RemoveAllListeners();
        //if (Terms_Button) Terms_Button.onClick.AddListener(delegate { UrlButtons(TermsUrl); });

        //if (Privacy_Button) Privacy_Button.onClick.RemoveAllListeners();
        //if (Privacy_Button) Privacy_Button.onClick.AddListener(delegate { UrlButtons(PrivacyUrl); });

        //StartCoroutine(DownloadImage(AbtImgUrl));
        // PopulateSymbolsPayout(symbolsText);
        //PopulateSpecialSymbols(Specialsymbols);
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].Multiplier[0][0] != 0)
            {
                text += "5x - " + paylines.symbols[i].Multiplier[0][0];
            }
            if (paylines.symbols[i].Multiplier[1][0] != 0)
            {
                text += "\n4x - " + paylines.symbols[i].Multiplier[1][0];
            }
            if (paylines.symbols[i].Multiplier[2][0] != 0)
            {
                text += "\n3x - " + paylines.symbols[i].Multiplier[2][0];
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {

            if (paylines.symbols[i].Name.ToUpper() == "SCATTER")
            {
                if (Scatter_Text) Scatter_Text.text = paylines.symbols[i].description.ToString();
            }

            if (paylines.symbols[i].Name.ToUpper() == "WILD")
            {
                if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
            }
        }

        // for (int i = 0; i < paylines.symbols.Count; i++)
        // {

        //     if (paylines.symbols[i].Name.ToUpper() == "SCATTER")
        //     {
        //        Scatter_Text.text = "Offers higher pay outs and awards.\nPayout:\n <color=#ED5B04>5x - " + paylines.symbols[i].Multiplier[0][0]+ "</color >\n" + "<color=#ED5B04>4x - " + paylines.symbols[i].Multiplier[1][0] + "</color >\n" + "<color=#ED5B04> 3x - " + paylines.symbols[i].Multiplier[2][0] + "</color >";
        //     }


        // }
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        socketManager.CloseSocket();
        //slotManager.CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }


    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        ActivePopup = Popup;
        paytableList[CurrentIndex = 0].SetActive(true);
    }

    private void ClosePopup()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (ActivePopup) ActivePopup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
        paytableList[CurrentIndex].SetActive(false);
    }



    private void Slide(int direction)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (CurrentIndex < paytableList.Length - 1 && direction > 0)
        {
            // Move to the next item
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex + 1].SetActive(true);

            CurrentIndex++;



        }
        else if (CurrentIndex >= 1 && direction < 0)
        {

            // Move to the previous item
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex - 1].SetActive(true);

            CurrentIndex--;


        }
        if (CurrentIndex == paytableList.Length - 1)
        {
            RightBtn.interactable = false;
        }
        else
        {
            RightBtn.interactable = true;

        }
        if (CurrentIndex == 0)
        {
            LeftBtn.interactable = false;
        }
        else
        {
            LeftBtn.interactable = true;
        }


    }

    internal void DisconnectionPopup()
    {

        //ClosePopup(ReconnectPopup_Object);
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }

    }



    private void ToggleMusic()
    {
        //private Button SoundOn_Button;
        //private Button SoundOff_Button;
        //private Button MusicOn_Button;
        //private Button MusicOff_Button;
        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Button) MusicOn_Button.interactable = false;
            if (MusicOff_Button) MusicOff_Button.interactable = true;
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Button) MusicOn_Button.interactable = true;
            if (MusicOff_Button) MusicOff_Button.interactable = false;
            audioController.ToggleMute(true, "bg");
        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Button) SoundOn_Button.interactable = false;
            if (SoundOff_Button) SoundOff_Button.interactable = true;
            if (audioController) audioController.ToggleMute(false, "button");
            if (audioController) audioController.ToggleMute(false, "wl");
        }
        else
        {
            if (SoundOn_Button) SoundOn_Button.interactable = true;
            if (SoundOff_Button) SoundOff_Button.interactable = false;
            if (audioController) audioController.ToggleMute(true, "button");
            if (audioController) audioController.ToggleMute(true, "wl");
        }
    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }


    private IEnumerator DownloadImage(string url)
    {
        // Create a UnityWebRequest object to download the image
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // Wait for the download to complete
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Apply the sprite to the target image
            //  AboutLogo_Image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
    }
}
