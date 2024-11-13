using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/**
 * <summary>
 * KinectManager 클래스는 Kinect v1 센서를 이용하여 사용자 스켈레톤 데이터를 관리하고,
 * 이를 기반으로 아바타의 움직임 및 제스처를 처리하는 기능을 제공합니다.
 * </summary>
 */
public class KinectManager : MonoBehaviour
{
    /*
    KinectManager 클래스는 Kinect v1 센서를 이용하여 사용자 스켈레톤 데이터를 관리하고,
    이를 기반으로 아바타의 움직임 및 제스처를 처리하는 기능을 제공하는 클래스입니다.
    이 클래스의 주요 기능과 요소를 요약하면 다음과 같습니다.

    주요 기능
        Kinect 초기화:
            Awake() 메소드에서 Kinect 센서를 초기화하고, 스켈레톤 트래킹을 설정합니다.
            깊이 및 색상 스트림을 활성화하고, Kinect의 기울기 각도를 설정합니다.

        사용자 감지 및 트래킹:
            사용자의 스켈레톤 데이터를 실시간으로 감지하고 처리합니다.
            사용자가 감지되면 해당 사용자의 ID와 정보를 관리합니다.

        스켈레톤 데이터 처리:
            Update() 메소드에서 Kinect 사용자로부터 깊이 및 색상 데이터를 가져와 업데이트합니다.
            스켈레톤 데이터를 처리하고 각 관절의 위치와 회전을 계산하여 아바타에 적용합니다.

        제스처 인식:
            특정 제스처를 감지하고, 이를 기반으로 다양한 동작을 실행합니다.
            사용자의 제스처 진행률을 추적하고, 완료된 제스처에 대한 처리를 수행합니다.
    
        사용자 맵 계산:
            사용자가 감지된 깊이 데이터를 기반으로 사용자 맵과 색상 맵을 계산하여 GUI에 표시합니다.

        상태 관리:
            아바타의 상태를 관리하며, 아바타의 위치, 회전, 및 스켈레톤을 업데이트합니다.
            사용자와의 상호작용을 위한 다양한 설정을 제공합니다.

    클래스 구성 요소
        변수:
            다양한 설정을 위한 공개 및 비공개 변수들이 정의되어 있으며,
            이를 통해 Kinect 사용자의 수, 스켈레톤 데이터 처리 방식, GUI 표시 여부 등을 조정할 수 있습니다.

        메소드:
            Kinect 초기화, 스켈레톤 데이터 처리, 제스처 감지 및 업데이트하는 여러 메소드들이 포함되어 있습니다.
            주요 메소드는 Awake(), Update(), ProcessSkeleton(), UpdateUserMap(), DetectGesture() 등입니다.

        싱글톤 패턴:
            KinectManager 클래스는 싱글톤 패턴으로 구현되어 있어, 클래스의 인스턴스가 하나만 존재하도록 보장합니다.
            이를 통해 전역적으로 KinectManager에 접근할 수 있습니다.

    이 클래스는 Kinect 센서를 통해 사용자의 움직임을 추적하고,
    이를 기반으로 아바타를 제어하며,
    제스처 인식을 통해 다양한 상호작용을 가능하게 하는 핵심 역할을 합니다.
    이 클래스의 구조와 메소드를 이해함으로써 Kinect 기반 게임이나 애플리케이션 개발에 필요한 다양한 기능을 구현할 수 있습니다.
    */


    // 스무딩 옵션을 정의하는 열거형
    public enum Smoothing : int { None, Default, Medium, Aggressive }

    // 두 사용자가 있는지 여부를 설정하는 공개 변수
    public bool TwoUsers = false;

    // 사용자 맵을 계산할 것인지 여부를 설정하는 공개 변수
    public bool ComputeUserMap = false;

    // 색상 맵을 계산할 것인지 여부를 설정하는 공개 변수
    public bool ComputeColorMap = false;

    // 사용자 맵을 GUI에 표시할 것인지 여부를 설정하는 공개 변수
    public bool DisplayUserMap = false;

    // 색상 맵을 GUI에 표시할 것인지 여부를 설정하는 공개 변수
    public bool DisplayColorMap = false;

    // 사용자 맵에서 스켈레톤 선을 표시할 것인지 여부를 설정하는 공개 변수
    public bool DisplaySkeletonLines = false;

    // 이미지의 너비를 설정하는 공개 변수 (카메라 너비의 비율로)
    public float DisplayMapsWidthPercent = 20f;

    // 센서의 높이를 설정하는 공개 변수 (미터 단위)
    public float SensorHeight = 1.0f;

    // 센서의 기울기 각도를 설정하는 공개 변수 (도 단위)
    public int SensorAngle = 0;

    // 사용자 스켈레톤 데이터를 처리하기 위한 최소 거리
    public float MinUserDistance = 1.0f;

    // 사용자 스켈레톤 데이터의 최대 거리 (0은 제한 없음)
    public float MaxUserDistance = 0f;

    // 가장 가까운 사용자만 감지할지 여부를 설정하는 공개 변수
    public bool DetectClosestUser = true;

    // 추정된 관절을 무시할 것인지 여부를 설정하는 공개 변수
    public bool IgnoreInferredJoints = true;

    // 스무딩 매개변수 선택
    public Smoothing smoothing = Smoothing.Default;

    // 추가 필터 사용 여부를 설정하는 공개 변수
    public bool UseBoneOrientationsFilter = false;
    public bool UseClippedLegsFilter = false;
    public bool UseBoneOrientationsConstraint = true;
    public bool UseSelfIntersectionConstraint = false;

    // 각 플레이어의 아바타를 제어할 GameObject 리스트
    public List<GameObject> Player1Avatars;
    public List<GameObject> Player2Avatars;

    // 각 플레이어의 보정 포즈 설정
    public KinectGestures.Gestures Player1CalibrationPose;
    public KinectGestures.Gestures Player2CalibrationPose;

    // 각 플레이어의 감지할 제스처 리스트
    public List<KinectGestures.Gestures> Player1Gestures;
    public List<KinectGestures.Gestures> Player2Gestures;

    // 제스처 감지 간 최소 시간
    public float MinTimeBetweenGestures = 0.7f;

    // 제스처 리스너 리스트
    public List<MonoBehaviour> GestureListeners;

    // GUI 메시지를 표시할 GUIText
    public GUIText CalibrationText;

    // 플레이어 1 및 2의 손 커서를 표시할 GUI Texture
    public GameObject HandCursor1;
    public GameObject HandCursor2;

    // 마우스 커서와 클릭 제스처로 마우스 커서를 제어할지 여부
    public bool ControlMouseCursor = false;

    // 제스처 디버그 메시지를 표시할 GUIText
    public GUIText GesturesDebugText;

    // Kinect 초기화 여부를 추적하는 비공개 변수
    private bool KinectInitialized = false;

    // 보정된 플레이어 추적 여부를 저장하는 비공개 변수
    private bool Player1Calibrated = false;
    private bool Player2Calibrated = false;

    // 모든 플레이어가 보정되었는지 여부를 저장하는 비공개 변수
    private bool AllPlayersCalibrated = false;

    // Player 1 및 Player 2의 ID를 저장하는 변수
    private uint Player1ID;
    private uint Player2ID;

    // Player 1 및 Player 2의 인덱스 변수
    private int Player1Index;
    private int Player2Index;

    // 아바타 컨트롤러 리스트
    private List<AvatarController> Player1Controllers;
    private List<AvatarController> Player2Controllers;

    // 사용자 맵 관련 변수
    private Texture2D usersLblTex;
    private Color32[] usersMapColors;
    private ushort[] usersPrevState;
    private Rect usersMapRect;
    private int usersMapSize;

    // 색상 맵 관련 변수
    private Texture2D usersClrTex;
    private Rect usersClrRect;

    // 사용자 깊이 맵
    private ushort[] usersDepthMap;
    private float[] usersHistogramMap;

    // 모든 사용자 리스트
    private List<uint> allUsers;

    // Kinect의 이미지 스트림 핸들
    private IntPtr colorStreamHandle;
    private IntPtr depthStreamHandle;

    // 색상 이미지 데이터
    private Color32[] colorImage;
    private byte[] usersColorMap;

    // 스켈레톤 관련 구조체
    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    private KinectWrapper.NuiTransformSmoothParameters smoothParameters;
    private int player1Index, player2Index;

    // 플레이어의 위치 및 방향
    private Vector3 player1Pos, player2Pos;
    private Matrix4x4 player1Ori, player2Ori;
    private bool[] player1JointsTracked, player2JointsTracked;
    private bool[] player1PrevTracked, player2PrevTracked;
    private Vector3[] player1JointsPos, player2JointsPos;
    private Matrix4x4[] player1JointsOri, player2JointsOri;
    private KinectWrapper.NuiSkeletonBoneOrientation[] jointOrientations;

