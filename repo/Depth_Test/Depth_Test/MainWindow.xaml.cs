using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Depth_Test
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            /*
             * 결과물
             * 이미지1: 뎁스스트림을 이용하여 인식된 거리에 따라
             *          밝기가 다른 흑백영상 출력
             */

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
                                    ImageParam.Width * ImageParam.BytesPerPixel);   // 픽셀 데이터가 저장된 메모리의 크기

                                    // 96: 일반적인 모니터 해상도
                                    // Gray16: 65536가지의 회색조를 표현할 수 있는 16BPP 회색조 채널을 표시하는 형식

                image1.Source = src;
            }
        }
    }
}
