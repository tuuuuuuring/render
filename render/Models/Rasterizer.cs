using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Numerics;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace render.Models
{
    public enum Buffers
    {
        Color = 1,
        Depth = 2
    };

    public enum Primitive
    {
        Line,
        Triangle
    };

    struct pos_buf_id
    {
        int pos_id;

        public pos_buf_id()
        {
            pos_id = 0;
        }
        public pos_buf_id(int id)
        {
            pos_id = id;
        }
    }
    struct ind_buf_id
    {
        int ind_id;

        public ind_buf_id()
        {
            ind_id = 0;
        }
        public ind_buf_id(int id)
        {
            ind_id = id;
        }
    }
    struct col_buf_id
    {
        int col_id;

        public col_buf_id()
        {
            col_id = 0;
        }

        public col_buf_id(int id)
        {
            col_id = id;
        }
    }
    internal class Rasterizer
    {
        public static int triangle_count = 0;

        private Matrix4x4 model;
        private Matrix4x4 view;
        private Matrix4x4 projection;

        int normal_id = -1;

        SortedDictionary<int, List<Vector3>> pos_buf;
        SortedDictionary<int, List<Vector3>> ind_buf;
        SortedDictionary<int, List<Vector3>> col_buf;
        SortedDictionary<int, List<Vector3>> nor_buf;

        public Texture? texture { get; set; }

        public Func<fragment_shader_payload, Class1, Vector3>? FragmentShader { get; set; }
        public Func<vertex_shader_payload, Vector3>? VertexShader { get; set; }

        public List<Vector3> frame_buf { get; set; }
        List<float> depth_buf;

        int width, height;

        int next_id = 0;

        public Rasterizer(int w, int h)
        {
            // 初始化 width 和 height
            this.width = w;
            this.height = h;

            // 初始化矩阵
            model = Matrix4x4.Identity;
            view = Matrix4x4.Identity;
            projection = Matrix4x4.Identity;

            // 初始化 SortedDictionary
            pos_buf = new SortedDictionary<int, List<Vector3>>();
            ind_buf = new SortedDictionary<int, List<Vector3>>();
            col_buf = new SortedDictionary<int, List<Vector3>>();
            nor_buf = new SortedDictionary<int, List<Vector3>>();

            // 初始化 frame_buf 和 depth_buf
            frame_buf = new List<Vector3>(w * h);
            depth_buf = new List<float>(w * h);

            // 初始化 frame_buf 和 depth_buf 的默认值
            for (int i = 0; i < width * height; i++)
            {
                frame_buf.Add(new Vector3(0, 0, 0)); // 默认颜色为黑色
                depth_buf.Add(float.MaxValue);       // 默认深度为最大值
            }

            // 其他成员变量使用默认值
            texture = null;
            FragmentShader = null;
            VertexShader = null;
        }

        public pos_buf_id load_positions(List<Vector3> positions)
        {
            var id = get_next_id();
            pos_buf.Add(id, positions);
            return new pos_buf_id(id);
        }

        public ind_buf_id load_indices(List<Vector3> indices)
        {
            var id = get_next_id();
            ind_buf.Add(id, indices);
            return new ind_buf_id(id);
        }

        public col_buf_id load_colors(List<Vector3> cols)
        {
            var id = get_next_id();
            col_buf.Add(id, cols);
            return new col_buf_id(id);
        }

        public col_buf_id load_normals(List<Vector3> normals)
        {
            var id = get_next_id();
            nor_buf.Add(id, normals);
            normal_id = id;
            return new col_buf_id(id);
        }

        public void set_model(Matrix4x4 m)
        {
            model = m;
        }

        public void set_projection(Matrix4x4 p)
        {
            projection = p;
        }

        public void set_view(Matrix4x4 v)
        {
            view = v;
        }

        public void clear(Buffers buff)
        {
            if((buff & Buffers.Color) == Buffers.Color)
            {
                frame_buf.ForEach(item => item = new Vector3());
            }
            if((buff & Buffers.Depth) == Buffers.Depth)
            {
                depth_buf.ForEach(item => item = float.MaxValue);
            }
        }

        int get_next_id() { return next_id++; }
        private int get_index(int x, int y)
        {
            return (height - y) * width + x;
        }

        public static string sdf(Matrix4x4 matrix)
        {
            return $"M11: {matrix.M11:F2}, M12: {matrix.M12:F2}, M13: {matrix.M13:F2}, M14: {matrix.M14:F2}\n" +
                   $"M21: {matrix.M21:F2}, M22: {matrix.M22:F2}, M23: {matrix.M23:F2}, M24: {matrix.M24:F2}\n" +
                   $"M31: {matrix.M31:F2}, M32: {matrix.M32:F2}, M33: {matrix.M33:F2}, M34: {matrix.M34:F2}\n" +
                   $"M41: {matrix.M41:F2}, M42: {matrix.M42:F2}, M43: {matrix.M43:F2}, M44: {matrix.M44:F2}";
        }

        public void draw(List<Triangle> TriangleList, Class1 class1, IProgress<double> progress)
        {
            float f1 = (float)((50 - 0.1) / 2.0);
            float f2 = (float)((50 + 0.1) / 2.0);

            Matrix4x4 mvp = projection * view * model;

            fragment_shader_payload payload = new fragment_shader_payload(new Vector3(), new Vector3(), new Vector2(), texture);

            int howMany = TriangleList.Count;
            int tens = (howMany - howMany % 100) / 100;
            int j = 0;
            
            foreach (var t in TriangleList)
            {
                Triangle new_triangle = t;

                Vector4[] mm = new Vector4[] 
                { 
                    TransposeAndMultiply(view * model, t.v[0]), 
                    TransposeAndMultiply(view * model, t.v[1]), 
                    TransposeAndMultiply(view * model, t.v[2])
                };
                Vector3[] viewspace_pos = new Vector3[]
                {
                    new Vector3(mm[0].X, mm[0].Y, mm[0].Z),
                    new Vector3(mm[1].X, mm[1].Y, mm[1].Z),
                    new Vector3(mm[2].X, mm[2].Y, mm[2].Z)
                };

                Vector4[] v = new Vector4[]
                {
                    TransposeAndMultiply(mvp, t.v[0]),
                    TransposeAndMultiply(mvp, t.v[1]),
                    TransposeAndMultiply(mvp, t.v[2])
                };
                for(int i=0;i<3;i++)
                {
                    v[i].X /= v[i].W;
                    v[i].Y /= v[i].W;
                    v[i].Z /= v[i].W;
                }

                Matrix4x4 inv;
                bool invtf = Matrix4x4.Invert(view * model,out inv);
                Matrix4x4 inv_trans = Matrix4x4.Transpose(inv);

                Vector4[] n = new Vector4[]
                {
                    TransposeAndMultiply(inv_trans, to_vec4(t.normal[0], 0.0f)),
                    TransposeAndMultiply(inv_trans, to_vec4(t.normal[1], 0.0f)),
                    TransposeAndMultiply(inv_trans, to_vec4(t.normal[2], 0.0f))
                };

                for (int i = 0; i < 3; i++)
                {
                    v[i].X = 0.5f * width * (v[i].X + 1.0f);
                    v[i].Y = 0.5f * height * (v[i].Y + 1.0f);
                    v[i].Z = v[i].Z * f1 + f2;
                }

                for (int i = 0; i < 3; ++i)
                {
                    //screen space coordinates
                    new_triangle.setVertex(i, v[i]);
                }

                for (int i = 0; i < 3; ++i)
                {
                    //view space normal
                    new_triangle.setNormal(i, new Vector3(n[i].X, n[i].Y, n[i].Z));
                }

                new_triangle.setColor(0, 148f, 121.0f, 92.0f);
                new_triangle.setColor(1, 148f, 121.0f, 92.0f);
                new_triangle.setColor(2, 148f, 121.0f, 92.0f);

                rasterize_triangle(new_triangle, viewspace_pos, class1, payload);
                j++;
                if (j % tens == 0)
                {
                    progress.Report(j/ tens);
                }
            }
        }


        public static Vector4 TransposeAndMultiply(Matrix4x4 matrix, Vector4 vector)
        {
            return new Vector4(
                Vector4.Dot(new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14), vector),
                Vector4.Dot(new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24), vector),
                Vector4.Dot(new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34), vector),
                Vector4.Dot(new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44), vector)
                );
        }

        private void rasterize_triangle(Triangle t, Vector3[] view_pos, Class1 class1, fragment_shader_payload payloada)
        {
            Vector4[] v = t.toVector4();

            float min_x = Math.Min(v[0].X, Math.Min(v[1].X, v[2].X));
            float max_x = Math.Max(v[0].X, Math.Max(v[1].X, v[2].X));
            float min_y = Math.Min(v[0].Y, Math.Min(v[1].Y, v[2].Y));
            float max_y = Math.Max(v[0].Y, Math.Max(v[1].Y, v[2].Y));


            min_x = MathF.Floor(min_x);
            max_x = MathF.Ceiling(max_x);
            min_y = MathF.Floor(min_y);
            max_y = MathF.Ceiling(max_y);

            for (int x = (int)min_x; x < max_x; x++)
            {
                for (int y = (int)min_y; y < max_y; y++)
                {
                    if (insideTriangle(x, y, t.v))
                    {
                        float min_depth = float.MaxValue;
                        var aby = computeBarycentric2D(x, y, t.v);
                        float Z = (float)1.0 / (aby.Item1 / v[0].W + aby.Item2 / v[1].W + aby.Item3 / v[2].W);
                        float zp = aby.Item1 * v[0].Z / v[0].W + aby.Item2 * v[1].Z / v[1].W + aby.Item3 * v[2].Z / v[2].W;
                        zp *= Z;
                        min_depth = MathF.Min(min_depth, zp);
                        if (min_depth < depth_buf[get_index(x, y)])
                        {
                            
                            var interpolated_color = t.color[0] * aby.Item1 + t.color[1] * aby.Item2 + t.color[2] * aby.Item3;
                            var interpolated_normal = t.normal[0] * aby.Item1 + t.normal[1] * aby.Item2 + t.normal[2] * aby.Item3 ;
                            var interpolated_texcoords = t.tex_coords[0] * aby.Item1 + t.tex_coords[1] * aby.Item2 + t.tex_coords[2] * aby.Item3;
                            var interpolated_shadingcoords = view_pos[0] * aby.Item1 + view_pos[1] * aby.Item2 + view_pos[2] * aby.Item3;

                            //fragment_shader_payload payload = new fragment_shader_payload(interpolated_color, interpolated_normal.Normalize(), interpolated_texcoords, texture);
                            payloada.color = interpolated_color;
                            payloada.normal = interpolated_normal.Normalize();
                            payloada.tex_coords = interpolated_texcoords;
                            payloada.view_pos = interpolated_shadingcoords;
                            Vector3? pixel_color = FragmentShader?.Invoke(payloada, class1);
                            depth_buf[get_index(x, y)] = min_depth;
                            Vector2 point = new Vector2(x, y);
                            if (pixel_color.HasValue)
                            {
                                set_pixel(point, pixel_color.Value);
                                triangle_count++;
                            }
                            else
                                Debug.WriteLine("no value");
                        }
                    }
                }
            }

        }

        Vector4 to_vec4(Vector3 v3, float w = 1.0f)
        {
            return new Vector4(v3.X, v3.Y, v3.Z, w);
        }

        private void set_pixel(Vector2 point, Vector3 color)
        {
            int ind = (int)((height - point.Y) * width + point.X);
            frame_buf[ind] = color;
        }

        public static Tuple<float, float, float> computeBarycentric2D(int x, int y, Vector4[] v)
        {
            float c1 = (x * (v[1].Y - v[2].Y) + (v[2].X - v[1].X) * y + v[1].X * v[2].Y - v[2].X * v[1].Y) / (v[0].X * (v[1].Y - v[2].Y) + (v[2].X - v[1].X) * v[0].Y + v[1].X * v[2].Y - v[2].X * v[1].Y);
            float c2 = (x * (v[2].Y - v[0].Y) + (v[0].X - v[2].X) * y + v[2].X * v[0].Y - v[0].X * v[2].Y) / (v[1].X * (v[2].Y - v[0].Y) + (v[0].X - v[2].X) * v[1].Y + v[2].X * v[0].Y - v[0].X * v[2].Y);
            float c3 = (x * (v[0].Y - v[1].Y) + (v[1].X - v[0].X) * y + v[0].X * v[1].Y - v[1].X * v[0].Y) / (v[2].X * (v[0].Y - v[1].Y) + (v[1].X - v[0].X) * v[2].Y + v[0].X * v[1].Y - v[1].X * v[0].Y);
            return new Tuple<float, float, float>(c1, c2, c3);
        }

        bool insideTriangle(int x, int y, Vector4[] _v)
        {
            Vector3[] v = new Vector3[3];
            for(int i = 0; i < 3; i++)
            {
                v[i] = new Vector3(_v[i].X, _v[i].Y, 1.0f);
            }

            Vector3 f0, f1, f2;
            f0 = Vector3.Cross(v[1], v[0]);
            f1 = Vector3.Cross(v[2], v[1]);
            f2 = Vector3.Cross(v[0], v[2]);
            Vector3 p = new Vector3(x, y, 1.0f);

            if (Vector3.Dot(p, f0) * Vector3.Dot(f0, v[2]) > 0 &&
                Vector3.Dot(p, f1) * Vector3.Dot(f1, v[0]) > 0 &&
                Vector3.Dot(p, f2) * Vector3.Dot(f2, v[1]) > 0)
                return true;
            return false;
        }
    }
}
