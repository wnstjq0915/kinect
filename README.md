# kinect
- [진행](https://github.com/wnstjq0915/kinect/blob/main/%EC%A7%84%ED%96%89.md)
- [GPT](https://github.com/wnstjq0915/kinect/blob/main/gpt.md)

- 메모: vs 22버전에서 ctrl T 누르면 변수명 코드검색 쉽게 가능

## 메모
### 캐릭터가 벽에 부딛치면 잠시 키넥트 연동스크립트 끊기
#### 1.
```csharp
GameObject obj;
obj.AddComponent<TestScript>(); 
TestScript script = obj.GetComponent();
Destroy(script);
```

```csharp
using UnityEngine;

public static class ExtensionMethods
{
    public static void RemoveComponent<Component>(this GameObject obj, bool immediate = false)
    {
        Component component = obj.GetComponent<Component>();

        if (component != null)
        {
            if (immediate)
            {
                Object.DestroyImmediate(component as Object, true);
            }
            else
            {
                Object.Destroy(component as Object);
            }

        }
    }
}
```

#### 2. 추천
```csharp
gameObj.setActive(true);
// component.setActive(true);
```

## 목차
### [1. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#1-%EB%8E%81%EC%8A%A4%EC%8A%A4%ED%8A%B8%EB%A6%BC-1)뎁스스트림
### [2. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#2-%EC%98%81%EC%83%81%EB%AA%A8%EB%8B%88%ED%84%B0-1)영상모니터
### [3. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#3-%EC%82%AC%EC%9A%A9%EC%9E%90%EC%9D%B8%EB%8D%B1%EC%8A%A4-1)사용자인덱스
### [4. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#4-%EA%B1%B0%EB%A6%AC-1)거리
### [5. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#5-%EC%82%AC%EC%9A%A9%EC%9E%90%EC%A0%80%EC%9E%A5-1)사용자저장
### [6. ](https://github.com/wnstjq0915/kinect?tab=readme-ov-file#6-%EB%A9%B4%EC%A0%81%EC%B8%A1%EC%A0%95-1)면적측정

## 1. 뎁스스트림
### 결과물
- 이미지1: 뎁스스트림을 이용하여 인식된 거리에 따라 밝기가 다른 흑백영상 출력
```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {

        InitializeComponent();
        InitializeNui(); // 키넥트 초기화 메소드
        KinectSensor nui = null;
        void InitializeNui()
        {
            nui = KinectSensor.KinectSensors[0];    // 연결된 키넥트 중 첫번째 키넥트 참조
            nui.DepthStream.Enable();               // Depth Stream 활성화
            nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
            // nui.DepthFrameReady: Kinect 센서가 새로운 Depth Frame을 감지했을 때 발생하는 이벤트
            nui.Start();
        }
        void nui_DepthFrameReady(Object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame ImageParam = e.OpenDepthImageFrame();

            if (ImageParam == null) return;

            short[] ImageBits = new short[ImageParam.PixelDataLength];
            ImageParam.CopyPixelDataTo(ImageBits);

            BitmapSource src = null;    // BitmapSource: 이미지를 다루는 클래스
            src = BitmapSource.Create(ImageParam.Width, // 이미지의 너비
                                ImageParam.Height,      // 이미지의 높이
                                96, 96,                 // 이미지의 가로, 세로 (출력장치)해상도
                                PixelFormats.Gray16,    // 이미지의 픽셀 형식(픽셀이 어떻게 저장될지)
                                null,                   // 제한된 색상을 사용할 때 이용
                                ImageBits,              // 이미지 데이터가 있는 메모리 영역
                                ImageParam.Width * ImageParam.BytesPerPixel);
                                // 픽셀 데이터가 저장된 메모리의 크기

            // 96: 일반적인 모니터 해상도
            // Gray16: 65536가지의 회색조를 표현할 수 있는 16BPP 회색조 채널을 표시하는 형식

            image1.Source = src;
        }
    }
}
```

## 2. 영상모니터
### 결과물
- 이미지1: 컬러스트림
- 이미지2: 스켈레톤을 이용하여 사용자가 인식된 부분만 흰색으로 표시하는 뎁스스트림
- 이미지3: 솔루션1에서 표현한 뎁스스트림
```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNui();
    }
    KinectSensor nui = null;

    void InitializeNui()
    {
        nui = KinectSensor.KinectSensors[0];
        // 컬러스트림 생성
        nui.ColorStream.Enable();   // 5주차에 진행한 컬러스트림 활성화
        nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);

        // 뎁스스트림 생성
        nui.DepthStream.Enable();
        nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);

        // 사용자 인식 정보 얻기
        nui.SkeletonStream.Enable(); // 스켈레톤 활성화
        nui.Start();
    }


    void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
    {
        ColorImageFrame ImageParam = e.OpenColorImageFrame();

        if (ImageParam == null) return;

        byte[] ImageBits = new byte[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);

        BitmapSource src = null;
        src = BitmapSource.Create(ImageParam.Width,
                                ImageParam.Height,
                                96, 96,
                                PixelFormats.Bgr32,
                                null,
                                ImageBits,
                                ImageParam.Width * ImageParam.BytesPerPixel);
        image1.Source = src;
    }


    void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
    {
        DepthImageFrame ImageParam = e.OpenDepthImageFrame();

        if (ImageParam == null) return;

        short[] ImageBits = new short[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);

        WriteableBitmap wb = null;
        // 읽기 전용인 BitmapSource 클래스와는 다르게
        // 픽셀 조작 및 실시간 이미지 처리가 필요한 경우 사용
        wb = new WriteableBitmap(ImageParam.Width,
                                ImageParam.Height,
                                96, 96,
                                PixelFormats.Bgr32,
                                null);
        wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                Players(ImageParam,
                    ImageBits,
                    ((KinectSensor)sender).DepthStream),
                ImageParam.Width * 4,
                0);
        image2.Source = wb;

        BitmapSource src = null;
        src = BitmapSource.Create(ImageParam.Width,
                        ImageParam.Height,
                        96, 96,
                        PixelFormats.Gray16,
                        null,
                        ImageBits,
                        ImageParam.Width * ImageParam.BytesPerPixel);
        image3.Source = src;
    }


    byte[] Players(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream)
    {
        byte[] nPlayers = new byte[PImage.Width * PImage.Height * 4];

        // 각각의 픽셀에 대한 작업 수행
        for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < nPlayers.Length;
            i16++, i32 += 4)
        {
            // i16: 16비트 단위로 Depth 데이터 처리
            // i32: 32비트 단위로 RGBA 데이터 처리

            // 플레이어 정보 확인
            int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;

            if (player > 0)
            {
                // 사용자정보인 것이 확인되면 해당영역을 흰 색으로 출력
                nPlayers[i32 + 2] = 255;
                nPlayers[i32 + 1] = 255;
                nPlayers[i32 + 0] = 255;
            }
        }
        return nPlayers;
    }
}
```

## 3. 사용자인덱스
### 결과물
- 뎁스스트림을 이용하여 사용자마다 색을 다르게 출력
```
솔루션2에서 Players 함수에서의 if문만
player > 0   -> 흰 색
조건을
player == 1  -> 흰색
player == 2  -> 연붉은색
등
플레이어마다 색을 지정하여 출력
```

```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNui();
    }

    KinectSensor nui = null;
    void InitializeNui()
    {
        nui = KinectSensor.KinectSensors[0];
        nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
        nui.SkeletonStream.Enable();
        nui.Start();
    }
    void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
    {
        DepthImageFrame ImageParam = e.OpenDepthImageFrame();
        if (ImageParam == null) return;
        short[] ImageBits = new short[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);
        WriteableBitmap wb = new WriteableBitmap(ImageParam.Width,
                                                ImageParam.Height,
                                                96, 96,
                                                PixelFormats.Bgr32,
                                                null);
        wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                        Players(ImageParam,
                            ImageBits,
                            nui.DepthStream),
                        ImageParam.Width * 4,
                        0);

        image1.Source = wb;
    }

    private byte[] Players(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream)
    {
        byte[] nPlayers = new byte[PImage.Width * PImage.Height * 4];
        for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < nPlayers.Length;
            i16++, i32 += 4)
        {
            int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
            SetRGB(nPlayers, i32, 0, 0, 0);

            // 각각의 플레이어마다 색 지정
            if (player == 1) SetRGB(nPlayers, i32, 0xFF, 0x00, 0x00); // 1번 사용자
            if (player == 2) SetRGB(nPlayers, i32, 0xFF, 0x7F, 0x7F); // 연붉은색
            if (player == 3) SetRGB(nPlayers, i32, 0x00, 0xFF, 0x00); // 녹색
            if (player == 4) SetRGB(nPlayers, i32, 0x7F, 0xFF, 0x7F); // 파란색
            if (player == 5) SetRGB(nPlayers, i32, 0x00, 0x00, 0xFF);
            if (player == 6) SetRGB(nPlayers, i32, 0x7F, 0x7F, 0xFF);
            if (player == 7) SetRGB(nPlayers, i32, 0xFF, 0xFF, 0x00);
            if (player == 8) SetRGB(nPlayers, i32, 0x00, 0xFF, 0xFF);
            if (player == 9) SetRGB(nPlayers, i32, 0xFF, 0x00, 0xFF);
        }
        return nPlayers;
    }

    void SetRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b)
    {
        nPlayers[nPos + 2] = r;
        nPlayers[nPos + 1] = g;
        nPlayers[nPos + 0] = b;
    }
}
```

## 4. 거리
### 결과물
- 뎁스스트림을 이용하여 모든 요소의 거리를 측정한 뒤에 각각의 거리를 조건문을 통하여 색을 다르게 표현

```
ex)
3500 거리 이상: 흰색
3000 거리 이상: 빨간색
2500 거리 이상: 초록색
등
```

- 각각의 플레이어 인식
```c#
int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
```
- 각각의 거리 인식
```c#
int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
```
```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNui();
    }

    KinectSensor nui = null;

    void InitializeNui()
    {
        nui = KinectSensor.KinectSensors[0];
        nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        nui.DepthFrameReady += new
            EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
        // nui.SkeletonStream.Enable(); // 사용자를 인식해야 하는 코드가 아니므로 생략 가능
        nui.Start();
    }

    void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
    {
        DepthImageFrame ImageParam = e.OpenDepthImageFrame();

        if (ImageParam == null) return;

        short[] ImageBits = new short[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);
        WriteableBitmap wb = new WriteableBitmap(ImageParam.Width,
                                                ImageParam.Height,
                                                96, 96,
                                                PixelFormats.Bgr32,
                                                null);
        wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                    GetRGB(ImageParam,
                        ImageBits,
                        nui.DepthStream),
                    ImageParam.Width * 4,
                    0);
        image1.Source = wb;
    }

    byte[] GetRGB(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream)
    {
        byte[] rgbs = new byte[PImage.Width * PImage.Height * 4];

        for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < rgbs.Length; i16++,
            i32 += 4)
        {
            // 각 거리 계산
            int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            // 깊이 데이터를 플레이어 정보 비트마스크로 오른쪽으로 시프트(비트연산)하여
            // 플레이어 인덱스를 무시하고, 순수 깊이 값만 추출
            SetRGB(rgbs, i32, 0x00, 0x00, 0x00);

            if (nDistance > 3500) SetRGB(rgbs, i32, 0xFF, 0xFF, 0xFF);
            else if (nDistance > 3000) SetRGB(rgbs, i32, 0xFF, 0x00, 0x00);
            else if (nDistance > 2500) SetRGB(rgbs, i32, 0x00, 0xFF, 0x00);
            else if (nDistance > 2000) SetRGB(rgbs, i32, 0x00, 0x00, 0xFF);
            else if (nDistance > 1500) SetRGB(rgbs, i32, 0xFF, 0xFF, 0x00);
            else if (nDistance > 1000) SetRGB(rgbs, i32, 0x00, 0xFF, 0xFF);
            else if (nDistance > 800) SetRGB(rgbs, i32, 0xFF, 0x00, 0xFF);
            else if (nDistance > 0) SetRGB(rgbs, i32, 0x7F, 0x00, 0x00);
        }

        return rgbs;
    }

    void SetRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b)
    {
        nPlayers[nPos + 2] = r;
        nPlayers[nPos + 1] = g;
        nPlayers[nPos + 0] = b;
    }
}
```

## 5. 사용자저장
### 결과물
- 솔루션3을 이용하여 각 유저의 색을 다르게 출력하고 사용자 각각 화면에 얼마나 출력되는지를 픽셀 수로 출력

- 저장 버튼을 누를 경우 각각의 유저의 픽셀만이 담긴 사진을 생성하여 저장하고 사진 열기

### 동작
- {0, 0, 0}의 플레이어1~3의 픽셀 수가 저장되는 배열을 생성한다.
- 픽셀을 순회하며 플레이어가 인식되면 배열[player-1 인덱스]의 값을 1 더하여 최종적으로 각 플레이어의 픽셀 수를 텍스트블록에 출력한다.

```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNui();
    }

    KinectSensor nui = null;
    void InitializeNui()
    {
        nui = KinectSensor.KinectSensors[0];
        nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        nui.DepthFrameReady += new
            EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
        nui.SkeletonStream.Enable();
        nui.Start();
    }

    WriteableBitmap wb1 = null;
    WriteableBitmap wb2 = null;
    WriteableBitmap wb3 = null;

    void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
    {
        DepthImageFrame ImageParam = e.OpenDepthImageFrame();
        if (ImageParam == null) return;
        short[] ImageBits = new short[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);
        WriteableBitmap wb = new WriteableBitmap(ImageParam.Width,
                                            ImageParam.Height,
                                            96, 96,
                                            PixelFormats.Bgr32,
                                            null);
        wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                    GetPlayer(ImageParam,
                        ImageBits,
                        nui.DepthStream,
                        0),
                    ImageParam.Width * 4,
                    0);
        image1.Source = wb;

        wb1 = new WriteableBitmap(ImageParam.Width,
                                ImageParam.Height,
                                96, 96,
                                PixelFormats.Bgr32,
                                null);
        wb2 = new WriteableBitmap(ImageParam.Width,
                                ImageParam.Height,
                                96, 96,
                                PixelFormats.Bgr32,
                                null);
        wb3 = new WriteableBitmap(ImageParam.Width,
                                ImageParam.Height,
                                96, 96,
                                PixelFormats.Bgr32,
                                null);

        Int32Rect iRect = new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height);

        wb1.WritePixels(iRect, GetPlayer(ImageParam,
                                        ImageBits,
                                        nui.DepthStream, 1),
                        ImageParam.Width * 4,
                        0);
        wb2.WritePixels(iRect, GetPlayer(ImageParam,
                                        ImageBits,
                                        nui.DepthStream, 2),
                        ImageParam.Width * 4,
                        0);
        wb3.WritePixels(iRect, GetPlayer(ImageParam,
                                        ImageBits,
                                        nui.DepthStream, 3),
                        ImageParam.Width * 4,
                        0);
    }

    byte[] GetPlayer(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream, int nSel)
    {
        byte[] playerCoded = new byte[PImage.Width * PImage.Height * 4];

        byte[] cColorR = { 0x00, 0x00, 0xFF };
        byte[] cColorG = { 0x00, 0xFF, 0x00 };
        byte[] cColorB = { 0xFF, 0x00, 0x00 };
        long[] lCount = { 0, 0, 0 }; // 각 플레이어의 픽셀수가 담길 배열

        // 각각의 픽셀에 대한 작업 수행
        for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < playerCoded.Length;
            i16++, i32 += 4)
        {

            int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;

            SetRGB(playerCoded, i32, 0x00, 0x00, 0x00);

            // nSel 매개변수가 플레이어와 일치할 경우
            // 그 플레이어에 해당 색상을 픽셀에 설정하고
            // 플레이어의 픽셀 수 증가

            for (int j = 0; j < 3; j++)
            {

                if (player == j + 1)
                {
                    if (nSel == 0 || player == nSel)
                    {
                        SetRGB(playerCoded, i32, cColorR[j], cColorG[j], cColorB[j]);
                        lCount[j] += 1;
                    }
                    break;
                }
            }

        }

        // nSel이 0인 경우 각 플레이어의 픽셀 수를 텍스트 블록에 표시
        if (nSel == 0)
        {
            textBlock1.Text = string.Format("P1픽셀수 : {0}", lCount[0]);
            textBlock2.Text = string.Format("P2픽셀수 : {0}", lCount[1]);
            textBlock3.Text = string.Format("P3픽셀수 : {0}", lCount[2]);
        }

        return playerCoded;
    }
    void SetRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b)
    {
        nPlayers[nPos + 2] = r;
        nPlayers[nPos + 1] = g;
        nPlayers[nPos + 0] = b;
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
        SavePng(wb1, "c:\\Temp\\p1.png");
        SavePng(wb2, "c:\\Temp\\p2.png");
        SavePng(wb3, "c:\\Temp\\p3.png");

        OpenFile("c:\\Temp\\p1.png");
        OpenFile("c:\\Temp\\p2.png");
        OpenFile("c:\\Temp\\p3.png");
    }

    void SavePng(WriteableBitmap src, String strFilename) // 각각의 사용자의 픽셀만을 저장
    {
        BitmapEncoder encoder = null;
        encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(src));
        FileStream stream = new FileStream(strFilename,
                                        FileMode.Create,
                                        FileAccess.Write);
        encoder.Save(stream);
        stream.Close();
    }

    void OpenFile(String strFilename)
    {
        System.Diagnostics.Process exe = new System.Diagnostics.Process();
        exe.StartInfo.FileName = strFilename;
        exe.Start();
    }
}
```

## 6. 면적측정
### 결과물
- 처음 인식된 사용자 한명만 이용

- 솔루션5를 이용  
해당 사용자의 픽셀 수 계산 및 출력

- 솔루션4를 이용  
해당 사용자가 인식된 픽셀의 거리의 총 합 / 해당 사용자의 픽셀 수를 이용하여 평균거리 계산 및 출력

  
- (총 픽셀 수 * 총 거리 수) / 1000000000 를 하여 무게 추정 및 출력

```c#
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNui();
    }

    KinectSensor nui = null;

    void InitializeNui()
    {
        nui = KinectSensor.KinectSensors[0];
        nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        nui.DepthFrameReady += new
            EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
        nui.SkeletonStream.Enable();
        nui.Start();
    }

    void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
    {
        DepthImageFrame ImageParam = e.OpenDepthImageFrame();

        if (ImageParam == null) return;

        short[] ImageBits = new short[ImageParam.PixelDataLength];
        ImageParam.CopyPixelDataTo(ImageBits);

        WriteableBitmap wb = new WriteableBitmap(ImageParam.Width,
                                                ImageParam.Height,
                                                96, 96,
                                                PixelFormats.Bgr32,
                                                null);
        wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                        GetPlayer(ImageParam,
                                ImageBits, ((KinectSensor)sender).DepthStream),
                        ImageParam.Width * 4,
                        0);
        image1.Source = wb;
    }

    byte[] GetPlayer(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream)
    {
        byte[] playerCoded = new byte[PImage.Width * PImage.Height * 4];

        long lPixel = 0;
        long lDist = 0;
        int nPlayer = -1;

        for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < playerCoded.Length;
            i16++, i32 += 4)
        {
            int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
            int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            SetRGB(playerCoded, i32, 0x00, 0x00, 0x00);


            // 조건: 처음 인식된 플레이어 한명만 작동되도록.
            if (player > 0 && nPlayer <= 0) nPlayer = player;
            if (player == nPlayer)
            {
                if (nDistance < depthStream.TooFarDepth &&
                    nDistance > depthStream.TooNearDepth)
                {
                    // 플레이어가 적당한 거리에 있을 경우 실행
                    lDist += nDistance;
                    lPixel += 1;
                    // 처음 인식된 플레이어만 흰 색
                    SetRGB(playerCoded, i32, 0xFF, 0xFF, 0xFF);
                }
            }

        }
        if (lPixel > 0)
        {
            textBlock1.Text = string.Format("픽셀: {0}", lPixel); // 솔루션5와 같이 계산된 픽셀
            textBlock2.Text = string.Format("거리: {0}", lDist / lPixel); // 1픽셀 당 평균 거리

            float weight = (lPixel * lDist) / 1000000000; // 거리와 면적을 이용하여 무게 추정
            textBlock3.Text = string.Format("무게: {0:0} kg", weight);
        }

        return playerCoded;
    }

    void SetRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b)
    {
        nPlayers[nPos + 2] = r;
        nPlayers[nPos + 1] = g;
        nPlayers[nPos + 0] = b;
    }
}
```

## 기타 문서작성 메모
- C#에서 함수와 변수 등을 문서화 할 때 코드에 doxy 이용하면 쉽게 논문에 나오는 정의서 추출 가능