
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class BonusManager : MonoBehaviour
{


    [SerializeField] private GameObject bonusPanel;
    [SerializeField] private Transform rotator;

    [SerializeField] private TMP_Text winText;
    [SerializeField] private GameObject winPopup;
    [SerializeField] private Tween rotationTween = null;

    // [SerializeField] internal int stopIndex;
    [SerializeField] int segmentCount;
    private float degreesPerSegment;
    private float offSet;

    [SerializeField] private TMP_Text[] valueTextList;
    [SerializeField] internal List<int> values;

    [SerializeField] private GameObject lightOff;

    [SerializeField] private Button Spin_Button;
    internal int targetIndex;
    internal double multipler;

    internal bool isBonusPlaying=false;
    void Start()
    {
        degreesPerSegment = 360f / segmentCount;
        offSet = degreesPerSegment / 2;

        Spin_Button.onClick.AddListener(() => OnSpinStart());
        // OnSpinStart();
    }

    internal void StartBonus()
    {
        bonusPanel.SetActive(true);
        for (int i = 0; i < valueTextList.Length; i++)
        {
            valueTextList[i].text = values[i].ToString();
        }
        isBonusPlaying=true;
    }
    private void OnSpinStart()
    {


        Spin_Button.interactable = false;
        rotationTween ??= rotator.DOLocalRotate(new Vector3(0, 0, -360), 2.5f, RotateMode.LocalAxisAdd)
              .SetLoops(-1, LoopType.Incremental)
              .SetEase(Ease.Linear);

        InvokeRepeating(nameof(LightAnimation), 0, 0.25f);

        StartCoroutine(WaitForTargetIndex());
    }

    private IEnumerator WaitForTargetIndex()
    {
        Debug.Log("started stopping");
        if (rotationTween == null)
            yield break;

        yield return new WaitForSeconds(3f);
        winText.text = (values[targetIndex] * multipler).ToString();
        rotationTween.timeScale = 0.5f;
        float targetAngle = targetIndex * degreesPerSegment;
        float threshold = 5f;
        bool hasCrossedTarget = false;
        float distanceToTarget = 0;
        float currentAngle = 0;

        while (true)
        {
            currentAngle = rotator.localEulerAngles.z % 360f;
            if (currentAngle < 0) currentAngle += 360f;
            distanceToTarget = Mathf.Abs(currentAngle - targetAngle);

            if (targetAngle == 0)
            {
                hasCrossedTarget = currentAngle > 180;
            }
            else
            {
                hasCrossedTarget = (currentAngle > targetAngle && currentAngle - targetAngle < 180) || (currentAngle < targetAngle && targetAngle - currentAngle > 180);
            }

            if (distanceToTarget < threshold && !hasCrossedTarget)
            {
                rotationTween.Kill();
                rotationTween=null;
                Debug.Log(targetAngle);
                rotator.DOLocalRotate(new Vector3(0, 0, targetAngle - offSet), 0.25f).SetEase(Ease.Linear);
                CancelInvoke(nameof(LightAnimation));
                lightOff.SetActive(false);
                yield return new WaitForSeconds(1.5f);
                OnSpinEnd();
                break;
            }
            yield return null;
        }
    }



    void OnSpinEnd()
    {
        winPopup.transform.localScale = Vector3.zero;
        winPopup.SetActive(true);

        winPopup.transform.DOScale(Vector3.one, 1.3f).SetEase(Ease.OutElastic).OnComplete(() =>
        {
            winPopup.SetActive(false);
            bonusPanel.SetActive(false);
            // winText.text = "";
            lightOff.SetActive(true);
            Spin_Button.interactable = true;
            isBonusPlaying=false;
            // for (int i = 0; i < valueTextList.Length; i++)
            // {
            //     valueTextList[i].text = "";
            // }
            // values.Clear();

        });
        
    }

    void LightAnimation()
    {
        lightOff.SetActive(!lightOff.activeSelf);
    }
}