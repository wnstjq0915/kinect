using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/**
 * <summary>
 * AvatarController Ŭ������ Kinect�� ����Ͽ� �ƹ�Ÿ�� �����Ӱ� ȸ���� �����մϴ�.
 * ������� ���̷��� �����Ϳ� ����Ͽ� �ƹ�Ÿ�� �� ���� ������Ʈ�մϴ�.
 * </summary>
 */
[RequireComponent(typeof(Animator))] // Animator ������Ʈ�� �ʿ����� ��Ÿ���� Ư��
public class AvatarController : MonoBehaviour
{
    /*
    �ֿ� ���� ���
        ����:
            mirroredMovement: �ƹ�Ÿ�� �������� �������� ���θ� �����ϴ� �Ҹ��� �����Դϴ�.
            verticalMovement: �ƹ�Ÿ�� ���� �̵��� ������� ���θ� �����ϴ� �����Դϴ�.
            moveRate: �ƹ�Ÿ�� �̵� �ӵ��� �����ϴ� �����Դϴ�.
            smoothFactor: �ƹ�Ÿ�� �������� �ε巴�� �ϱ� ���� ���� ����Դϴ�.
            bones: �ƹ�Ÿ�� ���� �����ϴ� �迭�Դϴ�.
            initialRotations: Kinect ���� ���� �� �� ���� �ʱ� ȸ���� �����ϴ� �迭�Դϴ�.

        �޼ҵ�:
            Awake(): �ƹ�Ÿ�� �ʱ� ������ �����ϰ�, ���� ������ �� �ʱ� ȸ���� �����ɴϴ�.
            UpdateAvatar(uint UserID): �� �����Ӹ��� �ƹ�Ÿ�� ������Ʈ�մϴ�.
                ������� ���̷��� �����Ϳ� ���� �ƹ�Ÿ�� ��ġ�� ȸ���� �����մϴ�.
            ResetToInitialPosition(): �ƹ�Ÿ�� ���� �ʱ� ��ġ�� ȸ������ �����մϴ�.
            SuccessfulCalibration(uint userId):
                ����ڰ� ���������� �����Ǿ��� �� ȣ��Ǿ� �ƹ�Ÿ�� ��ġ�� �����ϰ� �������� �纸���մϴ�.
            TransformBone(...): Kinect���� ������ ���� ȸ���� �ƹ�Ÿ�� ���� �����մϴ�.
            TransformSpecialBone(...): Ư�� ������ ���� ȸ���� �����մϴ�.
            MoveAvatar(uint UserID): �ƹ�Ÿ�� 3D �������� �̵���Ű��,
                ������� ô�� ��ġ�� ������� ��Ʈ�� �̵��մϴ�.
            MapBones(): �ƹ�Ÿ�� ���� Kinect ������ �����ϴ� ����� �����մϴ�.
            GetInitialRotations(): ���� �ʱ� ȸ���� ĸó�մϴ�.
            Kinect2AvatarRot(...): Kinect ���� ȸ���� �ƹ�Ÿ ���� ȸ������ ��ȯ�մϴ�.
            Kinect2AvatarPos(...): Kinect ��ġ�� �ƹ�Ÿ ���̷��� ��ġ�� ��ȯ�մϴ�.
    ��� ���
        �ƹ�Ÿ ����: Kinect �������� ���� ���̷��� �����͸� ������� �ƹ�Ÿ�� ���� ������Ʈ�Ͽ� ���� ������� �������� �ƹ�Ÿ�� �ݿ��մϴ�.
        �ʱ�ȭ �� ����: �ƹ�Ÿ�� �ʱ� ȸ���� �����ϰ� �ʿ信 ���� �ʱ� ���·� �����ϴ� ����� �����մϴ�.
        ���� ó��: ����ڰ� ���������� �����Ǿ��� �� �ƹ�Ÿ�� ��ġ�� �����ϰ�, ������ �������� ó���� �� �ֽ��ϴ�.
        �ε巯�� �̵�: �ƹ�Ÿ�� �������� �ε巴�� ó���ϱ� ���� ���� ����� �����մϴ�.

    �� Ŭ������ Kinect�� Ȱ���� ��ȣ�ۿ��� ���ø����̼ǿ��� �ƹ�Ÿ �ִϸ��̼��� �����ϴ� �� �ٽ����� ������ �ϸ�,
    ������� ���̷��� �����͸� ȿ�������� ó���Ͽ� �ڿ������� �ƹ�Ÿ �������� �����մϴ�.
    */

