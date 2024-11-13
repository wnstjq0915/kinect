using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/**
 * <summary>
 * KinectManager Ŭ������ Kinect v1 ������ �̿��Ͽ� ����� ���̷��� �����͸� �����ϰ�,
 * �̸� ������� �ƹ�Ÿ�� ������ �� ����ó�� ó���ϴ� ����� �����մϴ�.
 * </summary>
 */
public class KinectManager : MonoBehaviour
{
    /*
    KinectManager Ŭ������ Kinect v1 ������ �̿��Ͽ� ����� ���̷��� �����͸� �����ϰ�,
    �̸� ������� �ƹ�Ÿ�� ������ �� ����ó�� ó���ϴ� ����� �����ϴ� Ŭ�����Դϴ�.
    �� Ŭ������ �ֿ� ��ɰ� ��Ҹ� ����ϸ� ������ �����ϴ�.

    �ֿ� ���
        Kinect �ʱ�ȭ:
            Awake() �޼ҵ忡�� Kinect ������ �ʱ�ȭ�ϰ�, ���̷��� Ʈ��ŷ�� �����մϴ�.
            ���� �� ���� ��Ʈ���� Ȱ��ȭ�ϰ�, Kinect�� ���� ������ �����մϴ�.

        ����� ���� �� Ʈ��ŷ:
            ������� ���̷��� �����͸� �ǽð����� �����ϰ� ó���մϴ�.
            ����ڰ� �����Ǹ� �ش� ������� ID�� ������ �����մϴ�.

        ���̷��� ������ ó��:
            Update() �޼ҵ忡�� Kinect ����ڷκ��� ���� �� ���� �����͸� ������ ������Ʈ�մϴ�.
            ���̷��� �����͸� ó���ϰ� �� ������ ��ġ�� ȸ���� ����Ͽ� �ƹ�Ÿ�� �����մϴ�.

        ����ó �ν�:
            Ư�� ����ó�� �����ϰ�, �̸� ������� �پ��� ������ �����մϴ�.
            ������� ����ó ������� �����ϰ�, �Ϸ�� ����ó�� ���� ó���� �����մϴ�.
    
        ����� �� ���:
            ����ڰ� ������ ���� �����͸� ������� ����� �ʰ� ���� ���� ����Ͽ� GUI�� ǥ���մϴ�.

        ���� ����:
            �ƹ�Ÿ�� ���¸� �����ϸ�, �ƹ�Ÿ�� ��ġ, ȸ��, �� ���̷����� ������Ʈ�մϴ�.
            ����ڿ��� ��ȣ�ۿ��� ���� �پ��� ������ �����մϴ�.

    Ŭ���� ���� ���
        ����:
            �پ��� ������ ���� ���� �� ����� �������� ���ǵǾ� ������,
            �̸� ���� Kinect ������� ��, ���̷��� ������ ó�� ���, GUI ǥ�� ���� ���� ������ �� �ֽ��ϴ�.

        �޼ҵ�:
            Kinect �ʱ�ȭ, ���̷��� ������ ó��, ����ó ���� �� ������Ʈ�ϴ� ���� �޼ҵ���� ���ԵǾ� �ֽ��ϴ�.
            �ֿ� �޼ҵ�� Awake(), Update(), ProcessSkeleton(), UpdateUserMap(), DetectGesture() ���Դϴ�.

        �̱��� ����:
            KinectManager Ŭ������ �̱��� �������� �����Ǿ� �־�, Ŭ������ �ν��Ͻ��� �ϳ��� �����ϵ��� �����մϴ�.
            �̸� ���� ���������� KinectManager�� ������ �� �ֽ��ϴ�.

    �� Ŭ������ Kinect ������ ���� ������� �������� �����ϰ�,
    �̸� ������� �ƹ�Ÿ�� �����ϸ�,
    ����ó �ν��� ���� �پ��� ��ȣ�ۿ��� �����ϰ� �ϴ� �ٽ� ������ �մϴ�.
    �� Ŭ������ ������ �޼ҵ带 ���������ν� Kinect ��� �����̳� ���ø����̼� ���߿� �ʿ��� �پ��� ����� ������ �� �ֽ��ϴ�.
    */


    // ������ �ɼ��� �����ϴ� ������
    public enum Smoothing : int { None, Default, Medium, Aggressive }

    // �� ����ڰ� �ִ��� ���θ� �����ϴ� ���� ����
    public bool TwoUsers = false;

    // ����� ���� ����� ������ ���θ� �����ϴ� ���� ����
    public bool ComputeUserMap = false;

    // ���� ���� ����� ������ ���θ� �����ϴ� ���� ����
    public bool ComputeColorMap = false;

    // ����� ���� GUI�� ǥ���� ������ ���θ� �����ϴ� ���� ����
    public bool DisplayUserMap = false;

    // ���� ���� GUI�� ǥ���� ������ ���θ� �����ϴ� ���� ����
    public bool DisplayColorMap = false;

    // ����� �ʿ��� ���̷��� ���� ǥ���� ������ ���θ� �����ϴ� ���� ����
    public bool DisplaySkeletonLines = false;

    // �̹����� �ʺ� �����ϴ� ���� ���� (ī�޶� �ʺ��� ������)
    public float DisplayMapsWidthPercent = 20f;

    // ������ ���̸� �����ϴ� ���� ���� (���� ����)
    public float SensorHeight = 1.0f;

    // ������ ���� ������ �����ϴ� ���� ���� (�� ����)
    public int SensorAngle = 0;

    // ����� ���̷��� �����͸� ó���ϱ� ���� �ּ� �Ÿ�
    public float MinUserDistance = 1.0f;

    // ����� ���̷��� �������� �ִ� �Ÿ� (0�� ���� ����)
    public float MaxUserDistance = 0f;

    // ���� ����� ����ڸ� �������� ���θ� �����ϴ� ���� ����
    public bool DetectClosestUser = true;

    // ������ ������ ������ ������ ���θ� �����ϴ� ���� ����
    public bool IgnoreInferredJoints = true;