    // 보정 제스처 데이터
    private KinectGestures.GestureData player1CalibrationData;
    private KinectGestures.GestureData player2CalibrationData;

    // 제스처 데이터 리스트
    private List<KinectGestures.GestureData> player1Gestures = new List<KinectGestures.GestureData>();
    private List<KinectGestures.GestureData> player2Gestures = new List<KinectGestures.GestureData>();

    // 제스처 추적 시작 시간
    private float[] gestureTrackingAtTime;

    // 제스처 리스너 리스트
    public List<KinectGestures.GestureListenerInterface> gestureListeners;

    // Kinect 공간에서 월드 공간으로 변환하는 매트릭스
    private Matrix4x4 kinectToWorld, flipMatrix;
    private static KinectManager instance;

    // 필터링과 관련된 타이머
    private float lastNuiTime;

    // 필터 변수
    private TrackingStateFilter[] trackingStateFilter;
    private BoneOrientationsFilter[] boneOrientationFilter;
    private ClippedLegsFilter[] clippedLegsFilter;
    private BoneOrientationsConstraint boneConstraintsFilter;
    private SelfIntersectionConstraint selfIntersectionConstraint;

    // 싱글톤 인스턴스를 반환
    public static KinectManager Instance
    {
        get
        {
            return instance;
        }
    }

    // Kinect가 초기화되었는지 확인
    public static bool IsKinectInitialized()
    {
        return instance != null ? instance.KinectInitialized : false;
    }

    // Kinect가 초기화되었는지 확인
    public bool IsInitialized()
    {
        return KinectInitialized;
    }

    // 내부적으로 AvatarController에 의해 사용되는 함수
    public static bool IsCalibrationNeeded()
    {
        return false;
    }

    // 원시 깊이/사용자 데이터를 반환 (ComputeUserMap이 true일 때)
    /// <summary>
    /// GetRawDepthMap 메소드는 원시 깊이/사용자 데이터를 반환합니다.
    /// ComputeUserMap이 true일 때만 유효합니다.
    /// </summary>
    /// <returns>원시 깊이 데이터 배열</returns>
    public ushort[] GetRawDepthMap()
    {
        return usersDepthMap;
    }

    // 특정 픽셀에 대한 깊이 데이터를 반환 (ComputeUserMap이 true일 때)
    /// <summary>
    /// GetDepthForPixel 메소드는 특정 픽셀에 대한 깊이 데이터를 반환합니다.
    /// ComputeUserMap이 true일 때만 유효합니다.
    /// </summary>
    /// <param name="x">픽셀의 x좌표</param>
    /// <param name="y">픽셀의 y좌표</param>
    /// <returns>주어진 픽셀의 깊이 값</returns>
    public ushort GetDepthForPixel(int x, int y)
    {
        int index = y * KinectWrapper.Constants.DepthImageWidth + x;

        if (index >= 0 && index < usersDepthMap.Length)
            return usersDepthMap[index];
        else
            return 0;
    }

    // 3D 관절 위치에 대한 깊이 맵 위치를 반환
    public Vector2 GetDepthMapPosForJointPos(Vector3 posJoint)
    {
        Vector3 vDepthPos = KinectWrapper.MapSkeletonPointToDepthPoint(posJoint);
        Vector2 vMapPos = new Vector2(vDepthPos.x, vDepthPos.y);

        return vMapPos;
    }

    // 깊이 2D 위치에 대한 색상 맵 위치를 반환
    public Vector2 GetColorMapPosForDepthPos(Vector2 posDepth)
    {
        int cx, cy;

        // 임시 구조체 생성
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        // 깊이 픽셀로부터 색상 픽셀 좌표를 가져옴
        KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
            KinectWrapper.Constants.ColorImageResolution,
            KinectWrapper.Constants.DepthImageResolution,
            ref pcViewArea,
            (int)posDepth.x, (int)posDepth.y, GetDepthForPixel((int)posDepth.x, (int)posDepth.y),
            out cx, out cy);

