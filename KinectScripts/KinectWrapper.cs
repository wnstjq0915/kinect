using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text; 

// 다양한 구조 및 DLL 수입을 보유하는 래퍼 클래스
// Kinect와 함께 모델을 설정해야했습니다.
public class KinectWrapper
{
	public static class Constants
	{
		public const int NuiSkeletonCount = 6;
    	public const int NuiSkeletonMaxTracked = 2;
    	public const int NuiSkeletonInvalidTrackingID = 0;
		
		public const float NuiDepthHorizontalFOV = 58.5f;
		public const float NuiDepthVerticalFOV = 45.6f;
		
		public const int ColorImageWidth = 640;
		public const int ColorImageHeight = 480;
		public const NuiImageResolution ColorImageResolution = NuiImageResolution.resolution640x480;
		
		public const int DepthImageWidth = 640;
		public const int DepthImageHeight = 480;
		public const NuiImageResolution DepthImageResolution = NuiImageResolution.resolution640x480;
		
		public const bool IsNearMode = false;
		
		public const float MinTimeBetweenSameGestures = 0.0f;
		public const float PoseCompleteDuration = 1.0f;
		public const float ClickStayDuration = 2.5f;
	}
	
	/// <summary>
	///Structs and constants for interfacing C# with the Kinect.dll 
	/// </summary>

    [Flags]
    public enum NuiInitializeFlags : uint
    {
		UsesAudio = 0x10000000,
        UsesDepthAndPlayerIndex = 0x00000001,
        UsesColor = 0x00000002,
        UsesSkeleton = 0x00000008,
        UsesDepth = 0x00000020,
		UsesHighQualityColor = 0x00000040
    }
	
	public enum NuiErrorCodes : uint
	{
		FrameNoData = 0x83010001,
		StreamNotEnabled = 0x83010002,
		ImageStreamInUse = 0x83010003,
		FrameLimitExceeded = 0x83010004,
		FeatureNotInitialized = 0x83010005,
		DeviceNotGenuine = 0x83010006,
		InsufficientBandwidth = 0x83010007,
		DeviceNotSupported = 0x83010008,
		DeviceInUse = 0x83010009,
		
		DatabaseNotFound = 0x8301000D,
		DatabaseVersionMismatch = 0x8301000E,
		HardwareFeatureUnavailable = 0x8301000F,
		
		DeviceNotConnected = 0x83010014,
		DeviceNotReady = 0x83010015,
		SkeletalEngineBusy = 0x830100AA,
		DeviceNotPowered = 0x8301027F,
	}

    public enum NuiSkeletonPositionIndex : int
    {
        HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19,
        Count = 20
    }

    public enum NuiSkeletonPositionTrackingState
    {
        NotTracked = 0,
        Inferred,
        Tracked
    }

    public enum NuiSkeletonTrackingState
    {
        NotTracked = 0,
        PositionOnly,
        SkeletonTracked
    }
	
	public enum NuiImageType
	{
		DepthAndPlayerIndex = 0,	// USHORT
		Color,						// RGB32 data
		ColorYUV,					// YUY2 stream from camera h/w, but converted to RGB32 before user getting it.
		ColorRawYUV,				// YUY2 stream from camera h/w.
		Depth						// USHORT
	}
	
	public enum NuiImageResolution
	{
		resolutionInvalid = -1,
		resolution80x60 = 0,
		resolution320x240 = 1,
		resolution640x480 = 2,
		resolution1280x960 = 3     // for hires color only
	}
	
	public enum NuiImageStreamFlags
	{
		None = 0x00000000,
		SupressNoFrameData = 0x0001000,
		EnableNearMode = 0x00020000,
		TooFarIsNonZero = 0x0004000
	}
	
    [Flags]
    public enum FrameEdges
    {
        None = 0,
        Right = 1,
        Left = 2,
        Top = 4,
        Bottom = 8
    }

	public struct NuiSkeletonData
    {
        public NuiSkeletonTrackingState eTrackingState;
        public uint dwTrackingID;
        public uint dwEnrollmentIndex_NotUsed;
        public uint dwUserIndex;
        public Vector4 Position;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public Vector4[] SkeletonPositions;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonPositionTrackingState[] eSkeletonPositionTrackingState;
        public uint dwQualityFlags;
    }
	
