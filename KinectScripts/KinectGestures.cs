using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinectGestures
{

	public interface GestureListenerInterface
	{
		// 새 사용자가 감지되고 추적이 시작될 때 호출
		// 여기서 Kinectmanager.detectgesture ()로 제스처 감지를 시작할 수 있습니다.
		void UserDetected(uint userId, int userIndex);
		
		// 사용자가 손실되면 호출됩니다
		// 이 사용자를위한 제스처는 자동으로 지워지지 만 중고 리소스를 자유롭게 할 수 있습니다.
		void UserLost(uint userId, int userIndex);
		
		// 제스처가 진행 중일 때 호출됩니다 
		void GestureInProgress(uint userId, int userIndex, Gestures gesture, float progress, 
		                       KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

		// 제스처가 완료되면 호출.
		// 제스처 탐지를 다시 시작 해야하는 경우 true를 반환합니다.
		bool GestureCompleted(uint userId, int userIndex, Gestures gesture,
		                      KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

		// 제스처가 취소되면 호출됩니다.
		// 제스처 탐지를 다시 타야하는 경우 true를 반환합니다. 그렇지 않으면 false
		bool GestureCancelled(uint userId, int userIndex, Gestures gesture, 
		                      KinectWrapper.NuiSkeletonPositionIndex joint);
	}
	
	
	public enum Gestures
	{
		None = 0,
		RaiseRightHand,
		RaiseLeftHand,
		Psi,
		Tpose,
		Stop,
		Wave,
		Click,
		SwipeLeft,
		SwipeRight,
		SwipeUp,
		SwipeDown,
		RightHandCursor,
		LeftHandCursor,
		ZoomOut,
		ZoomIn,
		Wheel,
		Jump,
		Squat,
		Push,
		Pull
	}
	
	
	public struct GestureData
	{
		public uint userId;
		public Gestures gesture;
		public int state;
		public float timestamp;
		public int joint;
		public Vector3 jointPos;
		public Vector3 screenPos;
		public float tagFloat;
		public Vector3 tagVector;
		public Vector3 tagVector2;
		public float progress;
		public bool complete;
		public bool cancelled;
		public List<Gestures> checkForGestures;
		public float startTrackingAtTime;
	}
	

	
	// 제스처 관련 상수, 변수 및 기능
	private const int leftHandIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft;
	private const int rightHandIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HandRight;
		
	private const int leftElbowIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft;
	private const int rightElbowIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight;
		
	private const int leftShoulderIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft;
	private const int rightShoulderIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight;
	
	private const int hipCenterIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter;
	private const int shoulderCenterIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter;
	private const int leftHipIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft;
	private const int rightHipIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HipRight;
	
	
	private static void SetGestureJoint(ref GestureData gestureData, float timestamp, int joint, Vector3 jointPos)
	{
		gestureData.joint = joint;
		gestureData.jointPos = jointPos;
		gestureData.timestamp = timestamp;
		gestureData.state++;
	}
	
	private static void SetGestureCancelled(ref GestureData gestureData)
	{
		gestureData.state = 0;
		gestureData.progress = 0f;
		gestureData.cancelled = true;
	}
	
	private static void CheckPoseComplete(ref GestureData gestureData, float timestamp, Vector3 jointPos, bool isInPose, float durationToComplete)
	{
		if(isInPose)
		{
			float timeLeft = timestamp - gestureData.timestamp;
			gestureData.progress = durationToComplete > 0f ? Mathf.Clamp01(timeLeft / durationToComplete) : 1.0f;
	
			if(timeLeft >= durationToComplete)
			{
				gestureData.timestamp = timestamp;
				gestureData.jointPos = jointPos;
				gestureData.state++;
				gestureData.complete = true;
			}
		}
		else
		{
			SetGestureCancelled(ref gestureData);
		}
	}
	
	private static void SetScreenPos(uint userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		Vector3 handPos = jointsPos[rightHandIndex];
		bool calculateCoords = false;
		
		if(gestureData.joint == rightHandIndex)
		{
			if(jointsTracked[rightHandIndex])
			{
				calculateCoords = true;
			}
		}
		else if(gestureData.joint == leftHandIndex)
		{
			if(jointsTracked[leftHandIndex])
			{
				handPos = jointsPos[leftHandIndex];
				
				calculateCoords = true;
			}
		}
		
		if(calculateCoords)
		{
			
			if(jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && 
				jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex])
			{
				Vector3 neckToHips = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
				Vector3 rightToLeft = jointsPos[rightShoulderIndex] - jointsPos[leftShoulderIndex];
				
				gestureData.tagVector2.x = rightToLeft.x; // * 1.2f;
				gestureData.tagVector2.y = neckToHips.y; // * 1.2f;
				
				if(gestureData.joint == rightHandIndex)
				{
					gestureData.tagVector.x = jointsPos[rightShoulderIndex].x - gestureData.tagVector2.x / 2;
					gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
				}
				else
				{
					gestureData.tagVector.x = jointsPos[leftShoulderIndex].x - gestureData.tagVector2.x / 2;
					gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
				}
			}
			
			if(gestureData.tagVector2.x != 0 && gestureData.tagVector2.y != 0)
			{
				Vector3 relHandPos = handPos - gestureData.tagVector;
				gestureData.screenPos.x = Mathf.Clamp01(relHandPos.x / gestureData.tagVector2.x);
				gestureData.screenPos.y = Mathf.Clamp01(relHandPos.y / gestureData.tagVector2.y);
			}
		}
	}
	
	private static void SetZoomFactor(uint userId, ref GestureData gestureData, float initialZoom, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		Vector3 vectorZooming = jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
		
		if(gestureData.tagFloat == 0f || gestureData.userId != userId)
		{
			gestureData.tagFloat = 0.5f; // this is 100%
		}

		float distZooming = vectorZooming.magnitude;
		gestureData.screenPos.z = initialZoom + (distZooming / gestureData.tagFloat);
	}
	
	// 제스처의 다음 상태와 완전성을 추정하십시오
	public static void CheckForGesture(uint userId, ref GestureData gestureData, float timestamp, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		if(gestureData.complete)
			return;
		
		float bandSize = (jointsPos[shoulderCenterIndex].y - jointsPos[hipCenterIndex].y);
		float gestureTop = jointsPos[shoulderCenterIndex].y + bandSize / 2;
		float gestureBottom = jointsPos[shoulderCenterIndex].y - bandSize;
		float gestureRight = jointsPos[rightHipIndex].x;
		float gestureLeft = jointsPos[leftHipIndex].x;
		
		switch(gestureData.gesture)
		{
			// 8 번째를 확인하십시오
			case Gestures.RaiseRightHand:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// Raiselefthand를 확인하십시오
			case Gestures.RaiseLeftHand:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
						bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// PSI를 확인하십시오
			case Gestures.Psi:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
					       jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
					       (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
							jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// tpose를 확인하십시오
			case Gestures.Tpose:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
						   Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.07f
						   Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
						   jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
						   Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
						   Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						break;
					
					case 1:  // gesture complete
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
							Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
								Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
								Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f;
						
						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
						break;
				}
				break;
			
			// 정지를 확인하십시오
			case Gestures.Stop:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
						   (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.1f &&
						   (jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
						        (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.1f &&
						        (jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
						}
						break;
				
					case 1:  // gesture complete
						bool isInPose = (gestureData.joint == rightHandIndex) ?
							(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.1f &&
							(jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f) :
							(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.1f &&
							(jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f);
						
						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
						break;

				}
				break;

			// 파도를 확인하십시오
			case Gestures.Wave:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
					       (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.3f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
					            (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture - phase 2
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f && 
								(jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) < -0.05f :
								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
								(jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) > 0.05f;
				
							if(isInPose)
							{
								gestureData.timestamp = timestamp;
								gestureData.state++;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
									
					case 2:  // gesture phase 3 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f && 
								(jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f :
								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
								(jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 클릭을 확인하십시오
			case Gestures.Click:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.3f;

							// 가장 정확한 클릭 위치이기 때문에 시작시 화면 위치 설정
							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.3f;

							// 가장 정확한 클릭 위치이기 때문에 시작시 화면 위치 설정
							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
						}
						break;
				
					case 1:
						{
							// 장소에 있는지 확인하십시오
							Vector3 distVector = jointsPos[gestureData.joint] - gestureData.jointPos;
							bool isInPose = distVector.magnitude < 0.05f;

							Vector3 jointPos = jointsPos[gestureData.joint];
							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.ClickStayDuration);
						}
						break;
				}
				break;

			// Swipeleft를 확인하십시오
			case Gestures.SwipeLeft:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
							jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
				   			jointsPos[rightHandIndex].x <= gestureRight && jointsPos[rightHandIndex].x > gestureLeft)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.1f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{

							bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].x < gestureLeft;
							
							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
							else if(jointsPos[rightHandIndex].x <= gestureRight)
							{
								float gestureSize = gestureRight - gestureLeft;
								gestureData.progress = gestureSize > 0.01f ? (gestureRight - jointsPos[rightHandIndex].x) / gestureSize : 0f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// Swiperight를 확인하십시오
			case Gestures.SwipeRight:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
				
						if(jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
				   			jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
				   			jointsPos[leftHandIndex].x >= gestureLeft && jointsPos[leftHandIndex].x < gestureRight)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.1f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{

							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[leftHandIndex].x > gestureRight;
							
							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
							else if(jointsPos[leftHandIndex].x >= gestureLeft)
							{
								float gestureSize = gestureRight - gestureLeft;
								gestureData.progress = gestureSize > 0.01f ? (jointsPos[leftHandIndex].x - gestureLeft) / gestureSize : 0f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// Swipeup을 확인하십시오
			case Gestures.SwipeUp:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
					       	(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < 0.0f &&
				   	        (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.15f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < 0.0f &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.15f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.05f && 
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.05f && 
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 스 와이딩을 확인하십시오
			case Gestures.SwipeDown:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < -0.15f && 
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < -0.15f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// RighthandCursor를 확인하십시오
			case Gestures.RightHandCursor:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1 (perpetual)
						if(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) > -0.1f)
						{
							gestureData.joint = rightHandIndex;
							gestureData.timestamp = timestamp;
							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
							gestureData.progress = 0.7f;
						}
						else
						{
							// 제스처를 취소하십시오
							gestureData.progress = 0f;
						}
						break;
				
				}
				break;

			// LefthandCursor를 확인하십시오
			case Gestures.LeftHandCursor:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1 (perpetual)
						if(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) > -0.1f)
						{
							gestureData.joint = leftHandIndex;
							gestureData.timestamp = timestamp;
							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
							gestureData.progress = 0.7f;
						}
						else
						{
							// 제스처를 취소하십시오
							gestureData.progress = 0f;
						}
						break;
				
				}
				break;

			// Zoomout을 확인하십시오
			case Gestures.ZoomOut:
				Vector3 vectorZoomOut = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distZoomOut = vectorZoomOut.magnitude;

				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
				   			jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
				   			jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
							distZoomOut < 0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = 0f;
							gestureData.progress = 0.3f;
						}
						break;
						
					case 1:  // gesture phase 2 = zooming
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							float angleZoomOut = Vector3.Angle(gestureData.tagVector, vectorZoomOut) * Mathf.Sign(vectorZoomOut.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
									distZoomOut < 1.5f && Mathf.Abs(angleZoomOut) < 20f;
							
							if(isInPose)
							{
								SetZoomFactor(userId, ref gestureData, 1.0f, ref jointsPos, ref jointsTracked);
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;

				}
				break;

			// Zoomin을 확인하십시오
			case Gestures.ZoomIn:
				Vector3 vectorZoomIn = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distZoomIn = vectorZoomIn.magnitude;
				
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
						   jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
						   distZoomIn >= 0.7f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = distZoomIn;
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture phase 2 = zooming
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							float angleZoomIn = Vector3.Angle(gestureData.tagVector, vectorZoomIn) * Mathf.Sign(vectorZoomIn.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
									distZoomIn >= 0.2f && Mathf.Abs(angleZoomIn) < 20f;
							
							if(isInPose)
							{
								SetZoomFactor(userId, ref gestureData, 0.0f, ref jointsPos, ref jointsTracked);
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;

				}
				break;

			// 휠을 확인하십시오
			case Gestures.Wheel:
				Vector3 vectorWheel = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distWheel = vectorWheel.magnitude;

				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
						   jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
						   distWheel >= 0.3f && distWheel < 0.7f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = distWheel;
							gestureData.progress = 0.3f;
						}
						break;

					case 1:  // gesture phase 2 = turning wheel
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							float angle = Vector3.Angle(gestureData.tagVector, vectorWheel) * Mathf.Sign(vectorWheel.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
								jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
								jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
								distWheel >= 0.3f && distWheel < 0.7f && 
								Mathf.Abs(distWheel - gestureData.tagFloat) < 0.1f;
							
							if(isInPose)
							{
								gestureData.screenPos.z = angle;  // wheel angle
								gestureData.timestamp = timestamp;
								gestureData.tagFloat = distWheel;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;

				}
				break;
			
			// 점프를 확인하십시오
			case Gestures.Jump:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[hipCenterIndex] && 
							(jointsPos[hipCenterIndex].y > 0.9f) && (jointsPos[hipCenterIndex].y < 1.3f))
						{
							SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = jointsTracked[hipCenterIndex] &&
								(jointsPos[hipCenterIndex].y - gestureData.jointPos.y) > 0.15f && 
								Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 스쿼트를 확인하십시오
			case Gestures.Squat:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[hipCenterIndex] && 
							(jointsPos[hipCenterIndex].y <= 0.9f))
						{
							SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = jointsTracked[hipCenterIndex] &&
								(jointsPos[hipCenterIndex].y - gestureData.jointPos.y) < -0.15f && 
								Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 푸시를 확인하십시오
			case Gestures.Push:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
				   			(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
				   			Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
				   			(jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.2f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
								(jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.2f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) < -0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) < -0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 풀을 확인하십시오
			case Gestures.Pull:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
						   (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
						   Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
						   (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
						        (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
						        Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
						        (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) > 0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) > 0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// 제스처를 취소하십시오
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// 여기에 더 많은 제스처 사례가 있습니다
		}
	}

}
