using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/**
 * <summary>
 * AvatarController 클래스는 Kinect를 사용하여 아바타의 움직임과 회전을 제어합니다.
 * 사용자의 스켈레톤 데이터에 기반하여 아바타의 각 뼈를 업데이트합니다.
 * </summary>
 */
[RequireComponent(typeof(Animator))] // Animator 컴포넌트가 필요함을 나타내는 특성
public class AvatarController : MonoBehaviour
{
    /*
    주요 구성 요소
        변수:
            mirroredMovement: 아바타의 움직임을 반전할지 여부를 설정하는 불리언 변수입니다.
            verticalMovement: 아바타의 수직 이동을 허용할지 여부를 결정하는 변수입니다.
            moveRate: 아바타의 이동 속도를 설정하는 변수입니다.
            smoothFactor: 아바타의 움직임을 부드럽게 하기 위한 보간 계수입니다.
            bones: 아바타의 뼈를 저장하는 배열입니다.
            initialRotations: Kinect 추적 시작 시 각 뼈의 초기 회전을 저장하는 배열입니다.

        메소드:
            Awake(): 아바타의 초기 설정을 수행하고, 뼈를 매핑한 후 초기 회전을 가져옵니다.
            UpdateAvatar(uint UserID): 매 프레임마다 아바타를 업데이트합니다.
                사용자의 스켈레톤 데이터에 따라 아바타의 위치와 회전을 조정합니다.
            ResetToInitialPosition(): 아바타의 뼈를 초기 위치와 회전으로 리셋합니다.
            SuccessfulCalibration(uint userId):
                사용자가 성공적으로 보정되었을 때 호출되어 아바타의 위치를 리셋하고 오프셋을 재보정합니다.
            TransformBone(...): Kinect에서 추적된 관절 회전을 아바타의 뼈에 적용합니다.
            TransformSpecialBone(...): 특수 관절에 대한 회전을 적용합니다.
            MoveAvatar(uint UserID): 아바타를 3D 공간에서 이동시키며,
                사용자의 척추 위치를 기반으로 루트를 이동합니다.
            MapBones(): 아바타의 뼈를 Kinect 관절에 매핑하는 기능을 수행합니다.
            GetInitialRotations(): 뼈의 초기 회전을 캡처합니다.
            Kinect2AvatarRot(...): Kinect 관절 회전을 아바타 관절 회전으로 변환합니다.
            Kinect2AvatarPos(...): Kinect 위치를 아바타 스켈레톤 위치로 변환합니다.
    기능 요약
        아바타 제어: Kinect 센서에서 얻은 스켈레톤 데이터를 기반으로 아바타의 뼈를 업데이트하여 실제 사용자의 움직임을 아바타에 반영합니다.
        초기화 및 리셋: 아바타의 초기 회전을 저장하고 필요에 따라 초기 상태로 리셋하는 기능을 제공합니다.
        보정 처리: 사용자가 성공적으로 보정되었을 때 아바타의 위치를 조정하고, 반전된 움직임을 처리할 수 있습니다.
        부드러운 이동: 아바타의 움직임을 부드럽게 처리하기 위한 보간 기능을 제공합니다.

    이 클래스는 Kinect를 활용한 상호작용형 애플리케이션에서 아바타 애니메이션을 구현하는 데 핵심적인 역할을 하며,
    사용자의 스켈레톤 데이터를 효과적으로 처리하여 자연스러운 아바타 움직임을 제공합니다.
    */

    // 캐릭터의 행동을 거울에 비친 것처럼 반전할지 여부
    public bool mirroredMovement = false;

    // 아바타의 수직 이동 허용 여부
    public bool verticalMovement = false;

    // 아바타가 씬을 이동하는 속도 비율
    protected int moveRate = 1; // 1 = 기본 속도

    // 스무딩을 위한 보간 계수
    public float smoothFactor = 5f;

    // 오프셋 노드를 사용하여 사용자의 좌표에 상대적으로 위치를 조정할지 여부
    public bool offsetRelativeToSensor = false;

    // 본체 루트 노드
    protected Transform bodyRoot;

    // 아바타가 회전할 수 있도록 하는 변수
    protected GameObject offsetNode;

