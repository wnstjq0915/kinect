using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// KinectGestures Ŭ������ Kinect ������ ����Ͽ� ����� ����ó�� �����ϴ� ����� �����մϴ�.
/// </summary>
public class KinectGestures
{
    /*
    KinectGestures Ŭ������ Kinect ������ ����Ͽ� ������� ����ó�� �����ϰ� ó���ϴ� ����� �����մϴ�.

    �ֿ� ���:
    1. GestureListenerInterface: ����ó ���� ����� �����ϱ� ���� �������̽��Դϴ�.
        ����ڰ� �����Ǿ��� ��, ������ ����ó�� ���� ���� ��, �Ϸ�Ǿ��� ��, �Ǵ� ��ҵǾ��� �� ȣ��Ǵ� �޼ҵ带 �����մϴ�.

    2. Gestures ������: ������ �� �ִ� �پ��� ����ó�� �����մϴ�.
        ���� ���, ���� �ø���, ����, ��������, Ŭ�� �� ���� ���� ����ó�� ���ԵǾ� �ֽ��ϴ�.

    3. GestureData ����ü: �� ����ó�� ���¸� �����ϴ� ������ ����ü�Դϴ�.
        ������� ID, ����ó ����, ����, Ÿ�ӽ�����, ���� ��ġ, ��ũ�� ��ġ, ����� ���� �����մϴ�.

    4. ����ó ���� �޼ҵ��:

        SetGestureJoint: ����ó�� ���� ������ �����մϴ�.
        SetGestureCancelled: ����ó�� ��ҵǾ��� �� ���¸� �ʱ�ȭ�մϴ�.
        CheckPoseComplete: ����ó�� �Ϸ� ���¸� Ȯ���մϴ�.
        SetScreenPos: ����� ���� ��ũ�� ��ġ�� ����մϴ�.
        SetZoomFactor: �� ���� �����մϴ�.
        CheckForGesture: �־��� �����ӿ��� ����ó�� �����ϰ� ���¸� ������Ʈ�ϴ� �޼ҵ��Դϴ�.
            �� ������ ����ó�� ���� �پ��� ���¿� ������ Ȯ���մϴ�.
    5. ����ó üũ ����: CheckForGesture �޼ҵ�� �پ��� ����ó�� �����ϰ�,
        �� ����ó�� ����(���� ��, �Ϸ�, ��� ��) �� �����ϸ�,
        �ʿ��� ��� ����ó�� ������� ������Ʈ�մϴ�.

    �� Ŭ������ Kinect�� ���� ������� ������ �ν��ϰ�, �׿� ���� �پ��� ���ͷ����� �����ϰ� �ϴµ� �ʿ��� ������ ������ �����մϴ�.
    �̸� ���� ���� �����̳� ���ͷ�Ƽ�� ȯ�濡�� �ڿ������� ����� ������ ������ �� �ֽ��ϴ�.
    */


    public interface GestureListenerInterface
    {
        /// <summary>
        /// ���ο� ����ڰ� �����ǰ� ������ ���۵� �� ȣ��˴ϴ�.
        /// </summary>
        /// <param name="userId">����� ID</param>
        /// <param name="userIndex">����� �ε���</param>
        void UserDetected(uint userId, int userIndex);

        /// <summary>
        /// ����ڰ� �������� ���� �� ȣ��˴ϴ�.
        /// </summary>
        /// <param name="userId">����� ID</param>
        /// <param name="userIndex">����� �ε���</param>
        void UserLost(uint userId, int userIndex);

        /// <summary>
        /// ����ó�� ���� ���� �� ȣ��˴ϴ�.
        /// </summary>
        /// <param name="userId">����� ID</param>
        /// <param name="userIndex">����� �ε���</param>
        /// <param name="gesture">����ó ����</param>
        /// <param name="progress">����ó ���� ����</param>
        /// <param name="joint">���� ��ġ</param>
        /// <param name="screenPos">ȭ�� ��ġ</param>
        void GestureInProgress(uint userId, int userIndex, Gestures gesture, float progress,
                               KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

        /// <summary>
        /// ����ó�� �Ϸ�Ǿ��� �� ȣ��˴ϴ�.
        /// </summary>
        /// <param name="userId">����� ID</param>
        /// <param name="userIndex">����� �ε���</param>
        /// <param name="gesture">����ó ����</param>
        /// <param name="joint">���� ��ġ</param>
        /// <param name="screenPos">ȭ�� ��ġ</param>
        /// <returns>����ó ������ ������ؾ� ���� ����</returns>
        bool GestureCompleted(uint userId, int userIndex, Gestures gesture,
                              KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos);

        /// <summary>
        /// ����ó�� ��ҵǾ��� �� ȣ��˴ϴ�.
        /// </summary>
        /// <param name="userId">����� ID</param>
        /// <param name="userIndex">����� �ε���</param>
        /// <param name="gesture">����ó ����</param>
        /// <param name="joint">���� ��ġ</param>
        /// <returns>����ó ������ ������ؾ� ���� ����</returns>
        bool GestureCancelled(uint userId, int userIndex, Gestures gesture,
                              KinectWrapper.NuiSkeletonPositionIndex joint);
    }

