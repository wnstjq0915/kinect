using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;


public class KinectManager : MonoBehaviour
{
	public enum Smoothing : int { None, Default, Medium, Aggressive }
	
	
	// 공개 부리가 얼마나 많은 플레이어가 있는지 결정합니다.
    // 한 사용자의 기본값.
	public bool TwoUsers = false;
	
    // 센서가 가까운 모드에서 사용되는지 확인하기위한 public bool.

	// 사용자지도를 수신하고 계산할지 여부를 결정하기위한 공개 bool
	public bool ComputeUserMap = false;
	
	// 컬러 맵 수신 및 계산 여부를 결정하는 Public Bool
	public bool ComputeColorMap = false;
	
	// GUI에 사용자 맵을 표시할지 여부를 결정하는 Public Bool
	public bool DisplayUserMap = false;
	
	// GUI에 컬러 맵을 표시할지 여부를 결정하는 Public Bool
	public bool DisplayColorMap = false;
	
	// 사용자지도에 골격 라인을 표시할지 여부를 결정하기위한 공개 Bool
	public bool DisplaySkeletonLines = false;
	
	// 공개 플로트는 카메라 너비의 %로 깊이 및 색상 맵으로 사용되는 이미지 너비를 지정합니다.
    // 높이는 너비에 따라 계산됩니다.
	// 백분율이 0이면 선택된 폭과 깊이 이미지의 높이와 일치하도록 내부적으로 계산됩니다.
	public float DisplayMapsWidthPercent = 20f;

	// 센서 (미터)지면에서 얼마나 높은지.
	public float SensorHeight = 1.0f;

	// Kinect 고도 각도 (도)
	public int SensorAngle = 0;
	
	// 골격 데이터를 처리하기 위해 최소 사용자 거리
	public float MinUserDistance = 1.0f;
	
	// 최대 사용자 거리 (있는 경우).
    // 0은 최대 용기 제한이 없음을 의미합니다
	public float MaxUserDistance = 0f;
	
	// 가장 가까운 사용자 만 감지할지 여부를 결정하는 Public Bool
	public bool DetectClosestUser = true;
	
	// 추적 된 조인트 만 사용할지 여부를 결정하기위한 공개 Bool (및 추론 된 조인트를 무시하십시오)
	public bool IgnoreInferredJoints = true;
	
	// 스무딩 매개 변수 선택
	public Smoothing smoothing = Smoothing.Default;
	
	// 추가 필터의 사용을 결정하기위한 공개 bool
	public bool UseBoneOrientationsFilter = false;
	public bool UseClippedLegsFilter = false;
	public bool UseBoneOrientationsConstraint = true;
	public bool UseSelfIntersectionConstraint = false;
	
	// 어떤 플레이어가 제어 할 gameObjects 목록.
	public List<GameObject> Player1Avatars;
	public List<GameObject> Player2Avatars;
	
	// 필요한 경우 각 플레이어에 대한 교정이 있습니다
	public KinectGestures.Gestures Player1CalibrationPose;
	public KinectGestures.Gestures Player2CalibrationPose;
	
	// 각 플레이어에 대해 감지 할 제스처 목록
	public List<KinectGestures.Gestures> Player1Gestures;
	public List<KinectGestures.Gestures> Player2Gestures;
	
	// 제스처 탐지 사이의 최소 시간
	public float MinTimeBetweenGestures = 0.7f;
	
	// 제스처 청취자 목록.
    // kinectustures.gesturelistenerinterface를 구현해야합니다
	public List<MonoBehaviour> GestureListeners;
	
	// 메시지를 표시하는 GUI 텍스트.
	public GUIText CalibrationText;
	
	// GUI 텍스처 플레이어 1의 핸드 커서를 표시하는 GUI 텍스처
	public GameObject HandCursor1;
	
	// GUI 텍스처 플레이어 1의 핸드 커서를 표시하는 GUI 텍스처
	public GameObject HandCursor2;
	
	// BOOL 왼쪽/오른쪽 핸드-셔르 및 클릭 제스처가 마우스 커서를 제어하고 클릭 여부를 지정합니다.
	public bool ControlMouseCursor = false;

	// 제스처 디버그 메시지를 표시하는 GUI 텍스트.
	public GUIText GesturesDebugText;
	

	// Kinect가 초기화되었는지 여부를 추적하기위한 Bool
	private bool KinectInitialized = false; 
	
	// 누가 현재 교정 된 사람을 추적하는 부울.
	private bool Player1Calibrated = false;
	private bool Player2Calibrated = false;
	
	private bool AllPlayersCalibrated = false;
	
	// 어떤 ID (Kinect가 할당 한)를 추적하는 값은 플레이어 1 및 플레이어 2입니다.
	private uint Player1ID;
	private uint Player2ID;
	
	private int Player1Index;
	private int Player2Index;
	
	// 모델을 업데이트 할 수있는 AvatarControllers 목록.
	private List<AvatarController> Player1Controllers;
	private List<AvatarController> Player2Controllers;
	
	// 사용자 맵 vars.
	private Texture2D usersLblTex;
	private Color32[] usersMapColors;
	private ushort[] usersPrevState;
	private Rect usersMapRect;
	private int usersMapSize;

	private Texture2D usersClrTex;
	// Color [] usersclrcolors;
	private Rect usersClrRect;
	
	// 짧은 [] userlabelmap;
	private ushort[] usersDepthMap;
	private float[] usersHistogramMap;
	
	// 모든 사용자 목록
	private List<uint> allUsers;
	
	// Kinect의 이미지 스트림 핸들
	private IntPtr colorStreamHandle;
	private IntPtr depthStreamHandle;
	
	// 사용하는 경우 색상 이미지 데이터
	private Color32[] colorImage;
	private byte[] usersColorMap;
	
	// 골격 관련 구조
	private KinectWrapper.NuiSkeletonFrame skeletonFrame;
	private KinectWrapper.NuiTransformSmoothParameters smoothParameters;
	private int player1Index, player2Index;
	
	// 골격 추적 상태, 위치 및 관절 방향
	private Vector3 player1Pos, player2Pos;
	private Matrix4x4 player1Ori, player2Ori;
	private bool[] player1JointsTracked, player2JointsTracked;
	private bool[] player1PrevTracked, player2PrevTracked;
	private Vector3[] player1JointsPos, player2JointsPos;
	private Matrix4x4[] player1JointsOri, player2JointsOri;
	private KinectWrapper.NuiSkeletonBoneOrientation[] jointOrientations;
	
	// 각 플레이어의 교정 제스처 데이터
	private KinectGestures.GestureData player1CalibrationData;
	private KinectGestures.GestureData player2CalibrationData;
	
	// 각 플레이어에 대한 제스처 데이터 목록
	private List<KinectGestures.GestureData> player1Gestures = new List<KinectGestures.GestureData>();
	private List<KinectGestures.GestureData> player2Gestures = new List<KinectGestures.GestureData>();
	
	// 일반 제스처 추적 시간 시작
	private float[] gestureTrackingAtTime;
	
	// 제스처 청취자 목록.
    // kinectustures.gesturelistenerinterface를 구현해야합니다
	public List<KinectGestures.GestureListenerInterface> gestureListeners;
	
	private Matrix4x4 kinectToWorld, flipMatrix;
	private static KinectManager instance;
	
    // 필터 LERP 블렌드를 제어하기위한 타이머.
    private float lastNuiTime;

