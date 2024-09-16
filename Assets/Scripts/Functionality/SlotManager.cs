using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.UI.Extensions;
public class SlotManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField] private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> slotmatrix;

    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;
    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Maxbet_button;
    [SerializeField] private Button BetPlus_Button;
    [SerializeField] private Button BetMinus_Button;

    [Header("Animated Sprites")]
    [SerializeField] private Sprite[] bar_Sprite;
    [SerializeField] private Sprite[] goldSpin_Sprite;
    [SerializeField] private Sprite[] fiveBar_Sprite;
    [SerializeField] private Sprite[] spin_Sprite;
    [SerializeField] private Sprite[] trippleSeven_Sprite;
    [SerializeField] private Sprite[] doubleSeven_Sprite;
    [SerializeField] private Sprite[] seven_Sprite;
    [SerializeField] private Sprite[] wheelofFortune_Sprite;

    [Header("Slot positions")]
    private float tweenHeight = 0;
    [SerializeField] private float reelHeight = 0;
    [SerializeField] private float stopPosition = 0;

    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private Button MaxBet_Button;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text BetPerLine_text;
    [SerializeField] private TMP_Text Total_lines;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;


    [SerializeField] private GameObject Image_Prefab;

    [SerializeField] private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField] private List<ImageAnimation> TempList;

    [SerializeField] private int IconSizeFactor = 100;

    [SerializeField] private SocketIOManager SocketManager;
    [SerializeField] private UIManager uiManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine = null;
    private bool IsAutoSpin = false;
    private bool IsSpinning = false;
    [SerializeField] private int spacefactor;

    private int BetCounter = 0;
    private int LineCounter = 0;

    internal bool CheckPopups;
    private double currentBalance = 0;
    private double currentTotalBet = 0;

    public List<reel> slot;

    [SerializeField] private List<int> LineId;
    [SerializeField] private List<string> points_AnimString;

    [Header("payline")]
    [SerializeField] private Vector2 InitialLinePosition = new Vector2(-315, 100);
    [SerializeField] private int x_Distance;
    [SerializeField] private int y_Distance;
    [SerializeField] private Transform LineContainer;
    [SerializeField] private GameObject Line_Prefab;
    private void Start()
    {
        // SetButton(SlotStart_Button, () => StartSlots());
        // SetButton(AutoSpin_Button, () => AutoSpin());
        // SetButton(AutoSpinStop_Button, StopAutoSpin);
        // SetButton(BetPlus_Button, () => ChangeBet(true));
        // SetButton(BetMinus_Button, () => ChangeBet(false));
        // SetButton(MaxBet_Button, MaxBet);
        tweenHeight = reelHeight + stopPosition;
    }

    void SetButton(Button button, Action action)
    {
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }
    }
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }
    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }



    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if (BetCounter < 10)
            {
                BetCounter++;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
        }

        if (BetPerLine_text) BetPerLine_text.text = BetCounter.ToString();
        if (TotalBet_text) TotalBet_text.text = BetCounter.ToString();
        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        // currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();
    }

    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> x_points = null;
        List<int> y_points = null;
        x_points = x_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

    //just for testing purposes delete on production
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        // {
        //     StartSlots();
        // }
    }

    internal void SetInitialUI()
    {
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        LineCounter = SocketManager.initialData.LinesCount.Count - 1;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        if (Total_lines) Total_lines.text = SocketManager.initialData.Lines.Count.ToString();
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    //function to populate animation sprites accordingly

    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 8:
                for (int i = 0; i < wheelofFortune_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(wheelofFortune_Sprite[i]);
                }
                animScript.AnimationSpeed = wheelofFortune_Sprite.Length;
                break;
            case 1:
                for (int i = 0; i < seven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(seven_Sprite[i]);
                }
                animScript.AnimationSpeed = seven_Sprite.Length;
                break;
            case 2:
                for (int i = 0; i < doubleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(doubleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = doubleSeven_Sprite.Length;
                break;
            case 5:
                for (int i = 0; i < spin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(spin_Sprite[i]);
                }
                animScript.AnimationSpeed = spin_Sprite.Length;
                break;
            case 4:
                for (int i = 0; i < fiveBar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(fiveBar_Sprite[i]);
                }
                animScript.AnimationSpeed = fiveBar_Sprite.Length;
                break;
            case 6:
                for (int i = 0; i < goldSpin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(goldSpin_Sprite[i]);
                }
                animScript.AnimationSpeed = goldSpin_Sprite.Length;
                break;
            case 3:
                for (int i = 0; i < bar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(bar_Sprite[i]);
                }
                animScript.AnimationSpeed = bar_Sprite.Length;
                break;
            case 7:
                for (int i = 0; i < trippleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(trippleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = trippleSeven_Sprite.Length;
                break;
            case 0:
                break;

        }
    }
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        // WinningsAnim(false);
        if (!autoSpin)
        {
            if (audioController) audioController.PlayButtonAudio("spin");

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        if (TempList.Count > 0)
        {
            StopGameAnimation();


        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }


    internal void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;

        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;
        balance = balance - (bet);

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });

    }

    internal void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }


    internal void ResetLines()
    {
        foreach (Transform child in LineContainer)
        {
            Destroy(child.gameObject);
        }
    }
    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {

        yield return new WaitForSeconds(0.1f);
        if (audioController) audioController.PlaySpinAudio();
        IsSpinning = true;
        if (currentBalance < currentTotalBet)
        {
            CompareBalance();
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(1f);
            }
            yield break;
        }
        // ToggleButtonGrp(false);

        // InitiateSpin();

        // BalanceDeduction();

        SocketManager.AccumulateResult(BetCounter);
        // yield return new WaitForSeconds(0.5f);


        // for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        // {
        //     List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //     for (int i = 0; i < 5; i++)
        //     {
        //         if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
        //         PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
        //     }
        // }

        // InitiateForAnimation();

        // yield return new WaitForSeconds(0.5f);

        // TerminateSpin();

        // yield return new WaitForSeconds(0.5f);

        // if (audioController) audioController.StopSpinAudio();

        // CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit);
        // CheckPayoutLineBackend(LineId, points_AnimString);

        // KillAllTweens();

        // CheckPopups = true;

        // currentBalance = SocketManager.playerdata.Balance;

        // CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);

        //if (SocketManager.resultData.jackpot > 0)
        //{
        //    CheckPopups = true;
        //    uiManager.PopulateWin(4, SocketManager.resultData.jackpot);
        //}

        //yield return new WaitUntil(() => !CheckPopups);

        // if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.WinAmout.ToString("f2");
        // if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        if (!IsAutoSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;

        }
        else
        {
            IsSpinning = false;
            yield return new WaitForSeconds(2f);
        }

    }

    internal void InitiateForAnimation()
    {
        for (int i = 0; i < slotmatrix.Count; i++)
        {
            for (int j = 0; j < slotmatrix[i].slotImages.Count; j++)
            {
                slotmatrix[i].slotImages[j].sprite = myImages[slot[i].data[j]];
                PopulateAnimationSprites(slotmatrix[i].slotImages[slotmatrix[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), slot[i].data[j]);

            }
        }
    }

    internal IEnumerator TerminateSpin()
    {
        WaitForSeconds delay = new WaitForSeconds(0.3f);

        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Pause();
            Slot_Transform[i].localPosition = new Vector2(Slot_Transform[i].localPosition.x, 0);
            alltweens[i] = Slot_Transform[i].DOLocalMoveY(stopPosition, 0.3f).SetEase(Ease.OutElastic);
            yield return delay;
            alltweens[i].Kill();
            // yield return StopTweening(Slot_Transform[i], i);
        }
        alltweens.Clear();
        // KillAllTweens();





    }

    internal IEnumerator InitiateSpin()
    {
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        Tweener tweener = null;
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            tweener = Slot_Transform[i].DOLocalMoveY(-tweenHeight, 0.15f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
            // tweener.Play();
            alltweens.Add(tweener);
            // InitializeTweening(Slot_Transform[i]);
            yield return delay;
        }
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotmatrix.Count; i++)
        {
            int prevRandomIndex = UnityEngine.Random.Range(0, myImages.Length);

            for (int j = 0; j < 5; j++)
            {
                int randomIndex = UnityEngine.Random.Range(1, myImages.Length);

                if (prevRandomIndex != 0)
                    randomIndex = 0;

                slotmatrix[i].slotImages[j].sprite = myImages[randomIndex];
                prevRandomIndex = randomIndex;

            }
        }
    }

    // private void OnApplicationFocus(bool focus)
    // {
    //     if (focus)
    //     {
    //         if (!IsSpinning)
    //         {
    //             if (audioController) audioController.StopWLAaudio();
    //         }
    //     }
    // }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    internal void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;

    }

    internal bool CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            if (SlotStart_Button) SlotStart_Button.interactable = false;
            return false;
        }
        else
        {
            if (AutoSpin_Button) AutoSpin_Button.interactable = true;
            if (SlotStart_Button) SlotStart_Button.interactable = true;
            return true;

        }
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        int i = animObjects.transform.childCount;


        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        // animObjects.transform.GetChild(0).gameObject.SetActive(true);

        temp.StartAnimation();

        TempList.Add(temp);

    }

    //stop the icons animation
    internal void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
            // if (TempList[i].transform.childCount > 0)
            //     TempList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        TempList.Clear();
        // TempList.TrimExcess();
    }

    internal void ProcessPayoutLines(List<int> LineId)
    {
        List<int> y_points = new List<int>() { 2, 2, 2 }; // Default y_points

        for (int i = 0; i < LineId.Count; i++)
        {
            // y_points could be modified here if needed
            GeneratePayline(y_points, 3); // Generating the payline with y_points
        }
    }

    internal void ProcessPointsAnimations(List<string> points_AnimString)
    {
        List<int> points_anim;

        for (int i = 0; i < points_AnimString.Count; i++)
        {
            points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

            if (points_anim != null)
            {
                for (int k = 0; k < points_anim.Count; k++)
                {
                    int index = points_anim[k];
                    if (index >= 10)
                    {
                        StartGameAnimation(slotmatrix[(index / 10) % 10].slotImages[index % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(slotmatrix[0].slotImages[index].gameObject);
                    }
                }
            }
        }
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = new List<int>() { 2, 2, 2 };
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            WinningsAnim(true);
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < LineId.Count; i++)
            {
                // y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                GeneratePayline(y_points, 3);
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(slotmatrix[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(slotmatrix[0].slotImages[points_anim[k]].gameObject);
                    }
                }
            }
        }

    }

    private void GeneratePayline(List<int> y_index, int Count)
    {
        GameObject MyLineObj = Instantiate(Line_Prefab, LineContainer);
        MyLineObj.transform.localPosition = new Vector2(InitialLinePosition.x, InitialLinePosition.y);
        UILineRenderer MyLine = MyLineObj.GetComponent<UILineRenderer>();
        List<Vector2> pointlist = new List<Vector2>();
        for (int i = 0; i < Count; i++)
        {
            pointlist.Add(new Vector2(i * x_Distance, y_index[i] * -y_Distance));
        }

        MyLine.Points = pointlist.ToArray();
    }




    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        // slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.1f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(stopPosition, 0.3f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    internal void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}


[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public class reel
{
    public List<int> data;
}

