using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

/// <summary>
/// KinectWrapper 클래스는 Kinect 센서와 상호작용하기 위한 메소드와 구조체를 포함합니다.
/// </summary>
public class KinectWrapper
{
    /*
    이 코드는 Kinect와의 상호작용을 위한 다양한 구조체와 메소드가 포함되어 있습니다.
    Constants 클래스는 Kinect에서 사용할 수 있는 다양한 상수 값을 정의합니다.
    NuiInitializeFlags, NuiErrorCodes, NuiSkeletonPositionIndex 등의 열거형은 Kinect SDK와 관련된 플래그와 오류 코드, 조인트 인덱스를 정의합니다.
    NuiSkeletonData와 NuiSkeletonFrame 구조체는 스켈레톤 데이터를 저장하는 데 사용됩니다.
    여러 메소드는 Kinect의 다양한 기능을 호출하고, 스켈레톤 데이터를 처리하는 데 필요한 후처리를 수행합니다.
    */
    public static class Constants
    {
        public const int NuiSkeletonCount = 6; // 최대 스켈레톤 수
        public const int NuiSkeletonMaxTracked = 2; // 최대 추적 가능한 스켈레톤 수
        public const int NuiSkeletonInvalidTrackingID = 0; // 유효하지 않은 추적 ID

        public const float NuiDepthHorizontalFOV = 58.5f; // 깊이 수평 시야각
        public const float NuiDepthVerticalFOV = 45.6f; // 깊이 수직 시야각

        public const int ColorImageWidth = 640; // 색상 이미지 너비
        public const int ColorImageHeight = 480; // 색상 이미지 높이
        public const NuiImageResolution ColorImageResolution = NuiImageResolution.resolution640x480; // 색상 이미지 해상도

        public const int DepthImageWidth = 640; // 깊이 이미지 너비
        public const int DepthImageHeight = 480; // 깊이 이미지 높이
        public const NuiImageResolution DepthImageResolution = NuiImageResolution.resolution640x480; // 깊이 이미지 해상도

        public const bool IsNearMode = false; // 근거리 모드 사용 여부

        public const float MinTimeBetweenSameGestures = 0.0f; // 동일 제스처 간 최소 시간
        public const float PoseCompleteDuration = 1.0f; // 포즈 완료 지속 시간
        public const float ClickStayDuration = 2.5f; // 클릭 지속 시간
    }

    // 다양한 플래그를 정의하는 열거형
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

    // Kinect 오류 코드를 정의하는 열거형
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