    // ������ �Ű����� ����
    public Smoothing smoothing = Smoothing.Default;

    // �߰� ���� ��� ���θ� �����ϴ� ���� ����
    public bool UseBoneOrientationsFilter = false;
    public bool UseClippedLegsFilter = false;
    public bool UseBoneOrientationsConstraint = true;
    public bool UseSelfIntersectionConstraint = false;

    // �� �÷��̾��� �ƹ�Ÿ�� ������ GameObject ����Ʈ
    public List<GameObject> Player1Avatars;
    public List<GameObject> Player2Avatars;

    // �� �÷��̾��� ���� ���� ����
    public KinectGestures.Gestures Player1CalibrationPose;
    public KinectGestures.Gestures Player2CalibrationPose;

    // �� �÷��̾��� ������ ����ó ����Ʈ
    public List<KinectGestures.Gestures> Player1Gestures;
    public List<KinectGestures.Gestures> Player2Gestures;

    // ����ó ���� �� �ּ� �ð�
    public float MinTimeBetweenGestures = 0.7f;

    // ����ó ������ ����Ʈ
    public List<MonoBehaviour> GestureListeners;

    // GUI �޽����� ǥ���� GUIText
    public GUIText CalibrationText;

    // �÷��̾� 1 �� 2�� �� Ŀ���� ǥ���� GUI Texture
    public GameObject HandCursor1;
    public GameObject HandCursor2;

    // ���콺 Ŀ���� Ŭ�� ����ó�� ���콺 Ŀ���� �������� ����
    public bool ControlMouseCursor = false;

    // ����ó ����� �޽����� ǥ���� GUIText
    public GUIText GesturesDebugText;

    // Kinect �ʱ�ȭ ���θ� �����ϴ� ����� ����
    private bool KinectInitialized = false;

    // ������ �÷��̾� ���� ���θ� �����ϴ� ����� ����
    private bool Player1Calibrated = false;
    private bool Player2Calibrated = false;

    // ��� �÷��̾ �����Ǿ����� ���θ� �����ϴ� ����� ����
    private bool AllPlayersCalibrated = false;

    // Player 1 �� Player 2�� ID�� �����ϴ� ����
    private uint Player1ID;
    private uint Player2ID;

    // Player 1 �� Player 2�� �ε��� ����
    private int Player1Index;
    private int Player2Index;

    // �ƹ�Ÿ ��Ʈ�ѷ� ����Ʈ
    private List<AvatarController> Player1Controllers;
    private List<AvatarController> Player2Controllers;

    // ����� �� ���� ����
    private Texture2D usersLblTex;
    private Color32[] usersMapColors;
    private ushort[] usersPrevState;
    private Rect usersMapRect;
    private int usersMapSize;

    // ���� �� ���� ����
    private Texture2D usersClrTex;
    private Rect usersClrRect;

    // ����� ���� ��
    private ushort[] usersDepthMap;
    private float[] usersHistogramMap;

    // ��� ����� ����Ʈ
    private List<uint> allUsers;

    // Kinect�� �̹��� ��Ʈ�� �ڵ�
    private IntPtr colorStreamHandle;
    private IntPtr depthStreamHandle;

    // ���� �̹��� ������
    private Color32[] colorImage;
    private byte[] usersColorMap;

    // ���̷��� ���� ����ü
    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    private KinectWrapper.NuiTransformSmoothParameters smoothParameters;
    private int player1Index, player2Index;

    // �÷��̾��� ��ġ �� ����
    private Vector3 player1Pos, player2Pos;
    private Matrix4x4 player1Ori, player2Ori;
    private bool[] player1JointsTracked, player2JointsTracked;
    private bool[] player1PrevTracked, player2PrevTracked;
    private Vector3[] player1JointsPos, player2JointsPos;
    private Matrix4x4[] player1JointsOri, player2JointsOri;
    private KinectWrapper.NuiSkeletonBoneOrientation[] jointOrientations;

    // ���� ����ó ������
    private KinectGestures.GestureData player1CalibrationData;
    private KinectGestures.GestureData player2CalibrationData;

    // ����ó ������ ����Ʈ
    private List<KinectGestures.GestureData> player1Gestures = new List<KinectGestures.GestureData>();
    private List<KinectGestures.GestureData> player2Gestures = new List<KinectGestures.GestureData>();

    // ����ó ���� ���� �ð�
    private float[] gestureTrackingAtTime;

    // ����ó ������ ����Ʈ
    public List<KinectGestures.GestureListenerInterface> gestureListeners;

    // Kinect �������� ���� �������� ��ȯ�ϴ� ��Ʈ����
    private Matrix4x4 kinectToWorld, flipMatrix;
    private static KinectManager instance;

    // ���͸��� ���õ� Ÿ�̸�
    private float lastNuiTime;

    // ���� ����
    private TrackingStateFilter[] trackingStateFilter;
    private BoneOrientationsFilter[] boneOrientationFilter;
    private ClippedLegsFilter[] clippedLegsFilter;
    private BoneOrientationsConstraint boneConstraintsFilter;
    private SelfIntersectionConstraint selfIntersectionConstraint;

    // �̱��� �ν��Ͻ��� ��ȯ
    public static KinectManager Instance
    {
        get
        {
            return instance;
        }
    }

    // Kinect�� �ʱ�ȭ�Ǿ����� Ȯ��
    public static bool IsKinectInitialized()
    {
        return instance != null ? instance.KinectInitialized : false;
    }

    // Kinect�� �ʱ�ȭ�Ǿ����� Ȯ��
    public bool IsInitialized()
    {
        return KinectInitialized;
    }

    // ���������� AvatarController�� ���� ���Ǵ� �Լ�
    public static bool IsCalibrationNeeded()
    {
        return false;
    }

    // ���� ����/����� �����͸� ��ȯ (ComputeUserMap�� true�� ��)
    /// <summary>
    /// GetRawDepthMap �޼ҵ�� ���� ����/����� �����͸� ��ȯ�մϴ�.
    /// ComputeUserMap�� true�� ���� ��ȿ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ������ �迭</returns>
    public ushort[] GetRawDepthMap()
    {
        return usersDepthMap;
    }