    // 모든 뼈를 저장하는 변수 (초기 회전 크기만큼 초기화됨)
    protected Transform[] bones;

    // Kinect 추적 시작 시 뼈의 초기 회전
    protected Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;

    // 변환의 초기 위치와 회전
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    // 캐릭터 위치 보정 오프셋 변수
    protected bool offsetCalibrated = false;
    protected float xOffset, yOffset, zOffset;

    // KinectManager의 비공식 인스턴스
    protected KinectManager kinectManager;

    // Transform 캐싱을 통해 성능 향상
    private Transform _transformCache;
    public new Transform transform
    {
        get
        {
            if (!_transformCache)
                _transformCache = base.transform;

            return _transformCache;
        }
    }

    // Awake() 메소드: 초기 설정
    /// <summary>
    /// Awake 메소드는 아바타의 초기 설정을 수행합니다.
    /// 뼈를 매핑하고 초기 회전을 가져옵니다.
    /// </summary>
    public void Awake()
    {
        // 이중 시작 방지
        if (bones != null)
            return;

        // 뼈 배열 초기화
        bones = new Transform[22];

        // 뼈의 초기 회전과 방향 초기화
        initialRotations = new Quaternion[bones.Length];
        initialLocalRotations = new Quaternion[bones.Length];

        // 뼈를 Kinect가 추적하는 포인트에 매핑
        MapBones();

        // 초기 뼈 회전 가져오기
        GetInitialRotations();
    }

    // 매 프레임마다 아바타 업데이트
    /// <summary>
    /// UpdateAvatar 메소드는 매 프레임마다 아바타를 업데이트합니다.
    /// 사용자의 스켈레톤 데이터에 따라 아바타의 위치와 회전을 조정합니다.
    /// </summary>
    /// <param name="UserID">업데이트할 사용자 ID</param>
    public void UpdateAvatar(uint UserID)
    {
        if (!transform.gameObject.activeInHierarchy)
            return; // 아바타가 비활성화된 경우 종료

        // KinectManager 인스턴스 가져오기
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        // 아바타를 Kinect 위치로 이동
        MoveAvatar(UserID);

        // 각 뼈에 대해 회전 적용
        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue; // 뼈가 존재하지 않으면 건너뜀

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // 특수 뼈 (쇄골 등)
                List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    // 특수 뼈에 대한 변환 처리
                    //Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    //TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }
    }

