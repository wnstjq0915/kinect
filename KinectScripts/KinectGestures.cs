using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// KinectGestures 클래스는 Kinect 센서를 사용하여 사용자 제스처를 감지하는 기능을 제공합니다.
/// </summary>
public class KinectGestures
{
    /*
    KinectGestures 클래스는 Kinect 센서를 사용하여 사용자의 제스처를 감지하고 처리하는 기능을 제공합니다.

    주요 요소:
    1. GestureListenerInterface: 제스처 감지 결과를 통지하기 위한 인터페이스입니다.
        사용자가 감지되었을 때, 감지된 제스처가 진행 중일 때, 완료되었을 때, 또는 취소되었을 때 호출되는 메소드를 정의합니다.

    2. Gestures 열거형: 감지할 수 있는 다양한 제스처를 정의합니다.
        예를 들어, 손을 올리기, 정지, 스와이프, 클릭 등 여러 가지 제스처가 포함되어 있습니다.

    3. GestureData 구조체: 각 제스처의 상태를 저장하는 데이터 구조체입니다.
        사용자의 ID, 제스처 종류, 상태, 타임스탬프, 관절 위치, 스크린 위치, 진행률 등을 포함합니다.

    4. 제스처 감지 메소드들:

        SetGestureJoint: 제스처의 관절 정보를 설정합니다.
        SetGestureCancelled: 제스처가 취소되었을 때 상태를 초기화합니다.
        CheckPoseComplete: 제스처의 완료 상태를 확인합니다.
        SetScreenPos: 사용자 손의 스크린 위치를 계산합니다.
        SetZoomFactor: 줌 값을 설정합니다.
        CheckForGesture: 주어진 프레임에서 제스처를 감지하고 상태를 업데이트하는 메소드입니다.
            각 감지된 제스처에 따라 다양한 상태와 조건을 확인합니다.
    5. 제스처 체크 로직: CheckForGesture 메소드는 다양한 제스처를 감지하고,
        각 제스처의 상태(감지 중, 완료, 취소 등) 를 관리하며,
        필요한 경우 제스처의 진행률을 업데이트합니다.

    이 클래스는 Kinect를 통해 사용자의 동작을 인식하고, 그에 따라 다양한 인터랙션을 가능하게 하는데 필요한 구조와 로직을 제공합니다.
    이를 통해 게임 개발이나 인터랙티브 환경에서 자연스러운 사용자 경험을 구현할 수 있습니다.
    */


    public interface GestureListenerInterface
    {
        /// <summary>
        /// 새로운 사용자가 감지되고 추적이 시작될 때 호출됩니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userIndex">사용자 인덱스</param>
        void UserDetected(uint userId, int userIndex);

        /// <summary>
        /// 사용자가 감지되지 않을 때 호출됩니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userIndex">사용자 인덱스</param>
        void UserLost(uint userId, int userIndex);

