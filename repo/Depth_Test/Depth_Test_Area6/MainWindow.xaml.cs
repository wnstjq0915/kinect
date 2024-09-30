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

namespace Depth_Test_Area6
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
             * 처음 인식된 사용자 한명만 이용
             * 
             * 솔루션5를 이용하여
             * 해당 사용자의 픽셀 수 계산 및 출력
             * 
             * 솔루션4를 이용하여
             * 해당 사용자가 인식된 픽셀의 거리의 총 합 / 해당 사용자의 픽셀 수
             * 를 이용하여 평균거리 계산 및 출력
             * 
             * 
             * (총 픽셀 수 * 총 거리 수) / 1000000000
             * 를 하여 무게 추정 및 출력
             */

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
}