    /// <summary>
    /// ������ ����ó ������ �����մϴ�.
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
    /// ����ó ������ ����ü��, �� ����ó�� ���¸� �����մϴ�.
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

    // ����ó ���� ��� �� �ε���
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
    /// ����ó�� ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="gestureData">����ó ������</param>
    /// <param name="timestamp">���� Ÿ�ӽ�����</param>
    /// <param name="joint">���� �ε���</param>
    /// <param name="jointPos">���� ��ġ</param>
    private static void SetGestureJoint(ref GestureData gestureData, float timestamp, int joint, Vector3 jointPos)
    {
        gestureData.joint = joint;
        gestureData.jointPos = jointPos;
        gestureData.timestamp = timestamp;
        gestureData.state++;
    }

    /// <summary>
    /// ����ó�� ��ҵǾ��� �� ���¸� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="gestureData">����ó ������</param>
    private static void SetGestureCancelled(ref GestureData gestureData)
    {
        gestureData.state = 0;
        gestureData.progress = 0f;
        gestureData.cancelled = true;
    }

    /// <summary>
    /// ����ó�� �Ϸ� ���¸� Ȯ���մϴ�.
    /// </summary>
    /// <param name="gestureData">����ó ������</param>
    /// <param name="timestamp">���� Ÿ�ӽ�����</param>
    /// <param name="jointPos">���� ��ġ</param>
    /// <param name="isInPose">���� ����</param>
    /// <param name="durationToComplete">�Ϸ��ϴ� �� �ɸ��� �ð�</param>
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
    /// ��ũ�� ��ġ�� �����մϴ�.
    /// </summary>
    /// <param name="userId">����� ID</param>
    /// <param name="gestureData">����ó ������</param>
    /// <param name="jointsPos">���� ��ġ �迭</param>
    /// <param name="jointsTracked">���� ���� ���� �迭</param>
    private static void SetScreenPos(uint userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
    {
        Vector3 handPos = jointsPos[rightHandIndex];
        bool calculateCoords = false;

        // ������ ���� ��ġ Ȯ��
        if (gestureData.joint == rightHandIndex)
        {
            if (jointsTracked[rightHandIndex])
            {
                calculateCoords = true;
            }
        }
        // �޼� ���� ��ġ Ȯ��
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
            // ��� �ʿ��� ������ �����ǰ� �ִ��� Ȯ��
            if (jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
                jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex])
            {
                Vector3 neckToHips = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
                Vector3 rightToLeft = jointsPos[rightShoulderIndex] - jointsPos[leftShoulderIndex];

                gestureData.tagVector2.x = rightToLeft.x; // * 1.2f;
                gestureData.tagVector2.y = neckToHips.y; // * 1.2f;

                // �����տ� ���� ��ũ�� ��ġ ����
                if (gestureData.joint == rightHandIndex)
                {
                    gestureData.tagVector.x = jointsPos[rightShoulderIndex].x - gestureData.tagVector2.x / 2;
                    gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                }
                // �޼տ� ���� ��ũ�� ��ġ ����
                else
                {
                    gestureData.tagVector.x = jointsPos[leftShoulderIndex].x - gestureData.tagVector2.x / 2;
                    gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                }
            }

            // ��ũ�� ��ġ ���
            if (gestureData.tagVector2.x != 0 && gestureData.tagVector2.y != 0)
            {
                Vector3 relHandPos = handPos - gestureData.tagVector;
                gestureData.screenPos.x = Mathf.Clamp01(relHandPos.x / gestureData.tagVector2.x);
                gestureData.screenPos.y = Mathf.Clamp01(relHandPos.y / gestureData.tagVector2.y);
            }
        }
    }