        /// <summary>
        /// 제스처가 진행 중일 때 호출됩니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userIndex">사용자 인덱스</param>
        /// <param name="gesture">제스처 종류</param>
        /// <param name="progress">제스처 진행 상태</param>
        /// <param name="joint">관절 위치</param>
        /// <param name="screenPos">화면 위치</param>
        void GestureInProgress(uint userId, int userIndex, Gestures gesture, float progress,
                               KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

        /// <summary>
        /// 제스처가 완료되었을 때 호출됩니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userIndex">사용자 인덱스</param>
        /// <param name="gesture">제스처 종류</param>
        /// <param name="joint">관절 위치</param>
        /// <param name="screenPos">화면 위치</param>
        /// <returns>제스처 감지를 재시작해야 할지 여부</returns>
        bool GestureCompleted(uint userId, int userIndex, Gestures gesture,
                              KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

        /// <summary>
        /// 제스처가 취소되었을 때 호출됩니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userIndex">사용자 인덱스</param>
        /// <param name="gesture">제스처 종류</param>
        /// <param name="joint">관절 위치</param>
        /// <returns>제스처 감지를 재시작해야 할지 여부</returns>
        bool GestureCancelled(uint userId, int userIndex, Gestures gesture,
                              KinectWrapper.NuiSkeletonPositionIndex joint);
    }

    /// <summary>
    /// 감지할 제스처 종류를 정의합니다.
    /// </summary>
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

    /// <summary>
    /// 제스처 데이터 구조체로, 각 제스처의 상태를 저장합니다.
    /// </summary>
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

    // 제스처 관련 상수 및 인덱스
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

    /// <summary>
    /// 제스처의 관절 정보를 설정합니다.
    /// </summary>
    /// <param name="gestureData">제스처 데이터</param>
    /// <param name="timestamp">현재 타임스탬프</param>
    /// <param name="joint">관절 인덱스</param>
    /// <param name="jointPos">관절 위치</param>
    private static void SetGestureJoint(ref GestureData gestureData, float timestamp, int joint, Vector3 jointPos)
    {
        gestureData.joint = joint;
        gestureData.jointPos = jointPos;
        gestureData.timestamp = timestamp;
        gestureData.state++;
    }

    /// <summary>
    /// 제스처가 취소되었을 때 상태를 초기화합니다.
    /// </summary>
    /// <param name="gestureData">제스처 데이터</param>
    private static void SetGestureCancelled(ref GestureData gestureData)
    {
        gestureData.state = 0;
        gestureData.progress = 0f;
        gestureData.cancelled = true;
    }

    /// <summary>
    /// 제스처의 완료 상태를 확인합니다.
    /// </summary>
    /// <param name="gestureData">제스처 데이터</param>
    /// <param name="timestamp">현재 타임스탬프</param>
    /// <param name="jointPos">관절 위치</param>
    /// <param name="isInPose">포즈 상태</param>
    /// <param name="durationToComplete">완료하는 데 걸리는 시간</param>
    private static void CheckPoseComplete(ref GestureData gestureData, float timestamp, Vector3 jointPos, bool isInPose, float durationToComplete)
    {
        if (isInPose)
        {
            float timeLeft = timestamp - gestureData.timestamp;
            gestureData.progress = durationToComplete > 0f ? Mathf.Clamp01(timeLeft / durationToComplete) : 1.0f;

            if (timeLeft >= durationToComplete)
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

    /// <summary>
    /// 스크린 위치를 설정합니다.
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="gestureData">제스처 데이터</param>
    /// <param name="jointsPos">관절 위치 배열</param>
    /// <param name="jointsTracked">관절 추적 상태 배열</param>
    private static void SetScreenPos(uint userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
    {
        Vector3 handPos = jointsPos[rightHandIndex];
        bool calculateCoords = false;

        // 오른손 관절 위치 확인
        if (gestureData.joint == rightHandIndex)
        {
            if (jointsTracked[rightHandIndex])
            {
                calculateCoords = true;
            }
        }
        // 왼손 관절 위치 확인
        else if (gestureData.joint == leftHandIndex)
        {
            if (jointsTracked[leftHandIndex])
            {
                handPos = jointsPos[leftHandIndex];
                calculateCoords = true;
            }
        }

        if (calculateCoords)
        {
            // 모든 필요한 관절이 추적되고 있는지 확인
            if (jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
                jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex])
            {
                Vector3 neckToHips = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
                Vector3 rightToLeft = jointsPos[rightShoulderIndex] - jointsPos[leftShoulderIndex];

                gestureData.tagVector2.x = rightToLeft.x; // * 1.2f;
                gestureData.tagVector2.y = neckToHips.y; // * 1.2f;

                // 오른손에 대한 스크린 위치 설정
                if (gestureData.joint == rightHandIndex)
                {
                    gestureData.tagVector.x = jointsPos[rightShoulderIndex].x - gestureData.tagVector2.x / 2;
                    gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                }
                // 왼손에 대한 스크린 위치 설정
                else
                {
                    gestureData.tagVector.x = jointsPos[leftShoulderIndex].x - gestureData.tagVector2.x / 2;
                    gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                }
            }

            // 스크린 위치 계산
            if (gestureData.tagVector2.x != 0 && gestureData.tagVector2.y != 0)
            {
                Vector3 relHandPos = handPos - gestureData.tagVector;
                gestureData.screenPos.x = Mathf.Clamp01(relHandPos.x / gestureData.tagVector2.x);
                gestureData.screenPos.y = Mathf.Clamp01(relHandPos.y / gestureData.tagVector2.y);
            }
        }
    }

    /// <summary>
    /// 줌 팩터를 설정합니다.
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="gestureData">제스처 데이터</param>
    /// <param name="initialZoom">초기 줌 값</param>
    /// <param name="jointsPos">관절 위치 배열</param>
    /// <param name="jointsTracked">관절 추적 상태 배열</param>
    private static void SetZoomFactor(uint userId, ref GestureData gestureData, float initialZoom, ref Vector3[] jointsPos, ref bool[] jointsTracked)
    {
        Vector3 vectorZooming = jointsPos[rightHandIndex] - jointsPos[leftHandIndex];

        if (gestureData.tagFloat == 0f || gestureData.userId != userId)
        {
            gestureData.tagFloat = 0.5f; // this is 100%
        }

        float distZooming = vectorZooming.magnitude;
        gestureData.screenPos.z = initialZoom + (distZooming / gestureData.tagFloat);
    }

    /// <summary>
    /// 제스처 상태를 확인합니다.
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="gestureData">제스처 데이터</param>
    /// <param name="timestamp">현재 타임스탬프</param>
    /// <param name="jointsPos">관절 위치 배열</param>
    /// <param name="jointsTracked">관절 추적 상태 배열</param>
    public static void CheckForGesture(uint userId, ref GestureData gestureData, float timestamp, ref Vector3[] jointsPos, ref bool[] jointsTracked)
    {
        if (gestureData.complete)
            return;

        float bandSize = (jointsPos[shoulderCenterIndex].y - jointsPos[hipCenterIndex].y);
        float gestureTop = jointsPos[shoulderCenterIndex].y + bandSize / 2;
        float gestureBottom = jointsPos[shoulderCenterIndex].y - bandSize;
        float gestureRight = jointsPos[rightHipIndex].x;
        float gestureLeft = jointsPos[leftHipIndex].x;

        switch (gestureData.gesture)
        {
            // 오른손을 올리는 제스처 확인
            case Gestures.RaiseRightHand:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }
                        break;

                    case 1:  // 제스처 완료
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // 왼손을 올리는 제스처 확인
            case Gestures.RaiseLeftHand:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지
                        if (jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                        }
                        break;

                    case 1:  // 제스처 완료
                        bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // Psi 제스처 확인
            case Gestures.Psi:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                           jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                           (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }
                        break;

                    case 1:  // 제스처 완료
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                            jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // T-포즈 확인
            case Gestures.Tpose:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
                           Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f && // 0.07f
                           Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f && // 0.7f
                           jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
                           Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
                           Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }

                        break;

                    case 1:  // 제스처 완료
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

            // 정지 제스처 확인
            case Gestures.Stop:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.1f &&
                           (jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.1f &&
                                (jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                        }
                        break;

                    case 1:  // 제스처 완료
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

            // 손을 흔드는 제스처 확인
            case Gestures.Wave:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                           (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.3f;
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.3f;
                        }
                        break;

                    case 1:  // 제스처 - 단계 2
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                                (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) < -0.05f :
                                jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) > 0.05f;

                            if (isInPose)
                            {
                                gestureData.timestamp = timestamp;
                                gestureData.state++;
                                gestureData.progress = 0.7f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                    case 2:  // 제스처 단계 3 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                                (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f :
                                jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 클릭 제스처 확인
            case Gestures.Click:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.3f;

                            // 가장 정확한 클릭 위치에서 스크린 위치 설정
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.3f;

                            // 가장 정확한 클릭 위치에서 스크린 위치 설정
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                        }
                        break;

                    case 1:
                        {
                            // 제자리에 있는지 확인
                            Vector3 distVector = jointsPos[gestureData.joint] - gestureData.jointPos;
                            bool isInPose = distVector.magnitude < 0.05f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.ClickStayDuration);
                        }
                        break;
                }
                break;

            // 왼쪽으로 스와이프 제스처 확인
            case Gestures.SwipeLeft:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                            jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                            jointsPos[rightHandIndex].x <= gestureRight && jointsPos[rightHandIndex].x > gestureLeft)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.1f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                    jointsPos[rightHandIndex].x < gestureLeft;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                            else if (jointsPos[rightHandIndex].x <= gestureRight)
                            {
                                float gestureSize = gestureRight - gestureLeft;
                                gestureData.progress = gestureSize > 0.01f ? (gestureRight - jointsPos[rightHandIndex].x) / gestureSize : 0f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 오른쪽으로 스와이프 제스처 확인
            case Gestures.SwipeRight:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                            jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                            jointsPos[leftHandIndex].x >= gestureLeft && jointsPos[leftHandIndex].x < gestureRight)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.1f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                    jointsPos[leftHandIndex].x > gestureRight;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                            else if (jointsPos[leftHandIndex].x >= gestureLeft)
                            {
                                float gestureSize = gestureRight - gestureLeft;
                                gestureData.progress = gestureSize > 0.01f ? (jointsPos[leftHandIndex].x - gestureLeft) / gestureSize : 0f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 위로 스와이프 제스처 확인
            case Gestures.SwipeUp:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < 0.0f &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.15f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < 0.0f &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.15f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.05f &&
                                Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
                                jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.05f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 아래로 스와이프 제스처 확인
            case Gestures.SwipeDown:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.05f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.05f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < -0.15f &&
                                Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
                                jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < -0.15f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 오른손 커서 제스처 확인
            case Gestures.RightHandCursor:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1 (지속적)
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) > -0.1f)
                        {
                            gestureData.joint = rightHandIndex;
                            gestureData.timestamp = timestamp;
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                            gestureData.progress = 0.7f;
                        }
                        else
                        {
                            // 제스처 취소
                            gestureData.progress = 0f;
                        }
                        break;

                }
                break;

            // 왼손 커서 제스처 확인
            case Gestures.LeftHandCursor:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1 (지속적)
                        if (jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) > -0.1f)
                        {
                            gestureData.joint = leftHandIndex;
                            gestureData.timestamp = timestamp;
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                            gestureData.progress = 0.7f;
                        }
                        else
                        {
                            // 제스처 취소
                            gestureData.progress = 0f;
                        }
                        break;

                }
                break;

            // 줌 아웃 제스처 확인
            case Gestures.ZoomOut:
                Vector3 vectorZoomOut = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distZoomOut = vectorZoomOut.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
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

                    case 1:  // 제스처 단계 2 = 줌
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            float angleZoomOut = Vector3.Angle(gestureData.tagVector, vectorZoomOut) * Mathf.Sign(vectorZoomOut.y - gestureData.tagVector.y);
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                    jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                    distZoomOut < 1.5f && Mathf.Abs(angleZoomOut) < 20f;

                            if (isInPose)
                            {
                                SetZoomFactor(userId, ref gestureData, 1.0f, ref jointsPos, ref jointsTracked);
                                gestureData.timestamp = timestamp;
                                gestureData.progress = 0.7f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // 줌 인 제스처 확인
            case Gestures.ZoomIn:
                Vector3 vectorZoomIn = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distZoomIn = vectorZoomIn.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
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

                    case 1:  // 제스처 단계 2 = 줌
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            float angleZoomIn = Vector3.Angle(gestureData.tagVector, vectorZoomIn) * Mathf.Sign(vectorZoomIn.y - gestureData.tagVector.y);
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                    jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                    distZoomIn >= 0.2f && Mathf.Abs(angleZoomIn) < 20f;

                            if (isInPose)
                            {
                                SetZoomFactor(userId, ref gestureData, 0.0f, ref jointsPos, ref jointsTracked);
                                gestureData.timestamp = timestamp;
                                gestureData.progress = 0.7f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // 휠 제스처 확인
            case Gestures.Wheel:
                Vector3 vectorWheel = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distWheel = vectorWheel.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
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

                    case 1:  // 제스처 단계 2 = 휠 회전
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            float angle = Vector3.Angle(gestureData.tagVector, vectorWheel) * Mathf.Sign(vectorWheel.y - gestureData.tagVector.y);
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                distWheel >= 0.3f && distWheel < 0.7f &&
                                Mathf.Abs(distWheel - gestureData.tagFloat) < 0.1f;

                            if (isInPose)
                            {
                                gestureData.screenPos.z = angle;  // 휠 각도
                                gestureData.timestamp = timestamp;
                                gestureData.tagFloat = distWheel;
                                gestureData.progress = 0.7f;
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // 점프 제스처 확인
            case Gestures.Jump:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[hipCenterIndex] &&
                            (jointsPos[hipCenterIndex].y > 0.9f) && (jointsPos[hipCenterIndex].y < 1.3f))
                        {
                            SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = jointsTracked[hipCenterIndex] &&
                                (jointsPos[hipCenterIndex].y - gestureData.jointPos.y) > 0.15f &&
                                Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 스쿼트 제스처 확인
            case Gestures.Squat:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[hipCenterIndex] &&
                            (jointsPos[hipCenterIndex].y <= 0.9f))
                        {
                            SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = jointsTracked[hipCenterIndex] &&
                                (jointsPos[hipCenterIndex].y - gestureData.jointPos.y) < -0.15f &&
                                Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 밀기 제스처 확인
            case Gestures.Push:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                           Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
                           (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.2f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
                                (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.2f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                (jointsPos[rightHandIndex].z - jointsPos[rightHandIndex].z) < -0.1f :
                                jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                (jointsPos[leftHandIndex].z - jointsPos[leftHandIndex].z) < -0.1f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // 당기기 제스처 확인
            case Gestures.Pull:
                switch (gestureData.state)
                {
                    case 0:  // 제스처 감지 - 단계 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                           Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
                           (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.3f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
                                (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.3f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // 제스처 단계 2 = 완료
                        if ((timestamp - gestureData.timestamp) < 1.5f)
                        {
                            bool isInPose = gestureData.joint == rightHandIndex ?
                                jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                (jointsPos[rightHandIndex].z - jointsPos[rightHandIndex].z) > 0.1f :
                                jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                (jointsPos[leftHandIndex].z - jointsPos[leftHandIndex].z) > 0.1f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // 제스처 취소
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

                // 여기 추가적인 제스처 케이스가 올 수 있습니다.
        }
    }
}