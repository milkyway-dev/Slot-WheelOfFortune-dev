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

    //[Header("Menu UI")]
    //[SerializeField]
    //private Button Menu_Button;
    //[SerializeField]
    //private GameObject Menu_Object;
    //[SerializeField]
    //private RectTransform Menu_RT;

    [SerializeField]
    private Button About_Button;
    [SerializeField]
    private GameObject About_Object;
    [SerializeField]
    private RectTransform About_RT;

    //[Header("Settings UI")]

    //[SerializeField]
    //private GameObject Settings_Object;
    //[SerializeField]
    //private RectTransform Settings_RT;
    //[SerializeField]
    //private Button Terms_Button;
    //[SerializeField]
    //private Button Privacy_Button;

    [SerializeField]
    private Button Exit_Button;
    [SerializeField]
    private GameObject Exit_Object;
    [SerializeField]
    private RectTransform Exit_RT;

    //[SerializeField]
    //private Button Paytable_Button;
    //[SerializeField]
    //private GameObject Paytable_Object;
    //[SerializeField]
    //private RectTransform Paytable_RT;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;
    //[SerializeField]
    //private Image AboutLogo_Image;
    //[SerializeField]
    //private Button Support_Button;

    //[Header("Paytable Popup")]
    //[SerializeField]
    //private GameObject PaytablePopup_Object;
    //[SerializeField]
    //private Button PaytableExit_Button;
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

    //[SerializeField]
    //private GameObject MusicOn_Object;
    //[SerializeField]
    //private GameObject MusicOff_Object;
    //[SerializeField]
    //private GameObject SoundOn_Object;
    //[SerializeField]
    //private GameObject SoundOff_Object;

    [Header("all Win Popup")]
    [SerializeField]
    private Sprite BigWin_Sprite;
    [SerializeField]
    private Sprite HugeWin_Sprite;
    [SerializeField]
    private Sprite MegaWin_Sprite;
    [SerializeField]
    private Image Win_Image;
    [SerializeField]
    private GameObject WinPopup_Object;
    [SerializeField]
    private TMP_Text Win_Text;


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
    [SerializeField] private SlotBehaviour slotManager;
    [SerializeField] private SocketIOManager socketManager;

    [Header("disconnection popup")]
    [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;

    [Header("Quit Popup")]
    [SerializeField] private GameObject QuitPopupObject;
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


    private void Awake()
    {
        if (spalsh_screen) spalsh_screen.SetActive(true);
        StartCoroutine(LoadingRoutine());
    }

    private void Start()
    {

        //if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        //if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

        if (yes_Button) yes_Button.onClick.RemoveAllListeners();
        if (yes_Button) yes_Button.onClick.AddListener(CallOnExitFunction);

        if (no_Button) no_Button.onClick.RemoveAllListeners();
        if (no_Button) no_Button.onClick.AddListener(delegate { ClosePopup(QuitPopupObject); });

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopupObject); });

        if (About_Button) About_Button.onClick.RemoveAllListeners();
        if (About_Button) About_Button.onClick.AddListener(delegate { OpenPopup(AboutPopup_Object); });

        if (AboutExit_Button) AboutExit_Button.onClick.RemoveAllListeners();
        if (AboutExit_Button) AboutExit_Button.onClick.AddListener(delegate { ClosePopup(AboutPopup_Object); });


        //if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        //if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        //if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        //if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

        if (MusicOn_Button) MusicOn_Button.onClick.RemoveAllListeners();
        if (MusicOn_Button) MusicOn_Button.onClick.AddListener(ToggleMusic);

        if (MusicOff_Button) MusicOff_Button.onClick.RemoveAllListeners();
        if (MusicOff_Button) MusicOff_Button.onClick.AddListener(ToggleMusic);

        if (SoundOn_Button) SoundOn_Button.onClick.RemoveAllListeners();
        if (SoundOn_Button) SoundOn_Button.onClick.AddListener(ToggleSound);

        if (SoundOff_Button) SoundOff_Button.onClick.RemoveAllListeners();
        if (SoundOff_Button) SoundOff_Button.onClick.AddListener(ToggleSound);
        //if (MusicOn_Object) MusicOn_Object.SetActive(true);
        //if (MusicOff_Object) MusicOff_Object.SetActive(false);

        //if (SoundOn_Object) SoundOn_Object.SetActive(true);
        //if (SoundOff_Object) SoundOff_Object.SetActive(false);



        paytableList[CurrentIndex = 0].SetActive(true);

        if (LeftBtn) LeftBtn.onClick.RemoveAllListeners();
        if (LeftBtn) LeftBtn.onClick.AddListener(delegate { Slide(-1); });

        if (RightBtn) RightBtn.onClick.RemoveAllListeners();
        if (RightBtn) RightBtn.onClick.AddListener(delegate { Slide(+1); });


        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(CallOnExitFunction);

        if (Close_Button) Close_Button.onClick.AddListener(delegate { ClosePopup(LowBalancePopup_Object); });
        //if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        //if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });

        //if (audioController) audioController.ToggleMute(false);

        if (QuitSplash_button) QuitSplash_button.onClick.RemoveAllListeners();
        if (QuitSplash_button) QuitSplash_button.onClick.AddListener(delegate { OpenPopup(QuitPopupObject); });

        isMusic = false;
        isSound = false;
        ToggleMusic();
        ToggleSound();


    }

    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;

        }

        if (value == 4)
            StartPopupAnim(amount, true);
        else
            StartPopupAnim(amount, false);

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

    private void StartPopupAnim(double amount, bool jackpot = false)
    {
        int initAmount = 0;
        if (jackpot)
        {
            if (jackpot_Object) jackpot_Object.SetActive(true);
        }
        else
        {
            if (WinPopup_Object) WinPopup_Object.SetActive(true);

        }

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, (int)amount, 5f).OnUpdate(() =>
        {
            if (jackpot)
            {
                if (jackpot_Text) jackpot_Text.text = initAmount.ToString();
            }
            else
            {
                if (Win_Text) Win_Text.text = initAmount.ToString();

            }
        });

        DOVirtual.DelayedCall(6f, () =>
        {
            if (jackpot)
            {
                ClosePopup(jackpot_Object);

            }
            else
            {
                ClosePopup(WinPopup_Object);
            }
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
        PopulateSymbolsPayout(symbolsText);
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

    //private void OpenMenu()
    //{
    //   // audioController.PlayButtonAudio();
    //   // if (Menu_Object) Menu_Object.SetActive(false);
    //    if (Exit_Object) Exit_Object.SetActive(true);
    //    if (About_Object) About_Object.SetActive(true);
    //   // if (Paytable_Object) Paytable_Object.SetActive(true);
    //    if (Settings_Object) Settings_Object.SetActive(true);

    //    DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y + 150), 0.1f).OnUpdate(() =>
    //    {
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
    //    });

    //    //DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y + 300), 0.1f).OnUpdate(() =>
    //    //{
    //    //    LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
    //    //});

    //    DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y + 450), 0.1f).OnUpdate(() =>
    //    {
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
    //    });
    //}

    //private void CloseMenu()
    //{

    //    DOTween.To(() => About_RT.anchoredPosition, (val) => About_RT.anchoredPosition = val, new Vector2(About_RT.anchoredPosition.x, About_RT.anchoredPosition.y - 150), 0.1f).OnUpdate(() =>
    //    {
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(About_RT);
    //    });

    //   // DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x, Paytable_RT.anchoredPosition.y - 300), 0.1f).OnUpdate(() =>
    //    //{
    //    //    LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
    //    //});

    //    DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x, Settings_RT.anchoredPosition.y - 450), 0.1f).OnUpdate(() =>
    //    {
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
    //    });

    //    DOVirtual.DelayedCall(0.1f, () =>
    //    {
    //       // if (Menu_Object) Menu_Object.SetActive(true);
    //        if (Exit_Object) Exit_Object.SetActive(false);
    //        if (About_Object) About_Object.SetActive(false);
    //        //if (Paytable_Object) Paytable_Object.SetActive(false);
    //        if (Settings_Object) Settings_Object.SetActive(false);
    //    });
    //}

    private void OpenPopup(GameObject Popup)
    {
        if(audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        paytableList[CurrentIndex = 0].SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if(audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
        paytableList[CurrentIndex].SetActive(false);
    }



    private void Slide(int direction)
    {
        if(audioController) audioController.PlayButtonAudio();

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
        if(CurrentIndex==paytableList.Length - 1){
            RightBtn.interactable=false;
        }else{
            RightBtn.interactable=true;

        }
        if(CurrentIndex==0){
            LeftBtn.interactable=false;
        }else{
            LeftBtn.interactable=true;
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
