using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SlotManager slotManager;
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

    void Start()
    {
        ErrorHandler.RunSafely(() =>
        {
            SlotStart_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StartSpin,OnError));
            AutoSpin_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StartAutoSpin,OnError));
            AutoSpinStop_Button.onClick.AddListener(() => ErrorHandler.RunSafely(StopAutoSpin,OnError));
            slotManager.shuffleInitialMatrix();
        },OnError);
    }

    void StartAutoSpin()
    {
        if (IsSpinning) return;
        IsAutoSpin = true;

        ErrorHandler.RunSafely(() =>
        {
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);
            StartCoroutine(ErrorHandler.RunSafely(AutoSpinRoutine(),OnError));
        },OnError);
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
                StartCoroutine(ErrorHandler.RunSafely(StopAutoSpinCoroutine(),OnError));
            }
        },OnError);
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
            StartCoroutine(ErrorHandler.RunSafely(SpinRoutine(),OnError));
        },OnError);
    }

    bool OnSpinStart()
    {
        return ErrorHandler.RunSafely(() =>
        {
            slotManager.StopGameAnimation();
            slotManager.WinningsAnim(false);
            bool start = slotManager.CompareBalance();
            slotManager.ToggleButtonGrp(false);
            slotManager.ResetLines();
            return start;

        },OnError);
    }

    void OnSpin()
    {
        ErrorHandler.RunSafely(() =>
        {
            slotManager.InitiateForAnimation();
            slotManager.BalanceDeduction();
            // slotManager.CheckWinPopups();

        },OnError);
    }

    void OnSpinEnd()
    {
        ErrorHandler.RunSafely(() =>
        {
            slotManager.ProcessPayoutLines(LineId);
            slotManager.ProcessPointsAnimations(points_AnimString);
            if (!IsAutoSpin) slotManager.ToggleButtonGrp(true);


        },OnError);
    }

    IEnumerator SpinRoutine()
    {
        IsSpinning = true;

        OnSpinStart();
        yield return ErrorHandler.RunSafely(slotManager.InitiateSpin(),OnError);
        OnSpin();
        yield return new WaitForSeconds(0.5f);
        yield return ErrorHandler.RunSafely(slotManager.TerminateSpin(),OnError);

        if (!IsAutoSpin) IsSpinning = false;
        OnSpinEnd();
    }

    IEnumerator AutoSpinRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(2f);

        while (IsAutoSpin)
        {
            yield return ErrorHandler.RunSafely(SpinRoutine(),OnError);
            yield return delay;
        }

        IsSpinning = false;
    }

    void OnError(){

        slotManager.KillAllTweens();
    }
}
