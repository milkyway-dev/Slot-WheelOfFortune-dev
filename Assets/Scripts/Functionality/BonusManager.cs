
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class BonusManager : MonoBehaviour
{


    [SerializeField] private Transform rotator;
    [SerializeField] private Tween rotationTween;

    [SerializeField] internal int stopIndex;
    void Start()
    {

        Rotate();
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            // RotateToSegment(stopIndex);
            // StopAtInitialPosition();
            // StopRotate(stopIndex);
            // StopRotate1(stopIndex);
            StopAtIndex(stopIndex);
        }
    }

    void Rotate()
    {

        rotationTween = rotator.DOLocalRotate(new Vector3(0, 0, 360), 1f, RotateMode.LocalAxisAdd)
          .SetLoops(-1, LoopType.Incremental) // Set to loop indefinitely
          .SetEase(Ease.Linear);
    }

    void StopRotate(int index)
    {
        rotationTween.Kill();
        int indexc = index;
        rotationTween.OnStepComplete(() =>
        {
            rotationTween.Kill();
        });
        float targetAngle = (360 / 8) * index;
        float rotationAmount = (targetAngle) % 360f;

        //    if(targetAngle>180){
        //         indexc=Mathf.Abs((3-index )*45);
        //         roatator.localRotation=Quaternion.Euler(0,0,indexc );
        //    }else{
        //     roatator.localRotation=Quaternion.Euler(0,0,0);

        //    }


        //     float currentAngle = roatator.eulerAngles.z;



        //     float time=Mathf.Abs((currentAngle-targetAngle)/360);
        //     rotationTween = roatator.DOLocalRotate(new Vector3(0, 0, rotationAmount-22.5f), time).SetEase(Ease.InOutQuad)
        //                          .OnComplete(() => Debug.Log($"Stopped at segment {index} at angle {roatator.eulerAngles.z}"));

    }
    public void StopAtIndex(int targetIndex)
    {
        StartCoroutine(WaitForTargetIndex(targetIndex));
    }

    private IEnumerator WaitForTargetIndex(int targetIndex)
    {
        float degreesPerSegment = 360f / 8;
        float targetAngle = (targetIndex * degreesPerSegment);
        float threshold = 5f; // Angle threshold for stopping

        while (true)
        {
            float currentAngle = rotator.localEulerAngles.z;
            currentAngle = currentAngle % 360f;

            float distanceToTarget = Mathf.Abs(currentAngle - targetAngle);

            // If within threshold, stop the rotation
            if (distanceToTarget < threshold)
            {
                rotationTween.Kill(); // Stop the rotation
                rotator.DOLocalRotate(new Vector3(0,0,targetAngle-22.5f),0.25f).SetEase(Ease.Linear);
                // rotator.localRotation = Quaternion.Euler(0, 0, targetAngle); // Snap to the exact target angle
                Debug.Log($"Smoothly stopped at segment {targetIndex} with angle {targetAngle}");
                break;
            }
            yield return null; 
        }
    }

    public void StopRotate1(int index)
    {
        // Pause the current rotation
        rotationTween.OnStepComplete(() =>
        {

            rotator.localRotation = Quaternion.Euler(0, 0, -22.5f);
            rotationTween.Pause();


        });
        // The target angle is always 0 degrees
        float targetAngle = 0f;

        // Calculate the current angle
        float currentAngle = rotator.localEulerAngles.z;

        // Calculate the rotation needed to reach 0 degrees in an anti-clockwise direction
        float rotationAmount = (currentAngle - targetAngle + 360) % 360;

        // Start rotating toward the target angle anti-clockwise
        // rotationTween = roatator.DOLocalRotate(new Vector3(0, 0, currentAngle - rotationAmount), rotationAmount / 360f)
        //                         .SetEase(Ease.InOutQuad)
        //                         .OnComplete(() => Debug.Log($"Stopped at segment {index} at angle {roatator.eulerAngles.z}"));
    }


}