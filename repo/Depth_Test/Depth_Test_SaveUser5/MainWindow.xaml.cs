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
using System.IO;

namespace Depth_Test_SaveUser5
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
             * 솔루션3을 이용하여
             * 각 유저의 색을 다르게 출력하고
             * 
             * 사용자 각각 화면에 얼마나 출력되는지를
             * 픽셀 수로 출력
             * 
             * 저장 버튼을 누를 경우
             * 각각의 유저의 픽셀만이 담긴 사진을
             * 생성하여 저장하고 사진 열기
             * 
             * 동작
             * {0, 0, 0}의 플레이어1~3의 픽셀 수가 저장되는
             * 배열을 생성한다.
             * 
             * 픽셀을 순회하며 플레이어가 인식되면
             * 배열[player-1 인덱스]의 값을 1 더하여
             * 최종적으로 각 플레이어의 픽셀 수를
             * 텍스트블록에 출력한다.
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

                // 교안 코드
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

                
                // 최적화를 위해 위 코드와 같은지 테스트 해 볼 코드
                /*
                if(player > 0 )
                {
                    if (nSel == 0 || player == nSel)
                    {
                        int j = player - 1;
                        SetRGB(playerCoded, i32, cColorR[j], cColorG[j], cColorB[j]);
                        lCount[j] += 1;
                    }
                }
                */
                // ㅡㅡㅡㅡㅡㅡㅡㅡ

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
}
