using System;
using System.Collections;
using System.Collections.Generic;
using Deform;
using Unity.XR.CoreUtils;
using UnityEditor.Animations;
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
    private GameObject deformable;
    private SquashAndStretchDeformer deformer;
    
    private Pose originPose;
    private Vector3 middlePoint;
    private Vector3 movementMiddle;
    private float distance;
    private float maxDefromation = 1.5f;


    // Start is called before the first frame update
    void Start()
    {
        MeshCutter = GameObject.FindWithTag("Cutter");
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Deformable 특성이 있는 경우
        if (TryGetComponent(out Deformable deform))
        {
            deformable = transform.GetChild(0).gameObject;
            deformer = deformable.GetComponent<SquashAndStretchDeformer>();
        }
        
        maxPullDistance = 1.3f;
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
            
            // 두 손으로 Grab한 위치의 중간 지점
            middlePoint = (primaryAttachPose.position + secondaryAttachPose.position) / 2;
            movementMiddle = middlePoint;

            // 오브젝트가 중간에 위치함
            grabInteractable.trackPosition = false;
        }
    }
    
    void SetMeshCutter(Pose First, Pose Second)
    {
        //Debug.DrawRay(First.position, Second.position - First.position, Color.red, 0.5f, false);

        Vector3 handsVector = (Second.position - First.position).normalized;
        Quaternion rotationQuaternion = Quaternion.Euler(0, 90, 0);

        Vector3 handsUpVector = rotationQuaternion * handsVector;

        //Debug.DrawRay(middlePoint, handsUpVector.normalized, Color.blue, 0.5f, false);

        MeshCutter.transform.position = middlePoint + Vector3.up * 0.5f;
        MeshCutter.transform.rotation = Quaternion.LookRotation(handsUpVector);
    }

    void SetObjectMiddle()
    {
        middlePoint = (primaryAttachPose.position + secondaryAttachPose.position) / 2;
        
        // [FIX] 오브젝트가 중간에 위치함
        Debug.Log(middlePoint - movementMiddle);
        this.gameObject.transform.position += (middlePoint - movementMiddle);
    }
    
    void SetSlicePoint(Pose First, Pose Second)
    {
        // [FIX] 오브젝트가 중간에 위치함
        this.gameObject.transform.position += movementMiddle - middlePoint;
        
        // [HAVE TO] Update cut position
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
        MeshCutter.transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (grabInteractable.interactorsSelecting.Count == 2)
        {
            Initiate();
            SetObjectMiddle();
            distance = Vector3.Distance(primaryAttachPose.position, secondaryAttachPose.position);
            if (distance >= maxPullDistance)
            {
                activeCut = true;
            }

            // Mesh Cutter가 Player의 위쪽으로 Set
            if (!activeCut)
            {
                //this.GetComponent<MeshRenderer>().enabled = false;
                if (!(deformable == null))
                {
                    Vector3 handsVector = (secondaryAttachPose.position - primaryAttachPose.position).normalized;
                    deformable.transform.rotation = Quaternion.LookRotation(handsVector);
                    float weight = Mathf.Clamp(distance, 0, maxPullDistance) / maxPullDistance;
                    deformer.Factor = distance * weight;
                }
                
                SetMeshCutter(primaryAttachPose, secondaryAttachPose);
                //SetSlicePoint(primaryAttachPose, secondaryAttachPose);
            }
            else
            {
                this.GetComponent<MeshRenderer>().enabled = true;
                sliceObjcts();
            }
            
            movementMiddle = middlePoint;
        }
        else
        {
            isSetPosition = false;
            MeshCutter.GetComponent<MeshCutter>().enabled = false;
        }
    }
}
