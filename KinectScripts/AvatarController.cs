using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 


[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{	
	// 캐릭터 (플레이어를 향한) 액션이있는 부울은 반영됩니다.
    // 기본 거짓.
	public bool mirroredMovement = false;
	
	// 아바타가 수직 방향으로 움직일 수 있는지 여부를 결정하는 bool.
	public bool verticalMovement = false;
	
	// 아바타가 장면을 통해 이동하는 요율.
    // 속도는 이동 속도 (.001f, 즉 1000, Unity 's Framerate로 나누기)를 곱합니다.
	protected int moveRate = 1;
	
	// Slerp Smooth Factor
	public float smoothFactor = 5f;
	
	// 센서에서보고 한대로 오프셋 노드를 사용자의 좌표로 재배치 해야하는지 여부.
	public bool offsetRelativeToSensor = false;
	

	// 바디 루트 노드
	protected Transform bodyRoot;
	
	// 공간에서 모델을 회전하려는 경우 필요한 변수입니다.
	protected GameObject offsetNode;
	
	// 모든 뼈를 잡을 수있는 가변.
    // 초기 방향과 동일한 크기를 초기화합니다.
	protected Transform[] bones;
	
	// Kinect 추적이 시작될 때 뼈의 회전.
	protected Quaternion[] initialRotations;
	protected Quaternion[] initialLocalRotations;
	
	// 변환의 초기 위치 및 회전
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	
	// 문자 위치에 대한 교정 오프셋 변수.
	protected bool offsetCalibrated = false;
	protected float xOffset, yOffset, zOffset;

	// Kinectmanager의 개인 인스턴스
	protected KinectManager kinectManager;


	// Transform Caching은 Unity 호출 GetComponent <Fransform> () 이라기 때문에 성능 향상을 제공합니다. 
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
	
	public void Awake()
    {	
		// 더블 시작을 확인하십시오
		if(bones != null)
			return;
		
		// 뼈 어레이를 입력합니다
		bones = new Transform[22];
		
		// 뼈의 초기 회전 및 방향.
		initialRotations = new Quaternion[bones.Length];
		initialLocalRotations = new Quaternion[bones.Length];

		// Kinect 트랙의 지점에 뼈를 맵핑하십시오
		MapBones();

		// 초기 뼈 회전을 얻으십시오
		GetInitialRotations();
	}
	
	// 각 프레임 각 프레임을 업데이트하십시오.
    public void UpdateAvatar(uint UserID)
    {	
		if(!transform.gameObject.activeInHierarchy) 
			return;
		
		// Kinectmanager 인스턴스를 얻으십시오
		if(kinectManager == null)
		{
			kinectManager = KinectManager.Instance;
		}
		
		// 아바타를 Kinect 위치로 이동하십시오
		MoveAvatar(UserID);

		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!bones[boneIndex]) 
				continue;
			
			if(boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
				TransformBone(UserID, joint, boneIndex, !mirroredMovement);
			}
			else if(specIndex2JointMap.ContainsKey(boneIndex))
			{
				// 특수 뼈 (clavicles)
				List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];
				
			}
		}
	}
	
	// 뼈를 초기 위치와 회전으로 설정하십시오
	public void ResetToInitialPosition()
	{	
		if(bones == null)
			return;
		
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			transform.rotation = Quaternion.identity;
		}
		
		// 정의 된 각 뼈에 대해 초기 위치로 재설정하십시오.
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
		
		if(bodyRoot != null)
		{
			bodyRoot.localPosition = Vector3.zero;
			bodyRoot.localRotation = Quaternion.identity;
		}
		
		// 오프셋의 위치와 회전을 복원하십시오
		if(offsetNode != null)
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
	
	// 플레이어의 성공적인 보정으로 호출되었습니다.
	public void SuccessfulCalibration(uint userId)
	{
		// 모델 위치를 재설정하십시오
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		
		// 위치 오프셋을 다시 교정하십시오
		offsetCalibrated = false;
	}
	
	// Kinect가 추적 한 회전을 조인트에 적용하십시오.
	protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		int iJoint = (int)joint;
		if(iJoint < 0)
			return;
		
		// Kinect 공동 방향을 얻으십시오
		Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
		if(jointRotation == Quaternion.identity)
			return;
		
		// 새 회전으로 원활하게 전환
		Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
		
		if(smoothFactor != 0f)
        	boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
		else
			boneTransform.rotation = newRotation;
	}
	
	// Kinect가 추적 한 회전을 특수 조인트에 적용하십시오.
	protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		if(!kinectManager.IsJointTracked(userId, (int)joint) || 
		   !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}
		
		Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;
		
		
		if(jointRotation != Quaternion.identity)
		{
			// 새 회전으로 원활하게 전환
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
			
			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
		
	}
	
	// 아바타를 3D 공간으로 이동 - 척추의 추적 위치를 당겨 뿌리에 적용합니다.
	// 회전이 아닌 위치 만 가져옵니다.
	protected void MoveAvatar(uint UserID)
	{
		if(bodyRoot == null || kinectManager == null)
			return;
		if(!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
			return;
		
        // 몸의 위치를 얻고 보관하십시오.
		Vector3 trans = kinectManager.GetUserPosition(UserID);
		
		// 우리가 아바타를 처음으로 움직이는 경우 오프셋을 설정하십시오.
        // 그렇지 않으면 그것을 무시하십시오.
		if (!offsetCalibrated)
		{
			offsetCalibrated = true;
			
			xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			yOffset = trans.y * moveRate;
			zOffset = -trans.z * moveRate;
			
			if(offsetRelativeToSensor)
			{
				Vector3 cameraPos = Camera.main.transform.position;
				
				float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
				Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
				Vector3 offsetPos = cameraPos + relativePos;
				
				if(offsetNode != null)
				{
					offsetNode.transform.position = offsetPos;
				}
				else
				{
					transform.position = offsetPos;
				}
			}
		}
	
		// 새 위치로 부드럽게 전환합니다
		Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

		if(smoothFactor != 0f)
			bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
		else
			bodyRoot.localPosition = targetPos;
	}
	
	// 매핑 될 뼈가 선언 된 경우 해당 뼈를 모델에 매핑하십시오.
	protected virtual void MapBones()
	{
		// Model Transform의 부모로 오프셋 노드를 만듭니다.
		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
		offsetNode.transform.position = transform.position;
		offsetNode.transform.rotation = transform.rotation;
		offsetNode.transform.parent = transform.parent;
		
		transform.parent = offsetNode.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		
		// 모델 변환을 바디 루트로 사용하십시오
		bodyRoot = transform;
		
		// 애니메이터 구성 요소에서 뼈 변환을 얻습니다
		var animatorComponent = GetComponent<Animator>();
		
		for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;
			
			bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
		}
	}
	
	// 뼈의 초기 회전을 포착하십시오
	protected void GetInitialRotations()
	{
		// 초기 회전을 저장하십시오
		if(offsetNode != null)
		{
			initialPosition = offsetNode.transform.position;
			initialRotation = offsetNode.transform.rotation;
			
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			
			transform.rotation = Quaternion.identity;
		}
		
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
				initialLocalRotations[i] = bones[i].localRotation;
			}
		}
		
		// 초기 회전을 복원하십시오
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.rotation = initialRotation;
		}
	}
	
	// 조인트 초기 회전 및 오프셋 회전에 따라 Kinect 관절 회전을 아바타 조인트 회전으로 변환합니다.
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		// 새 회전을 적용하십시오.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		
		// 오프셋 노드가 지정된 경우 변환을 그와 결합합니다.
		// 본질적으로 노드에 대해 골격을 만드는 방향
		if (offsetNode != null)
		{
			// Euler와 Offset의 Euler를 추가하여 총 회전을 잡으십시오.
			Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
			// 우리의 새로운 회전을 잡으십시오.
			newRotation = Quaternion.Euler(totalRotation);
		}
		
		return newRotation;
	}
	
	// 초기 위치, 미러링 및 이동 속도에 따라 Kinect 위치를 아바타 골격 위치로 변환합니다.
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
		float xPos;
		float yPos;
		float zPos;
		
		// 움직임이 반영되면 반전하십시오.
		if(!mirroredMovement)
			xPos = jointPosition.x * moveRate - xOffset;
		else
			xPos = -jointPosition.x * moveRate - xOffset;
		
		yPos = jointPosition.y * moveRate - yOffset;
		zPos = -jointPosition.z * moveRate - zOffset;
		
		// 수직 이동을 추적하는 경우 y를 업데이트하십시오.
        // 그렇지 않으면 그것을 내버려 두십시오.
		Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);
		
		return avatarJointPos;
	}
	
	// 뼈 가공 속도를 높이는 사전
	// Mecanim-Bones 매핑에 대한 Kinect-Joints에 대한 훌륭한 아이디어의 저자
	// 다음 사전 IS를 포함한 초기 구현과 함께
	// Mikhail Korchun (korchoon@gmail.com).
    // 이 사람에게 큰 감사합니다!
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
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
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
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
}

