using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.UI.Extensions;
using Newtonsoft.Json;
using UnityEngine.Rendering;
public class SlotManager : MonoBehaviour
{

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

    [SerializeField] private UIManager uiManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine = null;
    private bool IsSpinning = false;
    [SerializeField] private int spacefactor;

    private int BetCounter = 0;
    private int LineCounter = 0;

    internal bool CheckPopups;
    private double currentBalance = 0;
    private double currentTotalBet = 0;


    [SerializeField] private List<int> LineId;
    [SerializeField] private List<string> points_AnimString;

    [SerializeField] private Transform[] slot1Segment;

    [Header("payline")]
    [SerializeField] private Vector2 InitialLinePosition = new Vector2(-315, 100);
    [SerializeField] private int x_Distance;
    [SerializeField] private int y_Distance;
    [SerializeField] private Transform LineContainer;
    [SerializeField] private GameObject Line_Prefab;
    private void Start()
    {
        tweenHeight = reelHeight + stopPosition;
    }


    void MoveTest(float delay)
    {

        Vector3[] originalPositions = new Vector3[slot1Segment.Length];
        Transform lastElement;

        for (int i = 0; i < slot1Segment.Length; i++)
        {
            originalPositions[i] = slot1Segment[i].localPosition;
        }


        for (int i = 0; i < slot1Segment.Length - 1; i++)
        {
            slot1Segment[i].DOLocalMoveY(originalPositions[i + 1].y, delay, false).SetEase(Ease.Linear);
        }

        // Move the last element to the first position and then perform reordering
        slot1Segment[slot1Segment.Length - 1].DOLocalMoveY(originalPositions[0].y, delay, false).SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       lastElement = slot1Segment[slot1Segment.Length - 1];

                       // Shift array elements down
                       for (int i = slot1Segment.Length - 1; i > 0; i--)
                       {
                           slot1Segment[i] = slot1Segment[i - 1];
                       }
                       slot1Segment[0] = lastElement;

                       // Reactivate the last element
                       slot1Segment[0].gameObject.SetActive(true);
                   });
    }

    IEnumerator Move()
    {
        while (true)
        {
            MoveTest(0.4f);
            yield return new WaitForSeconds(0.4f);
        }

    }


    // TODO: Wf update  bet text
    internal void UpdateBetText(double betperline, int  totalLines)
    {
        Debug.Log($"in update text {betperline},{totalLines}");
        if (audioController) audioController.PlayButtonAudio();
        if (BetPerLine_text) BetPerLine_text.text = betperline.ToString();
        if (TotalBet_text) TotalBet_text.text = (totalLines*betperline).ToString();
        // CompareBalance();
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


    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

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
            WinTween = TotalWin_text.transform.DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.transform.localScale = Vector3.one;
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

    internal void InitiateForAnimation(List<List<int>> result)
    {
        Debug.Log(JsonConvert.SerializeObject(result));
        for (int i = 0; i < slotmatrix.Count; i++)
        {
            for (int j = 0; j < slotmatrix[i].slotImages.Count; j++)
            {
                Debug.Log($"{myImages[result[i][j]].name}");
                slotmatrix[i].slotImages[j].sprite = myImages[result[i][j]];
                // PopulateAnimationSprites(slotmatrix[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), result[i][j]);

            }
        }
    }

    internal void UpdatePlayerData(PlayerData playerData)
    {

        TotalWin_text.text = playerData.currentWining.ToString();
        Balance_text.text = playerData.Balance.ToString();

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
            tweener = Slot_Transform[i].DOLocalMoveY(-tweenHeight, 0.5f, false).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
            // tweener.Play();
            alltweens.Add(tweener);
            // InitializeTweening(Slot_Transform[i]);
            yield return delay;
        }
    }




    internal int  CheckWinPopups(double WinAmout)
    {
        if (WinAmout >= currentTotalBet * 10 && WinAmout < currentTotalBet * 15)
        {
            return 1;
        }
        else if (WinAmout >= currentTotalBet * 15 && WinAmout < currentTotalBet * 20)
        {
            return 2;

        }
        else if (WinAmout >= currentTotalBet * 20)
        {
            return 3;
        }else{

            return 0;
        }

    }

    internal void shuffleInitialMatrix()
    {
        // int randomIndex = UnityEngine.Random.Range(1, myImages.Length);
        for (int i = 0; i < slotmatrix.Count; i++)
        {

            for (int j = 0; j < slotmatrix[i].slotImages.Count; j++)
            {

                slotmatrix[i].slotImages[j].sprite = myImages[UnityEngine.Random.Range(0, myImages.Length)];

            }

        }
    }

    internal void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;

    }

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

    internal void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }

}


[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}



