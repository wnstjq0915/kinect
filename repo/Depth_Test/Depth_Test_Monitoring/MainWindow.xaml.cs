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

namespace Depth_Test_Monitoring
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
             * 이미지1: 컬러스트림
             * 이미지2: 스켈레톤을 이용하여 사용자가 인식된 부분만
             *          흰색으로 표시하는 뎁스스트림
             * 이미지3: 솔루션1에서 표현한 뎁스스트림
             */

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
}
