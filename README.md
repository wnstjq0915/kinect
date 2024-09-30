# kinect
## 목차
### 1. 뎁스스트림
### 2. 영상모니터
### 3. 사용자인덱스
### 4. 거리
### 5. 사용자저장
### 6. 면적측정

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
