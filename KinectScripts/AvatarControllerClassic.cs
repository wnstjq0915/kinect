using UnityEngine;
// Windows.kinect 사용;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class AvatarControllerClassic : AvatarController
{	
	// 뼈와 일치하는 공개 변수.
    // 비어 있으면 Kinect는 단순히 추적하지 않습니다.
	public Transform HipCenter;
	public Transform Spine;
	public Transform Neck;
	public Transform Head;
	
	public Transform LeftClavicle;
	public Transform LeftUpperArm;
	public Transform LeftElbow; 
	public Transform LeftHand;
	private Transform LeftFingers = null;
	
	public Transform RightClavicle;
	public Transform RightUpperArm;
	public Transform RightElbow;
	public Transform RightHand;
	private Transform RightFingers = null;
	
	public Transform LeftThigh;
	public Transform LeftKnee;
	public Transform LeftFoot;
	private Transform LeftToes = null;
	
	public Transform RightThigh;
	public Transform RightKnee;
	public Transform RightFoot;
	private Transform RightToes = null;

	public Transform BodyRoot;
	public GameObject OffsetNode;
	

	// 매핑 될 뼈가 선언 된 경우 해당 뼈를 모델에 매핑하십시오.
	protected override void MapBones()
	{
		bones[0] = HipCenter;
		bones[1] = Spine;
		bones[2] = Neck;
		bones[3] = Head;
	
		bones[4] = LeftClavicle;
		bones[5] = LeftUpperArm;
		bones[6] = LeftElbow;
		bones[7] = LeftHand;
		bones[8] = LeftFingers;
	
		bones[9] = RightClavicle;
		bones[10] = RightUpperArm;
		bones[11] = RightElbow;
		bones[12] = RightHand;
		bones[13] = RightFingers;
	
		bones[14] = LeftThigh;
		bones[15] = LeftKnee;
		bones[16] = LeftFoot;
		bones[17] = LeftToes;
	
		bones[18] = RightThigh;
		bones[19] = RightKnee;
		bones[20] = RightFoot;
		bones[21] = RightToes;

		// 바디 뿌리 및 오프셋
		bodyRoot = BodyRoot;
		offsetNode = OffsetNode;

		if(offsetNode == null)
		{
			offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
			offsetNode.transform.position = transform.position;
			offsetNode.transform.rotation = transform.rotation;
			offsetNode.transform.parent = transform.parent;
			
			transform.parent = offsetNode.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}
	}
	
}

