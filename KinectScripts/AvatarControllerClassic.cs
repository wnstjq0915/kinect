using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/// <summary>
/// AvatarControllerClassic 클래스는 Kinect 데이터를 사용하여 아바타의 뼈대를 매핑하는 기능을 제공합니다.
/// </summary>
public class AvatarControllerClassic : AvatarController
{
    /*
    클래스 설명: AvatarControllerClassic는 Kinect 센서를 통해 아바타의 뼈대를 매핑하는 기능을 제공합니다.
        이 클래스는 AvatarController 클래스를 상속받으며,
        다양한 변환(Transform) 변수를 통해 아바타의 신체 부위에 대한 참조를 저장합니다.
    변수 설명: 각 변환 변수는 아바타의 특정 부위에 매핑됩니다.
        예를 들어, HipCenter는 엉덩이 중앙을 나타냅니다.
    MapBones 메소드: 이 메소드는 아바타의 뼈대를 매핑하는 주요 기능을 수행합니다.
        각 신체 부위의 변환을 배열에 할당하며, 오프셋 노드가 없는 경우 새로 생성합니다.
        새로 생성된 오프셋 노드는 아바타의 부모로 설정되고, 아바타의 로컬 위치와 회전이 초기화됩니다.
    */

    // Kinect 데이터에 매핑될 아바타의 뼈대 변환을 정의합니다.
    public Transform HipCenter; // 엉덩이 중앙
    public Transform Spine; // 척추
    public Transform Neck; // 목
    public Transform Head; // 머리

    public Transform LeftClavicle; // 왼쪽 쇄골
    public Transform LeftUpperArm; // 왼쪽 상완
    public Transform LeftElbow; // 왼쪽 팔꿈치
    public Transform LeftHand; // 왼쪽 손
    private Transform LeftFingers = null; // 왼쪽 손가락 (옵션)

    public Transform RightClavicle; // 오른쪽 쇄골
    public Transform RightUpperArm; // 오른쪽 상완
    public Transform RightElbow; // 오른쪽 팔꿈치
    public Transform RightHand; // 오른쪽 손
    private Transform RightFingers = null; // 오른쪽 손가락 (옵션)

    public Transform LeftThigh; // 왼쪽 허벅지
    public Transform LeftKnee; // 왼쪽 무릎
    public Transform LeftFoot; // 왼쪽 발
    private Transform LeftToes = null; // 왼쪽 발가락 (옵션)

    public Transform RightThigh; // 오른쪽 허벅지
    public Transform RightKnee; // 오른쪽 무릎
    public Transform RightFoot; // 오른쪽 발
    private Transform RightToes = null; // 오른쪽 발가락 (옵션)

    public Transform BodyRoot; // 아바타의 본체 루트
    public GameObject OffsetNode; // 오프셋 노드

    /// <summary>
    /// 뼈대를 아바타 모델에 매핑합니다.
    /// </summary>
    protected override void MapBones()
    {
        // 아바타 뼈대에 대한 변환을 배열에 매핑합니다.
        bones[0] = HipCenter; // 엉덩이 중앙
        bones[1] = Spine; // 척추
        bones[2] = Neck; // 목
        bones[3] = Head; // 머리

        bones[4] = LeftClavicle; // 왼쪽 쇄골
        bones[5] = LeftUpperArm; // 왼쪽 상완
        bones[6] = LeftElbow; // 왼쪽 팔꿈치
        bones[7] = LeftHand; // 왼쪽 손
        bones[8] = LeftFingers; // 왼쪽 손가락 (옵션)

        bones[9] = RightClavicle; // 오른쪽 쇄골
        bones[10] = RightUpperArm; // 오른쪽 상완
        bones[11] = RightElbow; // 오른쪽 팔꿈치
        bones[12] = RightHand; // 오른쪽 손
        bones[13] = RightFingers; // 오른쪽 손가락 (옵션)

        bones[14] = LeftThigh; // 왼쪽 허벅지
        bones[15] = LeftKnee; // 왼쪽 무릎
        bones[16] = LeftFoot; // 왼쪽 발
        bones[17] = LeftToes; // 왼쪽 발가락 (옵션)

        bones[18] = RightThigh; // 오른쪽 허벅지
        bones[19] = RightKnee; // 오른쪽 무릎
        bones[20] = RightFoot; // 오른쪽 발
        bones[21] = RightToes; // 오른쪽 발가락 (옵션)

        // 본체 루트와 오프셋 노드를 설정합니다.
        bodyRoot = BodyRoot;
        offsetNode = OffsetNode;

        // 오프셋 노드가 null인 경우 새로 생성합니다.
        if (offsetNode == null)
        {
            offsetNode = new GameObject(name + "Ctrl")
            {
                layer = transform.gameObject.layer,
                tag = transform.gameObject.tag
            };
            offsetNode.transform.position = transform.position;
            offsetNode.transform.rotation = transform.rotation;
            offsetNode.transform.parent = transform.parent;

            // 아바타를 오프셋 노드의 자식으로 설정합니다.
            transform.parent = offsetNode.transform;
            transform.localPosition = Vector3.zero; // 위치를 초기화
            transform.localRotation = Quaternion.identity; // 회전을 초기화
        }
    }
}