        return new Vector2(cx, cy);
    }

    // 사용자 레이블 텍스처 반환 (ComputeUserMap이 true일 때)
    public Texture2D GetUsersLblTex()
    {
        return usersLblTex;
    }

    // 사용자 색상 텍스처 반환 (ComputeColorMap이 true일 때)
    public Texture2D GetUsersClrTex()
    {
        return usersClrTex;
    }

    // 최소한 하나의 사용자가 감지되었는지 확인
    /// <summary>
    /// IsUserDetected 메소드는 최소한 하나의 사용자가 감지되었는지 확인합니다.
    /// </summary>
    /// <returns>사용자가 감지되었으면 true, 그렇지 않으면 false</returns>
    public bool IsUserDetected()
    {
        return KinectInitialized && (allUsers.Count > 0);
    }

    // Player1의 UserID를 반환 (감지되지 않으면 0)
    /// <summary>
    /// GetPlayer1ID 메소드는 Player 1의 UserID를 반환합니다.
    /// </summary>
    /// <returns>Player 1의 UserID 또는 0 (감지되지 않으면)</returns>
    public uint GetPlayer1ID()
    {
        return Player1ID;
    }

    // Player2의 UserID를 반환 (감지되지 않으면 0)
    /// <summary>
    /// GetPlayer2ID 메소드는 Player 2의 UserID를 반환합니다.
    /// </summary>
    /// <returns>Player 2의 UserID 또는 0 (감지되지 않으면)</returns>
    public uint GetPlayer2ID()
    {
        return Player2ID;
    }

    // Player1의 인덱스를 반환 (감지되지 않으면 0)
    public int GetPlayer1Index()
    {
        return Player1Index;
    }

    // Player2의 인덱스를 반환 (감지되지 않으면 0)
    public int GetPlayer2Index()
    {
        return Player2Index;
    }

    // 주어진 UserId의 사용자가 보정되었는지 여부를 반환
    /// <summary>
    /// IsPlayerCalibrated 메소드는 주어진 사용자가 보정되었는지 확인합니다.
    /// </summary>
    /// <param name="UserId">사용자의 ID</param>
    /// <returns>사용자가 보정되었으면 true, 그렇지 않으면 false</returns>
    public bool IsPlayerCalibrated(uint UserId)
    {
        if (UserId == Player1ID)
            return Player1Calibrated;
        else if (UserId == Player2ID)
            return Player2Calibrated;

        return false;
    }

    // 원시 비수정 관절 위치를 반환 (Kinect 센서에서 반환된 대로)
    public Vector3 GetRawSkeletonJointPos(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player1Index].SkeletonPositions[joint] : Vector3.zero;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player2Index].SkeletonPositions[joint] : Vector3.zero;

        return Vector3.zero;
    }

    // 사용자의 위치를 반환 (Kinect 센서에 상대적, 미터 단위)
    /// <summary>
    /// GetUserPosition 메소드는 주어진 사용자의 위치를 반환합니다.
    /// </summary>
    /// <param name="UserId">사용자의 ID</param>
    /// <returns>사용자의 위치(Vector3)</returns>
    public Vector3 GetUserPosition(uint UserId)
    {
        if (UserId == Player1ID)
            return player1Pos;
        else if (UserId == Player2ID)
            return player2Pos;

        return Vector3.zero;
    }

    // 사용자의 회전을 반환 (Kinect 센서에 상대적)
    public Quaternion GetUserOrientation(uint UserId, bool flip)
    {
        if (UserId == Player1ID && player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
            return ConvertMatrixToQuat(player1Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
        else if (UserId == Player2ID && player2JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
            return ConvertMatrixToQuat(player2Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);

        return Quaternion.identity;
    }

    // 특정 관절이 추적되고 있는지 여부를 반환
    public bool IsJointTracked(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsTracked.Length ? player1JointsTracked[joint] : false;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsTracked.Length ? player2JointsTracked[joint] : false;

        return false;
    }

    // 특정 사용자의 관절 위치를 반환 (Kinect 센서에 상대적, 미터 단위)
    public Vector3 GetJointPosition(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsPos.Length ? player1JointsPos[joint] : Vector3.zero;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsPos.Length ? player2JointsPos[joint] : Vector3.zero;

        return Vector3.zero;
    }

    // 부모 관절에 대한 관절의 지역 위치를 반환 (Kinect 센서에 상대적, 미터 단위)
    public Vector3 GetJointLocalPosition(uint UserId, int joint)
    {
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsPos.Length ?
                (player1JointsPos[joint] - player1JointsPos[parent]) : Vector3.zero;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsPos.Length ?
                (player2JointsPos[joint] - player2JointsPos[parent]) : Vector3.zero;

        return Vector3.zero;
    }

    // 특정 관절의 회전을 반환 (Kinect 센서에 상대적)
    public Quaternion GetJointOrientation(uint UserId, int joint, bool flip)
    {
        if (UserId == Player1ID)
        {
            if (joint >= 0 && joint < player1JointsOri.Length && player1JointsTracked[joint])
                return ConvertMatrixToQuat(player1JointsOri[joint], joint, flip);
        }
        else if (UserId == Player2ID)
        {
            if (joint >= 0 && joint < player2JointsOri.Length && player2JointsTracked[joint])
                return ConvertMatrixToQuat(player2JointsOri[joint], joint, flip);
        }

        return Quaternion.identity;
    }

    // 특정 관절의 지역 회전을 반환 (부모 관절에 상대적)
    public Quaternion GetJointLocalOrientation(uint UserId, int joint, bool flip)
    {
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

        if (UserId == Player1ID)
        {
            if (joint >= 0 && joint < player1JointsOri.Length && player1JointsTracked[joint])
            {
                Matrix4x4 localMat = (player1JointsOri[parent].inverse * player1JointsOri[joint]);
                return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
            }
        }
        else if (UserId == Player2ID)
        {
            if (joint >= 0 && joint < player2JointsOri.Length && player2JointsTracked[joint])
            {
                Matrix4x4 localMat = (player2JointsOri[parent].inverse * player2JointsOri[joint]);
                return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
            }
        }

        return Quaternion.identity;
    }

    // 기본 관절과 다음 관절 간의 방향을 반환
    public Vector3 GetDirectionBetweenJoints(uint UserId, int baseJoint, int nextJoint, bool flipX, bool flipZ)
    {
        Vector3 jointDir = Vector3.zero;

        if (UserId == Player1ID)
        {
            if (baseJoint >= 0 && baseJoint < player1JointsPos.Length && player1JointsTracked[baseJoint] &&
                nextJoint >= 0 && nextJoint < player1JointsPos.Length && player1JointsTracked[nextJoint])
            {
                jointDir = player1JointsPos[nextJoint] - player1JointsPos[baseJoint];
            }
        }
        else if (UserId == Player2ID)
        {
            if (baseJoint >= 0 && baseJoint < player2JointsPos.Length && player2JointsTracked[baseJoint] &&
                nextJoint >= 0 && nextJoint < player2JointsPos.Length && player2JointsTracked[nextJoint])
            {
                jointDir = player2JointsPos[nextJoint] - player2JointsPos[baseJoint];
            }
        }

        // 방향 벡터를 플립
        if (jointDir != Vector3.zero)
        {
            if (flipX)
                jointDir.x = -jointDir.x;

            if (flipZ)
                jointDir.z = -jointDir.z;
        }

        return jointDir;
    }

    // 주어진 사용자에 대한 제스처를 감지
    /// <summary>
    /// DetectGesture 메소드는 특정 제스처를 감지하고 이를 기반으로 다양한 동작을 실행합니다.
    /// 사용자의 제스처 진행률을 추적하고, 완료된 제스처에 대한 처리를 수행합니다.
    /// </summary>
    /// <param name="UserId">제스처를 감지할 사용자 ID</param>
    /// <param name="gesture">감지할 제스처 종류</param>
    public void DetectGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index >= 0)
            DeleteGesture(UserId, gesture);

        // 제스처 데이터 생성
        KinectGestures.GestureData gestureData = new KinectGestures.GestureData
        {
            userId = UserId,
            gesture = gesture,
            state = 0,
            joint = 0,
            progress = 0f,
            complete = false,
            cancelled = false,
            checkForGestures = new List<KinectGestures.Gestures>()
        };

        // 제스처에 따라 확인할 제스처 추가
        switch (gesture)
        {
            case KinectGestures.Gestures.ZoomIn:
                gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomOut);
                gestureData.checkForGestures.Add(KinectGestures.Gestures.Wheel);
                break;

            case KinectGestures.Gestures.ZoomOut:
                gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomIn);
                gestureData.checkForGestures.Add(KinectGestures.Gestures.Wheel);
                break;

            case KinectGestures.Gestures.Wheel:
                gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomIn);
                gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomOut);
                break;
        }

        // 사용자 ID에 따라 적절한 제스처 리스트에 추가
        if (UserId == Player1ID)
            player1Gestures.Add(gestureData);
        else if (UserId == Player2ID)
            player2Gestures.Add(gestureData);
    }

    // 특정 사용자의 제스처 상태를 리셋
    public bool ResetGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index < 0)
            return false;

        // 해당 제스처 데이터를 초기화
        KinectGestures.GestureData gestureData = (UserId == Player1ID) ? player1Gestures[index] : player2Gestures[index];

        gestureData.state = 0;
        gestureData.joint = 0;
        gestureData.progress = 0f;
        gestureData.complete = false;
        gestureData.cancelled = false;
        gestureData.startTrackingAtTime = Time.realtimeSinceStartup + KinectWrapper.Constants.MinTimeBetweenSameGestures;

        if (UserId == Player1ID)
            player1Gestures[index] = gestureData;
        else if (UserId == Player2ID)
            player2Gestures[index] = gestureData;

        return true;
    }

    // 모든 제스처의 상태를 리셋
    public void ResetPlayerGestures(uint UserId)
    {
        if (UserId == Player1ID)
        {
            int listSize = player1Gestures.Count;

            for (int i = 0; i < listSize; i++)
            {
                ResetGesture(UserId, player1Gestures[i].gesture);
            }
        }
        else if (UserId == Player2ID)
        {
            int listSize = player2Gestures.Count;

            for (int i = 0; i < listSize; i++)
            {
                ResetGesture(UserId, player2Gestures[i].gesture);
            }
        }
    }

    // 주어진 제스처를 삭제
    public bool DeleteGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index < 0)
            return false;

        // 해당 사용자 리스트에서 제스처 삭제
        if (UserId == Player1ID)
            player1Gestures.RemoveAt(index);
        else if (UserId == Player2ID)
            player2Gestures.RemoveAt(index);

        return true;
    }

    // 특정 사용자의 감지된 제스처 리스트를 비우기
    public void ClearGestures(uint UserId)
    {
        if (UserId == Player1ID)
        {
            player1Gestures.Clear();
        }
        else if (UserId == Player2ID)
        {
            player2Gestures.Clear();
        }
    }

    // 특정 사용자의 감지된 제스처 수를 반환
    public int GetGesturesCount(uint UserId)
    {
        if (UserId == Player1ID)
            return player1Gestures.Count;
        else if (UserId == Player2ID)
            return player2Gestures.Count;

        return 0;
    }

    // 특정 사용자의 감지된 제스처 리스트를 반환
    public List<KinectGestures.Gestures> GetGesturesList(uint UserId)
    {
        List<KinectGestures.Gestures> list = new List<KinectGestures.Gestures>();

        if (UserId == Player1ID)
        {
            foreach (KinectGestures.GestureData data in player1Gestures)
                list.Add(data.gesture);
        }
        else if (UserId == Player2ID)
        {
            foreach (KinectGestures.GestureData data in player2Gestures)
                list.Add(data.gesture);
        }

        return list;
    }

    // 특정 사용자의 감지된 제스처가 있는지 여부를 반환
    public bool IsGestureDetected(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        return index >= 0;
    }

    // 특정 사용자의 제스처가 완료되었는지 여부를 반환
    public bool IsGestureComplete(uint UserId, KinectGestures.Gestures gesture, bool bResetOnComplete)
    {
        int index = GetGestureIndex(UserId, gesture);

        if (index >= 0)
        {
            if (UserId == Player1ID)
            {
                KinectGestures.GestureData gestureData = player1Gestures[index];

                if (bResetOnComplete && gestureData.complete)
                {
                    ResetPlayerGestures(UserId);
                    return true;
                }

                return gestureData.complete;
            }
            else if (UserId == Player2ID)
            {
                KinectGestures.GestureData gestureData = player2Gestures[index];

                if (bResetOnComplete && gestureData.complete)
                {
                    ResetPlayerGestures(UserId);
                    return true;
                }

                return gestureData.complete;
            }
        }

        return false;
    }

    // 특정 사용자의 제스처가 취소되었는지 여부를 반환
    public bool IsGestureCancelled(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);

        if (index >= 0)
        {
            if (UserId == Player1ID)
            {
                KinectGestures.GestureData gestureData = player1Gestures[index];
                return gestureData.cancelled;
            }
            else if (UserId == Player2ID)
            {
                KinectGestures.GestureData gestureData = player2Gestures[index];
                return gestureData.cancelled;
            }
        }

        return false;
    }

    // 특정 사용자의 제스처 진행률을 반환
    public float GetGestureProgress(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);

        if (index >= 0)
        {
            if (UserId == Player1ID)
            {
                KinectGestures.GestureData gestureData = player1Gestures[index];
                return gestureData.progress;
            }
            else if (UserId == Player2ID)
            {
                KinectGestures.GestureData gestureData = player2Gestures[index];
                return gestureData.progress;
            }
        }

        return 0f;
    }

    // 특정 사용자의 제스처에 대한 현재 "화면 위치"를 반환
    public Vector3 GetGestureScreenPos(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);

        if (index >= 0)
        {
            if (UserId == Player1ID)
            {
                KinectGestures.GestureData gestureData = player1Gestures[index];
                return gestureData.screenPos;
            }
            else if (UserId == Player2ID)
            {
                KinectGestures.GestureData gestureData = player2Gestures[index];
                return gestureData.screenPos;
            }
        }

        return Vector3.zero;
    }

    // 제스처 리스너 리스트를 재설정
    public void ResetGestureListeners()
    {
        // 제스처 리스너 리스트 생성
        gestureListeners.Clear();

        foreach (MonoBehaviour script in GestureListeners)
        {
            if (script && (script is KinectGestures.GestureListenerInterface))
            {
                KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
                gestureListeners.Add(listener);
            }
        }
    }

    // 아바타 컨트롤러 리스트를 재설정
    public void ResetAvatarControllers()
    {
        if (Player1Avatars.Count == 0 && Player2Avatars.Count == 0)
        {
            AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];

            foreach (AvatarController avatar in avatars)
            {
                Player1Avatars.Add(avatar.gameObject);
            }
        }

        // Player1 아바타 컨트롤러 초기화
        if (Player1Controllers != null)
        {
            Player1Controllers.Clear();
            foreach (GameObject avatar in Player1Avatars)
            {
                if (avatar != null && avatar.activeInHierarchy)
                {
                    AvatarController controller = avatar.GetComponent<AvatarController>();
                    controller.ResetToInitialPosition();
                    controller.Awake();
                    Player1Controllers.Add(controller);
                }
            }
        }

        // Player2 아바타 컨트롤러 초기화
        if (Player2Controllers != null)
        {
            Player2Controllers.Clear();
            foreach (GameObject avatar in Player2Avatars)
            {
                if (avatar != null && avatar.activeInHierarchy)
                {
                    AvatarController controller = avatar.GetComponent<AvatarController>();
                    controller.ResetToInitialPosition();
                    controller.Awake();
                    Player2Controllers.Add(controller);
                }
            }
        }
    }

    // 현재 감지된 Kinect 사용자를 제거
    public void ClearKinectUsers()
    {
        if (!KinectInitialized)
            return;

        // 현재 사용자 제거
        for (int i = allUsers.Count - 1; i >= 0; i--)
        {
            uint userId = allUsers[i];
            RemoveUser(userId);
        }

        ResetFilters();
    }

    // Kinect의 버퍼를 지우고 필터를 재설정
    public void ResetFilters()
    {
        if (!KinectInitialized)
            return;

        // Kinect 변수 초기화
        player1Pos = Vector3.zero; player2Pos = Vector3.zero;
        player1Ori = Matrix4x4.identity; player2Ori = Matrix4x4.identity;

        int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        for (int i = 0; i < skeletonJointsCount; i++)
        {
            player1JointsTracked[i] = false; player2JointsTracked[i] = false;
            player1PrevTracked[i] = false; player2PrevTracked[i] = false;
            player1JointsPos[i] = Vector3.zero; player2JointsPos[i] = Vector3.zero;
            player1JointsOri[i] = Matrix4x4.identity; player2JointsOri[i] = Matrix4x4.identity;
        }

        // 각 필터 초기화
        if (trackingStateFilter != null)
        {
            for (int i = 0; i < trackingStateFilter.Length; i++)
                if (trackingStateFilter[i] != null)
                    trackingStateFilter[i].Reset();
        }

        if (boneOrientationFilter != null)
        {
            for (int i = 0; i < boneOrientationFilter.Length; i++)
                if (boneOrientationFilter[i] != null)
                    boneOrientationFilter[i].Reset();
        }

        if (clippedLegsFilter != null)
        {
            for (int i = 0; i < clippedLegsFilter.Length; i++)
                if (clippedLegsFilter[i] != null)
                    clippedLegsFilter[i].Reset();
        }
    }

    //----------------------------------- public 함수의 끝 --------------------------------------//

    void Awake()
    {
        int hr = 0;

        try
        {
            // Kinect 초기화
            hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton |
                KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex |
                (ComputeColorMap ? KinectWrapper.NuiInitializeFlags.UsesColor : 0));
            if (hr != 0)
            {
                throw new Exception("NuiInitialize Failed");
            }

            // 스켈레톤 트래킹 활성화
            hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);
            if (hr != 0)
            {
                throw new Exception("Cannot initialize Skeleton Data");
            }

            // 깊이 스트림 핸들 초기화
            depthStreamHandle = IntPtr.Zero;
            if (ComputeUserMap)
            {
                hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex,
                    KinectWrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref depthStreamHandle);
                if (hr != 0)
                {
                    throw new Exception("Cannot open depth stream");
                }
            }

            // 색상 스트림 핸들 초기화
            colorStreamHandle = IntPtr.Zero;
            if (ComputeColorMap)
            {
                hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color,
                    KinectWrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
                if (hr != 0)
                {
                    throw new Exception("Cannot open color stream");
                }
            }

            // Kinect의 기울기 각도 설정
            KinectWrapper.NuiCameraElevationSetAngle(SensorAngle);

            // 스켈레톤 구조체 초기화
            skeletonFrame = new KinectWrapper.NuiSkeletonFrame()
            {
                SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount]
            };

            // 스무딩 함수에 사용할 값 설정
            smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();

            switch (smoothing)
            {
                case Smoothing.Default:
                    smoothParameters.fSmoothing = 0.5f;
                    smoothParameters.fCorrection = 0.5f;
                    smoothParameters.fPrediction = 0.5f;
                    smoothParameters.fJitterRadius = 0.05f;
                    smoothParameters.fMaxDeviationRadius = 0.04f;
                    break;
                case Smoothing.Medium:
                    smoothParameters.fSmoothing = 0.5f;
                    smoothParameters.fCorrection = 0.1f;
                    smoothParameters.fPrediction = 0.5f;
                    smoothParameters.fJitterRadius = 0.1f;
                    smoothParameters.fMaxDeviationRadius = 0.1f;
                    break;
                case Smoothing.Aggressive:
                    smoothParameters.fSmoothing = 0.7f;
                    smoothParameters.fCorrection = 0.3f;
                    smoothParameters.fPrediction = 1.0f;
                    smoothParameters.fJitterRadius = 1.0f;
                    smoothParameters.fMaxDeviationRadius = 1.0f;
                    break;
            }

            // 트래킹 상태 필터 초기화
            trackingStateFilter = new TrackingStateFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < trackingStateFilter.Length; i++)
            {
                trackingStateFilter[i] = new TrackingStateFilter();
                trackingStateFilter[i].Init();
            }

            // 뼈 방향 필터 초기화
            boneOrientationFilter = new BoneOrientationsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < boneOrientationFilter.Length; i++)
            {
                boneOrientationFilter[i] = new BoneOrientationsFilter();
                boneOrientationFilter[i].Init();
            }

            // 잘린 다리 필터 초기화
            clippedLegsFilter = new ClippedLegsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < clippedLegsFilter.Length; i++)
            {
                clippedLegsFilter[i] = new ClippedLegsFilter();
            }

            // 뼈 방향 제약 초기화
            boneConstraintsFilter = new BoneOrientationsConstraint();
            boneConstraintsFilter.AddDefaultConstraints();
            selfIntersectionConstraint = new SelfIntersectionConstraint();

            // 관절 위치 및 방향 배열 생성
            int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

            player1JointsTracked = new bool[skeletonJointsCount];
            player2JointsTracked = new bool[skeletonJointsCount];
            player1PrevTracked = new bool[skeletonJointsCount];
            player2PrevTracked = new bool[skeletonJointsCount];

            player1JointsPos = new Vector3[skeletonJointsCount];
            player2JointsPos = new Vector3[skeletonJointsCount];

            player1JointsOri = new Matrix4x4[skeletonJointsCount];
            player2JointsOri = new Matrix4x4[skeletonJointsCount];

            gestureTrackingAtTime = new float[KinectWrapper.Constants.NuiSkeletonMaxTracked];

            // Kinect 공간에서 월드 공간 변환 매트릭스 생성
            Quaternion quatTiltAngle = new Quaternion();
            quatTiltAngle.eulerAngles = new Vector3(-SensorAngle, 0.0f, 0.0f);

            // 변환 매트릭스 생성 (Kinect에서 월드로)
            kinectToWorld.SetTRS(new Vector3(0.0f, SensorHeight, 0.0f), quatTiltAngle, Vector3.one);
            flipMatrix = Matrix4x4.identity;
            flipMatrix[2, 2] = -1;

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        catch (DllNotFoundException e)
        {
            string message = "Kinect SDK 설치를 확인하십시오.";
            Debug.LogError(message);
            Debug.LogError(e.ToString());
            if (CalibrationText != null)
                CalibrationText.GetComponent<GUIText>().text = message;

            return;
        }
        catch (Exception e)
        {
            string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
            Debug.LogError(message);
            Debug.LogError(e.ToString());
            if (CalibrationText != null)
                CalibrationText.GetComponent<GUIText>().text = message;

            return;
        }

        // 사용자 맵 관련 초기화
        if (ComputeUserMap)
        {
            usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
            usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
            usersMapColors = new Color32[usersMapSize];
            usersPrevState = new ushort[usersMapSize];
            usersDepthMap = new ushort[usersMapSize];
            usersHistogramMap = new float[8192];
        }

        // 색상 맵 관련 초기화
        if (ComputeColorMap)
        {
            usersClrTex = new Texture2D(KinectWrapper.GetColorWidth(), KinectWrapper.GetColorHeight());
            colorImage = new Color32[KinectWrapper.GetColorWidth() * KinectWrapper.GetColorHeight()];
            usersColorMap = new byte[colorImage.Length << 2];
        }

        // 아바타 컨트롤러 자동 검색
        if (Player1Avatars.Count == 0 && Player2Avatars.Count == 0)
        {
            AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];

            foreach (AvatarController avatar in avatars)
            {
                Player1Avatars.Add(avatar.gameObject);
            }
        }

        // 모든 사용자 리스트 초기화
        allUsers = new List<uint>();

        // 아바타 컨트롤러 리스트 초기화
        Player1Controllers = new List<AvatarController>();
        Player2Controllers = new List<AvatarController>();

        // 각 플레이어의 아바타 컨트롤러 추가
        foreach (GameObject avatar in Player1Avatars)
        {
            if (avatar != null && avatar.activeInHierarchy)
            {
                Player1Controllers.Add(avatar.GetComponent<AvatarController>());
            }
        }

        foreach (GameObject avatar in Player2Avatars)
        {
            if (avatar != null && avatar.activeInHierarchy)
            {
                Player2Controllers.Add(avatar.GetComponent<AvatarController>());
            }
        }

        // 제스처 리스너 리스트 생성
        gestureListeners = new List<KinectGestures.GestureListenerInterface>();

        foreach (MonoBehaviour script in GestureListeners)
        {
            if (script && (script is KinectGestures.GestureListenerInterface))
            {
                KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
                gestureListeners.Add(listener);
            }
        }

        // GUI 텍스트 초기화
        if (CalibrationText != null)
        {
            CalibrationText.GetComponent<GUIText>().text = "사용자를 기다리는 중...";
        }

        Debug.Log("사용자를 기다리는 중...");

        KinectInitialized = true;
    }

    /// <summary>
    /// Update 메소드는 매 프레임마다 호출되며, 사용자로부터 깊이 및 색상 데이터를 가져오고 업데이트합니다.
    /// 스켈레톤 데이터를 처리하여 각 관절의 위치와 회전을 계산하고 아바타에 적용합니다.
    /// </summary>
    void Update()
    {
        if (KinectInitialized)
        {
            // 사용자 맵 업데이트
            if (ComputeUserMap)
            {
                if (depthStreamHandle != IntPtr.Zero &&
                    KinectWrapper.PollDepth(depthStreamHandle, KinectWrapper.Constants.IsNearMode, ref usersDepthMap))
                {
                    UpdateUserMap();
                }
            }

            // 색상 맵 업데이트
            if (ComputeColorMap)
            {
                if (colorStreamHandle != IntPtr.Zero &&
                    KinectWrapper.PollColor(colorStreamHandle, ref usersColorMap, ref colorImage))
                {
                    UpdateColorMap();
                }
            }

            // 스켈레톤 데이터 업데이트
            if (KinectWrapper.PollSkeleton(ref smoothParameters, ref skeletonFrame))
            {
                ProcessSkeleton();
            }

            // 플레이어 1 모델 업데이트
            if (Player1Calibrated)
            {
                foreach (AvatarController controller in Player1Controllers)
                {
                    controller.UpdateAvatar(Player1ID);
                }

                // 제스처 완료 확인
                foreach (KinectGestures.GestureData gestureData in player1Gestures)
                {
                    if (gestureData.complete)
                    {
                        if (gestureData.gesture == KinectGestures.Gestures.Click)
                        {
                            if (ControlMouseCursor)
                            {
                                MouseControl.MouseClick();
                            }
                        }

                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            if (listener.GestureCompleted(Player1ID, 0, gestureData.gesture,
                                                         (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos))
                            {
                                ResetPlayerGestures(Player1ID);
                            }
                        }
                    }
                    else if (gestureData.cancelled)
                    {
                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            if (listener.GestureCancelled(Player1ID, 0, gestureData.gesture,
                                                         (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint))
                            {
                                ResetGesture(Player1ID, gestureData.gesture);
                            }
                        }
                    }
                    else if (gestureData.progress >= 0.1f)
                    {
                        if ((gestureData.gesture == KinectGestures.Gestures.RightHandCursor ||
                            gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) &&
                            gestureData.progress >= 0.5f)
                        {
                            if (GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
                            {
                                if (HandCursor1 != null)
                                {
                                    Vector3 vCursorPos = gestureData.screenPos;

                                    if (HandCursor1.GetComponent<GUITexture>() == null)
                                    {
                                        float zDist = HandCursor1.transform.position.z - Camera.main.transform.position.z;
                                        vCursorPos.z = zDist;

                                        vCursorPos = Camera.main.ViewportToWorldPoint(vCursorPos);
                                    }

                                    HandCursor1.transform.position = Vector3.Lerp(HandCursor1.transform.position, vCursorPos, 3 * Time.deltaTime);
                                }

                                if (ControlMouseCursor)
                                {
                                    Vector3 vCursorPos = HandCursor1.GetComponent<GUITexture>() != null ? HandCursor1.transform.position :
                                        Camera.main.WorldToViewportPoint(HandCursor1.transform.position);
                                    MouseControl.MouseMove(vCursorPos, CalibrationText);
                                }
                            }
                        }

                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            listener.GestureInProgress(Player1ID, 0, gestureData.gesture, gestureData.progress,
                                                       (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos);
                        }
                    }
                }
            }

            // 플레이어 2 모델 업데이트
            if (Player2Calibrated)
            {
                foreach (AvatarController controller in Player2Controllers)
                {
                    controller.UpdateAvatar(Player2ID);
                }

                // 제스처 완료 확인
                foreach (KinectGestures.GestureData gestureData in player2Gestures)
                {
                    if (gestureData.complete)
                    {
                        if (gestureData.gesture == KinectGestures.Gestures.Click)
                        {
                            if (ControlMouseCursor)
                            {
                                MouseControl.MouseClick();
                            }
                        }

                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            if (listener.GestureCompleted(Player2ID, 1, gestureData.gesture,
                                                         (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos))
                            {
                                ResetPlayerGestures(Player2ID);
                            }
                        }
                    }
                    else if (gestureData.cancelled)
                    {
                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            if (listener.GestureCancelled(Player2ID, 1, gestureData.gesture,
                                                         (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint))
                            {
                                ResetGesture(Player2ID, gestureData.gesture);
                            }
                        }
                    }
                    else if (gestureData.progress >= 0.1f)
                    {
                        if ((gestureData.gesture == KinectGestures.Gestures.RightHandCursor ||
                            gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) &&
                            gestureData.progress >= 0.5f)
                        {
                            if (GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
                            {
                                if (HandCursor2 != null)
                                {
                                    Vector3 vCursorPos = gestureData.screenPos;

                                    if (HandCursor2.GetComponent<GUITexture>() == null)
                                    {
                                        float zDist = HandCursor2.transform.position.z - Camera.main.transform.position.z;
                                        vCursorPos.z = zDist;

                                        vCursorPos = Camera.main.ViewportToWorldPoint(vCursorPos);
                                    }

                                    HandCursor2.transform.position = Vector3.Lerp(HandCursor2.transform.position, vCursorPos, 3 * Time.deltaTime);
                                }

                                if (ControlMouseCursor)
                                {
                                    Vector3 vCursorPos = HandCursor2.GetComponent<GUITexture>() != null ? HandCursor2.transform.position :
                                        Camera.main.WorldToViewportPoint(HandCursor2.transform.position);
                                    MouseControl.MouseMove(vCursorPos, CalibrationText);
                                }
                            }
                        }

                        foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                        {
                            listener.GestureInProgress(Player2ID, 1, gestureData.gesture, gestureData.progress,
                                                       (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos);
                        }
                    }
                }
            }
        }

        // ESC 키를 눌러 애플리케이션 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // 애플리케이션 종료 시 Kinect 종료
    /// <summary>
    /// OnApplicationQuit 메소드는 애플리케이션 종료 시 Kinect를 종료합니다.
    /// </summary>
    void OnApplicationQuit()
    {
        if (KinectInitialized)
        {
            // OpenNI 종료
            KinectWrapper.NuiShutdown();
            instance = null;
        }
    }

    // GUI에 히스토그램 맵 그리기
    void OnGUI()
    {
        if (KinectInitialized)
        {
            // 사용자 맵 표시
            if (ComputeUserMap && (/**(allUsers.Count == 0) ||*/ DisplayUserMap))
            {
                if (usersMapRect.width == 0 || usersMapRect.height == 0)
                {
                    // 메인 카메라의 사각형 가져오기
                    Rect cameraRect = Camera.main.pixelRect;

                    // 필요 시 너비와 높이를 비율로 계산
                    if (DisplayMapsWidthPercent == 0f)
                    {
                        DisplayMapsWidthPercent = (KinectWrapper.GetDepthWidth() / 2) * 100 / cameraRect.width;
                    }

                    float displayMapsWidthPercent = DisplayMapsWidthPercent / 100f;
                    float displayMapsHeightPercent = displayMapsWidthPercent * KinectWrapper.GetDepthHeight() / KinectWrapper.GetDepthWidth();

                    float displayWidth = cameraRect.width * displayMapsWidthPercent;
                    float displayHeight = cameraRect.width * displayMapsHeightPercent;

                    usersMapRect = new Rect(cameraRect.width - displayWidth, cameraRect.height, displayWidth, -displayHeight);
                }

                GUI.DrawTexture(usersMapRect, usersLblTex);
            }
            // 색상 맵 표시
            else if (ComputeColorMap && (DisplayColorMap))
            {
                if (usersClrRect.width == 0 || usersClrTex.height == 0)
                {
                    // 메인 카메라의 사각형 가져오기
                    Rect cameraRect = Camera.main.pixelRect;

                    // 필요 시 너비와 높이를 비율로 계산
                    if (DisplayMapsWidthPercent == 0f)
                    {
                        DisplayMapsWidthPercent = (KinectWrapper.GetDepthWidth() / 2) * 100 / cameraRect.width;
                    }

                    float displayMapsWidthPercent = DisplayMapsWidthPercent / 100f;
                    float displayMapsHeightPercent = displayMapsWidthPercent * KinectWrapper.GetColorHeight() / KinectWrapper.GetColorWidth();

                    float displayWidth = cameraRect.width * displayMapsWidthPercent;
                    float displayHeight = cameraRect.width * displayMapsHeightPercent;

                    usersClrRect = new Rect(cameraRect.width - displayWidth, cameraRect.height, displayWidth, -displayHeight);
                }

                GUI.DrawTexture(usersClrRect, usersClrTex);
            }
        }
    }

    // 사용자 맵 업데이트
    /// <summary>
    /// UpdateUserMap 메소드는 사용자 맵을 업데이트하여 GUI에 표시합니다.
    /// 깊이 데이터를 기반으로 사용자 맵을 생성하고 갱신합니다.
    /// </summary>
    void UpdateUserMap()
    {
        int numOfPoints = 0;
        Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // 깊이에 대한 누적 히스토그램 계산
        for (int i = 0; i < usersMapSize; i++)
        {
            // 사용자가 있는 깊이에 대해서만 계산
            if ((usersDepthMap[i] & 7) != 0)
            {
                ushort userDepth = (ushort)(usersDepthMap[i] >> 3);
                usersHistogramMap[userDepth]++;
                numOfPoints++;
            }
        }

        if (numOfPoints > 0)
        {
            for (int i = 1; i < usersHistogramMap.Length; i++)
            {
                usersHistogramMap[i] += usersHistogramMap[i - 1];
            }

            for (int i = 0; i < usersHistogramMap.Length; i++)
            {
                usersHistogramMap[i] = 1.0f - (usersHistogramMap[i] / numOfPoints);
            }
        }

        // 좌표 매퍼에 필요한 더미 구조체
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        // 레이블 맵과 깊이 히스토그램을 기반으로 실제 사용자 텍스처 생성
        Color32 clrClear = Color.clear;
        for (int i = 0; i < usersMapSize; i++)
        {
            // 텍스처를 뒤집어 레이블 맵을 색상 배열로 변환
            int flipIndex = i; // usersMapSize - i - 1;

            ushort userMap = (ushort)(usersDepthMap[i] & 7);
            ushort userDepth = (ushort)(usersDepthMap[i] >> 3);

            ushort nowUserPixel = userMap != 0 ? (ushort)((userMap << 13) | userDepth) : userDepth;
            ushort wasUserPixel = usersPrevState[flipIndex];

            // 변경된 픽셀만 그리기
            if (nowUserPixel != wasUserPixel)
            {
                usersPrevState[flipIndex] = nowUserPixel;

                if (userMap == 0)
                {
                    usersMapColors[flipIndex] = clrClear;
                }
                else
                {
                    if (colorImage != null)
                    {
                        int x = i % KinectWrapper.Constants.DepthImageWidth;
                        int y = i / KinectWrapper.Constants.DepthImageWidth;

                        int cx, cy;
                        int hr = KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
                            KinectWrapper.Constants.ColorImageResolution,
                            KinectWrapper.Constants.DepthImageResolution,
                            ref pcViewArea,
                            x, y, usersDepthMap[i],
                            out cx, out cy);

                        if (hr == 0)
                        {
                            int colorIndex = cx + cy * KinectWrapper.Constants.ColorImageWidth;
                            //colorIndex = usersMapSize - colorIndex - 1;
                            if (colorIndex >= 0 && colorIndex < usersMapSize)
                            {
                                Color32 colorPixel = colorImage[colorIndex];
                                usersMapColors[flipIndex] = colorPixel;
                                usersMapColors[flipIndex].a = 230; // 투명도 설정
                            }
                        }
                    }
                    else
                    {
                        // 깊이 히스토그램에 따라 혼합 색상 생성
                        float histDepth = usersHistogramMap[userDepth];
                        Color c = new Color(histDepth, histDepth, histDepth, 0.9f);

                        switch (userMap % 4)
                        {
                            case 0:
                                usersMapColors[flipIndex] = Color.red * c;
                                break;
                            case 1:
                                usersMapColors[flipIndex] = Color.green * c;
                                break;
                            case 2:
                                usersMapColors[flipIndex] = Color.blue * c;
                                break;
                            case 3:
                                usersMapColors[flipIndex] = Color.magenta * c;
                                break;
                        }
                    }
                }
            }
        }

        // 그리기!
        usersLblTex.SetPixels32(usersMapColors);

        if (!DisplaySkeletonLines)
        {
            usersLblTex.Apply();
        }
    }

    // 색상 맵 업데이트
    void UpdateColorMap()
    {
        usersClrTex.SetPixels32(colorImage);
        usersClrTex.Apply();
    }

    // 사용자 ID를 플레이어 1 또는 2에 할당
    void CalibrateUser(uint UserId, int UserIndex, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
        // 플레이어 1이 보정되지 않았다면, 그 사용자 ID를 할당
        if (!Player1Calibrated)
        {
            // 플레이어 2를 실수로 플레이어 1에 할당하지 않도록 확인
            if (!allUsers.Contains(UserId))
            {
                if (CheckForCalibrationPose(UserId, ref Player1CalibrationPose, ref player1CalibrationData, ref skeletonData))
                {
                    Player1Calibrated = true;
                    Player1ID = UserId;
                    Player1Index = UserIndex;

                    allUsers.Add(UserId);

                    foreach (AvatarController controller in Player1Controllers)
                    {
                        controller.SuccessfulCalibration(UserId);
                    }

                    // 감지할 제스처 추가
                    foreach (KinectGestures.Gestures gesture in Player1Gestures)
                    {
                        DetectGesture(UserId, gesture);
                    }

                    // 제스처 리스너에게 새로운 사용자 알림
                    foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                    {
                        listener.UserDetected(UserId, 0);
                    }

                    // 스켈레톤 필터 초기화
                    ResetFilters();

                    // 플레이어 수에 따라 모든 플레이어가 보정되었는지 확인
                    AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;
                }
            }
        }
        // 그렇지 않으면 플레이어 2에 할당
        else if (TwoUsers && !Player2Calibrated)
        {
            if (!allUsers.Contains(UserId))
            {
                if (CheckForCalibrationPose(UserId, ref Player2CalibrationPose, ref player2CalibrationData, ref skeletonData))
                {
                    Player2Calibrated = true;
                    Player2ID = UserId;
                    Player2Index = UserIndex;

                    allUsers.Add(UserId);

                    foreach (AvatarController controller in Player2Controllers)
                    {
                        controller.SuccessfulCalibration(UserId);
                    }

                    // 감지할 제스처 추가
                    foreach (KinectGestures.Gestures gesture in Player2Gestures)
                    {
                        DetectGesture(UserId, gesture);
                    }

                    // 제스처 리스너에게 새로운 사용자 알림
                    foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                    {
                        listener.UserDetected(UserId, 1);
                    }

                    // 스켈레톤 필터 초기화
                    ResetFilters();

                    // 모든 플레이어가 보정되었는지 확인
                    AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;
                }
            }
        }

        // 모든 플레이어가 보정된 경우, 더 이상 찾지 않도록 중지
        if (AllPlayersCalibrated)
        {
            Debug.Log("모든 플레이어가 보정되었습니다.");

            if (CalibrationText != null)
            {
                CalibrationText.GetComponent<GUIText>().text = "";
            }
        }
    }

    // 잃어버린 사용자 ID 제거
    void RemoveUser(uint UserId)
    {
        // 플레이어 1을 잃어버린 경우
        if (UserId == Player1ID)
        {
            // ID를 null로 설정하고, 해당 ID와 관련된 모든 모델을 재설정
            Player1ID = 0;
            Player1Index = 0;
            Player1Calibrated = false;

            foreach (AvatarController controller in Player1Controllers)
            {
                controller.ResetToInitialPosition();
            }

            foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
            {
                listener.UserLost(UserId, 0);
            }

            player1CalibrationData.userId = 0;
        }

        // 플레이어 2를 잃어버린 경우
        if (UserId == Player2ID)
        {
            // ID를 null로 설정하고, 해당 ID와 관련된 모든 모델을 재설정
            Player2ID = 0;
            Player2Index = 0;
            Player2Calibrated = false;

            foreach (AvatarController controller in Player2Controllers)
            {
                controller.ResetToInitialPosition();
            }

            foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
            {
                listener.UserLost(UserId, 1);
            }

            player2CalibrationData.userId = 0;
        }

        // 이 사용자의 제스처 리스트를 지움
        ClearGestures(UserId);

        // 글로벌 사용자 리스트에서 제거
        allUsers.Remove(UserId);
        AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;

        // 사용자 교체 시도
        Debug.Log("사용자를 기다리는 중...");

        if (CalibrationText != null)
        {
            CalibrationText.GetComponent<GUIText>().text = "사용자를 기다리는 중...";
        }
    }

    // 내부 상수
    private const int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
    private const int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;

    private int[] mustBeTrackedJoints = {
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
    };

    // 스켈레톤 데이터 처리
    /// <summary>
    /// ProcessSkeleton 메소드는 스켈레톤 데이터를 처리하여 사용자 정보를 업데이트합니다.
    /// 각 사용자에 대한 스켈레톤 위치와 상태를 관리합니다.
    /// </summary>
    void ProcessSkeleton()
    {
        List<uint> lostUsers = new List<uint>();
        lostUsers.AddRange(allUsers);

        // 마지막 업데이트 이후 경과 시간 계산
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - lastNuiTime;

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
            uint userId = skeletonData.dwTrackingID;

            if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                // 스켈레톤 위치 가져오기
                Vector3 skeletonPos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);

                if (!AllPlayersCalibrated)
                {
                    // 가장 가까운 사용자 확인
                    bool bClosestUser = true;

                    if (DetectClosestUser)
                    {
                        for (int j = 0; j < KinectWrapper.Constants.NuiSkeletonCount; j++)
                        {
                            if (j != i)
                            {
                                KinectWrapper.NuiSkeletonData skeletonDataOther = skeletonFrame.SkeletonData[j];

                                if ((skeletonDataOther.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked) &&
                                    (Mathf.Abs(kinectToWorld.MultiplyPoint3x4(skeletonDataOther.Position).z) < Mathf.Abs(skeletonPos.z)))
                                {
                                    bClosestUser = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (bClosestUser)
                    {
                        CalibrateUser(userId, i + 1, ref skeletonData);
                    }
                }

                // 플레이어 1의 데이터 처리
                if (userId == Player1ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
                   (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
                {
                    player1Index = i;

                    // 플레이어 위치 가져오기
                    player1Pos = skeletonPos;

                    // 트래킹 상태 필터 적용
                    trackingStateFilter[0].UpdateFilter(ref skeletonData);

                    // 아바타 외관 향상을 위한 스켈레톤 수정
                    if (UseClippedLegsFilter && clippedLegsFilter[0] != null)
                    {
                        clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);
                    }

                    if (UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
                    {
                        selfIntersectionConstraint.Constrain(ref skeletonData);
                    }

                    // 관절의 위치와 회전 가져오기
                    for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
                    {
                        bool playerTracked = IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                            (Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                            (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
                        player1JointsTracked[j] = player1PrevTracked[j] && playerTracked;
                        player1PrevTracked[j] = playerTracked;

                        if (player1JointsTracked[j])
                        {
                            player1JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
                        }

                    }

                    // 텍스처 위에 스켈레톤 그리기
                    if (DisplaySkeletonLines && ComputeUserMap)
                    {
                        DrawSkeleton(usersLblTex, ref skeletonData, ref player1JointsTracked);
                        usersLblTex.Apply();
                    }

                    // 관절의 방향 계산
                    KinectWrapper.GetSkeletonJointOrientation(ref player1JointsPos, ref player1JointsTracked, ref player1JointsOri);

                    // 방향 제약 필터
                    if (UseBoneOrientationsConstraint && boneConstraintsFilter != null)
                    {
                        boneConstraintsFilter.Constrain(ref player1JointsOri, ref player1JointsTracked);
                    }

                    // 관절 방향 필터
                    if (UseBoneOrientationsFilter && boneOrientationFilter[0] != null)
                    {
                        boneOrientationFilter[0].UpdateFilter(ref skeletonData, ref player1JointsOri);
                    }

                    // 플레이어 회전 가져오기
                    player1Ori = player1JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

                    // 제스처 확인
                    if (Time.realtimeSinceStartup >= gestureTrackingAtTime[0])
                    {
                        int listGestureSize = player1Gestures.Count;
                        float timestampNow = Time.realtimeSinceStartup;
                        string sDebugGestures = string.Empty;  // "추적된 제스처:\n";

                        for (int g = 0; g < listGestureSize; g++)
                        {
                            KinectGestures.GestureData gestureData = player1Gestures[g];

                            if ((timestampNow >= gestureData.startTrackingAtTime) &&
                                !IsConflictingGestureInProgress(gestureData))
                            {
                                KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup,
                                                               ref player1JointsPos, ref player1JointsTracked);
                                player1Gestures[g] = gestureData;

                                if (gestureData.complete)
                                {
                                    gestureTrackingAtTime[0] = timestampNow + MinTimeBetweenGestures;
                                }

                                {
                                    sDebugGestures += string.Format("{0} - 상태: {1}, 시간: {2:F1}, 진행률: {3}%\n",
                                                                    gestureData.gesture, gestureData.state,
                                                                    gestureData.timestamp,
                                                                    (int)(gestureData.progress * 100 + 0.5f));
                                }
                            }
                        }

                        if (GesturesDebugText)
                        {
                            sDebugGestures += string.Format("\n 왼쪽 손: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft].ToString() : "");
                            sDebugGestures += string.Format("\n 오른쪽 손: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight].ToString() : "");
                            sDebugGestures += string.Format("\n 왼쪽 팔꿈치: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft].ToString() : "");
                            sDebugGestures += string.Format("\n 오른쪽 팔꿈치: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight].ToString() : "");

                            sDebugGestures += string.Format("\n 왼쪽 어깨: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft].ToString() : "");
                            sDebugGestures += string.Format("\n 오른쪽 어깨: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight].ToString() : "");

                            sDebugGestures += string.Format("\n 목: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter].ToString() : "");
                            sDebugGestures += string.Format("\n 엉덩이: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter].ToString() : "");
                            sDebugGestures += string.Format("\n 왼쪽 엉덩이: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft].ToString() : "");
                            sDebugGestures += string.Format("\n 오른쪽 엉덩이: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight].ToString() : "");

                            GesturesDebugText.GetComponent<GUIText>().text = sDebugGestures;
                        }
                    }
                }
                // 플레이어 2 데이터 처리
                else if (userId == Player2ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
                        (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
                {
                    player2Index = i;

                    // 플레이어 위치 가져오기
                    player2Pos = skeletonPos;

                    // 트래킹 상태 필터 적용
                    trackingStateFilter[1].UpdateFilter(ref skeletonData);

                    // 아바타 외관 향상을 위한 스켈레톤 수정
                    if (UseClippedLegsFilter && clippedLegsFilter[1] != null)
                    {
                        clippedLegsFilter[1].FilterSkeleton(ref skeletonData, deltaNuiTime);
                    }

                    if (UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
                    {
                        selfIntersectionConstraint.Constrain(ref skeletonData);
                    }

                    // 관절의 위치와 회전 가져오기
                    for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
                    {
                        bool playerTracked = IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                            (Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                            (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
                        player2JointsTracked[j] = player2PrevTracked[j] && playerTracked;
                        player2PrevTracked[j] = playerTracked;

                        if (player2JointsTracked[j])
                        {
                            player2JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
                        }
                    }

                    // 텍스처 위에 스켈레톤 그리기
                    if (DisplaySkeletonLines && ComputeUserMap)
                    {
                        DrawSkeleton(usersLblTex, ref skeletonData, ref player2JointsTracked);
                        usersLblTex.Apply();
                    }

                    // 관절의 방향 계산
                    KinectWrapper.GetSkeletonJointOrientation(ref player2JointsPos, ref player2JointsTracked, ref player2JointsOri);

                    // 방향 제약 필터
                    if (UseBoneOrientationsConstraint && boneConstraintsFilter != null)
                    {
                        boneConstraintsFilter.Constrain(ref player2JointsOri, ref player2JointsTracked);
                    }

                    // 관절 방향 필터
                    if (UseBoneOrientationsFilter && boneOrientationFilter[1] != null)
                    {
                        boneOrientationFilter[1].UpdateFilter(ref skeletonData, ref player2JointsOri);
                    }

                    // 플레이어 회전 가져오기
                    player2Ori = player2JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

                    // 제스처 확인
                    if (Time.realtimeSinceStartup >= gestureTrackingAtTime[1])
                    {
                        int listGestureSize = player2Gestures.Count;
                        float timestampNow = Time.realtimeSinceStartup;

                        for (int g = 0; g < listGestureSize; g++)
                        {
                            KinectGestures.GestureData gestureData = player2Gestures[g];

                            if ((timestampNow >= gestureData.startTrackingAtTime) &&
                                !IsConflictingGestureInProgress(gestureData))
                            {
                                KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup,
                                                               ref player2JointsPos, ref player2JointsTracked);
                                player2Gestures[g] = gestureData;
                            }
                        }
                    }
                }

                // 잃어버린 사용자 제거
                lostUsers.Remove(userId);
            }
        }

        // NUI 타이머 업데이트
        lastNuiTime = currentNuiTime;

        // 잃어버린 사용자 제거
        if (lostUsers.Count > 0)
        {
            foreach (uint userId in lostUsers)
            {
                RemoveUser(userId);
            }

            lostUsers.Clear();
        }
    }

    // 텍스처에 스켈레톤 그리기
    private void DrawSkeleton(Texture2D aTexture, ref KinectWrapper.NuiSkeletonData skeletonData, ref bool[] playerJointsTracked)
    {
        int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

        for (int i = 0; i < jointsCount; i++)
        {
            int parent = KinectWrapper.GetSkeletonJointParent(i);

            if (playerJointsTracked[i] && playerJointsTracked[parent])
            {
                Vector3 posParent = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[parent]);
                Vector3 posJoint = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[i]);

                DrawLine(aTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);
            }
        }
    }

    // x1, y1과 x2, y2를 연결하는 선을 텍스처에 그립니다.
    private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
    {
        int width = a_Texture.width;  // 텍스처의 너비
        int height = a_Texture.height; // 텍스처의 높이

        int dy = y2 - y1; // y 방향의 변화
        int dx = x2 - x1; // x 방향의 변화

        int stepy = 1;
        if (dy < 0)
        {
            dy = -dy;
            stepy = -1; // y 방향이 감소하는 경우
        }

        int stepx = 1;
        if (dx < 0)
        {
            dx = -dx;
            stepx = -1; // x 방향이 감소하는 경우
        }

        dy <<= 1; // dy를 두 배로 증가
        dx <<= 1; // dx를 두 배로 증가

        // 시작 점(x1, y1)에서 픽셀 색상 설정
        if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    a_Texture.SetPixel(x1 + x, y1 + y, a_Color);

        // 선 그리기 알고리즘
        if (dx > dy)
        {
            int fraction = dy - (dx >> 1);

            while (x1 != x2)
            {
                if (fraction >= 0)
                {
                    y1 += stepy; // y를 증가
                    fraction -= dx; // fraction 업데이트
                }

                x1 += stepx; // x를 증가
                fraction += dy; // fraction 업데이트

                // 현재 픽셀이 텍스처 영역 내에 있는지 확인하고 색상 설정
                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
        else
        {
            int fraction = dx - (dy >> 1);

            while (y1 != y2)
            {
                if (fraction >= 0)
                {
                    x1 += stepx; // x를 증가
                    fraction -= dy; // fraction 업데이트
                }

                y1 += stepy; // y를 증가
                fraction += dx; // fraction 업데이트

                // 현재 픽셀이 텍스처 영역 내에 있는지 확인하고 색상 설정
                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
    }

    // 행렬을 쿼터니언으로 변환, 미러링을 고려
    private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
    {
        Vector4 vZ = mOrient.GetColumn(2); // Z 축
        Vector4 vY = mOrient.GetColumn(1); // Y 축

        // flip 여부에 따라 방향 조정
        if (!flip)
        {
            vZ.y = -vZ.y; // Y 축 반전
            vY.x = -vY.x; // X 축 반전
            vY.z = -vY.z; // Z 축 반전
        }
        else
        {
            vZ.x = -vZ.x; // X 축 반전
            vZ.y = -vZ.y; // Y 축 반전
            vY.z = -vY.z; // Z 축 반전
        }

        // Z 및 Y 벡터가 유효한 경우 쿼터니언 반환
        if (vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
            return Quaternion.LookRotation(vZ, vY);
        else
            return Quaternion.identity; // 유효한 벡터가 없는 경우 기본 쿼터니언 반환
    }

    // 제스처 리스트에서 제스처의 인덱스를 반환, 찾지 못하면 -1 반환
    private int GetGestureIndex(uint UserId, KinectGestures.Gestures gesture)
    {
        if (UserId == Player1ID)
        {
            int listSize = player1Gestures.Count;
            for (int i = 0; i < listSize; i++)
            {
                if (player1Gestures[i].gesture == gesture)
                    return i; // 인덱스 반환
            }
        }
        else if (UserId == Player2ID)
        {
            int listSize = player2Gestures.Count;
            for (int i = 0; i < listSize; i++)
            {
                if (player2Gestures[i].gesture == gesture)
                    return i; // 인덱스 반환
            }
        }

        return -1; // 제스처를 찾지 못한 경우
    }

    // 진행 중인 제스처가 충돌하는지 확인
    private bool IsConflictingGestureInProgress(KinectGestures.GestureData gestureData)
    {
        foreach (KinectGestures.Gestures gesture in gestureData.checkForGestures)
        {
            int index = GetGestureIndex(gestureData.userId, gesture);

            if (index >= 0)
            {
                if (gestureData.userId == Player1ID)
                {
                    if (player1Gestures[index].progress > 0f)
                        return true; // 충돌 발생
                }
                else if (gestureData.userId == Player2ID)
                {
                    if (player2Gestures[index].progress > 0f)
                        return true; // 충돌 발생
                }
            }
        }

        return false; // 충돌 안 함
    }

    // 주어진 사용자에 대한 보정 포즈가 완료되었는지 확인
    private bool CheckForCalibrationPose(uint userId, ref KinectGestures.Gestures calibrationGesture,
        ref KinectGestures.GestureData gestureData, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
        // 보정 제스처가 없으면 항상 true 반환
        if (calibrationGesture == KinectGestures.Gestures.None)
            return true;

        // 필요 시 제스처 데이터 초기화
        if (gestureData.userId != userId)
        {
            gestureData.userId = userId;
            gestureData.gesture = calibrationGesture;
            gestureData.state = 0;
            gestureData.joint = 0;
            gestureData.progress = 0f;
            gestureData.complete = false;
            gestureData.cancelled = false;
        }

        // 임시 관절 위치 가져오기
        int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        bool[] jointsTracked = new bool[skeletonJointsCount];
        Vector3[] jointsPos = new Vector3[skeletonJointsCount];

        int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
        int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;

        int[] mustBeTrackedJoints = {
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
    };

        // 관절 추적 상태 확인
        for (int j = 0; j < skeletonJointsCount; j++)
        {
            jointsTracked[j] = Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked;

            if (jointsTracked[j])
            {
                jointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]); // 위치 변환
            }
        }

        // 제스처 진행률 추정
        KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup,
            ref jointsPos, ref jointsTracked);

        // 제스처가 완료되었는지 확인
        if (gestureData.complete)
        {
            gestureData.userId = 0; // 사용자 ID 리셋
            return true; // 보정 포즈 완료
        }

        return false; // 보정 포즈 미완료
    }
}