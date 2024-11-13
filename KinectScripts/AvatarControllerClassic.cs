using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

/// <summary>
/// AvatarControllerClassic Ŭ������ Kinect �����͸� ����Ͽ� �ƹ�Ÿ�� ���븦 �����ϴ� ����� �����մϴ�.
/// </summary>
public class AvatarControllerClassic : AvatarController
{
    /*
    Ŭ���� ����: AvatarControllerClassic�� Kinect ������ ���� �ƹ�Ÿ�� ���븦 �����ϴ� ����� �����մϴ�.
        �� Ŭ������ AvatarController Ŭ������ ��ӹ�����,
        �پ��� ��ȯ(Transform) ������ ���� �ƹ�Ÿ�� ��ü ������ ���� ������ �����մϴ�.
    ���� ����: �� ��ȯ ������ �ƹ�Ÿ�� Ư�� ������ ���ε˴ϴ�.
        ���� ���, HipCenter�� ������ �߾��� ��Ÿ���ϴ�.
    MapBones �޼ҵ�: �� �޼ҵ�� �ƹ�Ÿ�� ���븦 �����ϴ� �ֿ� ����� �����մϴ�.
        �� ��ü ������ ��ȯ�� �迭�� �Ҵ��ϸ�, ������ ��尡 ���� ��� ���� �����մϴ�.
        ���� ������ ������ ���� �ƹ�Ÿ�� �θ�� �����ǰ�, �ƹ�Ÿ�� ���� ��ġ�� ȸ���� �ʱ�ȭ�˴ϴ�.
    */

    // Kinect �����Ϳ� ���ε� �ƹ�Ÿ�� ���� ��ȯ�� �����մϴ�.
    public Transform HipCenter; // ������ �߾�
    public Transform Spine; // ô��
    public Transform Neck; // ��
    public Transform Head; // �Ӹ�

    public Transform LeftClavicle; // ���� ���
    public Transform LeftUpperArm; // ���� ���
    public Transform LeftElbow; // ���� �Ȳ�ġ
    public Transform LeftHand; // ���� ��
    private Transform LeftFingers = null; // ���� �հ��� (�ɼ�)

    public Transform RightClavicle; // ������ ���
    public Transform RightUpperArm; // ������ ���
    public Transform RightElbow; // ������ �Ȳ�ġ
    public Transform RightHand; // ������ ��
    private Transform RightFingers = null; // ������ �հ��� (�ɼ�)

    public Transform LeftThigh; // ���� �����
    public Transform LeftKnee; // ���� ����
    public Transform LeftFoot; // ���� ��
    private Transform LeftToes = null; // ���� �߰��� (�ɼ�)

    public Transform RightThigh; // ������ �����
    public Transform RightKnee; // ������ ����
    public Transform RightFoot; // ������ ��
    private Transform RightToes = null; // ������ �߰��� (�ɼ�)

    public Transform BodyRoot; // �ƹ�Ÿ�� ��ü ��Ʈ
    public GameObject OffsetNode; // ������ ���

    /// <summary>
    /// ���븦 �ƹ�Ÿ �𵨿� �����մϴ�.
    /// </summary>
    protected override void MapBones()
    {
        // �ƹ�Ÿ ���뿡 ���� ��ȯ�� �迭�� �����մϴ�.
        bones[0] = HipCenter; // ������ �߾�
        bones[1] = Spine; // ô��
        bones[2] = Neck; // ��
        bones[3] = Head; // �Ӹ�

        bones[4] = LeftClavicle; // ���� ���
        bones[5] = LeftUpperArm; // ���� ���
        bones[6] = LeftElbow; // ���� �Ȳ�ġ
        bones[7] = LeftHand; // ���� ��
        bones[8] = LeftFingers; // ���� �հ��� (�ɼ�)

        bones[9] = RightClavicle; // ������ ���
        bones[10] = RightUpperArm; // ������ ���
        bones[11] = RightElbow; // ������ �Ȳ�ġ
        bones[12] = RightHand; // ������ ��
        bones[13] = RightFingers; // ������ �հ��� (�ɼ�)

        bones[14] = LeftThigh; // ���� �����
        bones[15] = LeftKnee; // ���� ����
        bones[16] = LeftFoot; // ���� ��
        bones[17] = LeftToes; // ���� �߰��� (�ɼ�)

        bones[18] = RightThigh; // ������ �����
        bones[19] = RightKnee; // ������ ����
        bones[20] = RightFoot; // ������ ��
        bones[21] = RightToes; // ������ �߰��� (�ɼ�)

        // ��ü ��Ʈ�� ������ ��带 �����մϴ�.
        bodyRoot = BodyRoot;
        offsetNode = OffsetNode;

        // ������ ��尡 null�� ��� ���� �����մϴ�.
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

            // �ƹ�Ÿ�� ������ ����� �ڽ����� �����մϴ�.
            transform.parent = offsetNode.transform;
            transform.localPosition = Vector3.zero; // ��ġ�� �ʱ�ȭ
            transform.localRotation = Quaternion.identity; // ȸ���� �ʱ�ȭ
        }
    }
}