    public struct NuiSkeletonFrame
    {
        public Int64 liTimeStamp;
        public uint dwFrameNumber;
        public uint dwFlags;
        public Vector4 vFloorClipPlane;
        public Vector4 vNormalToGravity;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonData[] SkeletonData;
    }
	
	public struct NuiTransformSmoothParameters
	{
		public float fSmoothing;
		public float fCorrection;
		public float fPrediction;
		public float fJitterRadius;
		public float fMaxDeviationRadius;
	}
	
    public struct NuiSkeletonBoneRotation
    {
        public Matrix4x4 rotationMatrix;
        public Quaternion rotationQuaternion;
    }

    public struct NuiSkeletonBoneOrientation
    {
        public NuiSkeletonPositionIndex endJoint;
        public NuiSkeletonPositionIndex startJoint;
        public NuiSkeletonBoneRotation hierarchicalRotation;
        public NuiSkeletonBoneRotation absoluteRotation;
    }
	
	public struct NuiImageViewArea
	{
	    public int eDigitalZoom;
	    public int lCenterX;
	    public int lCenterY;
	}
	
	public class NuiImageBuffer
	{
		public int m_Width;
		public int m_Height;
		public int m_BytesPerPixel;
		public IntPtr m_pBuffer;
	}
	
	public struct NuiImageFrame
	{
		public Int64 liTimeStamp;
		public uint dwFrameNumber;
		public NuiImageType eImageType;
		public NuiImageResolution eResolution;
		// [Marshalasattribute (UnmanagedType.interface)]
		public IntPtr pFrameTexture;
		public uint dwFrameFlags_NotUsed;
		public NuiImageViewArea ViewArea_NotUsed;
	}
	
	public struct NuiLockedRect
	{
		public int pitch;
		public int size;
		// [Marshalasattribute (UnmanagedType.U8)] 
		public IntPtr pBits; 
		
	}
	
	public struct ColorCust
	{
		public byte b;
		public byte g;
		public byte r;
		public byte a;
	}
	