    // 뼈를 초기 위치와 회전으로 리셋
    /// <summary>
    /// ResetToInitialPosition 메소드는 아바타의 뼈를 초기 위치와 회전으로 리셋합니다.
    /// </summary>
    public void ResetToInitialPosition()
    {
        if (bones == null)
            return;

        // 오프셋 노드의 회전 초기화
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // 각 정의된 뼈를 초기 위치로 리셋
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].rotation = initialRotations[i];
            }
        }

        // 본체 루트 초기화
        if (bodyRoot != null)
        {
            bodyRoot.localPosition = Vector3.zero;
            bodyRoot.localRotation = Quaternion.identity;
        }

        // 초기 위치와 회전 복원
        if (offsetNode != null)
        {
            offsetNode.transform.position = initialPosition;
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    // 사용자가 성공적으로 보정되었을 때 호출
    /// <summary>
    /// SuccessfulCalibration 메소드는 사용자가 성공적으로 보정되었을 때 호출됩니다.
    /// 아바타의 위치를 리셋하고 오프셋을 재보정합니다.
    /// </summary>
    /// <param name="userId">성공적으로 보정된 사용자 ID</param>
    public void SuccessfulCalibration(uint userId)
    {
        // 모델 위치 리셋
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }

        // 위치 오프셋 재보정
        offsetCalibrated = false;
    }

    // Kinect에서 추적된 회전을 관절에 적용
    /// <summary>
    /// TransformBone 메소드는 Kinect에서 추적된 관절 회전을 아바타의 뼈에 적용합니다.
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="joint">적용할 Kinect 관절</param>
    /// <param name="boneIndex">적용할 뼈 인덱스</param>
    /// <param name="flip">반전 여부</param>
    protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return; // 뼈가 없거나 KinectManager가 없으면 종료

        int iJoint = (int)joint;
        if (iJoint < 0)
            return; // 잘못된 관절 인덱스 반환

        // Kinect 관절의 회전 가져오기
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        if (jointRotation == Quaternion.identity)
            return; // 유효하지 않은 회전이면 종료

        // 새로운 회전으로 부드럽게 전환
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation; // 스무딩 없이 회전 적용
    }

    // 특별한 관절에 대한 회전 적용
    /// <summary>
    /// TransformSpecialBone 메소드는 특수 관절에 대한 회전을 적용합니다.
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="joint">적용할 Kinect 관절</param>
    /// <param name="jointParent">부모 관절</param>
    /// <param name="boneIndex">적용할 뼈 인덱스</param>
    /// <param name="baseDir">기본 방향</param>
    /// <param name="flip">반전 여부</param>
    protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return; // 뼈가 없거나 KinectManager가 없으면 종료

        if (!kinectManager.IsJointTracked(userId, (int)joint) ||
           !kinectManager.IsJointTracked(userId, (int)jointParent))
        {
            return; // 관절이 추적되지 않으면 종료
        }

        // 두 관절 간의 방향 가져오기
        Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        if (jointRotation != Quaternion.identity)
        {
            // 새로운 회전으로 부드럽게 전환
            Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

            if (smoothFactor != 0f)
                boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
            else
                boneTransform.rotation = newRotation; // 스무딩 없이 회전 적용
        }
    }

    // 아바타를 3D 공간에서 이동 - 척추의 추적 위치를 가져와 루트에 적용
    /// <summary>
    /// MoveAvatar 메소드는 아바타를 3D 공간에서 이동합니다.
    /// 사용자의 척추 위치를 기반으로 아바타의 루트를 이동합니다.
    /// </summary>
    /// <param name="UserID">이동할 사용자 ID</param>
    protected void MoveAvatar(uint UserID)
    {
        if (bodyRoot == null || kinectManager == null)
            return; // 본체 루트나 KinectManager가 없으면 종료
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
            return; // 엉덩이 센서가 추적되지 않으면 종료

        // 몸체의 위치 가져오기
        Vector3 trans = kinectManager.GetUserPosition(UserID);

        // 아바타를 처음 이동시키는 경우 오프셋 설정
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate; // 반전 여부에 따른 X 오프셋
            yOffset = trans.y * moveRate; // Y 오프셋
            zOffset = -trans.z * moveRate; // Z 오프셋

            if (offsetRelativeToSensor)
            {
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos; // 오프셋 노드 위치 설정
                }
                else
                {
                    transform.position = offsetPos; // 아바타 위치 설정
                }
            }
        }

        // 새로운 위치로 부드럽게 전환
        Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        if (smoothFactor != 0f)
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        else
            bodyRoot.localPosition = targetPos; // 스무딩 없이 위치 적용
    }

    // 매핑할 뼈가 정의되었는지 확인하고 모델에 해당 뼈를 매핑
    /// <summary>
    /// MapBones 메소드는 아바타의 뼈를 Kinect 관절에 매핑합니다.
    /// </summary>
    protected virtual void MapBones()
    {
        // 오프셋 노드를 모델 변환의 부모로 설정
        offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;

        transform.parent = offsetNode.transform; // 모델 변환의 부모를 오프셋 노드로 설정
        transform.localPosition = Vector3.zero; // 로컬 위치 초기화
        transform.localRotation = Quaternion.identity; // 로컬 회전 초기화

        // 본체 루트로서 모델 변환 사용
        bodyRoot = transform;

        // Animator 컴포넌트에서 뼈 변환 가져오기
        var animatorComponent = GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue; // 매핑되지 않은 뼈는 건너뜀

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]); // 뼈 변환 설정
        }
    }

    // 뼈의 초기 회전 캡처
    /// <summary>
    /// GetInitialRotations 메소드는 뼈의 초기 회전을 캡처합니다.
    /// </summary>
    protected void GetInitialRotations()
    {
        // 초기 회전 저장
        if (offsetNode != null)
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity; // 오프셋 노드 회전 초기화
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            transform.rotation = Quaternion.identity; // 아바타 회전 초기화
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation; // 초기 회전 캡처
                initialLocalRotations[i] = bones[i].localRotation; // 초기 로컬 회전 캡처
            }
        }

        // 초기 회전 복원
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation; // 오프셋 노드 회전 복원
        }
        else
        {
            transform.rotation = initialRotation; // 아바타 회전 복원
        }
    }

    // Kinect 관절 회전을 아바타 관절 회전으로 변환
    /// <summary>
    /// Kinect2AvatarRot 메소드는 Kinect 관절 회전을 아바타 관절 회전으로 변환합니다.
    /// </summary>
    /// <param name="jointRotation">Kinect 관절 회전</param>
    /// <param name="boneIndex">변환할 뼈 인덱스</param>
    /// <returns>변환된 아바타 관절 회전</returns>
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
    {
        // 새 회전 적용
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];

        // 오프셋 노드가 지정된 경우 회전 결합
        if (offsetNode != null)
        {
            // 오프셋의 오일러를 더하여 총 회전 획득
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            // 새로운 회전 가져오기
            newRotation = Quaternion.Euler(totalRotation);
        }

        return newRotation; // 변환된 회전 반환
    }

    // Kinect 위치를 아바타 스켈레톤 위치로 변환
    /// <summary>
    /// Kinect2AvatarPos 메소드는 Kinect 위치를 아바타 스켈레톤 위치로 변환합니다.
    /// </summary>
    /// <param name="jointPosition">Kinect 관절 위치</param>
    /// <param name="bMoveVertically">수직 이동 여부</param>
    /// <returns>변환된 아바타 위치</returns>
    protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
    {
        float xPos;
        float yPos;
        float zPos;

        // 이동이 반전되면 X 반전
        if (!mirroredMovement)
            xPos = jointPosition.x * moveRate - xOffset; // X 오프셋 적용
        else
            xPos = -jointPosition.x * moveRate - xOffset; // X 오프셋 적용 (반전)

        yPos = jointPosition.y * moveRate - yOffset; // Y 오프셋 적용
        zPos = -jointPosition.z * moveRate - zOffset; // Z 오프셋 적용

        // 수직 이동을 추적하는 경우 Y 값을 업데이트
        Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

        return avatarJointPos; // 변환된 위치 반환
    }

    // 뼈 매핑을 최적화하기 위한 딕셔너리
    // Kinect 관절과 Mecanim 뼈 매핑
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Hips},
        {1, HumanBodyBones.Spine},
        {2, HumanBodyBones.Neck},
        {3, HumanBodyBones.Head},

        {4, HumanBodyBones.LeftShoulder},
        {5, HumanBodyBones.LeftUpperArm},
        {6, HumanBodyBones.LeftLowerArm},
        {7, HumanBodyBones.LeftHand},
        {8, HumanBodyBones.LeftIndexProximal},

        {9, HumanBodyBones.RightShoulder},
        {10, HumanBodyBones.RightUpperArm},
        {11, HumanBodyBones.RightLowerArm},
        {12, HumanBodyBones.RightHand},
        {13, HumanBodyBones.RightIndexProximal},

        {14, HumanBodyBones.LeftUpperLeg},
        {15, HumanBodyBones.LeftLowerLeg},
        {16, HumanBodyBones.LeftFoot},
        {17, HumanBodyBones.LeftToes},

        {18, HumanBodyBones.RightUpperLeg},
        {19, HumanBodyBones.RightLowerLeg},
        {20, HumanBodyBones.RightFoot},
        {21, HumanBodyBones.RightToes},
    };

    // 뼈 인덱스를 Kinect 관절에 매핑
    protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {3, KinectWrapper.NuiSkeletonPositionIndex.Head},

        {5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {6, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {7, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        {8, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},

        {10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {11, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {12, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        {13, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
    };

    // 뼈 인덱스를 특수 관절에 매핑
    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };

    // 반전된 뼈 인덱스 매핑
    protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {3, KinectWrapper.NuiSkeletonPositionIndex.Head},

        {5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {6, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {7, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        {8, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {11, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {12, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        {13, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootRight},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
    };

    // 특수 뼈 인덱스를 반전된 관절에 매핑
    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };
}