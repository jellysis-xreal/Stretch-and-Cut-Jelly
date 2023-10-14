using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class PullAndCut : MonoBehaviour
{
    public GameObject MeshCutter;

    [SerializeField] private float maxPullDistance;
    [SerializeField] private bool isSetPosition = false;
    [SerializeField] private bool activeCut = false;
    
    private XRGrabInteractable grabInteractable;
    private Pose primaryAttachPose, secondaryAttachPose;

    private Pose originPose;
    private Vector3 middlePoint;
    private float distance;


    // Start is called before the first frame update
    void Start()
    {
        MeshCutter = GameObject.FindWithTag("Cutter");
        grabInteractable = GetComponent<XRGrabInteractable>();

        maxPullDistance = 1.0f;
    }

    void Initiate()
    {
        // object를 잡은 두 Hand
        primaryAttachPose = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).GetWorldPose();
        secondaryAttachPose = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable).GetWorldPose();
        MeshCutter.GetComponent<MeshCutter>().enabled = true;

        if (!isSetPosition)
        {
            // 두 손으로 Grab한 순간의 Sliceable object의 위치를 저장함
            originPose = this.gameObject.transform.GetWorldPose();
            isSetPosition = true;
            
            // [FIX] 오브젝트가 중간에 위치함
            grabInteractable.trackPosition = false;
        }
    }
    
    void SetMeshCutter(Pose First, Pose Second)
    {
        middlePoint = (First.position + Second.position) / 2;
        //Debug.DrawRay(First.position, Second.position - First.position, Color.red, 0.5f, false);

        Vector3 handsVector = (Second.position - First.position).normalized;
        Quaternion rotationQuaternion = Quaternion.Euler(0, 90, 0);

        Vector3 handsUpVector = rotationQuaternion * handsVector;

        //Debug.DrawRay(middlePoint, handsUpVector.normalized, Color.blue, 0.5f, false);

        MeshCutter.transform.position = middlePoint + Vector3.up * 0.5f;
        MeshCutter.transform.rotation = Quaternion.LookRotation(handsUpVector);
    }

    void SetSlicePoint(Pose First, Pose Second)
    {
        // [FIX] 오브젝트가 중간에 위치함
        //this.gameObject.transform.position = originPose.position;
        
        // Update cut position
        float negativeRatio, positiveRatio;
    }

    void sliceObjcts()
    {
        //Debug.Log("cut!");
        Vector3 targetPosition = new Vector3(originPose.position.x, 0.0f, originPose.position.z);
        Debug.DrawLine(MeshCutter.transform.position, targetPosition, Color.yellow);
        MeshCutter.transform.position =
            Vector3.MoveTowards(MeshCutter.transform.position, targetPosition, Time.deltaTime * 10.0f);

        // Cut이 완료된다면
        // if (MeshCutter.transform.position.y <= (middlePoint.y + Vector3.down.y * 0.5f))
        // {
        //     Debug.Log("여기가 먼저??");
        //     isSetPosition = false;
        //     activeCut = false;
        //     MeshCutter.SetActive(false);
        //     grabInteractable.trackPosition = true;
        // }
    }

    public void FinishSlice()
    {
        isSetPosition = false;
        activeCut = false;
        MeshCutter.GetComponent<MeshCutter>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (grabInteractable.interactorsSelecting.Count == 2)
        {

            Initiate();
            
            distance = Vector3.Distance(primaryAttachPose.position, secondaryAttachPose.position);
            if (distance >= maxPullDistance)
            {
                activeCut = true;
            }

            // Mesh Cutter가 Player의 위쪽으로 Set
            if (!activeCut)
            {
                SetMeshCutter(primaryAttachPose, secondaryAttachPose);
                SetSlicePoint(primaryAttachPose, secondaryAttachPose);
            }
            else
            {
                sliceObjcts();
            }

        }
        else
        {
            isSetPosition = false;
            MeshCutter.GetComponent<MeshCutter>().enabled = false;
        }
    }
}