	// 필터
	private TrackingStateFilter[] trackingStateFilter;
	private BoneOrientationsFilter[] boneOrientationFilter;
	private ClippedLegsFilter[] clippedLegsFilter;
	private BoneOrientationsConstraint boneConstraintsFilter;
	private SelfIntersectionConstraint selfIntersectionConstraint;
	
	
	// 단일 KinectManager 인스턴스를 반환합니다
    public static KinectManager Instance
    {
        get
        {
            return instance;
        }
    }
	
	// Kinect가 초기화되고 사용할 준비가되었는지 확인합니다.
    // 그렇지 않은 경우 Kinect-Sensor 초기화 중에 오류가 발생했습니다
	public static bool IsKinectInitialized()
	{
		return instance != null ? instance.KinectInitialized : false;
	}
	
	// Kinect가 초기화되고 사용할 준비가되었는지 확인합니다.
    // 그렇지 않은 경우 Kinect-Sensor 초기화 중에 오류가 발생했습니다
	public bool IsInitialized()
	{
		return KinectInitialized;
	}
	
	// 이 기능은 AvatarController에서 내부적으로 사용됩니다
	public static bool IsCalibrationNeeded()
	{
		return false;
	}
	
	// ComputeUsermap이 참이면 원시 깊이/사용자 데이터를 반환합니다
	public ushort[] GetRawDepthMap()
	{
		return usersDepthMap;
	}
	
	// ComputeUsermap이 true 인 경우 특정 픽셀의 깊이 데이터를 반환합니다.
	public ushort GetDepthForPixel(int x, int y)
	{
		int index = y * KinectWrapper.Constants.DepthImageWidth + x;
		
		if(index >= 0 && index < usersDepthMap.Length)
			return usersDepthMap[index];
		else
			return 0;
	}
	
	// 3D 조인트 위치에 대한 깊이 맵 위치를 반환합니다.
	public Vector2 GetDepthMapPosForJointPos(Vector3 posJoint)
	{
		Vector3 vDepthPos = KinectWrapper.MapSkeletonPointToDepthPoint(posJoint);
		Vector2 vMapPos = new Vector2(vDepthPos.x, vDepthPos.y);
		
		return vMapPos;
	}
	
	// 깊이 2D 위치의 컬러 맵 위치를 반환합니다.
	public Vector2 GetColorMapPosForDepthPos(Vector2 posDepth)
	{
		int cx, cy;

		KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea 
		{
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };
		
		KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
			KinectWrapper.Constants.ColorImageResolution,
			KinectWrapper.Constants.DepthImageResolution,
			ref pcViewArea,
			(int)posDepth.x, (int)posDepth.y, GetDepthForPixel((int)posDepth.x, (int)posDepth.y),
			out cx, out cy);
		