    // ĳ������ �ൿ�� �ſ￡ ��ģ ��ó�� �������� ����
    public bool mirroredMovement = false;

    // �ƹ�Ÿ�� ���� �̵� ��� ����
    public bool verticalMovement = false;

    // �ƹ�Ÿ�� ���� �̵��ϴ� �ӵ� ����
    protected int moveRate = 1; // 1 = �⺻ �ӵ�

    // �������� ���� ���� ���
    public float smoothFactor = 5f;

    // ������ ��带 ����Ͽ� ������� ��ǥ�� ��������� ��ġ�� �������� ����
    public bool offsetRelativeToSensor = false;

    // ��ü ��Ʈ ���
    protected Transform bodyRoot;

    // �ƹ�Ÿ�� ȸ���� �� �ֵ��� �ϴ� ����
    protected GameObject offsetNode;

    // ��� ���� �����ϴ� ���� (�ʱ� ȸ�� ũ�⸸ŭ �ʱ�ȭ��)
    protected Transform[] bones;

    // Kinect ���� ���� �� ���� �ʱ� ȸ��
    protected Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;

    // ��ȯ�� �ʱ� ��ġ�� ȸ��
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    // ĳ���� ��ġ ���� ������ ����
    protected bool offsetCalibrated = false;
    protected float xOffset, yOffset, zOffset;

    // KinectManager�� ����� �ν��Ͻ�
    protected KinectManager kinectManager;

    // Transform ĳ���� ���� ���� ���
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

    // Awake() �޼ҵ�: �ʱ� ����
    /// <summary>
    /// Awake �޼ҵ�� �ƹ�Ÿ�� �ʱ� ������ �����մϴ�.
    /// ���� �����ϰ� �ʱ� ȸ���� �����ɴϴ�.
    /// </summary>
    public void Awake()
    {
        // ���� ���� ����
        if (bones != null)
            return;

        // �� �迭 �ʱ�ȭ
        bones = new Transform[22];

        // ���� �ʱ� ȸ���� ���� �ʱ�ȭ
        initialRotations = new Quaternion[bones.Length];
        initialLocalRotations = new Quaternion[bones.Length];

        // ���� Kinect�� �����ϴ� ����Ʈ�� ����
        MapBones();

        // �ʱ� �� ȸ�� ��������
        GetInitialRotations();
    }

    // �� �����Ӹ��� �ƹ�Ÿ ������Ʈ
    /// <summary>
    /// UpdateAvatar �޼ҵ�� �� �����Ӹ��� �ƹ�Ÿ�� ������Ʈ�մϴ�.
    /// ������� ���̷��� �����Ϳ� ���� �ƹ�Ÿ�� ��ġ�� ȸ���� �����մϴ�.
    /// </summary>
    /// <param name="UserID">������Ʈ�� ����� ID</param>
    public void UpdateAvatar(uint UserID)
    {
        if (!transform.gameObject.activeInHierarchy)
            return; // �ƹ�Ÿ�� ��Ȱ��ȭ�� ��� ����

        // KinectManager �ν��Ͻ� ��������
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        // �ƹ�Ÿ�� Kinect ��ġ�� �̵�
        MoveAvatar(UserID);

        // �� ���� ���� ȸ�� ����
        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue; // ���� �������� ������ �ǳʶ�

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // Ư�� �� (��� ��)
                List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    // Ư�� ���� ���� ��ȯ ó��
                    //Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    //TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }
    }