    // 스켈레톤 포지션 인덱스를 정의하는 열거형
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
        DepthAndPlayerIndex = 0, // USHORT
        Color, // RGB32 데이터
        ColorYUV, // YUY2 카메라 하드웨어의 스트림, RGB32로 변환됨
        ColorRawYUV, // YUY2 카메라 하드웨어의 스트림
        Depth // USHORT
    }

    public enum NuiImageResolution
    {
        resolutionInvalid = -1,
        resolution80x60 = 0,
        resolution320x240 = 1,
        resolution640x480 = 2,
        resolution1280x960 = 3 // 고해상도 색상 전용
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

    // 스켈레톤 데이터 구조체
    public struct NuiSkeletonData
    {
        public NuiSkeletonTrackingState eTrackingState; // 추적 상태
        public uint dwTrackingID; // 추적 ID
        public uint dwEnrollmentIndex_NotUsed; // 사용되지 않는 인덱스
        public uint dwUserIndex; // 사용자 인덱스
        public Vector4 Position; // 스켈레톤 위치
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public Vector4[] SkeletonPositions; // 스켈레톤 위치 배열
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonPositionTrackingState[] eSkeletonPositionTrackingState; // 위치 추적 상태 배열
        public uint dwQualityFlags; // 품질 플래그
    }

    // 스켈레톤 프레임 구조체
    public struct NuiSkeletonFrame
    {
        public Int64 liTimeStamp; // 타임스탬프
        public uint dwFrameNumber; // 프레임 번호
        public uint dwFlags; // 플래그
        public Vector4 vFloorClipPlane; // 바닥 클립 평면
        public Vector4 vNormalToGravity; // 중력 방향
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonData[] SkeletonData; // 스켈레톤 데이터 배열
    }

    public struct NuiTransformSmoothParameters
    {
        public float fSmoothing; // 부드러움
        public float fCorrection; // 보정
        public float fPrediction; // 예측
        public float fJitterRadius; // 지터 반경
        public float fMaxDeviationRadius; // 최대 편차 반경
    }

    public struct NuiSkeletonBoneRotation
    {
        public Matrix4x4 rotationMatrix; // 회전 행렬
        public Quaternion rotationQuaternion; // 회전 쿼터니언
    }

    public struct NuiSkeletonBoneOrientation
    {
        public NuiSkeletonPositionIndex endJoint; // 끝 조인트
        public NuiSkeletonPositionIndex startJoint; // 시작 조인트
        public NuiSkeletonBoneRotation hierarchicalRotation; // 계층적 회전
        public NuiSkeletonBoneRotation absoluteRotation; // 절대 회전
    }

    public struct NuiImageViewArea
    {
        public int eDigitalZoom; // 디지털 줌
        public int lCenterX; // 중심 X
        public int lCenterY; // 중심 Y
    }

    public class NuiImageBuffer
    {
        public int m_Width; // 이미지 너비
        public int m_Height; // 이미지 높이
        public int m_BytesPerPixel; // 픽셀당 바이트 수
        public IntPtr m_pBuffer; // 이미지 버퍼 포인터
    }

    public struct NuiImageFrame
    {
        public Int64 liTimeStamp; // 타임스탬프
        public uint dwFrameNumber; // 프레임 번호
        public NuiImageType eImageType; // 이미지 타입
        public NuiImageResolution eResolution; // 이미지 해상도
        public IntPtr pFrameTexture; // 프레임 텍스처 포인터
        public uint dwFrameFlags_NotUsed; // 사용되지 않는 플래그
        public NuiImageViewArea ViewArea_NotUsed; // 사용되지 않는 뷰 영역
    }

    public struct NuiLockedRect
    {
        public int pitch; // 피치
        public int size; // 크기
        public IntPtr pBits; // 비트 포인터
    }

    public struct ColorCust
    {
        public byte b; // 블루
        public byte g; // 그린
        public byte r; // 레드
        public byte a; // 알파
    }

    public struct ColorBuffer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.Struct)]
        public ColorCust[] pixels; // 픽셀 배열
    }

    public struct DepthBuffer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.U2)]
        public ushort[] pixels; // 깊이 픽셀 배열
    }

    public struct NuiSurfaceDesc
    {
        uint width; // 너비
        uint height; // 높이
    }

    [Guid("13ea17f5-ff2e-4670-9ee5-1297a6e880d1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport()]
    public interface INuiFrameTexture
    {
        // 프레임 텍스처의 버퍼 길이를 반환합니다.
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [PreserveSig]
        int BufferLen();

        // 프레임 텍스처의 피치를 반환합니다.
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [PreserveSig]
        int Pitch();

        // 텍스처의 사각형을 잠그고 정보를 반환합니다.
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [PreserveSig]
        int LockRect(uint Level, ref NuiLockedRect pLockedRect, IntPtr pRect, uint Flags);

        // 레벨 설명을 반환합니다.
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [PreserveSig]
        int GetLevelDesc(uint Level, ref NuiSurfaceDesc pDesc);

        // 텍스처의 사각형 잠금을 해제합니다.
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [PreserveSig]
        int UnlockRect(uint Level);
    }

    // Kinect NUI (일반) 함수들
    [DllImport(@"Kinect10.dll", EntryPoint = "NuiInitialize")]
    public static extern int NuiInitialize(NuiInitializeFlags dwFlags);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiShutdown")]
    public static extern void NuiShutdown();

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiCameraElevationSetAngle")]
    public static extern int NuiCameraElevationSetAngle(int angle);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiCameraElevationGetAngle")]
    public static extern int NuiCameraElevationGetAngle(out int plAngleDegrees);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution")]
    public static extern int NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(NuiImageResolution eColorResolution, NuiImageResolution eDepthResolution, ref NuiImageViewArea pcViewArea, int lDepthX, int lDepthY, ushort sDepthValue, out int plColorX, out int plColorY);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiGetSensorCount")]
    public static extern int NuiGetSensorCount(out int pCount);

    // Kinect 스켈레톤 함수들
    [DllImport(@"Kinect10.dll", EntryPoint = "NuiSkeletonTrackingEnable")]
    public static extern int NuiSkeletonTrackingEnable(IntPtr hNextFrameEvent, uint dwFlags);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiSkeletonGetNextFrame")]
    public static extern int NuiSkeletonGetNextFrame(uint dwMillisecondsToWait, ref NuiSkeletonFrame pSkeletonFrame);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiTransformSmooth")]
    public static extern int NuiTransformSmooth(ref NuiSkeletonFrame pSkeletonFrame, ref NuiTransformSmoothParameters pSmoothingParams);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiSkeletonCalculateBoneOrientations")]
    public static extern int NuiSkeletonCalculateBoneOrientations(ref NuiSkeletonData pSkeletonData, NuiSkeletonBoneOrientation[] pBoneOrientations);

    // Kinect 비디오 함수들
    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageStreamOpen")]
    public static extern int NuiImageStreamOpen(NuiImageType eImageType, NuiImageResolution eResolution, uint dwImageFrameFlags_NotUsed, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageStreamGetNextFrame")]
    public static extern int NuiImageStreamGetNextFrame(IntPtr phStreamHandle, uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageStreamReleaseFrame")]
    public static extern int NuiImageStreamReleaseFrame(IntPtr phStreamHandle, IntPtr ppcImageFrame);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageStreamSetImageFrameFlags")]
    public static extern int NuiImageStreamSetImageFrameFlags(IntPtr phStreamHandle, NuiImageStreamFlags dvImageFrameFlags);

    [DllImport(@"Kinect10.dll", EntryPoint = "NuiImageResolutionToSize")]
    public static extern int NuiImageResolutionToSize(NuiImageResolution eResolution, out uint frameWidth, out uint frameHeight);

    /// <summary>
    /// Kinect 오류 코드를 문자열로 변환합니다.
    /// </summary>
    /// <param name="hr">오류 코드</param>
    /// <returns>오류 메시지</returns>
    public static string GetNuiErrorString(int hr)
    {
        string message = string.Empty;
        uint uhr = (uint)hr;

        switch (uhr)
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
        return Constants.DepthImageWidth; // 깊이 이미지 너비 반환
    }

    public static int GetDepthHeight()
    {
        return Constants.DepthImageHeight; // 깊이 이미지 높이 반환
    }

    public static int GetColorWidth()
    {
        return Constants.ColorImageWidth; // 색상 이미지 너비 반환
    }

    public static int GetColorHeight()
    {
        return Constants.ColorImageHeight; // 색상 이미지 높이 반환
    }

    /// <summary>
    /// 스켈레톤 포인트를 깊이 포인트로 매핑합니다.
    /// </summary>
    /// <param name="skeletonPoint">스켈레톤 포인트</param>
    /// <returns>매핑된 깊이 포인트</returns>
    public static Vector3 MapSkeletonPointToDepthPoint(Vector3 skeletonPoint)
    {
        float fDepthX;
        float fDepthY;
        float fDepthZ;

        NuiTransformSkeletonToDepthImage(skeletonPoint, out fDepthX, out fDepthY, out fDepthZ);

        Vector3 point = new Vector3();
        point.x = (int)((fDepthX * Constants.DepthImageWidth) + 0.5f);
        point.y = (int)((fDepthY * Constants.DepthImageHeight) + 0.5f);
        point.z = (int)(fDepthZ + 0.5f);

        return point;
    }

    // 스켈레톤 포인트를 깊이 이미지로 변환
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

    /// <summary>
    /// 주어진 조인트 인덱스의 부모 조인트 인덱스를 반환합니다.
    /// </summary>
    /// <param name="jointIndex">조인트 인덱스</param>
    /// <returns>부모 조인트 인덱스</returns>
    public static int GetSkeletonJointParent(int jointIndex)
    {
        switch (jointIndex)
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

        return (int)NuiSkeletonPositionIndex.HipCenter; // 기본값
    }

    /// <summary>
    /// 주어진 조인트 인덱스의 반대편 조인트 인덱스를 반환합니다.
    /// </summary>
    /// <param name="jointIndex">조인트 인덱스</param>
    /// <returns>반대편 조인트 인덱스</returns>
    public static int GetSkeletonMirroredJoint(int jointIndex)
    {
        switch (jointIndex)
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

        return jointIndex; // 기본값
    }

    /// <summary>
    /// 스켈레톤 프레임의 새로운 데이터를 가져오고 스무딩을 적용합니다.
    /// </summary>
    /// <param name="smoothParameters">스무딩 매개변수</param>
    /// <param name="skeletonFrame">스켈레톤 프레임</param>
    /// <returns>새로운 스켈레톤 데이터의 여부</returns>
    public static bool PollSkeleton(ref NuiTransformSmoothParameters smoothParameters, ref NuiSkeletonFrame skeletonFrame)
    {
        bool newSkeleton = false;

        int hr = KinectWrapper.NuiSkeletonGetNextFrame(0, ref skeletonFrame);
        if (hr == 0)
        {
            newSkeleton = true;
        }

        if (newSkeleton)
        {
            hr = KinectWrapper.NuiTransformSmooth(ref skeletonFrame, ref smoothParameters);
            if (hr != 0)
            {
                Debug.Log("Skeleton Data Smoothing failed");
            }
        }

        return newSkeleton;
    }

    /// <summary>
    /// 컬러 이미지 프레임을 가져옵니다.
    /// </summary>
    /// <param name="colorStreamHandle">컬러 스트림 핸들</param>
    /// <param name="videoBuffer">비디오 버퍼</param>
    /// <param name="colorImage">컬러 이미지 데이터</param>
    /// <returns>새로운 컬러 데이터의 여부</returns>
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

    /// <summary>
    /// 깊이 이미지 프레임을 가져옵니다.
    /// </summary>
    /// <param name="depthStreamHandle">깊이 스트림 핸들</param>
    /// <param name="isNearMode">근거리 모드 여부</param>
    /// <param name="depthPlayerData">깊이 및 플레이어 데이터</param>
    /// <returns>새로운 깊이 데이터의 여부</returns>
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

            frameTexture.LockRect(0, ref lockedRectPtr, r, 0);

            DepthBuffer db = (DepthBuffer)Marshal.PtrToStructure(lockedRectPtr.pBits, typeof(DepthBuffer));
            depthPlayerData = db.pixels;

            frameTexture.UnlockRect(0);
            hr = KinectWrapper.NuiImageStreamReleaseFrame(depthStreamHandle, imageFramePtr);
        }

        return newDepth;
    }

    // 두 인덱스 사이의 위치를 반환합니다.
    private static Vector3 GetPositionBetweenIndices(ref Vector3[] jointsPos, NuiSkeletonPositionIndex p1, NuiSkeletonPositionIndex p2)
    {
        Vector3 pVec1 = jointsPos[(int)p1];
        Vector3 pVec2 = jointsPos[(int)p2];

        return pVec2 - pVec1;
    }

    // 행렬을 열을 사용하여 채웁니다.
    private static void PopulateMatrix(ref Matrix4x4 jointOrientation, Vector3 xCol, Vector3 yCol, Vector3 zCol)
    {
        jointOrientation.SetColumn(0, xCol);
        jointOrientation.SetColumn(1, yCol);
        jointOrientation.SetColumn(2, zCol);
    }

    // x축을 지정하는 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromX(Vector3 v1, ref Matrix4x4 jointOrientation, bool flip)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 첫 번째 열을 현재 조인트와 이전 조인트 사이의 벡터로 설정
        xCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직인 임의의 벡터로 설정
        yCol.x = 0.0f;
        yCol.y = !flip ? xCol.z : -xCol.z;
        yCol.z = !flip ? -xCol.y : xCol.y;
        yCol.Normalize();

        // 세 번째 열은 첫 번째 두 열의 외적에 의해 결정됨
        zCol = Vector3.Cross(xCol, yCol);

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // y축을 지정하는 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromY(Vector3 v1, ref Matrix4x4 jointOrientation)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 첫 번째 열을 현재 조인트와 이전 조인트 사이의 벡터로 설정
        yCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직인 임의의 벡터로 설정
        xCol.x = yCol.y;
        xCol.y = -yCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        // 세 번째 열은 첫 번째 두 열의 외적에 의해 결정됨
        zCol = Vector3.Cross(xCol, yCol);

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // z축을 지정하는 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromZ(Vector3 v1, ref Matrix4x4 jointOrientation)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 첫 번째 열을 현재 조인트와 이전 조인트 사이의 벡터로 설정
        zCol = v1.normalized;

        // 두 번째 열을 첫 번째 열에 수직인 임의의 벡터로 설정
        xCol.x = zCol.y;
        xCol.y = -zCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        // 세 번째 열은 첫 번째 두 열의 외적에 의해 결정됨
        yCol = Vector3.Cross(zCol, xCol);

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // x축을 지정하는 두 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromXY(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 열을 재배열하고 플립하여 설정
        xCol = xUnnormalized.normalized;
        zCol = Vector3.Cross(xCol, yUnnormalized.normalized).normalized;
        yCol = Vector3.Cross(zCol, xCol).normalized;

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // y축을 지정하는 두 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromYX(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 열을 재배열하고 플립하여 설정
        yCol = yUnnormalized.normalized;
        zCol = Vector3.Cross(xUnnormalized.normalized, yCol).normalized;
        xCol = Vector3.Cross(yCol, zCol).normalized;

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // z축을 지정하는 두 벡터에서 방향을 만듭니다.
    private static void MakeMatrixFromYZ(Vector3 yUnnormalized, Vector3 zUnnormalized, ref Matrix4x4 jointOrientation)
    {
        // 행렬 열 변수
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        // 열을 재배열하고 플립하여 설정
        yCol = yUnnormalized.normalized;
        xCol = Vector3.Cross(yCol, zUnnormalized.normalized).normalized;
        zCol = Vector3.Cross(xCol, yCol).normalized;

        // 행렬에 값 복사
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    /// <summary>
    /// 조인트 위치와 추적 상태에 따라 조인트 방향을 계산합니다.
    /// </summary>
    /// <param name="jointsPos">조인트 위치 배열</param>
    /// <param name="jointsTracked">조인트 추적 상태 배열</param>
    /// <param name="jointOrients">조인트 방향 배열</param>
    public static void GetSkeletonJointOrientation(ref Vector3[] jointsPos, ref bool[] jointsTracked, ref Matrix4x4[] jointOrients)
    {
        Vector3 vx;
        Vector3 vy;
        Vector3 vz;

        // NUI_SKELETON_POSITION_HIP_CENTER
        if (jointsTracked[(int)NuiSkeletonPositionIndex.HipCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Spine] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipCenter, NuiSkeletonPositionIndex.Spine);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipCenter]);

            // 약 40도 앞쪽으로의 보정
            Matrix4x4 mat = jointOrients[(int)NuiSkeletonPositionIndex.HipCenter];
            Quaternion quat = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
            quat *= Quaternion.Euler(-40, 0, 0);
            jointOrients[(int)NuiSkeletonPositionIndex.HipCenter].SetTRS(Vector3.zero, quat, Vector3.one);
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight])
        {
            // NUI_SKELETON_POSITION_SPINE
            if (jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
            {
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Spine]);
            }

            if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
            {
                // NUI_SKELETON_POSITION_SHOULDER_CENTER
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderCenter, NuiSkeletonPositionIndex.Head);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderCenter]);

                // NUI_SKELETON_POSITION_HEAD
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
        {
            // NUI_SKELETON_POSITION_SHOULDER_LEFT
            {
                vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderLeft]);
            }

            // NUI_SKELETON_POSITION_ELBOW_LEFT
            {
                vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowLeft]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft] &&
         jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
        {
            vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristLeft]);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandLeft]);
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
        {
            // NUI_SKELETON_POSITION_SHOULDER_RIGHT
            {
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderRight]);
            }

            // NUI_SKELETON_POSITION_ELBOW_RIGHT
            {
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowRight]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
        {
            // NUI_SKELETON_POSITION_WRIST_RIGHT
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristRight]);

            // NUI_SKELETON_POSITION_HAND_RIGHT
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandRight]);
        }

        // NUI_SKELETON_POSITION_HIP_LEFT
        if (jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.HipLeft);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipLeft]);

            // NUI_SKELETON_POSITION_KNEE_LEFT
            if (jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft])
            {
                vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeLeft]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
        {
            // NUI_SKELETON_POSITION_ANKLE_LEFT
            vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
            vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);

            // NUI_SKELETON_POSITION_FOOT_LEFT
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
        }

        // NUI_SKELETON_POSITION_HIP_RIGHT
        if (jointsTracked[(int)NuiSkeletonPositionIndex.HipRight] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.HipRight);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipRight]);

            // NUI_SKELETON_POSITION_KNEE_RIGHT
            if (jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight])
            {
                vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeRight]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
        {
            // NUI_SKELETON_POSITION_ANKLE_RIGHT
            vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
            vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);

            // NUI_SKELETON_POSITION_FOOT_RIGHT
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
        }
    }
}