using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Diagnostics;

namespace render.Models
{
    public class Texture
    {
        private Mat _imageData;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(string filePath)
        {
            // 读取图像，使用 Color 模式（彩色图像）
            _imageData = CvInvoke.Imread(filePath, ImreadModes.Color);
            if (_imageData.IsEmpty)
            {
                throw new Exception("无法读取图像文件: " + filePath);
            }

            // 注意：Emgu CV 默认以 BGR 格式读取图像，
            // 如果你希望得到 RGB 格式，可以进行颜色转换
            CvInvoke.CvtColor(_imageData, _imageData, ColorConversion.Bgr2Rgb);

            Width = _imageData.Width;
            Height = _imageData.Height;
        }

        public Vector3 GetColor(float u, float v)
        {
            
            // 将 u,v 转换为图像像素坐标（注意对 v 进行反转，因为图像坐标从上到下）
            int uImg = (int)(u * Width);
            int vImg = (int)((1 - v) * Height);

            // 保证索引在有效范围内
            uImg = Math.Clamp(uImg, 0, Width - 1);
            vImg = Math.Clamp(vImg, 0, Height - 1);

            // 为了方便获取像素值，将 Mat 转换为 Image<Bgr, byte>
            using (Image<Bgr, byte> image = _imageData.ToImage<Bgr, byte>())
            {
                // 获取像素颜色（由于之前转换为 RGB，此处 color 的分量顺序即为 R、G、B）
                Bgr color = image[vImg, uImg];
                return new Vector3((float)color.Blue, (float)color.Green, (float)color.Red);
            }

            
        }
    }
}