    // ���� �ʱ� ��ġ�� ȸ������ ����
    /// <summary>
    /// ResetToInitialPosition �޼ҵ�� �ƹ�Ÿ�� ���� �ʱ� ��ġ�� ȸ������ �����մϴ�.
    /// </summary>
    public void ResetToInitialPosition()
    {
        if (bones == null)
            return;

        // ������ ����� ȸ�� �ʱ�ȭ
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // �� ���ǵ� ���� �ʱ� ��ġ�� ����
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].rotation = initialRotations[i];
            }
        }

        // ��ü ��Ʈ �ʱ�ȭ
        if (bodyRoot != null)
        {
            bodyRoot.localPosition = Vector3.zero;
            bodyRoot.localRotation = Quaternion.identity;
        }

        // �ʱ� ��ġ�� ȸ�� ����
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

    // ����ڰ� ���������� �����Ǿ��� �� ȣ��
    /// <summary>
    /// SuccessfulCalibration �޼ҵ�� ����ڰ� ���������� �����Ǿ��� �� ȣ��˴ϴ�.
    /// �ƹ�Ÿ�� ��ġ�� �����ϰ� �������� �纸���մϴ�.
    /// </summary>
    /// <param name="userId">���������� ������ ����� ID</param>
    public void SuccessfulCalibration(uint userId)
    {
        // �� ��ġ ����
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }

        // ��ġ ������ �纸��
        offsetCalibrated = false;
    }

    // Kinect���� ������ ȸ���� ������ ����
    /// <summary>
    /// TransformBone �޼ҵ�� Kinect���� ������ ���� ȸ���� �ƹ�Ÿ�� ���� �����մϴ�.
    /// </summary>
    /// <param name="userId">����� ID</param>
    /// <param name="joint">������ Kinect ����</param>
    /// <param name="boneIndex">������ �� �ε���</param>
    /// <param name="flip">���� ����</param>
    protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return; // ���� ���ų� KinectManager�� ������ ����

        int iJoint = (int)joint;
        if (iJoint < 0)
            return; // �߸��� ���� �ε��� ��ȯ

        // Kinect ������ ȸ�� ��������
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        if (jointRotation == Quaternion.identity)
            return; // ��ȿ���� ���� ȸ���̸� ����

        // ���ο� ȸ������ �ε巴�� ��ȯ
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation; // ������ ���� ȸ�� ����
    }

    // Ư���� ������ ���� ȸ�� ����
    /// <summary>
    /// TransformSpecialBone �޼ҵ�� Ư�� ������ ���� ȸ���� �����մϴ�.
    /// </summary>
    /// <param name="userId">����� ID</param>
    /// <param name="joint">������ Kinect ����</param>
    /// <param name="jointParent">�θ� ����</param>
    /// <param name="boneIndex">������ �� �ε���</param>
    /// <param name="baseDir">�⺻ ����</param>
    /// <param name="flip">���� ����</param>
    protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return; // ���� ���ų� KinectManager�� ������ ����

        if (!kinectManager.IsJointTracked(userId, (int)joint) ||
           !kinectManager.IsJointTracked(userId, (int)jointParent))
        {
            return; // ������ �������� ������ ����
        }

        // �� ���� ���� ���� ��������
        Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        if (jointRotation != Quaternion.identity)
        {
            // ���ο� ȸ������ �ε巴�� ��ȯ
            Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

            if (smoothFactor != 0f)
                boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
            else
                boneTransform.rotation = newRotation; // ������ ���� ȸ�� ����
        }
    }

    // �ƹ�Ÿ�� 3D �������� �̵� - ô���� ���� ��ġ�� ������ ��Ʈ�� ����
    /// <summary>
    /// MoveAvatar �޼ҵ�� �ƹ�Ÿ�� 3D �������� �̵��մϴ�.
    /// ������� ô�� ��ġ�� ������� �ƹ�Ÿ�� ��Ʈ�� �̵��մϴ�.
    /// </summary>
    /// <param name="UserID">�̵��� ����� ID</param>
    protected void MoveAvatar(uint UserID)
    {
        if (bodyRoot == null || kinectManager == null)
            return; // ��ü ��Ʈ�� KinectManager�� ������ ����
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
            return; // ������ ������ �������� ������ ����

        // ��ü�� ��ġ ��������
        Vector3 trans = kinectManager.GetUserPosition(UserID);

        // �ƹ�Ÿ�� ó�� �̵���Ű�� ��� ������ ����
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate; // ���� ���ο� ���� X ������
            yOffset = trans.y * moveRate; // Y ������
            zOffset = -trans.z * moveRate; // Z ������

            if (offsetRelativeToSensor)
            {
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos; // ������ ��� ��ġ ����
                }
                else
                {
                    transform.position = offsetPos; // �ƹ�Ÿ ��ġ ����
                }
            }
        }

        // ���ο� ��ġ�� �ε巴�� ��ȯ
        Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        if (smoothFactor != 0f)
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        else
            bodyRoot.localPosition = targetPos; // ������ ���� ��ġ ����
    }

    // ������ ���� ���ǵǾ����� Ȯ���ϰ� �𵨿� �ش� ���� ����
    /// <summary>
    /// MapBones �޼ҵ�� �ƹ�Ÿ�� ���� Kinect ������ �����մϴ�.
    /// </summary>
    protected virtual void MapBones()
    {
        // ������ ��带 �� ��ȯ�� �θ�� ����
        offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;

        transform.parent = offsetNode.transform; // �� ��ȯ�� �θ� ������ ���� ����
        transform.localPosition = Vector3.zero; // ���� ��ġ �ʱ�ȭ
        transform.localRotation = Quaternion.identity; // ���� ȸ�� �ʱ�ȭ

        // ��ü ��Ʈ�μ� �� ��ȯ ���
        bodyRoot = transform;

        // Animator ������Ʈ���� �� ��ȯ ��������
        var animatorComponent = GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue; // ���ε��� ���� ���� �ǳʶ�

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]); // �� ��ȯ ����
        }
    }

    // ���� �ʱ� ȸ�� ĸó
    /// <summary>
    /// GetInitialRotations �޼ҵ�� ���� �ʱ� ȸ���� ĸó�մϴ�.
    /// </summary>
    protected void GetInitialRotations()
    {
        // �ʱ� ȸ�� ����
        if (offsetNode != null)
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity; // ������ ��� ȸ�� �ʱ�ȭ
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            transform.rotation = Quaternion.identity; // �ƹ�Ÿ ȸ�� �ʱ�ȭ
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation; // �ʱ� ȸ�� ĸó
                initialLocalRotations[i] = bones[i].localRotation; // �ʱ� ���� ȸ�� ĸó
            }
        }

        // �ʱ� ȸ�� ����
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation; // ������ ��� ȸ�� ����
        }
        else
        {
            transform.rotation = initialRotation; // �ƹ�Ÿ ȸ�� ����
        }
    }

    // Kinect ���� ȸ���� �ƹ�Ÿ ���� ȸ������ ��ȯ
    /// <summary>
    /// Kinect2AvatarRot �޼ҵ�� Kinect ���� ȸ���� �ƹ�Ÿ ���� ȸ������ ��ȯ�մϴ�.
    /// </summary>
    /// <param name="jointRotation">Kinect ���� ȸ��</param>
    /// <param name="boneIndex">��ȯ�� �� �ε���</param>
    /// <returns>��ȯ�� �ƹ�Ÿ ���� ȸ��</returns>
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
    {
        // �� ȸ�� ����
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];

        // ������ ��尡 ������ ��� ȸ�� ����
        if (offsetNode != null)
        {
            // �������� ���Ϸ��� ���Ͽ� �� ȸ�� ȹ��
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            // ���ο� ȸ�� ��������
            newRotation = Quaternion.Euler(totalRotation);
        }

        return newRotation; // ��ȯ�� ȸ�� ��ȯ
    }

    // Kinect ��ġ�� �ƹ�Ÿ ���̷��� ��ġ�� ��ȯ
    /// <summary>
    /// Kinect2AvatarPos �޼ҵ�� Kinect ��ġ�� �ƹ�Ÿ ���̷��� ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="jointPosition">Kinect ���� ��ġ</param>
    /// <param name="bMoveVertically">���� �̵� ����</param>
    /// <returns>��ȯ�� �ƹ�Ÿ ��ġ</returns>
    protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
    {
        float xPos;
        float yPos;
        float zPos;

        // �̵��� �����Ǹ� X ����
        if (!mirroredMovement)
            xPos = jointPosition.x * moveRate - xOffset; // X ������ ����
        else
            xPos = -jointPosition.x * moveRate - xOffset; // X ������ ���� (����)

        yPos = jointPosition.y * moveRate - yOffset; // Y ������ ����
        zPos = -jointPosition.z * moveRate - zOffset; // Z ������ ����

        // ���� �̵��� �����ϴ� ��� Y ���� ������Ʈ
        Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

        return avatarJointPos; // ��ȯ�� ��ġ ��ȯ
    }

    // �� ������ ����ȭ�ϱ� ���� ��ųʸ�
    // Kinect ������ Mecanim �� ����
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

    // �� �ε����� Kinect ������ ����
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

    // �� �ε����� Ư�� ������ ����
    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };

    // ������ �� �ε��� ����
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

    // Ư�� �� �ε����� ������ ������ ����
    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };
}