		return new Vector2(cx, cy);
	}
	
	// ComputeUsermap이 사실 인 경우 깊이 이미지/사용자 히스토그램 텍스처를 반환합니다
    public Texture2D GetUsersLblTex()
    { 
		return usersLblTex;
	}
	
	// CompuTeColormap이 사실 인 경우 색상 이미지 텍스처를 반환합니다
    public Texture2D GetUsersClrTex()
    { 
		return usersClrTex;
	}
	
	// 한 명 이상의 사용자가 현재 센서에 의해 감지되면 true를 반환합니다.
	public bool IsUserDetected()
	{
		return KinectInitialized && (allUsers.Count > 0);
	}
	
	// Player1의 userId를 반환하거나 Player1이 감지되지 않은 경우 0
	public uint GetPlayer1ID()
	{
		return Player1ID;
	}
	
	// Player2의 userId를 반환하거나 Player2가 감지되지 않은 경우 0
	public uint GetPlayer2ID()
	{
		return Player2ID;
	}
	
	// Player1의 색인을 반환하거나 Player2가 감지되지 않은 경우 0
	public int GetPlayer1Index()
	{
		return Player1Index;
	}
	
	// Player2의 인덱스를 반환하거나 Player2가 감지되지 않은 경우 0
	public int GetPlayer2Index()
	{
		return Player2Index;
	}
	
	// 사용자가 보정되고 사용할 준비가되면 True를 반환합니다.
	public bool IsPlayerCalibrated(uint UserId)
	{
		if(UserId == Player1ID)
			return Player1Calibrated;
		else if(UserId == Player2ID)
			return Player2Calibrated;
		
		return false;
	}
	
	// Kinect 센서가 반환 한 원시 변형 조인트 위치를 반환합니다.
	public Vector3 GetRawSkeletonJointPos(uint UserId, int joint)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player1Index].SkeletonPositions[joint] : Vector3.zero;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player2Index].SkeletonPositions[joint] : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// Kinect-Sensor에 대해 사용자 위치를 미터로 반환합니다.
	public Vector3 GetUserPosition(uint UserId)
	{
		if(UserId == Player1ID)
			return player1Pos;
		else if(UserId == Player2ID)
			return player2Pos;
		
		return Vector3.zero;
	}
	
	// Kinect-Sensor에 대한 사용자 회전을 반환합니다
	public Quaternion GetUserOrientation(uint UserId, bool flip)
	{
		if(UserId == Player1ID && player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
			return ConvertMatrixToQuat(player1Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		else if(UserId == Player2ID && player2JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
			return ConvertMatrixToQuat(player2Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		
		return Quaternion.identity;
	}
	
	// 지정된 사용자의 주어진 조인트가 추적되는 경우 true를 반환합니다.
	public bool IsJointTracked(uint UserId, int joint)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsTracked.Length ? player1JointsTracked[joint] : false;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsTracked.Length ? player2JointsTracked[joint] : false;
		
		return false;
	}
	
	// Kinect-Sensor와 관련하여 지정된 사용자의 공동 위치를 미터로 반환합니다.
	public Vector3 GetJointPosition(uint UserId, int joint)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsPos.Length ? player1JointsPos[joint] : Vector3.zero;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsPos.Length ? player2JointsPos[joint] : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// 지정된 사용자의 로컬 조인트 위치를 모기 관절에 비해 미터로 반환합니다.
	public Vector3 GetJointLocalPosition(uint UserId, int joint)
	{
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsPos.Length ? 
				(player1JointsPos[joint] - player1JointsPos[parent]) : Vector3.zero;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsPos.Length ? 
				(player2JointsPos[joint] - player2JointsPos[parent]) : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// Kinect-Sensor와 관련하여 지정된 사용자의 공동 회전을 반환합니다.
	public Quaternion GetJointOrientation(uint UserId, int joint, bool flip)
	{
		if(UserId == Player1ID)
		{
			if(joint >= 0 && joint < player1JointsOri.Length && player1JointsTracked[joint])
				return ConvertMatrixToQuat(player1JointsOri[joint], joint, flip);
		}
		else if(UserId == Player2ID)
		{
			if(joint >= 0 && joint < player2JointsOri.Length && player2JointsTracked[joint])
				return ConvertMatrixToQuat(player2JointsOri[joint], joint, flip);
		}
		
		return Quaternion.identity;
	}
	
	// 부모 조인트와 관련하여 지정된 사용자의 공동 회전을 반환합니다.
	public Quaternion GetJointLocalOrientation(uint UserId, int joint, bool flip)
	{
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

		if(UserId == Player1ID)
		{
			if(joint >= 0 && joint < player1JointsOri.Length && player1JointsTracked[joint])
			{
				Matrix4x4 localMat = (player1JointsOri[parent].inverse * player1JointsOri[joint]);
				return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
			}
		}
		else if(UserId == Player2ID)
		{
			if(joint >= 0 && joint < player2JointsOri.Length && player2JointsTracked[joint])
			{
				Matrix4x4 localMat = (player2JointsOri[parent].inverse * player2JointsOri[joint]);
				return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
			}
		}
		
		return Quaternion.identity;
	}
	
	// 지정된 사용자를 위해 BaseJoint와 NextJoint 사이의 방향을 반환합니다.
	public Vector3 GetDirectionBetweenJoints(uint UserId, int baseJoint, int nextJoint, bool flipX, bool flipZ)
	{
		Vector3 jointDir = Vector3.zero;
		
		if(UserId == Player1ID)
		{
			if(baseJoint >= 0 && baseJoint < player1JointsPos.Length && player1JointsTracked[baseJoint] &&
				nextJoint >= 0 && nextJoint < player1JointsPos.Length && player1JointsTracked[nextJoint])
			{
				jointDir = player1JointsPos[nextJoint] - player1JointsPos[baseJoint];
			}
		}
		else if(UserId == Player2ID)
		{
			if(baseJoint >= 0 && baseJoint < player2JointsPos.Length && player2JointsTracked[baseJoint] &&
				nextJoint >= 0 && nextJoint < player2JointsPos.Length && player2JointsTracked[nextJoint])
			{
				jointDir = player2JointsPos[nextJoint] - player2JointsPos[baseJoint];
			}
		}
		
		if(jointDir != Vector3.zero)
		{
			if(flipX)
				jointDir.x = -jointDir.x;
			
			if(flipZ)
				jointDir.z = -jointDir.z;
		}
		
		return jointDir;
	}
	
	// 지정된 사용자를위한 감지 된 제스처 목록에 제스처를 추가합니다.
	public void DetectGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index >= 0)
			DeleteGesture(UserId, gesture);
		
		KinectGestures.GestureData gestureData = new KinectGestures.GestureData();
		
		gestureData.userId = UserId;
		gestureData.gesture = gesture;
		gestureData.state = 0;
		gestureData.joint = 0;
		gestureData.progress = 0f;
		gestureData.complete = false;
		gestureData.cancelled = false;
		
		gestureData.checkForGestures = new List<KinectGestures.Gestures>();
		switch(gesture)
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
		
		if(UserId == Player1ID)
			player1Gestures.Add(gestureData);
		else if(UserId == Player2ID)
			player2Gestures.Add(gestureData);
	}
	
	// 지정된 사용자의 주어진 제스처에 대해 제스처 데이터 상태를 재설정
	public bool ResetGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index < 0)
			return false;
		
		KinectGestures.GestureData gestureData = (UserId == Player1ID) ? player1Gestures[index] : player2Gestures[index];
		
		gestureData.state = 0;
		gestureData.joint = 0;
		gestureData.progress = 0f;
		gestureData.complete = false;
		gestureData.cancelled = false;
		gestureData.startTrackingAtTime = Time.realtimeSinceStartup + KinectWrapper.Constants.MinTimeBetweenSameGestures;

		if(UserId == Player1ID)
			player1Gestures[index] = gestureData;
		else if(UserId == Player2ID)
			player2Gestures[index] = gestureData;
		
		return true;
	}
	
	// 지정된 사용자의 모든 감지 된 제스처에 대해 제스처 데이터 상태를 재설정
	public void ResetPlayerGestures(uint UserId)
	{
		if(UserId == Player1ID)
		{
			int listSize = player1Gestures.Count;
			
			for(int i = 0; i < listSize; i++)
			{
				ResetGesture(UserId, player1Gestures[i].gesture);
			}
		}
		else if(UserId == Player2ID)
		{
			int listSize = player2Gestures.Count;
			
			for(int i = 0; i < listSize; i++)
			{
				ResetGesture(UserId, player2Gestures[i].gesture);
			}
		}
	}
	
	// 지정된 사용자에 대한 감지 된 제스처 목록에서 주어진 제스처를 삭제합니다.
	public bool DeleteGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index < 0)
			return false;
		
		if(UserId == Player1ID)
			player1Gestures.RemoveAt(index);
		else if(UserId == Player2ID)
			player2Gestures.RemoveAt(index);
		
		return true;
	}
	
	// 지정된 사용자의 감지 된 제스처 목록을 지 웁니다
	public void ClearGestures(uint UserId)
	{
		if(UserId == Player1ID)
		{
			player1Gestures.Clear();
		}
		else if(UserId == Player2ID)
		{
			player2Gestures.Clear();
		}
	}
	
	// 지정된 사용자에 대한 감지 된 제스처 목록에서 감지 된 제스처 수를 반환합니다.
	public int GetGesturesCount(uint UserId)
	{
		if(UserId == Player1ID)
			return player1Gestures.Count;
		else if(UserId == Player2ID)
			return player2Gestures.Count;
		
		return 0;
	}
	
	// 지정된 사용자에 대한 감지 된 제스처 목록을 반환합니다.
	public List<KinectGestures.Gestures> GetGesturesList(uint UserId)
	{
		List<KinectGestures.Gestures> list = new List<KinectGestures.Gestures>();

		if(UserId == Player1ID)
		{
			foreach(KinectGestures.GestureData data in player1Gestures)
				list.Add(data.gesture);
		}
		else if(UserId == Player2ID)
		{
			foreach(KinectGestures.GestureData data in player1Gestures)
				list.Add(data.gesture);
		}
		
		return list;
	}
	
	// 주어진 제스처가 지정된 사용자에 대한 감지 된 제스처 목록에있는 경우 true를 반환합니다.
	public bool IsGestureDetected(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		return index >= 0;
	}
	
	// 지정된 사용자에 대한 주어진 제스처가 완료되면 true를 반환합니다.
	public bool IsGestureComplete(uint UserId, KinectGestures.Gestures gesture, bool bResetOnComplete)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == Player1ID)
			{
				KinectGestures.GestureData gestureData = player1Gestures[index];
				
				if(bResetOnComplete && gestureData.complete)
				{
					ResetPlayerGestures(UserId);
					return true;
				}
				
				return gestureData.complete;
			}
			else if(UserId == Player2ID)
			{
				KinectGestures.GestureData gestureData = player2Gestures[index];

				if(bResetOnComplete && gestureData.complete)
				{
					ResetPlayerGestures(UserId);
					return true;
				}
				
				return gestureData.complete;
			}
		}
		
		return false;
	}
	
	// 지정된 사용자에 대한 주어진 제스처가 취소되면 true를 반환합니다.
	public bool IsGestureCancelled(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == Player1ID)
			{
				KinectGestures.GestureData gestureData = player1Gestures[index];
				return gestureData.cancelled;
			}
			else if(UserId == Player2ID)
			{
				KinectGestures.GestureData gestureData = player2Gestures[index];
				return gestureData.cancelled;
			}
		}
		
		return false;
	}
	
	// 지정된 사용자에 대한 주어진 제스처의 범위 [0, 1] 범위에서 진행 상황을 반환합니다.
	public float GetGestureProgress(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == Player1ID)
			{
				KinectGestures.GestureData gestureData = player1Gestures[index];
				return gestureData.progress;
			}
			else if(UserId == Player2ID)
			{
				KinectGestures.GestureData gestureData = player2Gestures[index];
				return gestureData.progress;
			}
		}
		
		return 0f;
	}
	
	// 지정된 사용자에 대한 주어진 제스처의 현재 "화면 위치"를 반환합니다.
	public Vector3 GetGestureScreenPos(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == Player1ID)
			{
				KinectGestures.GestureData gestureData = player1Gestures[index];
				return gestureData.screenPos;
			}
			else if(UserId == Player2ID)
			{
				KinectGestures.GestureData gestureData = player2Gestures[index];
				return gestureData.screenPos;
			}
		}
		
		return Vector3.zero;
	}
	
	// 제스처 청취자의 내부 목록을 재창조하고 재개합니다
	public void ResetGestureListeners()
	{
		// 제스처 청취자 목록을 만듭니다
		gestureListeners.Clear();
		
		foreach(MonoBehaviour script in GestureListeners)
		{
			if(script && (script is KinectGestures.GestureListenerInterface))
			{
				KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
				gestureListeners.Add(listener);
			}
		}
		
	}
	
	// 플레이어 1/2를위한 아바타 목록이 변경된 후 아바타 컨트롤러 목록을 재현하고 재개합니다.
	public void ResetAvatarControllers()
	{
		if(Player1Avatars.Count == 0 && Player2Avatars.Count == 0)
		{
			AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];
			
			foreach(AvatarController avatar in avatars)
			{
				Player1Avatars.Add(avatar.gameObject);
			}
		}
		
		if(Player1Controllers != null)
		{
			Player1Controllers.Clear();
	
			foreach(GameObject avatar in Player1Avatars)
			{
				if(avatar != null && avatar.activeInHierarchy)
				{
					AvatarController controller = avatar.GetComponent<AvatarController>();
					controller.ResetToInitialPosition();
					controller.Awake();
					
					Player1Controllers.Add(controller);
				}
			}
		}
		
		if(Player2Controllers != null)
		{
			Player2Controllers.Clear();
			
			foreach(GameObject avatar in Player2Avatars)
			{
				if(avatar != null && avatar.activeInHierarchy)
				{
					AvatarController controller = avatar.GetComponent<AvatarController>();
					controller.ResetToInitialPosition();
					controller.Awake();
					
					Player2Controllers.Add(controller);
				}
			}
		}
	}
	
	// 현재 감지 된 Kinect 사용자를 제거하여 새 감지/교정 프로세스가 시작될 수 있습니다.
	public void ClearKinectUsers()
	{
		if(!KinectInitialized)
			return;

		// 현재 사용자를 제거하십시오
		for(int i = allUsers.Count - 1; i >= 0; i--)
		{
			uint userId = allUsers[i];
			RemoveUser(userId);
		}
		
		ResetFilters();
	}
	
	// Kinect 버퍼를 지우고 필터를 재설정합니다
	public void ResetFilters()
	{
		if(!KinectInitialized)
			return;
		
		// Kinect Vars를 클리어합니다
		player1Pos = Vector3.zero; player2Pos = Vector3.zero;
		player1Ori = Matrix4x4.identity; player2Ori = Matrix4x4.identity;
		
		int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		for(int i = 0; i < skeletonJointsCount; i++)
		{
			player1JointsTracked[i] = false; player2JointsTracked[i] = false;
			player1PrevTracked[i] = false; player2PrevTracked[i] = false;
			player1JointsPos[i] = Vector3.zero; player2JointsPos[i] = Vector3.zero;
			player1JointsOri[i] = Matrix4x4.identity; player2JointsOri[i] = Matrix4x4.identity;
		}
		
		if(trackingStateFilter != null)
		{
			for(int i = 0; i < trackingStateFilter.Length; i++)
				if(trackingStateFilter[i] != null)
					trackingStateFilter[i].Reset();
		}
		
		if(boneOrientationFilter != null)
		{
			for(int i = 0; i < boneOrientationFilter.Length; i++)
				if(boneOrientationFilter[i] != null)
					boneOrientationFilter[i].Reset();
		}
		
		if(clippedLegsFilter != null)
		{
			for(int i = 0; i < clippedLegsFilter.Length; i++)
				if(clippedLegsFilter[i] != null)
					clippedLegsFilter[i].Reset();
		}
	}
	
	
	// ------------------------------------- 공공 기능의 끝 -------------------------------------- //

	void Awake()
	{
		int hr = 0;
		
		try
		{
			hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton |
				KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex |
				(ComputeColorMap ? KinectWrapper.NuiInitializeFlags.UsesColor : 0));
            if (hr != 0)
			{
            	throw new Exception("NuiInitialize Failed");
			}
			
			hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);  // 0, 12,8
			if (hr != 0)
			{
				throw new Exception("Cannot initialize Skeleton Data");
			}
			
			depthStreamHandle = IntPtr.Zero;
			if(ComputeUserMap)
			{
				hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex, 
					KinectWrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref depthStreamHandle);
				if (hr != 0)
				{
					throw new Exception("Cannot open depth stream");
				}
			}
			
			colorStreamHandle = IntPtr.Zero;
			if(ComputeColorMap)
			{
				hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, 
					KinectWrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
				if (hr != 0)
				{
					throw new Exception("Cannot open color stream");
				}
			}

			// Kinect 고도 각도를 설정하십시오
			KinectWrapper.NuiCameraElevationSetAngle(SensorAngle);
			
			// 골격 구조 시작
			skeletonFrame = new KinectWrapper.NuiSkeletonFrame() 
							{ 
								SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount] 
							};
			
			// 스무딩 기능으로 전달하는 데 사용되는 값
			smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();
			
			switch(smoothing)
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
			
			// 추적 상태 필터를 시작하십시오
			trackingStateFilter = new TrackingStateFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < trackingStateFilter.Length; i++)
			{
				trackingStateFilter[i] = new TrackingStateFilter();
				trackingStateFilter[i].Init();
			}
			
			// 뼈 방향 필터를 시작하십시오
			boneOrientationFilter = new BoneOrientationsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < boneOrientationFilter.Length; i++)
			{
				boneOrientationFilter[i] = new BoneOrientationsFilter();
				boneOrientationFilter[i].Init();
			}
			
			// 잘린 다리 필터를 시작하십시오
			clippedLegsFilter = new ClippedLegsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < clippedLegsFilter.Length; i++)
			{
				clippedLegsFilter[i] = new ClippedLegsFilter();
			}

			// 뼈 방향 제약을 시작하십시오
			boneConstraintsFilter = new BoneOrientationsConstraint();
			boneConstraintsFilter.AddDefaultConstraints();
			// 자체 교차로 제약을 시작하십시오
			selfIntersectionConstraint = new SelfIntersectionConstraint();
			
			// 조인트 위치 및 공동 방향을위한 배열을 만듭니다
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
			
			// Kinect 공간에서 월드 공간으로 변환하는 변환 매트릭스 생성
			Quaternion quatTiltAngle = new Quaternion();
			quatTiltAngle.eulerAngles = new Vector3(-SensorAngle, 0.0f, 0.0f);
			
			// Float HeightaboveHips = SensorHeight -1.0f;
			
			// 변환 매트릭스 - Kinect를 세계로
			kinectToWorld.SetTRS(new Vector3(0.0f, SensorHeight, 0.0f), quatTiltAngle, Vector3.one);
			flipMatrix = Matrix4x4.identity;
			flipMatrix[2, 2] = -1;
			
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		catch(DllNotFoundException e)
		{
			string message = "Please check the Kinect SDK installation.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(CalibrationText != null)
				CalibrationText.GetComponent<GUIText>().text = message;
				
			return;
		}
		catch (Exception e)
		{
			string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(CalibrationText != null)
				CalibrationText.GetComponent<GUIText>().text = message;
				
			return;
		}
		
		if(ComputeUserMap)
		{
	        // 깊이 및 레이블 맵 관련 제품을 초기화합니다
	        usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
	        usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
	        usersMapColors = new Color32[usersMapSize];
			usersPrevState = new ushort[usersMapSize];

	        usersDepthMap = new ushort[usersMapSize];
	        usersHistogramMap = new float[8192];
		}
		
		if(ComputeColorMap)
		{
			// 컬러 맵 관련 물건을 초기화하십시오
	        usersClrTex = new Texture2D(KinectWrapper.GetColorWidth(), KinectWrapper.GetColorHeight());

			colorImage = new Color32[KinectWrapper.GetColorWidth() * KinectWrapper.GetColorHeight()];
			usersColorMap = new byte[colorImage.Length << 2];
		}
		
		// 장면에서 사용 가능한 아바타 컨트롤러를 자동으로 찾으십시오.
		if(Player1Avatars.Count == 0 && Player2Avatars.Count == 0)
		{
			AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];
			
			foreach(AvatarController avatar in avatars)
			{
				Player1Avatars.Add(avatar.gameObject);
			}
		}
		
        // 모든 사용자를 포함하도록 사용자 목록을 초기화하십시오.
        allUsers = new List<uint>();
        
		// 각 플레이어 아바타에서 아바타 콘 트롤러를 당깁니다.
		Player1Controllers = new List<AvatarController>();
		Player2Controllers = new List<AvatarController>();
		
		// 각 아바타 컨트롤러를 각 플레이어의 목록에 추가하십시오.
		foreach(GameObject avatar in Player1Avatars)
		{
			if(avatar != null && avatar.activeInHierarchy)
			{
				Player1Controllers.Add(avatar.GetComponent<AvatarController>());
			}
		}
		
		foreach(GameObject avatar in Player2Avatars)
		{
			if(avatar != null && avatar.activeInHierarchy)
			{
				Player2Controllers.Add(avatar.GetComponent<AvatarController>());
			}
		}
		
		// 제스처 청취자 목록을 만듭니다
		gestureListeners = new List<KinectGestures.GestureListenerInterface>();
		
		foreach(MonoBehaviour script in GestureListeners)
		{
			if(script && (script is KinectGestures.GestureListenerInterface))
			{
				KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
				gestureListeners.Add(listener);
			}
		}
		
		// GUI 텍스트.
		if(CalibrationText != null)
		{
			CalibrationText.GetComponent<GUIText>().text = "WAITING FOR USERS";
		}
		
		Debug.Log("Waiting for users.");
			
		KinectInitialized = true;
	}
	
	void Update()
	{
		if(KinectInitialized)
		{
			// 다음 프레임을 확인하기 위해 Kinecextras의 기본 래퍼가 필요합니다.
			// Extras '래퍼를 사용하는 경우 아래 줄을 타협하지만 Extras'관리자는 없습니다.
			
	        // 플레이어가 아직 교정되지 않은 경우 사용자 맵을 그립니다.
			if(ComputeUserMap)
			{
				if(depthStreamHandle != IntPtr.Zero &&
					KinectWrapper.PollDepth(depthStreamHandle, KinectWrapper.Constants.IsNearMode, ref usersDepthMap))
				{
		        	UpdateUserMap();
				}
			}
			
			if(ComputeColorMap)
			{
				if(colorStreamHandle != IntPtr.Zero &&
					KinectWrapper.PollColor(colorStreamHandle, ref usersColorMap, ref colorImage))
				{
					UpdateColorMap();
				}
			}
			
			if(KinectWrapper.PollSkeleton(ref smoothParameters, ref skeletonFrame))
			{
				ProcessSkeleton();
			}
			
			// 플레이어 1의 모델이 교정되고 모델이 활성화되면 업데이트하십시오.
			if(Player1Calibrated)
			{
				foreach (AvatarController controller in Player1Controllers)
				{
					// if (Controller.Active)
					{
						controller.UpdateAvatar(Player1ID);
					}
				}
					
				// 완전한 제스처를 확인하십시오
				foreach(KinectGestures.GestureData gestureData in player1Gestures)
				{
					if(gestureData.complete)
					{
						if(gestureData.gesture == KinectGestures.Gestures.Click)
						{
							if(ControlMouseCursor)
							{
								MouseControl.MouseClick();
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							if(listener.GestureCompleted(Player1ID, 0, gestureData.gesture, 
							                             (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos))
							{
								ResetPlayerGestures(Player1ID);
							}
						}
					}
					else if(gestureData.cancelled)
					{
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							if(listener.GestureCancelled(Player1ID, 0, gestureData.gesture, 
							                             (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint))
							{
								ResetGesture(Player1ID, gestureData.gesture);
							}
						}
					}
					else if(gestureData.progress >= 0.1f)
					{
						if((gestureData.gesture == KinectGestures.Gestures.RightHandCursor || 
							gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) && 
							gestureData.progress >= 0.5f)
						{
							if(GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
							{
								if(HandCursor1 != null)
								{
									Vector3 vCursorPos = gestureData.screenPos;
									
									if(HandCursor1.GetComponent<GUITexture>() == null)
									{
										float zDist = HandCursor1.transform.position.z - Camera.main.transform.position.z;
										vCursorPos.z = zDist;
										
										vCursorPos = Camera.main.ViewportToWorldPoint(vCursorPos);
									}

									HandCursor1.transform.position = Vector3.Lerp(HandCursor1.transform.position, vCursorPos, 3 * Time.deltaTime);
								}
								
								if(ControlMouseCursor)
								{
									Vector3 vCursorPos = HandCursor1.GetComponent<GUITexture>() != null ? HandCursor1.transform.position :
										Camera.main.WorldToViewportPoint(HandCursor1.transform.position);
									MouseControl.MouseMove(vCursorPos, CalibrationText);
								}
							}
						}
			
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							listener.GestureInProgress(Player1ID, 0, gestureData.gesture, gestureData.progress, 
							                           (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos);
						}
					}
				}
			}
			
			// 플레이어 2의 모델을 보정하고 모델이 활성화되면 업데이트하십시오.
			if(Player2Calibrated)
			{
				foreach (AvatarController controller in Player2Controllers)
				{
					{
						controller.UpdateAvatar(Player2ID);
					}
				}

				// 완전한 제스처를 확인하십시오
				foreach(KinectGestures.GestureData gestureData in player2Gestures)
				{
					if(gestureData.complete)
					{
						if(gestureData.gesture == KinectGestures.Gestures.Click)
						{
							if(ControlMouseCursor)
							{
								MouseControl.MouseClick();
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							if(listener.GestureCompleted(Player2ID, 1, gestureData.gesture, 
							                             (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos))
							{
								ResetPlayerGestures(Player2ID);
							}
						}
					}
					else if(gestureData.cancelled)
					{
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							if(listener.GestureCancelled(Player2ID, 1, gestureData.gesture, 
							                             (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint))
							{
								ResetGesture(Player2ID, gestureData.gesture);
							}
						}
					}
					else if(gestureData.progress >= 0.1f)
					{
						if((gestureData.gesture == KinectGestures.Gestures.RightHandCursor || 
							gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) && 
							gestureData.progress >= 0.5f)
						{
							if(GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
							{
								if(HandCursor2 != null)
								{
									Vector3 vCursorPos = gestureData.screenPos;
									
									if(HandCursor2.GetComponent<GUITexture>() == null)
									{
										float zDist = HandCursor2.transform.position.z - Camera.main.transform.position.z;
										vCursorPos.z = zDist;
										
										vCursorPos = Camera.main.ViewportToWorldPoint(vCursorPos);
									}
									
									HandCursor2.transform.position = Vector3.Lerp(HandCursor2.transform.position, vCursorPos, 3 * Time.deltaTime);
								}
								
								if(ControlMouseCursor)
								{
									Vector3 vCursorPos = HandCursor2.GetComponent<GUITexture>() != null ? HandCursor2.transform.position :
										Camera.main.WorldToViewportPoint(HandCursor2.transform.position);
									MouseControl.MouseMove(vCursorPos, CalibrationText);
								}
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
						{
							listener.GestureInProgress(Player2ID, 1, gestureData.gesture, gestureData.progress, 
							                           (KinectWrapper.NuiSkeletonPositionIndex)gestureData.joint, gestureData.screenPos);
						}
					}
				}
			}
		}
		
		// ESC로 프로그램을 죽입니다.
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
	
	// 금연시 Kinect를 죽이십시오.
	void OnApplicationQuit()
	{
		if(KinectInitialized)
		{
			// 종료 오픈 니
			KinectWrapper.NuiShutdown();
			instance = null;
		}
	}
	
	// GUI에 히스토그램 맵을 그립니다.
    void OnGUI()
    {
		if(KinectInitialized)
		{
	        if(ComputeUserMap && (/**(allUsers.Count == 0) ||*/ DisplayUserMap))
	        {
				if(usersMapRect.width == 0 || usersMapRect.height == 0)
				{
					// 메인 카메라 사각형을 얻으십시오
					Rect cameraRect = Camera.main.pixelRect;
					
					// 필요한 경우 맵 너비와 높이를 퍼센트로 계산하십시오
					if(DisplayMapsWidthPercent == 0f)
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

			else if(ComputeColorMap && (DisplayColorMap))
			{
				if(usersClrRect.width == 0 || usersClrTex.height == 0)
				{
					// 메인 카메라 사각형을 얻으십시오
					Rect cameraRect = Camera.main.pixelRect;
					
					// 필요한 경우 맵 너비와 높이를 퍼센트로 계산하십시오
					if(DisplayMapsWidthPercent == 0f)
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
	
	// 사용자 맵을 업데이트하십시오
    void UpdateUserMap()
    {
        int numOfPoints = 0;
		Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // 깊이에 대한 누적 히스토그램을 계산하십시오
        for (int i = 0; i < usersMapSize; i++)
        {
            // 사용자가 포함 된 깊이에 대해서만 계산합니다
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
		
		// 좌표 맵퍼에 필요한 더미 구조
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
	        // 레이블 맵을 색상 배열로 변환하면서 텍스처를 뒤집습니다.
            int flipIndex = i; // usersMapSize - i - 1;
			
			ushort userMap = (ushort)(usersDepthMap[i] & 7);
			ushort userDepth = (ushort)(usersDepthMap[i] >> 3);
			
			ushort nowUserPixel = userMap != 0 ? (ushort)((userMap << 13) | userDepth) : userDepth;
			ushort wasUserPixel = usersPrevState[flipIndex];
			
			// 변경된 픽셀 만 그립니다
			if(nowUserPixel != wasUserPixel)
			{
				usersPrevState[flipIndex] = nowUserPixel;
				
	            if (userMap == 0)
	            {
	                usersMapColors[flipIndex] = clrClear;
	            }
	            else
	            {
					if(colorImage != null)
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
						
						if(hr == 0)
						{
							int colorIndex = cx + cy * KinectWrapper.Constants.ColorImageWidth;
							// ColorIndex = userSmapsize -ColorIndex -1;
							if(colorIndex >= 0 && colorIndex < usersMapSize)
							{
								Color32 colorPixel = colorImage[colorIndex];
								usersMapColors[flipIndex] = colorPixel;  // new Color(colorPixel.r / 256f, colorPixel.g / 256f, colorPixel.b / 256f, 0.9f);
								usersMapColors[flipIndex].a = 230; // 0.9f
							}
						}
					}
					else
					{
		                // 깊이 히스토그램을 기반으로 블렌딩 색상을 만듭니다
						float histDepth = usersHistogramMap[userDepth];
		                Color c = new Color(histDepth, histDepth, histDepth, 0.9f);
		                
						switch(userMap % 4)
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
		
		// 그것을 그리세요!
        usersLblTex.SetPixels32(usersMapColors);

		if(!DisplaySkeletonLines)
		{
			usersLblTex.Apply();
		}
	}
	
	// 컬러 맵을 업데이트하십시오
	void UpdateColorMap()
	{
        usersClrTex.SetPixels32(colorImage);
        usersClrTex.Apply();
	}
	
	// userID를 플레이어 1 또는 2에 할당하십시오.
    void CalibrateUser(uint UserId, int UserIndex, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
		// 플레이어 1이 교정되지 않은 경우 해당 userID를 할당하십시오.
		if(!Player1Calibrated)
		{
			// 실수로 플레이어 2를 플레이어 1에 할당하지 않도록 확인하십시오.
			if (!allUsers.Contains(UserId))
			{
				if(CheckForCalibrationPose(UserId, ref Player1CalibrationPose, ref player1CalibrationData, ref skeletonData))
				{
					Player1Calibrated = true;
					Player1ID = UserId;
					Player1Index = UserIndex;
					
					allUsers.Add(UserId);
					
					foreach(AvatarController controller in Player1Controllers)
					{
						controller.SuccessfulCalibration(UserId);
					}
	
					// 제스처를 추가하여 검출하십시오
					foreach(KinectGestures.Gestures gesture in Player1Gestures)
					{
						DetectGesture(UserId, gesture);
					}
					
					// 새로운 사용자에 대해 제스처 리스너에게 알리십시오
					foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
					{
						listener.UserDetected(UserId, 0);
					}
					
					// 골격 필터를 재설정하십시오
					ResetFilters();
					
					// 우리가 2 명의 사용자를 사용하지 않는 경우 모두 교정됩니다.
					// if (! twousers)
					{
						AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2; // true;
					}
				}
			}
		}
		// 그렇지 않으면 플레이어 2에 할당하십시오.
		else if(TwoUsers && !Player2Calibrated)
		{
			if (!allUsers.Contains(UserId))
			{
				if(CheckForCalibrationPose(UserId, ref Player2CalibrationPose, ref player2CalibrationData, ref skeletonData))
				{
					Player2Calibrated = true;
					Player2ID = UserId;
					Player2Index = UserIndex;
					
					allUsers.Add(UserId);
					
					foreach(AvatarController controller in Player2Controllers)
					{
						controller.SuccessfulCalibration(UserId);
					}
					
					// 제스처를 추가하여 검출하십시오
					foreach(KinectGestures.Gestures gesture in Player2Gestures)
					{
						DetectGesture(UserId, gesture);
					}
					
					// 새로운 사용자에 대해 제스처 리스너에게 알리십시오
					foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
					{
						listener.UserDetected(UserId, 1);
					}
					
					// 골격 필터를 재설정하십시오
					ResetFilters();
					
					// 모든 사용자가 교정됩니다!
					AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2; // true;
				}
			}
		}
		
		// 모든 사용자가 보정되면 찾아보십시오.
		if(AllPlayersCalibrated)
		{
			Debug.Log("All players calibrated.");
			
			if(CalibrationText != null)
			{
				CalibrationText.GetComponent<GUIText>().text = "";
			}
		}
    }
	
	// 잃어버린 userID를 제거하십시오
	void RemoveUser(uint UserId)
	{
		// 선수 1을 잃으면 ...
		if(UserId == Player1ID)
		{
			// ID를 무효화하고 해당 ID와 관련된 모든 모델을 재설정하십시오.
			Player1ID = 0;
			Player1Index = 0;
			Player1Calibrated = false;
			
			foreach(AvatarController controller in Player1Controllers)
			{
				controller.ResetToInitialPosition();
			}
			
			foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
			{
				listener.UserLost(UserId, 0);
			}
			
			player1CalibrationData.userId = 0;
		}
		
		// 선수 2를 잃으면 ...
		if(UserId == Player2ID)
		{
			// ID를 무효화하고 해당 ID와 관련된 모든 모델을 재설정하십시오.
			Player2ID = 0;
			Player2Index = 0;
			Player2Calibrated = false;
			
			foreach(AvatarController controller in Player2Controllers)
			{
				controller.ResetToInitialPosition();
			}
			
			foreach(KinectGestures.GestureListenerInterface listener in gestureListeners)
			{
				listener.UserLost(UserId, 1);
			}
			
			player2CalibrationData.userId = 0;
		}
		
		// 이 사용자를위한 명확한 제스처 목록
		ClearGestures(UserId);

        // 이 사용자를위한 명확한 제스처 목록
        allUsers.Remove(UserId);
		AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2; // false;
		
		// 이 사용자를위한 명확한 제스처 목록
		Debug.Log("Waiting for users.");

		if(CalibrationText != null)
		{
			CalibrationText.GetComponent<GUIText>().text = "WAITING FOR USERS";
		}
	}
	
	// 일부 내부 상수
	private const int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
	private const int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;
	
	private int [] mustBeTrackedJoints = { 
		(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
		(int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
		(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
		(int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
	};
	
	// 골격 데이터를 처리하십시오
	void ProcessSkeleton()
	{
		List<uint> lostUsers = new List<uint>();
		lostUsers.AddRange(allUsers);
		
		// 마지막 업데이트 이후 시간을 계산하십시오
		float currentNuiTime = Time.realtimeSinceStartup;
		float deltaNuiTime = currentNuiTime - lastNuiTime;
		
		for(int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
		{
			KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
			uint userId = skeletonData.dwTrackingID;
			
			if(skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
			{
				// 골격 위치를 얻으십시오
				Vector3 skeletonPos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
				
				if(!AllPlayersCalibrated)
				{
					// 이것이 가장 가까운 사용자인지 확인하십시오
					bool bClosestUser = true;
					
					if(DetectClosestUser)
					{
						for(int j = 0; j < KinectWrapper.Constants.NuiSkeletonCount; j++)
						{
							if(j != i)
							{
								KinectWrapper.NuiSkeletonData skeletonDataOther = skeletonFrame.SkeletonData[j];
								
								if((skeletonDataOther.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked) &&
									(Mathf.Abs(kinectToWorld.MultiplyPoint3x4(skeletonDataOther.Position).z) < Mathf.Abs(skeletonPos.z)))
								{
									bClosestUser = false;
									break;
								}
							}
						}
					}
					
					if(bClosestUser)
					{
						CalibrateUser(userId, i + 1, ref skeletonData);
					}
				}

				// 관절 방향을 얻습니다
				
				if(userId == Player1ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
				   (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
				{
					player1Index = i;

					// 플레이어 위치를 얻으십시오
					player1Pos = skeletonPos;
					
					// 먼저 추적 상태 필터를 적용하십시오
					trackingStateFilter[0].UpdateFilter(ref skeletonData);
					
					// 아바타 외관을 개선하기위한 고정 골격.
					if(UseClippedLegsFilter && clippedLegsFilter[0] != null)
					{
						clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);
					}
	
					if(UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
					{
						selfIntersectionConstraint.Constrain(ref skeletonData);
					}
	
					// 관절의 위치와 회전을 얻습니다
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						bool playerTracked = IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
						player1JointsTracked[j] = player1PrevTracked[j] && playerTracked;
						player1PrevTracked[j] = playerTracked;
						
						if(player1JointsTracked[j])
						{
							player1JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
						}
						
					}
					
					// 질감 위에 골격을 그립니다
					if(DisplaySkeletonLines && ComputeUserMap)
					{
						DrawSkeleton(usersLblTex, ref skeletonData, ref player1JointsTracked);
						usersLblTex.Apply();
					}
					
					// 관절 방향을 계산합니다
					KinectWrapper.GetSkeletonJointOrientation(ref player1JointsPos, ref player1JointsTracked, ref player1JointsOri);
					
					// 필터 방향 제약 조건
					if(UseBoneOrientationsConstraint && boneConstraintsFilter != null)
					{
						boneConstraintsFilter.Constrain(ref player1JointsOri, ref player1JointsTracked);
					}
					
                    // 공동 방향을 필터링하십시오.
                    // 모든 공동 위치 수정 후에 수행해야합니다.
	                if(UseBoneOrientationsFilter && boneOrientationFilter[0] != null)
	                {
	                    boneOrientationFilter[0].UpdateFilter(ref skeletonData, ref player1JointsOri);
	                }
	
					// 플레이어 로테이션을 받으십시오
					player1Ori = player1JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
					
					// 제스처를 확인하십시오
					if(Time.realtimeSinceStartup >= gestureTrackingAtTime[0])
					{
						int listGestureSize = player1Gestures.Count;
						float timestampNow = Time.realtimeSinceStartup;
						string sDebugGestures = string.Empty;  // "Tracked Gestures:\n";

						for(int g = 0; g < listGestureSize; g++)
						{
							KinectGestures.GestureData gestureData = player1Gestures[g];
							
							if((timestampNow >= gestureData.startTrackingAtTime) && 
								!IsConflictingGestureInProgress(gestureData))
							{
								KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
									ref player1JointsPos, ref player1JointsTracked);
								player1Gestures[g] = gestureData;

								if(gestureData.complete)
								{
									gestureTrackingAtTime[0] = timestampNow + MinTimeBetweenGestures;
								}

								{
									sDebugGestures += string.Format("{0} - state: {1}, time: {2:F1}, progress: {3}%\n", 
									                            	gestureData.gesture, gestureData.state, 
									                                gestureData.timestamp,
									                            	(int)(gestureData.progress * 100 + 0.5f));
								}
							}
						}

						if(GesturesDebugText)
						{
							sDebugGestures += string.Format("\n HandLeft: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft].ToString() : "");
							sDebugGestures += string.Format("\n HandRight: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight].ToString() : "");
							sDebugGestures += string.Format("\n ElbowLeft: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft].ToString() : "");
							sDebugGestures += string.Format("\n ElbowRight: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight].ToString() : "");

							sDebugGestures += string.Format("\n ShoulderLeft: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft].ToString() : "");
							sDebugGestures += string.Format("\n ShoulderRight: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight].ToString() : "");
							
							sDebugGestures += string.Format("\n Neck: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter].ToString() : "");
							sDebugGestures += string.Format("\n Hips: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter].ToString() : "");
							sDebugGestures += string.Format("\n HipLeft: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft].ToString() : "");
							sDebugGestures += string.Format("\n HipRight: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight] ?
							                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight].ToString() : "");

							GesturesDebugText.GetComponent<GUIText>().text = sDebugGestures;
						}
					}
				}
				else if(userId == Player2ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
				        (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
				{
					player2Index = i;

					// 플레이어 위치를 얻으십시오
					player2Pos = skeletonPos;
					
					// 먼저 추적 상태 필터를 적용하십시오
					trackingStateFilter[1].UpdateFilter(ref skeletonData);
					
					// 아바타 외관을 개선하기위한 고정 골격.
					if(UseClippedLegsFilter && clippedLegsFilter[1] != null)
					{
						clippedLegsFilter[1].FilterSkeleton(ref skeletonData, deltaNuiTime);
					}
	
					if(UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
					{
						selfIntersectionConstraint.Constrain(ref skeletonData);
					}

					// 관절의 위치와 회전을 얻습니다
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						bool playerTracked = IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
						player2JointsTracked[j] = player2PrevTracked[j] && playerTracked;
						player2PrevTracked[j] = playerTracked;
						
						if(player2JointsTracked[j])
						{
							player2JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
						}
					}
					
					// 질감 위에 골격을 그립니다
					if(DisplaySkeletonLines && ComputeUserMap)
					{
						DrawSkeleton(usersLblTex, ref skeletonData, ref player2JointsTracked);
						usersLblTex.Apply();
					}
					
					// 관절 방향을 계산합니다
					KinectWrapper.GetSkeletonJointOrientation(ref player2JointsPos, ref player2JointsTracked, ref player2JointsOri);
					
					// 필터 방향 제약 조건
					if(UseBoneOrientationsConstraint && boneConstraintsFilter != null)
					{
						boneConstraintsFilter.Constrain(ref player2JointsOri, ref player2JointsTracked);
					}
					
                    // 공동 방향을 필터링하십시오.
                    // 모든 공동 위치 수정 후에 수행해야합니다.
	                if(UseBoneOrientationsFilter && boneOrientationFilter[1] != null)
	                {
	                    boneOrientationFilter[1].UpdateFilter(ref skeletonData, ref player2JointsOri);
	                }
	
					// 플레이어 로테이션을 받으십시오
					player2Ori = player2JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
					
					// 제스처를 확인하십시오
					if(Time.realtimeSinceStartup >= gestureTrackingAtTime[1])
					{
						int listGestureSize = player2Gestures.Count;
						float timestampNow = Time.realtimeSinceStartup;
						
						for(int g = 0; g < listGestureSize; g++)
						{
							KinectGestures.GestureData gestureData = player2Gestures[g];
							
							if((timestampNow >= gestureData.startTrackingAtTime) &&
								!IsConflictingGestureInProgress(gestureData))
							{
								KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
									ref player2JointsPos, ref player2JointsTracked);
								player2Gestures[g] = gestureData;

								if(gestureData.complete)
								{
									gestureTrackingAtTime[1] = timestampNow + MinTimeBetweenGestures;
								}
							}
						}
					}
				}
				
				lostUsers.Remove(userId);
			}
		}
		
		// NUI 타이머를 업데이트하십시오
		lastNuiTime = currentNuiTime;
		
		// 잃어버린 사용자를 제거하십시오
		if(lostUsers.Count > 0)
		{
			foreach(uint userId in lostUsers)
			{
				RemoveUser(userId);
			}
			
			lostUsers.Clear();
		}
	}
	
	// 주어진 질감으로 골격을 그립니다
	private void DrawSkeleton(Texture2D aTexture, ref KinectWrapper.NuiSkeletonData skeletonData, ref bool[] playerJointsTracked)
	{
		int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		
		for(int i = 0; i < jointsCount; i++)
		{
			int parent = KinectWrapper.GetSkeletonJointParent(i);
			
			if(playerJointsTracked[i] && playerJointsTracked[parent])
			{
				Vector3 posParent = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[parent]);
				Vector3 posJoint = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[i]);
				
				DrawLine(aTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);
			}
		}
	}
	
	// 질감으로 선을 그립니다
	private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
	{
		int width = a_Texture.width;  // KinectWrapper.Constants.DepthImageWidth;
		int height = a_Texture.height;  // KinectWrapper.Constants.DepthImageHeight;
		
		int dy = y2 - y1;
		int dx = x2 - x1;
	 
		int stepy = 1;
		if (dy < 0) 
		{
			dy = -dy; 
			stepy = -1;
		}
		
		int stepx = 1;
		if (dx < 0) 
		{
			dx = -dx; 
			stepx = -1;
		}
		
		dy <<= 1;
		dx <<= 1;
	 
		if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
			for(int x = -1; x <= 1; x++)
				for(int y = -1; y <= 1; y++)
					a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
		
		if (dx > dy) 
		{
			int fraction = dy - (dx >> 1);
			
			while (x1 != x2) 
			{
				if (fraction >= 0) 
				{
					y1 += stepy;
					fraction -= dx;
				}
				
				x1 += stepx;
				fraction += dy;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
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
					x1 += stepx;
					fraction -= dy;
				}
				
				y1 += stepy;
				fraction += dx;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
							a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
			}
		}
		
	}
	
	// 매트릭스를 거울을 돌보고 쿼터니온으로 변환하십시오.
	private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
	{
		Vector4 vZ = mOrient.GetColumn(2);
		Vector4 vY = mOrient.GetColumn(1);

		if(!flip)
		{
			vZ.y = -vZ.y;
			vY.x = -vY.x;
			vY.z = -vY.z;
		}
		else
		{
			vZ.x = -vZ.x;
			vZ.y = -vZ.y;
			vY.z = -vY.z;
		}
		
		if(vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
			return Quaternion.LookRotation(vZ, vY);
		else
			return Quaternion.identity;
	}
	
	// 목록에서 제스처 색인을 반환하거나 찾을 수없는 경우 -1
	private int GetGestureIndex(uint UserId, KinectGestures.Gestures gesture)
	{
		if(UserId == Player1ID)
		{
			int listSize = player1Gestures.Count;
			for(int i = 0; i < listSize; i++)
			{
				if(player1Gestures[i].gesture == gesture)
					return i;
			}
		}
		else if(UserId == Player2ID)
		{
			int listSize = player2Gestures.Count;
			for(int i = 0; i < listSize; i++)
			{
				if(player2Gestures[i].gesture == gesture)
					return i;
			}
		}
		
		return -1;
	}
	
	private bool IsConflictingGestureInProgress(KinectGestures.GestureData gestureData)
	{
		foreach(KinectGestures.Gestures gesture in gestureData.checkForGestures)
		{
			int index = GetGestureIndex(gestureData.userId, gesture);
			
			if(index >= 0)
			{
				if(gestureData.userId == Player1ID)
				{
					if(player1Gestures[index].progress > 0f)
						return true;
				}
				else if(gestureData.userId == Player2ID)
				{
					if(player2Gestures[index].progress > 0f)
						return true;
				}
			}
		}
		
		return false;
	}
	
	// 주어진 사용자에 대해 교정 자세가 완료되었는지 확인하십시오.
	private bool CheckForCalibrationPose(uint userId, ref KinectGestures.Gestures calibrationGesture, 
		ref KinectGestures.GestureData gestureData, ref KinectWrapper.NuiSkeletonData skeletonData)
	{
		if(calibrationGesture == KinectGestures.Gestures.None)
			return true;
		
		// 필요한 경우 제스처 데이터를 시작하십시오
		if(gestureData.userId != userId)
		{
			gestureData.userId = userId;
			gestureData.gesture = calibrationGesture;
			gestureData.state = 0;
			gestureData.joint = 0;
			gestureData.progress = 0f;
			gestureData.complete = false;
			gestureData.cancelled = false;
		}
		
		// 임시 관절의 위치를 얻으십시오
		int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		bool[] jointsTracked = new bool[skeletonJointsCount];
		Vector3[] jointsPos = new Vector3[skeletonJointsCount];

		int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
		int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;
		
		int [] mustBeTrackedJoints = { 
			(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
			(int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
			(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
			(int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
		};
		
		for (int j = 0; j < skeletonJointsCount; j++)
		{
			jointsTracked[j] = Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
				(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked;
			
			if(jointsTracked[j])
			{
				jointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
			}
		}
		
		// 제스처 progess를 추정하십시오
		KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
			ref jointsPos, ref jointsTracked);
		
		// 제스처가 완료되었는지 확인하십시오
		if(gestureData.complete)
		{
			gestureData.userId = 0;
			return true;
		}
		
		return false;
	}
	
}