    // Ư�� �ȼ��� ���� ���� �����͸� ��ȯ (ComputeUserMap�� true�� ��)
    /// <summary>
    /// GetDepthForPixel �޼ҵ�� Ư�� �ȼ��� ���� ���� �����͸� ��ȯ�մϴ�.
    /// ComputeUserMap�� true�� ���� ��ȿ�մϴ�.
    /// </summary>
    /// <param name="x">�ȼ��� x��ǥ</param>
    /// <param name="y">�ȼ��� y��ǥ</param>
    /// <returns>�־��� �ȼ��� ���� ��</returns>
    public ushort GetDepthForPixel(int x, int y)
    {
        int index = y * KinectWrapper.Constants.DepthImageWidth + x;

        if (index >= 0 && index < usersDepthMap.Length)
            return usersDepthMap[index];
        else
            return 0;
    }

    // 3D ���� ��ġ�� ���� ���� �� ��ġ�� ��ȯ
    public Vector2 GetDepthMapPosForJointPos(Vector3 posJoint)
    {
        Vector3 vDepthPos = KinectWrapper.MapSkeletonPointToDepthPoint(posJoint);
        Vector2 vMapPos = new Vector2(vDepthPos.x, vDepthPos.y);

        return vMapPos;
    }

    // ���� 2D ��ġ�� ���� ���� �� ��ġ�� ��ȯ
    public Vector2 GetColorMapPosForDepthPos(Vector2 posDepth)
    {
        int cx, cy;

        // �ӽ� ����ü ����
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        // ���� �ȼ��κ��� ���� �ȼ� ��ǥ�� ������
        KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
            KinectWrapper.Constants.ColorImageResolution,
            KinectWrapper.Constants.DepthImageResolution,
            ref pcViewArea,
            (int)posDepth.x, (int)posDepth.y, GetDepthForPixel((int)posDepth.x, (int)posDepth.y),
            out cx, out cy);

