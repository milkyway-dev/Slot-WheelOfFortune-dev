using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private SocketIOManager socketManager;
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

    private int BetCounter;
    void Start()
    {
        ErrorHandler.RunSafely(() =>
        {
            socketManager.OpenSocket();
            SlotStart_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StartSpin, OnError));
            AutoSpin_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StartAutoSpin, OnError));
            AutoSpinStop_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StopAutoSpin, OnError));
            slotManager.shuffleInitialMatrix();
            slotManager.UpdatePlayerData(socketManager.socketModel.playerData);

        }, OnError);
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


void OnBetChange(bool IncDec){

        // if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if (socketManager.socketModel.initGameData.Bets.Count < 10)
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
        // if (BetPerLine_text) BetPerLine_text.text = socketManager.socketModel.initGameData.Bets[BetCounter].ToString();
        // if (TotalBet_text) TotalBet_text.text = (socketManager.socketModel.initGameData.Bets[BetCounter]*3).ToString();

        // slotManager.UpdateBetText(socketManager.socketModel.initGameData.Bets[BetCounter]);
}


    bool OnSpinStart()
    {
        return ErrorHandler.RunSafely(() =>
        {

            slotManager.StopGameAnimation();
            slotManager.WinningsAnim(false);
            slotManager.ResetLines();
            bool start = slotManager.CompareBalance();
            if (start)
            {
                var spinData = new { data = new { currentBet = 0, currentLines = 20, spins = 1 }, id = "SPIN" };
                socketManager.SendData("message", spinData);
            }
            slotManager.ToggleButtonGrp(false);
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

    void OnSpinEnd()
    {
        ErrorHandler.RunSafely(() =>
        {
            slotManager.UpdatePlayerData(socketManager.socketModel.playerData);
            slotManager.ProcessPayoutLines(LineId);
            // TODO: WF enable animation
            // slotManager.ProcessPointsAnimations(points_AnimString);
            if (!IsAutoSpin) slotManager.ToggleButtonGrp(true);


        }, OnError);
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
    //     yield return new WaitUntil(() => socketManager.isResultdone);
    //     OnSpin(socketManager.socketModel.resultGameData.ResultReel);
    //     yield return new WaitForSeconds(0.5f);
    //     yield return ErrorHandler.RunSafely(slotManager.TerminateSpin(), OnError);
    //     if (!IsAutoSpin) IsSpinning = false;
    //     OnSpinEnd();
    }

    IEnumerator AutoSpinRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(2f);

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
}
