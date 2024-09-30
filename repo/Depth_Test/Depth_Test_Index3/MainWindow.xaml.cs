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

namespace Depth_Test_Index3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            /*
             * 테스트해볼 것
             * 스켈레톤 활성화 코드가 있어야 작동이 되는지
             * 또는 차이점이 있는지 확인하기
             * 
             * 결과물
             * 뎁스스트림을 이용하여
             * 사용자마다 색을 다르게 출력
             * 
             * 솔루션2에서 Players 함수에서의 if문만
             * player > 0   -> 흰 색
             * 조건을
             * player == 1  -> 흰색
             * player == 2  -> 연붉은색
             * 등
             * 플레이어마다 색을 지정하여 출력
             */

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
}