	public struct ColorBuffer
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.Struct)]
		public ColorCust[] pixels;
	}
	
	public struct DepthBuffer
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.U2)]
		public ushort[] pixels;
	}
	
	public struct NuiSurfaceDesc
	{
		uint width;
		uint height;
	}
	
	[Guid("13ea17f5-ff2e-4670-9ee5-1297a6e880d1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport()]
	public interface INuiFrameTexture
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int BufferLen();
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int Pitch();
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int LockRect(uint Level,ref NuiLockedRect pLockedRect,IntPtr pRect, uint Flags);
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetLevelDesc(uint Level, ref NuiSurfaceDesc pDesc);
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int UnlockRect(uint Level);
	}

    /* 
	 * kinect NUI (general) functions
	 */

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiInitialize")]
    public static extern int NuiInitialize(NuiInitializeFlags dwFlags);

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiShutdown")]
    public static extern void NuiShutdown();

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiCameraElevationSetAngle")]
    public static extern int NuiCameraElevationSetAngle(int angle);

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiCameraElevationGetAngle")]
    public static extern int NuiCameraElevationGetAngle(out int plAngleDegrees);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution")]
    public static extern int NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(NuiImageResolution eColorResolution, NuiImageResolution eDepthResolution, ref NuiImageViewArea pcViewArea, int lDepthX, int lDepthY, ushort sDepthValue, out int plColorX, out int plColorY);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiGetSensorCount")]
    public static extern int NuiGetSensorCount(out int pCount);

    /*
	 * kinect skeleton functions
	 */

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiSkeletonTrackingEnable")]
    public static extern int NuiSkeletonTrackingEnable(IntPtr hNextFrameEvent, uint dwFlags);

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiSkeletonGetNextFrame")]
    public static extern int NuiSkeletonGetNextFrame(uint dwMillisecondsToWait, ref NuiSkeletonFrame pSkeletonFrame);

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiTransformSmooth")]
    public static extern int NuiTransformSmooth(ref NuiSkeletonFrame pSkeletonFrame, ref NuiTransformSmoothParameters pSmoothingParams);
	
    [DllImport(@"Kinect10.dll", EntryPoint = "NuiSkeletonCalculateBoneOrientations")]
    public static extern int NuiSkeletonCalculateBoneOrientations(ref NuiSkeletonData pSkeletonData, NuiSkeletonBoneOrientation[] pBoneOrientations);

    /*
	 * kinect video functions
	 */
    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamOpen")]
    public static extern int NuiImageStreamOpen(NuiImageType eImageType, NuiImageResolution eResolution, uint dwImageFrameFlags_NotUsed, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle);

    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamGetNextFrame")]
    public static extern int NuiImageStreamGetNextFrame(IntPtr phStreamHandle, uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamReleaseFrame")]
    public static extern int NuiImageStreamReleaseFrame(IntPtr phStreamHandle, IntPtr ppcImageFrame);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamSetImageFrameFlags")]
	public static extern int NuiImageStreamSetImageFrameFlags (IntPtr phStreamHandle, NuiImageStreamFlags dvImageFrameFlags);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageResolutionToSize")]
    public static extern int NuiImageResolutionToSize(NuiImageResolution eResolution,out uint frameWidth,out uint frameHeight);
	
	
	public static string GetNuiErrorString(int hr)
	{
		string message = string.Empty;
		uint uhr = (uint)hr;
		
		switch(uhr)
		{
			case (uint)NuiErrorCodes.FrameNoData:
				message = "Frame contains no data.";
				break;
			case (uint)NuiErrorCodes.StreamNotEnabled:
				message = "Stream is not enabled.";
				break;
			case (uint)NuiErrorCodes.ImageStreamInUse:
				message = "Image stream is already in use.";
				break;
			case (uint)NuiErrorCodes.FrameLimitExceeded:
				message = "Frame limit is exceeded.";
				break;
			case (uint)NuiErrorCodes.FeatureNotInitialized:
				message = "Feature is not initialized.";
				break;
			case (uint)NuiErrorCodes.DeviceNotGenuine:
				message = "Device is not genuine.";
				break;
			case (uint)NuiErrorCodes.InsufficientBandwidth:
				message = "Bandwidth is not sufficient.";
				break;
			case (uint)NuiErrorCodes.DeviceNotSupported:
				message = "Device is not supported (e.g. Kinect for XBox 360).";
				break;
			case (uint)NuiErrorCodes.DeviceInUse:
				message = "Device is already in use.";
				break;
			case (uint)NuiErrorCodes.DatabaseNotFound:
				message = "Database not found.";
				break;
			case (uint)NuiErrorCodes.DatabaseVersionMismatch:
				message = "Database version mismatch.";
				break;
			case (uint)NuiErrorCodes.HardwareFeatureUnavailable:
				message = "Hardware feature is not available.";
				break;
			case (uint)NuiErrorCodes.DeviceNotConnected:
				message = "Device is not connected.";
				break;
			case (uint)NuiErrorCodes.DeviceNotReady:
				message = "Device is not ready.";
				break;
			case (uint)NuiErrorCodes.SkeletalEngineBusy:
				message = "Skeletal engine is busy.";
				break;
			case (uint)NuiErrorCodes.DeviceNotPowered:
				message = "Device is not powered.";
				break;
				
			default:
				message = "hr=0x" + uhr.ToString("X");
				break;
		}
		
		return message;
	}
	
	public static int GetDepthWidth()
	{
		return Constants.DepthImageWidth;
	}
	
	public static int GetDepthHeight()
	{
		return Constants.DepthImageHeight;
	}
	
	public static int GetColorWidth()
	{
		return Constants.ColorImageWidth;
	}
	
	public static int GetColorHeight()
	{
		return Constants.ColorImageHeight;
	}
	
	public static Vector3 MapSkeletonPointToDepthPoint(Vector3 skeletonPoint)
    {
        float fDepthX;
        float fDepthY;
        float fDepthZ;

		NuiTransformSkeletonToDepthImage(skeletonPoint, out fDepthX, out fDepthY, out fDepthZ);
        
		Vector3 point = new Vector3();
        point.x = (int) ((fDepthX * Constants.DepthImageWidth) + 0.5f);
        point.y = (int) ((fDepthY * Constants.DepthImageHeight) + 0.5f);
        point.z = (int) (fDepthZ + 0.5f);

		return point;
    }


    private static void NuiTransformSkeletonToDepthImage(Vector3 vPoint, out float pfDepthX, out float pfDepthY, out float pfDepthZ)
    {
        if (vPoint.z > float.Epsilon)
        {
            pfDepthX = 0.5f + ((vPoint.x * 285.63f) / (vPoint.z * 320f));
            pfDepthY = 0.5f - ((vPoint.y * 285.63f) / (vPoint.z * 240f));
            pfDepthZ = vPoint.z * 1000f;
        }
        else
        {
            pfDepthX = 0f;
            pfDepthY = 0f;
            pfDepthZ = 0f;
        }
    }

	public static int GetSkeletonJointParent(int jointIndex)
	{
		switch(jointIndex)
		{
			case (int)NuiSkeletonPositionIndex.HipCenter:
				return (int)NuiSkeletonPositionIndex.HipCenter;
			case (int)NuiSkeletonPositionIndex.Spine:
				return (int)NuiSkeletonPositionIndex.HipCenter;
			case (int)NuiSkeletonPositionIndex.ShoulderCenter:
				return (int)NuiSkeletonPositionIndex.Spine;
			case (int)NuiSkeletonPositionIndex.Head:
				return (int)NuiSkeletonPositionIndex.ShoulderCenter;
			case (int)NuiSkeletonPositionIndex.ShoulderLeft:
				return (int)NuiSkeletonPositionIndex.ShoulderCenter;
			case (int)NuiSkeletonPositionIndex.ElbowLeft:
				return (int)NuiSkeletonPositionIndex.ShoulderLeft;
			case (int)NuiSkeletonPositionIndex.WristLeft:
				return (int)NuiSkeletonPositionIndex.ElbowLeft;
			case (int)NuiSkeletonPositionIndex.HandLeft:
				return (int)NuiSkeletonPositionIndex.WristLeft;
			case (int)NuiSkeletonPositionIndex.ShoulderRight:
				return (int)NuiSkeletonPositionIndex.ShoulderCenter;
			case (int)NuiSkeletonPositionIndex.ElbowRight:
				return (int)NuiSkeletonPositionIndex.ShoulderRight;
			case (int)NuiSkeletonPositionIndex.WristRight:
				return (int)NuiSkeletonPositionIndex.ElbowRight;
			case (int)NuiSkeletonPositionIndex.HandRight:
				return (int)NuiSkeletonPositionIndex.WristRight;
			case (int)NuiSkeletonPositionIndex.HipLeft:
				return (int)NuiSkeletonPositionIndex.HipCenter;
			case (int)NuiSkeletonPositionIndex.KneeLeft:
				return (int)NuiSkeletonPositionIndex.HipLeft;
			case (int)NuiSkeletonPositionIndex.AnkleLeft:
				return (int)NuiSkeletonPositionIndex.KneeLeft;
			case (int)NuiSkeletonPositionIndex.FootLeft:
				return (int)NuiSkeletonPositionIndex.AnkleLeft;
			case (int)NuiSkeletonPositionIndex.HipRight:
				return (int)NuiSkeletonPositionIndex.HipCenter;
			case (int)NuiSkeletonPositionIndex.KneeRight:
				return (int)NuiSkeletonPositionIndex.HipRight;
			case (int)NuiSkeletonPositionIndex.AnkleRight:
				return (int)NuiSkeletonPositionIndex.KneeRight;
			case (int)NuiSkeletonPositionIndex.FootRight:
				return (int)NuiSkeletonPositionIndex.AnkleRight;
		}
		
		return (int)NuiSkeletonPositionIndex.HipCenter;
	}

	public static int GetSkeletonMirroredJoint(int jointIndex)
	{
		switch(jointIndex)
		{
			case (int)NuiSkeletonPositionIndex.ShoulderLeft:
				return (int)NuiSkeletonPositionIndex.ShoulderRight;
			case (int)NuiSkeletonPositionIndex.ElbowLeft:
				return (int)NuiSkeletonPositionIndex.ElbowRight;
			case (int)NuiSkeletonPositionIndex.WristLeft:
				return (int)NuiSkeletonPositionIndex.WristRight;
			case (int)NuiSkeletonPositionIndex.HandLeft:
				return (int)NuiSkeletonPositionIndex.HandRight;
			case (int)NuiSkeletonPositionIndex.ShoulderRight:
				return (int)NuiSkeletonPositionIndex.ShoulderLeft;
			case (int)NuiSkeletonPositionIndex.ElbowRight:
				return (int)NuiSkeletonPositionIndex.ElbowLeft;
			case (int)NuiSkeletonPositionIndex.WristRight:
				return (int)NuiSkeletonPositionIndex.WristLeft;
			case (int)NuiSkeletonPositionIndex.HandRight:
				return (int)NuiSkeletonPositionIndex.HandLeft;
			case (int)NuiSkeletonPositionIndex.HipLeft:
				return (int)NuiSkeletonPositionIndex.HipRight;
			case (int)NuiSkeletonPositionIndex.KneeLeft:
				return (int)NuiSkeletonPositionIndex.KneeRight;
			case (int)NuiSkeletonPositionIndex.AnkleLeft:
				return (int)NuiSkeletonPositionIndex.AnkleRight;
			case (int)NuiSkeletonPositionIndex.FootLeft:
				return (int)NuiSkeletonPositionIndex.FootRight;
			case (int)NuiSkeletonPositionIndex.HipRight:
				return (int)NuiSkeletonPositionIndex.HipLeft;
			case (int)NuiSkeletonPositionIndex.KneeRight:
				return (int)NuiSkeletonPositionIndex.KneeLeft;
			case (int)NuiSkeletonPositionIndex.AnkleRight:
				return (int)NuiSkeletonPositionIndex.AnkleLeft;
			case (int)NuiSkeletonPositionIndex.FootRight:
				return (int)NuiSkeletonPositionIndex.FootLeft;
		}
		
		return jointIndex;
	}

	public static bool PollSkeleton(ref NuiTransformSmoothParameters smoothParameters, ref NuiSkeletonFrame skeletonFrame)
	{
		bool newSkeleton = false;
		
		int hr = KinectWrapper.NuiSkeletonGetNextFrame(0, ref skeletonFrame);
		if(hr == 0)
		{
			newSkeleton = true;
		}
		
		if(newSkeleton)
		{
			hr = KinectWrapper.NuiTransformSmooth(ref skeletonFrame, ref smoothParameters);
			if(hr != 0)
			{
				Debug.Log("Skeleton Data Smoothing failed");
			}
		}
		
		return newSkeleton;
	}
	
	public static bool PollColor(IntPtr colorStreamHandle, ref byte[] videoBuffer, ref Color32[] colorImage)
	{

		IntPtr imageFramePtr = IntPtr.Zero;
		bool newColor = false;
	
		int hr = KinectWrapper.NuiImageStreamGetNextFrame(colorStreamHandle, 0, ref imageFramePtr);
		if (hr == 0)
		{
			newColor = true;
			
			NuiImageFrame imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
			INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
			
			NuiLockedRect lockedRectPtr = new NuiLockedRect();
			IntPtr r = IntPtr.Zero;
			
			frameTexture.LockRect(0, ref lockedRectPtr, r, 0);
			
			ColorBuffer cb = (ColorBuffer)Marshal.PtrToStructure(lockedRectPtr.pBits, typeof(ColorBuffer));
			int totalPixels = Constants.ColorImageWidth * Constants.ColorImageHeight;
			
			for (int pix = 0; pix < totalPixels; pix++)
			{
				int ind = pix; // totalPixels - pix - 1;
				
				colorImage[ind].r = cb.pixels[pix].r;
				colorImage[ind].g = cb.pixels[pix].g;
				colorImage[ind].b = cb.pixels[pix].b;
				colorImage[ind].a = 255;
			}
			
			frameTexture.UnlockRect(0);
			hr = KinectWrapper.NuiImageStreamReleaseFrame(colorStreamHandle, imageFramePtr);
		}
		
		return newColor;
	}
	
	public static bool PollDepth(IntPtr depthStreamHandle, bool isNearMode, ref ushort[] depthPlayerData)
	{
		IntPtr imageFramePtr = IntPtr.Zero;
		bool newDepth = false;

		if (isNearMode)
		{
			KinectWrapper.NuiImageStreamSetImageFrameFlags(depthStreamHandle, NuiImageStreamFlags.EnableNearMode);
		}
		else
		{
			KinectWrapper.NuiImageStreamSetImageFrameFlags(depthStreamHandle, NuiImageStreamFlags.None);
		}
		
		int hr = KinectWrapper.NuiImageStreamGetNextFrame(depthStreamHandle, 0, ref imageFramePtr);
		if (hr == 0)
		{
			newDepth = true;
			
			NuiImageFrame imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
			INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
			
			NuiLockedRect lockedRectPtr = new NuiLockedRect();
			IntPtr r = IntPtr.Zero;
			
			frameTexture.LockRect(0, ref lockedRectPtr,r,0);
			// DecThplayerData = ExtractDepThimage (lockedRectPtr);
			
			DepthBuffer db = (DepthBuffer)Marshal.PtrToStructure(lockedRectPtr.pBits, typeof(DepthBuffer));
			depthPlayerData = db.pixels;

			frameTexture.UnlockRect(0);
			hr = KinectWrapper.NuiImageStreamReleaseFrame(depthStreamHandle, imageFramePtr);
		}
		
		return newDepth;
	}
	
    private static Vector3 GetPositionBetweenIndices(ref Vector3[] jointsPos, NuiSkeletonPositionIndex p1, NuiSkeletonPositionIndex p2) 
	{
		Vector3 pVec1 = jointsPos[(int)p1];
		Vector3 pVec2 = jointsPos[(int)p2];
		
        return pVec2 - pVec1;
    }
           
    // 열을 사용하여 매트릭스를 채 웁니다
    private static void PopulateMatrix(ref Matrix4x4 jointOrientation, Vector3 xCol, Vector3 yCol, Vector3 zCol) 
	{
    	jointOrientation.SetColumn(0, xCol);
    	jointOrientation.SetColumn(1, yCol);
    	jointOrientation.SetColumn(2, zCol);
    }

    // X 축을 지정하는 벡터로부터 방향을 구성합니다.
    private static void MakeMatrixFromX(Vector3 v1, ref Matrix4x4 jointOrientation, bool flip) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 첫 번째 열을 이전 조인트와 현재의 벡터로 설정하면 2 개의 자유도를 설정합니다.
        xCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직 인 임의의 벡터로 설정
        yCol.x = 0.0f;
        yCol.y = !flip ? xCol.z : -xCol.z;
        yCol.z = !flip ? -xCol.y : xCol.y;
        yCol.Normalize();

        // 세 번째 열은 처음 두 개에 의해 완전히 결정되며 크로스 제품이어야합니다.
        zCol = Vector3.Cross(xCol, yCol);

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // y 축을 지정하는 벡터로부터 방향을 구성합니다.
    private static void MakeMatrixFromY(Vector3 v1, ref Matrix4x4 jointOrientation) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 첫 번째 열을 이전 조인트와 현재의 벡터로 설정하면 2 개의 자유도를 설정합니다.
        yCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직 인 임의의 벡터로 설정
        xCol.x = yCol.y;
        xCol.y = -yCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        // 세 번째 열은 처음 두 개에 의해 완전히 결정되며 크로스 제품이어야합니다.
        zCol = Vector3.Cross(xCol, yCol);

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    // X 축을 지정하는 벡터로부터 방향을 구성합니다.
    private static void MakeMatrixFromZ(Vector3 v1, ref Matrix4x4 jointOrientation) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

         // 첫 번째 열을 이전 조인트와 현재의 벡터로 설정하면 2 개의 자유도를 설정합니다.
        zCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직 인 임의의 벡터로 설정
        xCol.x = zCol.y;
        xCol.y = -zCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        // 세 번째 열은 처음 두 개에 의해 완전히 결정되며 크로스 제품이어야합니다.
        yCol = Vector3.Cross(zCol, xCol);

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // 2 개의 벡터로부터 방향을 구성한다 : 첫 번째는 x 축을 지정하고 다음은 y 축을 지정한다.
    // 첫 번째 벡터를 X 축으로 사용한 다음 크로스 제품을 사용하여 다른 축을 구성합니다.
    private static void MakeMatrixFromXY(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 재배치하고 뒤집기 위해 세 개의 다른 열을 설정
        xCol = xUnnormalized.normalized;
        zCol = Vector3.Cross(xCol, yUnnormalized.normalized).normalized;
        yCol = Vector3.Cross(zCol, xCol).normalized;

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    // 2 개의 벡터로부터 방향을 구성한다 : 첫 번째는 x 축을 지정하고 다음은 y 축을 지정한다.
    // 두 번째 벡터를 Y 축으로 사용한 다음 크로스 제품을 사용하여 다른 축을 구성합니다.
    private static void MakeMatrixFromYX(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 재배치하고 뒤집기 위해 세 개의 다른 열을 설정
        yCol = yUnnormalized.normalized;
        zCol = Vector3.Cross(xUnnormalized.normalized, yCol).normalized;
        xCol = Vector3.Cross(yCol, zCol).normalized;

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    // 2 개의 벡터로부터 방향을 구성한다 : 첫 번째는 x 축을 지정하고 다음은 y 축을 지정한다.
    // 두 번째 벡터를 Y 축으로 사용한 다음 크로스 제품을 사용하여 다른 축을 구성합니다.
    private static void MakeMatrixFromYZ(Vector3 yUnnormalized, Vector3 zUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        // 매트릭스 열
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 재배치하고 뒤집기 위해 세 개의 다른 열을 설정
        yCol = yUnnormalized.normalized;
        xCol = Vector3.Cross(yCol, zUnnormalized.normalized).normalized;
        zCol = Vector3.Cross(xCol, yCol).normalized;

        // 값을 매트릭스로 복사하십시오
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
	
	// 공동 위치 및 추적 상태에 따라 관절 방향을 계산합니다.
    public static void GetSkeletonJointOrientation(ref Vector3[] jointsPos, ref bool[] jointsTracked, ref Matrix4x4 [] jointOrients)
    {
        Vector3 vx;
        Vector3 vy;
        Vector3 vz;

	    // nui_skeleton_position_hip_center
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Spine] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
		{
			vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipCenter, NuiSkeletonPositionIndex.Spine);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipCenter]);
			
			// 정면으로 약 40도를 수정하십시오.
			Matrix4x4 mat = jointOrients[(int)NuiSkeletonPositionIndex.HipCenter];
			Quaternion quat = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
			quat *= Quaternion.Euler(-40, 0, 0);
			jointOrients[(int)NuiSkeletonPositionIndex.HipCenter].SetTRS(Vector3.zero, quat, Vector3.one);
		}
       
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight])
		{
		    // nui_skeleton_position_spine
			if(jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
			{
		        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
		        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Spine]);
			}
	       
			if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
			{
			    // nui_skeleton_position_shoulder_center
			    vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderCenter, NuiSkeletonPositionIndex.Head);
			    vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
			    MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderCenter]);
		
			    // nui_skeleton_position_head
			    MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
			}
		}
       
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
		{
		    // nui_skeleton_position_shoulder_left
			{
		        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderLeft]);
			}
	       
		    // nui_skeleton_position_elbow_left
			{
		        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowLeft]);
			}
		}
			
       	if(jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
		{
		        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristLeft]);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandLeft]);
		}
       
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
		{
		    // nui_skeleton_position_shoulder_right
			{
		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderRight]);
			}
	       
		    // nui_skeleton_position_elbow_right
			{
		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowRight]);
			}
		}
       
		if(jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
		{
		    // nui_skeleton_position_wrist_right
		    vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
			vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
		    MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristRight]);
	       
		    // nui_skeleton_position_hand_right
		    MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandRight]);
		}
        
	    // nui_skeleton_position_hip_left
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.HipLeft);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipLeft]);
       
		    // nui_skeleton_position_knee_left
			if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft])
			{
		        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
		        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeLeft]);
			}
		}
		
		if(jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
		{
		    vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
		    vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
		    MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);
	       
		    // nui_skeleton_position_foot_left
		    MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
		}
       
	    // nui_skeleton_position_hip_right
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipRight] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.HipRight);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipRight]);

		    // nui_skeleton_position_knee_right
			if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight])
			{
		        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
		        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeRight]);
			}
		}
       
		if(jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] &&
			jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
		{
		    // nui_skeleton_position_ankle_right
		    vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
		    vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
		    MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);

		    // nui_skeleton_position_foot_right
		    MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
		}
    }
}