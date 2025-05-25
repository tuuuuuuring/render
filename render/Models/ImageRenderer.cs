using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using render.Models;
using System.Diagnostics;
using SkiaSharp;
using Emgu.CV.CvEnum;
using Emgu.CV;
using System.Runtime.InteropServices;

namespace render.Models
{
    public class ImageRenderer
    {
        /*public static System.Drawing.Bitmap ConvertToBitmap(List<Models.Vector3> frameBuffer)
        {
            Debug.WriteLine("ConvertToBitmap called");
            int width = 700;
            int height = 700;

            // 创建Bitmap对象（24位RGB格式）
            var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // 锁定内存区域以直接操作像素数据
            var bmpData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );

            try
            {
                int bytesPerPixel = 3; // 24bpp = 3字节（BGR格式）
                byte[] pixelData = new byte[bmpData.Stride * height];

                // 遍历所有像素
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // 计算frameBuffer中的索引（假设按行优先存储）
                        int index = y * width + x;
                        var pixel = frameBuffer[index];

                        // 将RGB分量钳制到[0,1]范围并转换为字节
                        byte r = (byte)(Math.Clamp(pixel.X, 0f, 1f));
                        byte g = (byte)(Math.Clamp(pixel.Y, 0f, 1f));
                        byte b = (byte)(Math.Clamp(pixel.Z, 0f, 1f));

                        // 计算在pixelData中的偏移量（BGR顺序）
                        int offset = y * bmpData.Stride + x * bytesPerPixel;
                        pixelData[offset] = b;     // 蓝色通道
                        pixelData[offset + 1] = g; // 绿色通道
                        pixelData[offset + 2] = r; // 红色通道
                    }
                }

                // 将数据复制到Bitmap内存
                System.Runtime.InteropServices.Marshal.Copy(
                    pixelData, 0, bmpData.Scan0, pixelData.Length
                );
            }
            finally
            {
                bitmap.UnlockBits(bmpData); // 确保解锁内存
            }

            return bitmap;
        }*/

        public static void SaveImage(string filename, List<Models.Vector3> frameBuffer)
        {
            int width = 700;
            int height = 700;

            // 创建一个空的图像矩阵，类型为 8UC3 (BGR)
            Mat image = new Mat(height, width, DepthType.Cv8U, 3);

            // 获取图像数据指针
            IntPtr data = image.DataPointer;

            // 填充图像数据
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (index < frameBuffer.Count)
                    {
                        Models.Vector3 pixel = frameBuffer[index];
                        // 每个像素占3个字节 (BGR)，因此偏移量是 3 * (y * width + x)
                        int offset = (y * width + x) * 3;

                        // 将像素的 RGB 值转换为 BGR
                        Marshal.WriteByte(data, offset, (byte)(pixel.Z));       // B
                        Marshal.WriteByte(data, offset + 1, (byte)(pixel.Y));   // G
                        Marshal.WriteByte(data, offset + 2, (byte)(pixel.X));   // R

                        index++;
                    }
                }
            }

            // 保存图像
            CvInvoke.Imwrite(filename, image);
        }

        public static SKBitmap ConvertToBitmap(List<Models.Vector3> frameBuffer)
        {
            int width = 700;
            int height = 700;

            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    var pixel = frameBuffer[index];

                    // 浮点数转字节（0~1 → 0~255）
                    byte r = (byte)(pixel.X);
                    byte g = (byte)(pixel.Y);
                    byte b = (byte)(pixel.Z);

                    bitmap.SetPixel(x, y, new SKColor(r, g, b));
                }
            }

            return bitmap;
        }

        public SKBitmap Render(string Obj_path, string texture_path, Class1 class1, IProgress<double> progress)
        {
            Debug.WriteLine("Render called");
            List<Triangle> triangles = new List<Triangle>();
            float angle = 140.0f;
            Models.Vector3 eye_pos = new Models.Vector3(0, 0, 10f);

            // string filename = @"D:\kj\GAMES101_作业\Assignment3\Code\build\output.png";
            ObjLoader Loader = new ObjLoader();
           
            bool loadout = Loader.LoadFile(Obj_path);

            Debug.WriteLine("obj loaded");
            foreach (var mesh in Loader.LoadedMeshes)
            {
                for (int i = 0; i < mesh.Vertices.Count; i += 3)
                {
                    Triangle t = new Triangle();
                    for (int j = 0; j < 3; j++)
                    {
                        t.setVertex(j, new Vector4(mesh.Vertices[i + j].Position.X, mesh.Vertices[i + j].Position.Y, mesh.Vertices[i + j].Position.Z, 1.0f));
                        t.setNormal(j, new Models.Vector3(mesh.Vertices[i + j].Normal.X, mesh.Vertices[i + j].Normal.Y, mesh.Vertices[i + j].Normal.Z));
                        t.setTexCoord(j, new Models.Vector2(mesh.Vertices[i + j].TextureCoordinate.X, mesh.Vertices[i + j].TextureCoordinate.Y));
                    }
                    triangles.Add(t);
                }
            }

            Rasterizer r = new Rasterizer(700, 700);
            Debug.WriteLine("Rasterizer created     ");
            r.FragmentShader = Minor.texture_fragment_shader;
            
            r.texture = new Texture(texture_path);
            r.VertexShader = Minor.vertex_shader;
            
            r.clear(Buffers.Color | Buffers.Depth);
            r.set_model(Minor.get_model_matrix(angle));
            r.set_view(Minor.get_view_matrix(eye_pos));
            r.set_projection(Minor.get_projection_matrix(45.0f, 1f, 0.1f, 50.0f));

            r.draw(triangles, class1, progress);

            List<Models.Vector3> frameBuffer = r.frame_buf;

            // 1. 检查数据长度
            if (frameBuffer.Count != 700 * 700)
            {
                throw new ArgumentException($"帧缓冲大小 {frameBuffer.Count} 不匹配预期的 700x700 图像尺寸。");
            }

            string outPath = @"D:\kj\bunny\build\output.png";
            SaveImage(outPath, frameBuffer);
            return ConvertToBitmap(frameBuffer);
        }
    }
}