        return new Vector2(cx, cy);
    }

    // ����� ���̺� �ؽ�ó ��ȯ (ComputeUserMap�� true�� ��)
    public Texture2D GetUsersLblTex()
    {
        return usersLblTex;
    }

    // ����� ���� �ؽ�ó ��ȯ (ComputeColorMap�� true�� ��)
    public Texture2D GetUsersClrTex()
    {
        return usersClrTex;
    }

    // �ּ��� �ϳ��� ����ڰ� �����Ǿ����� Ȯ��
    /// <summary>
    /// IsUserDetected �޼ҵ�� �ּ��� �ϳ��� ����ڰ� �����Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <returns>����ڰ� �����Ǿ����� true, �׷��� ������ false</returns>
    public bool IsUserDetected()
    {
        return KinectInitialized && (allUsers.Count > 0);
    }

    // Player1�� UserID�� ��ȯ (�������� ������ 0)
    /// <summary>
    /// GetPlayer1ID �޼ҵ�� Player 1�� UserID�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>Player 1�� UserID �Ǵ� 0 (�������� ������)</returns>
    public uint GetPlayer1ID()
    {
        return Player1ID;
    }

    // Player2�� UserID�� ��ȯ (�������� ������ 0)
    /// <summary>
    /// GetPlayer2ID �޼ҵ�� Player 2�� UserID�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>Player 2�� UserID �Ǵ� 0 (�������� ������)</returns>
    public uint GetPlayer2ID()
    {
        return Player2ID;
    }

    // Player1�� �ε����� ��ȯ (�������� ������ 0)
    public int GetPlayer1Index()
    {
        return Player1Index;
    }

    // Player2�� �ε����� ��ȯ (�������� ������ 0)
    public int GetPlayer2Index()
    {
        return Player2Index;
    }

    // �־��� UserId�� ����ڰ� �����Ǿ����� ���θ� ��ȯ
    /// <summary>
    /// IsPlayerCalibrated �޼ҵ�� �־��� ����ڰ� �����Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="UserId">������� ID</param>
    /// <returns>����ڰ� �����Ǿ����� true, �׷��� ������ false</returns>
    public bool IsPlayerCalibrated(uint UserId)
    {
        if (UserId == Player1ID)
            return Player1Calibrated;
        else if (UserId == Player2ID)
            return Player2Calibrated;

        return false;
    }

    // ���� ����� ���� ��ġ�� ��ȯ (Kinect �������� ��ȯ�� ���)
    public Vector3 GetRawSkeletonJointPos(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player1Index].SkeletonPositions[joint] : Vector3.zero;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsPos.Length ? (Vector3)skeletonFrame.SkeletonData[player2Index].SkeletonPositions[joint] : Vector3.zero;

        return Vector3.zero;
    }

    // ������� ��ġ�� ��ȯ (Kinect ������ �����, ���� ����)
    /// <summary>
    /// GetUserPosition �޼ҵ�� �־��� ������� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="UserId">������� ID</param>
    /// <returns>������� ��ġ(Vector3)</returns>
    public Vector3 GetUserPosition(uint UserId)
    {
        if (UserId == Player1ID)
            return player1Pos;
        else if (UserId == Player2ID)
            return player2Pos;

        return Vector3.zero;
    }

    // ������� ȸ���� ��ȯ (Kinect ������ �����)
    public Quaternion GetUserOrientation(uint UserId, bool flip)
    {
        if (UserId == Player1ID && player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
            return ConvertMatrixToQuat(player1Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
        else if (UserId == Player2ID && player2JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
            return ConvertMatrixToQuat(player2Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);

        return Quaternion.identity;
    }

    // Ư�� ������ �����ǰ� �ִ��� ���θ� ��ȯ
    public bool IsJointTracked(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsTracked.Length ? player1JointsTracked[joint] : false;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsTracked.Length ? player2JointsTracked[joint] : false;

        return false;
    }

    // Ư�� ������� ���� ��ġ�� ��ȯ (Kinect ������ �����, ���� ����)
    public Vector3 GetJointPosition(uint UserId, int joint)
    {
        if (UserId == Player1ID)
            return joint >= 0 && joint < player1JointsPos.Length ? player1JointsPos[joint] : Vector3.zero;
        else if (UserId == Player2ID)
            return joint >= 0 && joint < player2JointsPos.Length ? player2JointsPos[joint] : Vector3.zero;

        return Vector3.zero;
    }

    // �θ� ������ ���� ������ ���� ��ġ�� ��ȯ (Kinect ������ �����, ���� ����)
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

    // Ư�� ������ ȸ���� ��ȯ (Kinect ������ �����)
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

    // Ư�� ������ ���� ȸ���� ��ȯ (�θ� ������ �����)
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

    // �⺻ ������ ���� ���� ���� ������ ��ȯ
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

        // ���� ���͸� �ø�
        if (jointDir != Vector3.zero)
        {
            if (flipX)
                jointDir.x = -jointDir.x;

            if (flipZ)
                jointDir.z = -jointDir.z;
        }

        return jointDir;
    }

    // �־��� ����ڿ� ���� ����ó�� ����
    /// <summary>
    /// DetectGesture �޼ҵ�� Ư�� ����ó�� �����ϰ� �̸� ������� �پ��� ������ �����մϴ�.
    /// ������� ����ó ������� �����ϰ�, �Ϸ�� ����ó�� ���� ó���� �����մϴ�.
    /// </summary>
    /// <param name="UserId">����ó�� ������ ����� ID</param>
    /// <param name="gesture">������ ����ó ����</param>
    public void DetectGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index >= 0)
            DeleteGesture(UserId, gesture);

        // ����ó ������ ����
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

        // ����ó�� ���� Ȯ���� ����ó �߰�
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

        // ����� ID�� ���� ������ ����ó ����Ʈ�� �߰�
        if (UserId == Player1ID)
            player1Gestures.Add(gestureData);
        else if (UserId == Player2ID)
            player2Gestures.Add(gestureData);
    }

    // Ư�� ������� ����ó ���¸� ����
    public bool ResetGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index < 0)
            return false;

        // �ش� ����ó �����͸� �ʱ�ȭ
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

    // ��� ����ó�� ���¸� ����
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

    // �־��� ����ó�� ����
    public bool DeleteGesture(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        if (index < 0)
            return false;

        // �ش� ����� ����Ʈ���� ����ó ����
        if (UserId == Player1ID)
            player1Gestures.RemoveAt(index);
        else if (UserId == Player2ID)
            player2Gestures.RemoveAt(index);

        return true;
    }

    // Ư�� ������� ������ ����ó ����Ʈ�� ����
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

    // Ư�� ������� ������ ����ó ���� ��ȯ
    public int GetGesturesCount(uint UserId)
    {
        if (UserId == Player1ID)
            return player1Gestures.Count;
        else if (UserId == Player2ID)
            return player2Gestures.Count;

        return 0;
    }

    // Ư�� ������� ������ ����ó ����Ʈ�� ��ȯ
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

    // Ư�� ������� ������ ����ó�� �ִ��� ���θ� ��ȯ
    public bool IsGestureDetected(uint UserId, KinectGestures.Gestures gesture)
    {
        int index = GetGestureIndex(UserId, gesture);
        return index >= 0;
    }

    // Ư�� ������� ����ó�� �Ϸ�Ǿ����� ���θ� ��ȯ
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

    // Ư�� ������� ����ó�� ��ҵǾ����� ���θ� ��ȯ
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

    // Ư�� ������� ����ó ������� ��ȯ
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

    // Ư�� ������� ����ó�� ���� ���� "ȭ�� ��ġ"�� ��ȯ
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

    // ����ó ������ ����Ʈ�� �缳��
    public void ResetGestureListeners()
    {
        // ����ó ������ ����Ʈ ����
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

    // �ƹ�Ÿ ��Ʈ�ѷ� ����Ʈ�� �缳��
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

        // Player1 �ƹ�Ÿ ��Ʈ�ѷ� �ʱ�ȭ
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

        // Player2 �ƹ�Ÿ ��Ʈ�ѷ� �ʱ�ȭ
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

    // ���� ������ Kinect ����ڸ� ����
    public void ClearKinectUsers()
    {
        if (!KinectInitialized)
            return;

        // ���� ����� ����
        for (int i = allUsers.Count - 1; i >= 0; i--)
        {
            uint userId = allUsers[i];
            RemoveUser(userId);
        }

        ResetFilters();
    }

    // Kinect�� ���۸� ����� ���͸� �缳��
    public void ResetFilters()
    {
        if (!KinectInitialized)
            return;

        // Kinect ���� �ʱ�ȭ
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

        // �� ���� �ʱ�ȭ
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

    //----------------------------------- public �Լ��� �� --------------------------------------//

    void Awake()
    {
        int hr = 0;

        try
        {
            // Kinect �ʱ�ȭ
            hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton |
                KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex |
                (ComputeColorMap ? KinectWrapper.NuiInitializeFlags.UsesColor : 0));
            if (hr != 0)
            {
                throw new Exception("NuiInitialize Failed");
            }

            // ���̷��� Ʈ��ŷ Ȱ��ȭ
            hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);
            if (hr != 0)
            {
                throw new Exception("Cannot initialize Skeleton Data");
            }

            // ���� ��Ʈ�� �ڵ� �ʱ�ȭ
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

            // ���� ��Ʈ�� �ڵ� �ʱ�ȭ
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

            // Kinect�� ���� ���� ����
            KinectWrapper.NuiCameraElevationSetAngle(SensorAngle);

            // ���̷��� ����ü �ʱ�ȭ
            skeletonFrame = new KinectWrapper.NuiSkeletonFrame()
            {
                SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount]
            };

            // ������ �Լ��� ����� �� ����
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

            // Ʈ��ŷ ���� ���� �ʱ�ȭ
            trackingStateFilter = new TrackingStateFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < trackingStateFilter.Length; i++)
            {
                trackingStateFilter[i] = new TrackingStateFilter();
                trackingStateFilter[i].Init();
            }

            // �� ���� ���� �ʱ�ȭ
            boneOrientationFilter = new BoneOrientationsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < boneOrientationFilter.Length; i++)
            {
                boneOrientationFilter[i] = new BoneOrientationsFilter();
                boneOrientationFilter[i].Init();
            }

            // �߸� �ٸ� ���� �ʱ�ȭ
            clippedLegsFilter = new ClippedLegsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < clippedLegsFilter.Length; i++)
            {
                clippedLegsFilter[i] = new ClippedLegsFilter();
            }

            // �� ���� ���� �ʱ�ȭ
            boneConstraintsFilter = new BoneOrientationsConstraint();
            boneConstraintsFilter.AddDefaultConstraints();
            selfIntersectionConstraint = new SelfIntersectionConstraint();

            // ���� ��ġ �� ���� �迭 ����
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

            // Kinect �������� ���� ���� ��ȯ ��Ʈ���� ����
            Quaternion quatTiltAngle = new Quaternion();
            quatTiltAngle.eulerAngles = new Vector3(-SensorAngle, 0.0f, 0.0f);

            // ��ȯ ��Ʈ���� ���� (Kinect���� �����)
            kinectToWorld.SetTRS(new Vector3(0.0f, SensorHeight, 0.0f), quatTiltAngle, Vector3.one);
            flipMatrix = Matrix4x4.identity;
            flipMatrix[2, 2] = -1;

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        catch (DllNotFoundException e)
        {
            string message = "Kinect SDK ��ġ�� Ȯ���Ͻʽÿ�.";
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

        // ����� �� ���� �ʱ�ȭ
        if (ComputeUserMap)
        {
            usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
            usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
            usersMapColors = new Color32[usersMapSize];
            usersPrevState = new ushort[usersMapSize];
            usersDepthMap = new ushort[usersMapSize];
            usersHistogramMap = new float[8192];
        }

        // ���� �� ���� �ʱ�ȭ
        if (ComputeColorMap)
        {
            usersClrTex = new Texture2D(KinectWrapper.GetColorWidth(), KinectWrapper.GetColorHeight());
            colorImage = new Color32[KinectWrapper.GetColorWidth() * KinectWrapper.GetColorHeight()];
            usersColorMap = new byte[colorImage.Length << 2];
        }

        // �ƹ�Ÿ ��Ʈ�ѷ� �ڵ� �˻�
        if (Player1Avatars.Count == 0 && Player2Avatars.Count == 0)
        {
            AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];

            foreach (AvatarController avatar in avatars)
            {
                Player1Avatars.Add(avatar.gameObject);
            }
        }

        // ��� ����� ����Ʈ �ʱ�ȭ
        allUsers = new List<uint>();

        // �ƹ�Ÿ ��Ʈ�ѷ� ����Ʈ �ʱ�ȭ
        Player1Controllers = new List<AvatarController>();
        Player2Controllers = new List<AvatarController>();

        // �� �÷��̾��� �ƹ�Ÿ ��Ʈ�ѷ� �߰�
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

        // ����ó ������ ����Ʈ ����
        gestureListeners = new List<KinectGestures.GestureListenerInterface>();

        foreach (MonoBehaviour script in GestureListeners)
        {
            if (script && (script is KinectGestures.GestureListenerInterface))
            {
                KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
                gestureListeners.Add(listener);
            }
        }

        // GUI �ؽ�Ʈ �ʱ�ȭ
        if (CalibrationText != null)
        {
            CalibrationText.GetComponent<GUIText>().text = "����ڸ� ��ٸ��� ��...";
        }

        Debug.Log("����ڸ� ��ٸ��� ��...");

        KinectInitialized = true;
    }

    /// <summary>
    /// Update �޼ҵ�� �� �����Ӹ��� ȣ��Ǹ�, ����ڷκ��� ���� �� ���� �����͸� �������� ������Ʈ�մϴ�.
    /// ���̷��� �����͸� ó���Ͽ� �� ������ ��ġ�� ȸ���� ����ϰ� �ƹ�Ÿ�� �����մϴ�.
    /// </summary>
    void Update()
    {
        if (KinectInitialized)
        {
            // ����� �� ������Ʈ
            if (ComputeUserMap)
            {
                if (depthStreamHandle != IntPtr.Zero &&
                    KinectWrapper.PollDepth(depthStreamHandle, KinectWrapper.Constants.IsNearMode, ref usersDepthMap))
                {
                    UpdateUserMap();
                }
            }

            // ���� �� ������Ʈ
            if (ComputeColorMap)
            {
                if (colorStreamHandle != IntPtr.Zero &&
                    KinectWrapper.PollColor(colorStreamHandle, ref usersColorMap, ref colorImage))
                {
                    UpdateColorMap();
                }
            }

            // ���̷��� ������ ������Ʈ
            if (KinectWrapper.PollSkeleton(ref smoothParameters, ref skeletonFrame))
            {
                ProcessSkeleton();
            }

            // �÷��̾� 1 �� ������Ʈ
            if (Player1Calibrated)
            {
                foreach (AvatarController controller in Player1Controllers)
                {
                    controller.UpdateAvatar(Player1ID);
                }

                // ����ó �Ϸ� Ȯ��
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

            // �÷��̾� 2 �� ������Ʈ
            if (Player2Calibrated)
            {
                foreach (AvatarController controller in Player2Controllers)
                {
                    controller.UpdateAvatar(Player2ID);
                }

                // ����ó �Ϸ� Ȯ��
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

        // ESC Ű�� ���� ���ø����̼� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // ���ø����̼� ���� �� Kinect ����
    /// <summary>
    /// OnApplicationQuit �޼ҵ�� ���ø����̼� ���� �� Kinect�� �����մϴ�.
    /// </summary>
    void OnApplicationQuit()
    {
        if (KinectInitialized)
        {
            // OpenNI ����
            KinectWrapper.NuiShutdown();
            instance = null;
        }
    }

    // GUI�� ������׷� �� �׸���
    void OnGUI()
    {
        if (KinectInitialized)
        {
            // ����� �� ǥ��
            if (ComputeUserMap && (/**(allUsers.Count == 0) ||*/ DisplayUserMap))
            {
                if (usersMapRect.width == 0 || usersMapRect.height == 0)
                {
                    // ���� ī�޶��� �簢�� ��������
                    Rect cameraRect = Camera.main.pixelRect;

                    // �ʿ� �� �ʺ�� ���̸� ������ ���
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
            // ���� �� ǥ��
            else if (ComputeColorMap && (DisplayColorMap))
            {
                if (usersClrRect.width == 0 || usersClrTex.height == 0)
                {
                    // ���� ī�޶��� �簢�� ��������
                    Rect cameraRect = Camera.main.pixelRect;

                    // �ʿ� �� �ʺ�� ���̸� ������ ���
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

    // ����� �� ������Ʈ
    /// <summary>
    /// UpdateUserMap �޼ҵ�� ����� ���� ������Ʈ�Ͽ� GUI�� ǥ���մϴ�.
    /// ���� �����͸� ������� ����� ���� �����ϰ� �����մϴ�.
    /// </summary>
    void UpdateUserMap()
    {
        int numOfPoints = 0;
        Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // ���̿� ���� ���� ������׷� ���
        for (int i = 0; i < usersMapSize; i++)
        {
            // ����ڰ� �ִ� ���̿� ���ؼ��� ���
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

        // ��ǥ ���ۿ� �ʿ��� ���� ����ü
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        // ���̺� �ʰ� ���� ������׷��� ������� ���� ����� �ؽ�ó ����
        Color32 clrClear = Color.clear;
        for (int i = 0; i < usersMapSize; i++)
        {
            // �ؽ�ó�� ������ ���̺� ���� ���� �迭�� ��ȯ
            int flipIndex = i; // usersMapSize - i - 1;

            ushort userMap = (ushort)(usersDepthMap[i] & 7);
            ushort userDepth = (ushort)(usersDepthMap[i] >> 3);

            ushort nowUserPixel = userMap != 0 ? (ushort)((userMap << 13) | userDepth) : userDepth;
            ushort wasUserPixel = usersPrevState[flipIndex];

            // ����� �ȼ��� �׸���
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
                                usersMapColors[flipIndex].a = 230; // ���� ����
                            }
                        }
                    }
                    else
                    {
                        // ���� ������׷��� ���� ȥ�� ���� ����
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

        // �׸���!
        usersLblTex.SetPixels32(usersMapColors);

        if (!DisplaySkeletonLines)
        {
            usersLblTex.Apply();
        }
    }

    // ���� �� ������Ʈ
    void UpdateColorMap()
    {
        usersClrTex.SetPixels32(colorImage);
        usersClrTex.Apply();
    }

    // ����� ID�� �÷��̾� 1 �Ǵ� 2�� �Ҵ�
    void CalibrateUser(uint UserId, int UserIndex, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
        // �÷��̾� 1�� �������� �ʾҴٸ�, �� ����� ID�� �Ҵ�
        if (!Player1Calibrated)
        {
            // �÷��̾� 2�� �Ǽ��� �÷��̾� 1�� �Ҵ����� �ʵ��� Ȯ��
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

                    // ������ ����ó �߰�
                    foreach (KinectGestures.Gestures gesture in Player1Gestures)
                    {
                        DetectGesture(UserId, gesture);
                    }

                    // ����ó �����ʿ��� ���ο� ����� �˸�
                    foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                    {
                        listener.UserDetected(UserId, 0);
                    }

                    // ���̷��� ���� �ʱ�ȭ
                    ResetFilters();

                    // �÷��̾� ���� ���� ��� �÷��̾ �����Ǿ����� Ȯ��
                    AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;
                }
            }
        }
        // �׷��� ������ �÷��̾� 2�� �Ҵ�
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

                    // ������ ����ó �߰�
                    foreach (KinectGestures.Gestures gesture in Player2Gestures)
                    {
                        DetectGesture(UserId, gesture);
                    }

                    // ����ó �����ʿ��� ���ο� ����� �˸�
                    foreach (KinectGestures.GestureListenerInterface listener in gestureListeners)
                    {
                        listener.UserDetected(UserId, 1);
                    }

                    // ���̷��� ���� �ʱ�ȭ
                    ResetFilters();

                    // ��� �÷��̾ �����Ǿ����� Ȯ��
                    AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;
                }
            }
        }

        // ��� �÷��̾ ������ ���, �� �̻� ã�� �ʵ��� ����
        if (AllPlayersCalibrated)
        {
            Debug.Log("��� �÷��̾ �����Ǿ����ϴ�.");

            if (CalibrationText != null)
            {
                CalibrationText.GetComponent<GUIText>().text = "";
            }
        }
    }

    // �Ҿ���� ����� ID ����
    void RemoveUser(uint UserId)
    {
        // �÷��̾� 1�� �Ҿ���� ���
        if (UserId == Player1ID)
        {
            // ID�� null�� �����ϰ�, �ش� ID�� ���õ� ��� ���� �缳��
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

        // �÷��̾� 2�� �Ҿ���� ���
        if (UserId == Player2ID)
        {
            // ID�� null�� �����ϰ�, �ش� ID�� ���õ� ��� ���� �缳��
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

        // �� ������� ����ó ����Ʈ�� ����
        ClearGestures(UserId);

        // �۷ι� ����� ����Ʈ���� ����
        allUsers.Remove(UserId);
        AllPlayersCalibrated = !TwoUsers ? allUsers.Count >= 1 : allUsers.Count >= 2;

        // ����� ��ü �õ�
        Debug.Log("����ڸ� ��ٸ��� ��...");

        if (CalibrationText != null)
        {
            CalibrationText.GetComponent<GUIText>().text = "����ڸ� ��ٸ��� ��...";
        }
    }

    // ���� ���
    private const int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
    private const int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;

    private int[] mustBeTrackedJoints = {
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
        (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
        (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
    };

    // ���̷��� ������ ó��
    /// <summary>
    /// ProcessSkeleton �޼ҵ�� ���̷��� �����͸� ó���Ͽ� ����� ������ ������Ʈ�մϴ�.
    /// �� ����ڿ� ���� ���̷��� ��ġ�� ���¸� �����մϴ�.
    /// </summary>
    void ProcessSkeleton()
    {
        List<uint> lostUsers = new List<uint>();
        lostUsers.AddRange(allUsers);

        // ������ ������Ʈ ���� ��� �ð� ���
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - lastNuiTime;

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
            uint userId = skeletonData.dwTrackingID;

            if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                // ���̷��� ��ġ ��������
                Vector3 skeletonPos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);

                if (!AllPlayersCalibrated)
                {
                    // ���� ����� ����� Ȯ��
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

                // �÷��̾� 1�� ������ ó��
                if (userId == Player1ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
                   (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
                {
                    player1Index = i;

                    // �÷��̾� ��ġ ��������
                    player1Pos = skeletonPos;

                    // Ʈ��ŷ ���� ���� ����
                    trackingStateFilter[0].UpdateFilter(ref skeletonData);

                    // �ƹ�Ÿ �ܰ� ����� ���� ���̷��� ����
                    if (UseClippedLegsFilter && clippedLegsFilter[0] != null)
                    {
                        clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);
                    }

                    if (UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
                    {
                        selfIntersectionConstraint.Constrain(ref skeletonData);
                    }

                    // ������ ��ġ�� ȸ�� ��������
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

                    // �ؽ�ó ���� ���̷��� �׸���
                    if (DisplaySkeletonLines && ComputeUserMap)
                    {
                        DrawSkeleton(usersLblTex, ref skeletonData, ref player1JointsTracked);
                        usersLblTex.Apply();
                    }

                    // ������ ���� ���
                    KinectWrapper.GetSkeletonJointOrientation(ref player1JointsPos, ref player1JointsTracked, ref player1JointsOri);

                    // ���� ���� ����
                    if (UseBoneOrientationsConstraint && boneConstraintsFilter != null)
                    {
                        boneConstraintsFilter.Constrain(ref player1JointsOri, ref player1JointsTracked);
                    }

                    // ���� ���� ����
                    if (UseBoneOrientationsFilter && boneOrientationFilter[0] != null)
                    {
                        boneOrientationFilter[0].UpdateFilter(ref skeletonData, ref player1JointsOri);
                    }

                    // �÷��̾� ȸ�� ��������
                    player1Ori = player1JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

                    // ����ó Ȯ��
                    if (Time.realtimeSinceStartup >= gestureTrackingAtTime[0])
                    {
                        int listGestureSize = player1Gestures.Count;
                        float timestampNow = Time.realtimeSinceStartup;
                        string sDebugGestures = string.Empty;  // "������ ����ó:\n";

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
                                    sDebugGestures += string.Format("{0} - ����: {1}, �ð�: {2:F1}, �����: {3}%\n",
                                                                    gestureData.gesture, gestureData.state,
                                                                    gestureData.timestamp,
                                                                    (int)(gestureData.progress * 100 + 0.5f));
                                }
                            }
                        }

                        if (GesturesDebugText)
                        {
                            sDebugGestures += string.Format("\n ���� ��: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft].ToString() : "");
                            sDebugGestures += string.Format("\n ������ ��: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight].ToString() : "");
                            sDebugGestures += string.Format("\n ���� �Ȳ�ġ: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft].ToString() : "");
                            sDebugGestures += string.Format("\n ������ �Ȳ�ġ: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight].ToString() : "");

                            sDebugGestures += string.Format("\n ���� ���: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft].ToString() : "");
                            sDebugGestures += string.Format("\n ������ ���: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight].ToString() : "");

                            sDebugGestures += string.Format("\n ��: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter].ToString() : "");
                            sDebugGestures += string.Format("\n ������: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter].ToString() : "");
                            sDebugGestures += string.Format("\n ���� ������: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft].ToString() : "");
                            sDebugGestures += string.Format("\n ������ ������: {0}", player1JointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight] ?
                                                                player1JointsPos[(int)KinectWrapper.NuiSkeletonPositionIndex.HipRight].ToString() : "");

                            GesturesDebugText.GetComponent<GUIText>().text = sDebugGestures;
                        }
                    }
                }
                // �÷��̾� 2 ������ ó��
                else if (userId == Player2ID && Mathf.Abs(skeletonPos.z) >= MinUserDistance &&
                        (MaxUserDistance <= 0f || Mathf.Abs(skeletonPos.z) <= MaxUserDistance))
                {
                    player2Index = i;

                    // �÷��̾� ��ġ ��������
                    player2Pos = skeletonPos;

                    // Ʈ��ŷ ���� ���� ����
                    trackingStateFilter[1].UpdateFilter(ref skeletonData);

                    // �ƹ�Ÿ �ܰ� ����� ���� ���̷��� ����
                    if (UseClippedLegsFilter && clippedLegsFilter[1] != null)
                    {
                        clippedLegsFilter[1].FilterSkeleton(ref skeletonData, deltaNuiTime);
                    }

                    if (UseSelfIntersectionConstraint && selfIntersectionConstraint != null)
                    {
                        selfIntersectionConstraint.Constrain(ref skeletonData);
                    }

                    // ������ ��ġ�� ȸ�� ��������
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

                    // �ؽ�ó ���� ���̷��� �׸���
                    if (DisplaySkeletonLines && ComputeUserMap)
                    {
                        DrawSkeleton(usersLblTex, ref skeletonData, ref player2JointsTracked);
                        usersLblTex.Apply();
                    }

                    // ������ ���� ���
                    KinectWrapper.GetSkeletonJointOrientation(ref player2JointsPos, ref player2JointsTracked, ref player2JointsOri);

                    // ���� ���� ����
                    if (UseBoneOrientationsConstraint && boneConstraintsFilter != null)
                    {
                        boneConstraintsFilter.Constrain(ref player2JointsOri, ref player2JointsTracked);
                    }

                    // ���� ���� ����
                    if (UseBoneOrientationsFilter && boneOrientationFilter[1] != null)
                    {
                        boneOrientationFilter[1].UpdateFilter(ref skeletonData, ref player2JointsOri);
                    }

                    // �÷��̾� ȸ�� ��������
                    player2Ori = player2JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

                    // ����ó Ȯ��
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

                // �Ҿ���� ����� ����
                lostUsers.Remove(userId);
            }
        }

        // NUI Ÿ�̸� ������Ʈ
        lastNuiTime = currentNuiTime;

        // �Ҿ���� ����� ����
        if (lostUsers.Count > 0)
        {
            foreach (uint userId in lostUsers)
            {
                RemoveUser(userId);
            }

            lostUsers.Clear();
        }
    }

    // �ؽ�ó�� ���̷��� �׸���
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

    // x1, y1�� x2, y2�� �����ϴ� ���� �ؽ�ó�� �׸��ϴ�.
    private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
    {
        int width = a_Texture.width;  // �ؽ�ó�� �ʺ�
        int height = a_Texture.height; // �ؽ�ó�� ����

        int dy = y2 - y1; // y ������ ��ȭ
        int dx = x2 - x1; // x ������ ��ȭ

        int stepy = 1;
        if (dy < 0)
        {
            dy = -dy;
            stepy = -1; // y ������ �����ϴ� ���
        }

        int stepx = 1;
        if (dx < 0)
        {
            dx = -dx;
            stepx = -1; // x ������ �����ϴ� ���
        }

        dy <<= 1; // dy�� �� ��� ����
        dx <<= 1; // dx�� �� ��� ����

        // ���� ��(x1, y1)���� �ȼ� ���� ����
        if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    a_Texture.SetPixel(x1 + x, y1 + y, a_Color);

        // �� �׸��� �˰���
        if (dx > dy)
        {
            int fraction = dy - (dx >> 1);

            while (x1 != x2)
            {
                if (fraction >= 0)
                {
                    y1 += stepy; // y�� ����
                    fraction -= dx; // fraction ������Ʈ
                }

                x1 += stepx; // x�� ����
                fraction += dy; // fraction ������Ʈ

                // ���� �ȼ��� �ؽ�ó ���� ���� �ִ��� Ȯ���ϰ� ���� ����
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
                    x1 += stepx; // x�� ����
                    fraction -= dy; // fraction ������Ʈ
                }

                y1 += stepy; // y�� ����
                fraction += dx; // fraction ������Ʈ

                // ���� �ȼ��� �ؽ�ó ���� ���� �ִ��� Ȯ���ϰ� ���� ����
                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
    }

    // ����� ���ʹϾ����� ��ȯ, �̷����� ���
    private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
    {
        Vector4 vZ = mOrient.GetColumn(2); // Z ��
        Vector4 vY = mOrient.GetColumn(1); // Y ��

        // flip ���ο� ���� ���� ����
        if (!flip)
        {
            vZ.y = -vZ.y; // Y �� ����
            vY.x = -vY.x; // X �� ����
            vY.z = -vY.z; // Z �� ����
        }
        else
        {
            vZ.x = -vZ.x; // X �� ����
            vZ.y = -vZ.y; // Y �� ����
            vY.z = -vY.z; // Z �� ����
        }

        // Z �� Y ���Ͱ� ��ȿ�� ��� ���ʹϾ� ��ȯ
        if (vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
            return Quaternion.LookRotation(vZ, vY);
        else
            return Quaternion.identity; // ��ȿ�� ���Ͱ� ���� ��� �⺻ ���ʹϾ� ��ȯ
    }

    // ����ó ����Ʈ���� ����ó�� �ε����� ��ȯ, ã�� ���ϸ� -1 ��ȯ
    private int GetGestureIndex(uint UserId, KinectGestures.Gestures gesture)
    {
        if (UserId == Player1ID)
        {
            int listSize = player1Gestures.Count;
            for (int i = 0; i < listSize; i++)
            {
                if (player1Gestures[i].gesture == gesture)
                    return i; // �ε��� ��ȯ
            }
        }
        else if (UserId == Player2ID)
        {
            int listSize = player2Gestures.Count;
            for (int i = 0; i < listSize; i++)
            {
                if (player2Gestures[i].gesture == gesture)
                    return i; // �ε��� ��ȯ
            }
        }

        return -1; // ����ó�� ã�� ���� ���
    }

    // ���� ���� ����ó�� �浹�ϴ��� Ȯ��
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
                        return true; // �浹 �߻�
                }
                else if (gestureData.userId == Player2ID)
                {
                    if (player2Gestures[index].progress > 0f)
                        return true; // �浹 �߻�
                }
            }
        }

        return false; // �浹 �� ��
    }

    // �־��� ����ڿ� ���� ���� ��� �Ϸ�Ǿ����� Ȯ��
    private bool CheckForCalibrationPose(uint userId, ref KinectGestures.Gestures calibrationGesture,
        ref KinectGestures.GestureData gestureData, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
        // ���� ����ó�� ������ �׻� true ��ȯ
        if (calibrationGesture == KinectGestures.Gestures.None)
            return true;

        // �ʿ� �� ����ó ������ �ʱ�ȭ
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

        // �ӽ� ���� ��ġ ��������
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

        // ���� ���� ���� Ȯ��
        for (int j = 0; j < skeletonJointsCount; j++)
        {
            jointsTracked[j] = Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
                (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked;

            if (jointsTracked[j])
            {
                jointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]); // ��ġ ��ȯ
            }
        }

        // ����ó ����� ����
        KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup,
            ref jointsPos, ref jointsTracked);

        // ����ó�� �Ϸ�Ǿ����� Ȯ��
        if (gestureData.complete)
        {
            gestureData.userId = 0; // ����� ID ����
            return true; // ���� ���� �Ϸ�
        }

        return false; // ���� ���� �̿Ϸ�
    }
}