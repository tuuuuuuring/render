using render.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;

namespace render
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取文本框中的内容
            string text = MyTextBox.Text;

            List<Triangle> triangles = new List<Triangle>();
            float angle = 140.0f;
            Models.Vector3 eye_pos = new Models.Vector3(0, 0, 10f);

            string filename = @"D:\kj\GAMES101_作业\Assignment3\Code\build\output.png";
            ObjLoader Loader = new ObjLoader();
            string obj_path = @"D:\kj\bunny\spot_texture.png";
            string texture_path = "spot_texture.png";

            bool loadout = Loader.LoadFile(text);
            

            foreach (var mesh in Loader.LoadedMeshes)
            {
                for(int i = 0; i < mesh.Vertices.Count; i += 3)
                {
                    Triangle t = new Triangle();
                    for(int j = 0; j < 3; j++)
                    {
                        t.setVertex(j, new Vector4(mesh.Vertices[i + j].Position.X, mesh.Vertices[i + j].Position.Y, mesh.Vertices[i + j].Position.Z, 1.0f));
                        t.setNormal(j, new Models.Vector3(mesh.Vertices[i + j].Normal.X, mesh.Vertices[i + j].Normal.Y, mesh.Vertices[i + j].Normal.Z));
                        t.setTexCoord(j, new Models.Vector2(mesh.Vertices[i + j].TextureCoordinate.X, mesh.Vertices[i + j].TextureCoordinate.Y));
                    }
                    triangles.Add(t);
                }
            }
            
            Rasterizer r = new Rasterizer(700, 700);
            r.FragmentShader = Minor.texture_fragment_shader;

            r.texture = new Texture(obj_path);
            r.VertexShader = Minor.vertex_shader;
            
            r.clear(Buffers.Color | Buffers.Depth);
            r.set_model(Minor.get_model_matrix(angle));
            r.set_view(Minor.get_view_matrix(eye_pos));
            r.set_projection(Minor.get_projection_matrix(45.0f, 1f, 0.1f, 50.0f));

            r.draw(triangles);

            List<Models.Vector3> frameBuffer = r.frame_buf;

            // 1. 检查数据长度
            if (frameBuffer.Count != 700 * 700)
            {
                throw new ArgumentException($"帧缓冲大小 {frameBuffer.Count} 不匹配预期的 700x700 图像尺寸。");
            }

            SaveImage(filename, frameBuffer);

        }*/
    }
}