    /// <summary>
    /// �� ���͸� �����մϴ�.
    /// </summary>
    /// <param name="userId">����� ID</param>
    /// <param name="gestureData">����ó ������</param>
    /// <param name="initialZoom">�ʱ� �� ��</param>
    /// <param name="jointsPos">���� ��ġ �迭</param>
    /// <param name="jointsTracked">���� ���� ���� �迭</param>
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
    /// ����ó ���¸� Ȯ���մϴ�.
    /// </summary>
    /// <param name="userId">����� ID</param>
    /// <param name="gestureData">����ó ������</param>
    /// <param name="timestamp">���� Ÿ�ӽ�����</param>
    /// <param name="jointsPos">���� ��ġ �迭</param>
    /// <param name="jointsTracked">���� ���� ���� �迭</param>
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
            // �������� �ø��� ����ó Ȯ��
            case Gestures.RaiseRightHand:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ����
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }
                        break;

                    case 1:  // ����ó �Ϸ�
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // �޼��� �ø��� ����ó Ȯ��
            case Gestures.RaiseLeftHand:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ����
                        if (jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                        }
                        break;

                    case 1:  // ����ó �Ϸ�
                        bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // Psi ����ó Ȯ��
            case Gestures.Psi:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ����
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                           jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                           (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                        }
                        break;

                    case 1:  // ����ó �Ϸ�
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                            jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f;

                        Vector3 jointPos = jointsPos[gestureData.joint];
                        CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.PoseCompleteDuration);
                        break;
                }
                break;

            // T-���� Ȯ��
            case Gestures.Tpose:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ����
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

                    case 1:  // ����ó �Ϸ�
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

            // ���� ����ó Ȯ��
            case Gestures.Stop:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ����
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

                    case 1:  // ����ó �Ϸ�
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

            // ���� ���� ����ó Ȯ��
            case Gestures.Wave:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó - �ܰ� 2
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                    case 2:  // ����ó �ܰ� 3 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // Ŭ�� ����ó Ȯ��
            case Gestures.Click:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                           (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.3f;

                            // ���� ��Ȯ�� Ŭ�� ��ġ���� ��ũ�� ��ġ ����
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                        }
                        else if (jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.3f;

                            // ���� ��Ȯ�� Ŭ�� ��ġ���� ��ũ�� ��ġ ����
                            SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
                        }
                        break;

                    case 1:
                        {
                            // ���ڸ��� �ִ��� Ȯ��
                            Vector3 distVector = jointsPos[gestureData.joint] - gestureData.jointPos;
                            bool isInPose = distVector.magnitude < 0.05f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectWrapper.Constants.ClickStayDuration);
                        }
                        break;
                }
                break;

            // �������� �������� ����ó Ȯ��
            case Gestures.SwipeLeft:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
                        if (jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                            jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                            jointsPos[rightHandIndex].x <= gestureRight && jointsPos[rightHandIndex].x > gestureLeft)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.1f;
                        }
                        break;

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // ���������� �������� ����ó Ȯ��
            case Gestures.SwipeRight:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
                        if (jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                            jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                            jointsPos[leftHandIndex].x >= gestureLeft && jointsPos[leftHandIndex].x < gestureRight)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.1f;
                        }
                        break;

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // ���� �������� ����ó Ȯ��
            case Gestures.SwipeUp:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // �Ʒ��� �������� ����ó Ȯ��
            case Gestures.SwipeDown:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // ������ Ŀ�� ����ó Ȯ��
            case Gestures.RightHandCursor:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1 (������)
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
                            // ����ó ���
                            gestureData.progress = 0f;
                        }
                        break;

                }
                break;

            // �޼� Ŀ�� ����ó Ȯ��
            case Gestures.LeftHandCursor:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1 (������)
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
                            // ����ó ���
                            gestureData.progress = 0f;
                        }
                        break;

                }
                break;

            // �� �ƿ� ����ó Ȯ��
            case Gestures.ZoomOut:
                Vector3 vectorZoomOut = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distZoomOut = vectorZoomOut.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = ��
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // �� �� ����ó Ȯ��
            case Gestures.ZoomIn:
                Vector3 vectorZoomIn = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distZoomIn = vectorZoomIn.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = ��
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // �� ����ó Ȯ��
            case Gestures.Wheel:
                Vector3 vectorWheel = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                float distWheel = vectorWheel.magnitude;

                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = �� ȸ��
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
                                gestureData.screenPos.z = angle;  // �� ����
                                gestureData.timestamp = timestamp;
                                gestureData.tagFloat = distWheel;
                                gestureData.progress = 0.7f;
                            }
                        }
                        else
                        {
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;

                }
                break;

            // ���� ����ó Ȯ��
            case Gestures.Jump:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
                        if (jointsTracked[hipCenterIndex] &&
                            (jointsPos[hipCenterIndex].y > 0.9f) && (jointsPos[hipCenterIndex].y < 1.3f))
                        {
                            SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // ����Ʈ ����ó Ȯ��
            case Gestures.Squat:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
                        if (jointsTracked[hipCenterIndex] &&
                            (jointsPos[hipCenterIndex].y <= 0.9f))
                        {
                            SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // �б� ����ó Ȯ��
            case Gestures.Push:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

            // ���� ����ó Ȯ��
            case Gestures.Pull:
                switch (gestureData.state)
                {
                    case 0:  // ����ó ���� - �ܰ� 1
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

                    case 1:  // ����ó �ܰ� 2 = �Ϸ�
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
                            // ����ó ���
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;

                // ���� �߰����� ����ó ���̽��� �� �� �ֽ��ϴ�.
        }
    }
}