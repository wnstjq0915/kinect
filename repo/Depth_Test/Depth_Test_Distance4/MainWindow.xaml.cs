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

namespace Depth_Test_Distance4
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
             * 뎁스스트림을 이용하여
             * 모든 요소의 거리를 측정한 뒤에
             * 각각의 거리를 조건문을 통하여
             * 색을 다르게 표현
             * 
             * ex)
             * 3500 거리 이상: 흰색
             * 3000 거리 이상: 빨간색
             * 2500 거리 이상: 초록색
             * 등
             * 
             * 각각의 플레이어 인식
             * int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
             * 
             * 각각의 거리 인식
             * int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
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
}
