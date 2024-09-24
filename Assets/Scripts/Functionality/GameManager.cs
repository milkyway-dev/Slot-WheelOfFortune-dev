using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private SocketIOManager socketManager;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private BonusManager bonusManager;
    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Maxbet_button;
    [SerializeField] private Button BetPlus_Button;
    [SerializeField] private Button BetMinus_Button;
    public bool IsSpinning = false;
    public bool IsAutoSpin = false;

    [Header("fake results")]
    [SerializeField] private List<int> LineId;
    [SerializeField] private List<string> points_AnimString;

    [SerializeField] private int BetCounter = 0;

    private double currentBalance = 0;
    private double currentTotalBet = 0;
    private void Start()
    {

        ErrorHandler.RunSafely(() =>
        {
            StartCoroutine(ErrorHandler.RunSafely(StartGameRouitne(),OnError));

        }, OnError);
    }
    IEnumerator StartGameRouitne()
    {

        socketManager.OpenSocket();
        yield return new WaitUntil(() => !socketManager.isLoading);

        SlotStart_Button.onClick.AddListener(StartSpin);
        AutoSpin_Button.onClick.AddListener(StartAutoSpin);
        AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);
        slotManager.shuffleInitialMatrix();
        slotManager.UpdatePlayerData(socketManager.socketModel.playerData);
        CompareBalance();
        BetPlus_Button.onClick.AddListener(delegate { OnBetChange(true); });
        BetMinus_Button.onClick.AddListener(delegate { OnBetChange(false); });
        slotManager.UpdateBetText(socketManager.socketModel.initGameData.Bets[BetCounter], socketManager.socketModel.initGameData.Lines.Count);
        currentTotalBet=socketManager.socketModel.initGameData.Bets[BetCounter]*socketManager.socketModel.initGameData.Lines.Count;
        currentBalance=socketManager.socketModel.playerData.Balance;
        bonusManager.values=socketManager.socketModel.initGameData.BonusPayout;
        Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");

        //  uIManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    void StartAutoSpin()
    {
        if (IsSpinning) return;
        IsAutoSpin = true;

        ErrorHandler.RunSafely(() =>
        {
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);
            StartCoroutine(ErrorHandler.RunSafely(AutoSpinRoutine(), OnError));
        }, OnError);
    }

    private void StopAutoSpin()
    {
        ErrorHandler.RunSafely(() =>
        {
            if (IsAutoSpin)
            {
                IsAutoSpin = false;
                if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
                if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
                StartCoroutine(ErrorHandler.RunSafely(StopAutoSpinCoroutine(), OnError));
            }
        }, OnError);
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        StopAllCoroutines();
        slotManager.ToggleButtonGrp(true);
    }

    void StartSpin()
    {
        ErrorHandler.RunSafely(() =>
        {
            StartCoroutine(ErrorHandler.RunSafely(SpinRoutine(), OnError));
        }, OnError);
    }


    void OnBetChange(bool IncDec)
    {

        // if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if (BetCounter < socketManager.socketModel.initGameData.Bets.Count - 1)
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
        // TODO: WF to be done
        currentTotalBet=socketManager.socketModel.initGameData.Bets[BetCounter]*socketManager.socketModel.initGameData.Lines.Count;

        slotManager.UpdateBetText(socketManager.socketModel.initGameData.Bets[BetCounter], socketManager.socketModel.initGameData.Lines.Count);
        CompareBalance();

    }


    bool OnSpinStart()
    {
        return ErrorHandler.RunSafely(() =>
        {

            slotManager.StopGameAnimation();
            slotManager.WinningsAnim(false);
            slotManager.ResetLines();
            bool start = CompareBalance();
            if (start)
            {
                var spinData = new { data = new { currentBet = BetCounter, currentLines = socketManager.socketModel.initGameData.Lines.Count, spins = 1 }, id = "SPIN" };
                socketManager.SendData("message", spinData);
                slotManager.ToggleButtonGrp(false);
            }
            return start;

        }, OnError);
    }

    void OnSpin(List<List<int>> result)
    {
        ErrorHandler.RunSafely(() =>
        {
            
            slotManager.InitiateForAnimation(result);
            slotManager.BalanceDeduction();
        }, OnError);
    }

    IEnumerator OnSpinEnd()
    {

            currentBalance=socketManager.socketModel.playerData.Balance;
            slotManager.UpdatePlayerData(socketManager.socketModel.playerData);
            slotManager.ProcessPayoutLines(LineId);
            // TODO: WF enable animation
            // slotManager.ProcessPointsAnimations(points_AnimString);
            if(socketManager.socketModel.resultGameData.isbonus){
                bonusManager.targetIndex=socketManager.socketModel.resultGameData.BonusIndex;
                bonusManager.multipler=socketManager.socketModel.initGameData.Lines.Count;
                bonusManager.StartBonus();
                yield return new WaitUntil(()=>!bonusManager.isBonusPlaying);
            }
            else if(socketManager.socketModel.playerData.currentWining>0){
                int wintype=CheckWinPopups(socketManager.socketModel.playerData.currentWining);

                if(wintype>0){
                    bool checking=true;
                    uIManager.PopulateWin(wintype,socketManager.socketModel.playerData.currentWining,(state)=>checking=state);
                    yield return new WaitUntil(()=>!checking);
                }

                slotManager.WinningsAnim(true);
                yield return new WaitUntil(()=>!bonusManager.isBonusPlaying);

            }
            if (!IsAutoSpin) slotManager.ToggleButtonGrp(true);

            yield return null;

        ;
    }

    IEnumerator SpinRoutine()
    {
        IsSpinning = true;

        bool start = OnSpinStart();
        if (!start)
        {
            OnSpinEnd();
            yield break;
        }
        yield return ErrorHandler.RunSafely(slotManager.InitiateSpin(), OnError);
        yield return new WaitUntil(() => socketManager.isResultdone);
        OnSpin(socketManager.socketModel.resultGameData.ResultReel);
        yield return new WaitForSeconds(0.5f);
        yield return ErrorHandler.RunSafely(slotManager.TerminateSpin(), OnError);
        if (!IsAutoSpin) IsSpinning = false;
        yield return ErrorHandler.RunSafely(OnSpinEnd(),OnError);
    }

    IEnumerator AutoSpinRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(1.2f);

        while (IsAutoSpin)
        {
            yield return ErrorHandler.RunSafely(SpinRoutine(), OnError);
            yield return delay;
        }

        IsSpinning = false;
    }

    void OnError()
    {
        slotManager.KillAllTweens();
    }


    private bool CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uIManager.LowBalPopup();
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
}
