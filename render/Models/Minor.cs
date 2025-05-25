using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;

namespace render.Models
{
    public static class Minor
    {

        public static Matrix4x4 get_view_matrix(Vector3 eye_pos)
        {
            Matrix4x4 view = Matrix4x4.Identity;
            Matrix4x4 translate = Matrix4x4.Identity;
            translate.M14 = -eye_pos.X;
            translate.M24 = -eye_pos.Y;
            translate.M34 = -eye_pos.Z;
            view = translate * view;
            return view;
        }

        public static Matrix4x4 get_model_matrix(float angle)
        {
            angle = angle * MathF.PI / 180;

            Matrix4x4 rotation = Matrix4x4.Identity;
            rotation.M11 = MathF.Cos(angle);
            rotation.M13 = MathF.Sin(angle);
            rotation.M31 = -MathF.Sin(angle);
            rotation.M33 = MathF.Cos(angle);

            Matrix4x4 scale = Matrix4x4.Identity;
            scale = Matrix4x4.Multiply(scale, 2.5f);
            scale.M44 = 1.0f;

            Matrix4x4 translation = Matrix4x4.Identity;

            return translation * rotation * scale;
        }

        public static string sdf(Matrix4x4 matrix)
        {
            return $"M11: {matrix.M11:F2}, M12: {matrix.M12:F2}, M13: {matrix.M13:F2}, M14: {matrix.M14:F2}\n" +
                   $"M21: {matrix.M21:F2}, M22: {matrix.M22:F2}, M23: {matrix.M23:F2}, M24: {matrix.M24:F2}\n" +
                   $"M31: {matrix.M31:F2}, M32: {matrix.M32:F2}, M33: {matrix.M33:F2}, M34: {matrix.M34:F2}\n" +
                   $"M41: {matrix.M41:F2}, M42: {matrix.M42:F2}, M43: {matrix.M43:F2}, M44: {matrix.M44:F2}";
        }

        public static Matrix4x4 get_projection_matrix(float eye_fov, float aspect_ratio, float zNear, float zFar)
        {
            float angle = eye_fov / 180.0f * MathF.PI;
            float t = zNear * MathF.Tan(angle / 2);
            float r = t * aspect_ratio;
            float l = -r;
            float b = -t;

            Matrix4x4 MorthoScale = Matrix4x4.Identity;
            MorthoScale.M11 = 2 / (r - l);
            MorthoScale.M22 = 2 / (t - b);
            MorthoScale.M33 = 2 / (zFar - zNear);

            Matrix4x4 MorthoPos = Matrix4x4.Identity;
            MorthoPos.M14 = -(r + l) / 2;
            MorthoPos.M24 = -(t + b) / 2;
            MorthoPos.M34 = -(zNear + zFar) / 2;

            Matrix4x4 Mpersp2ortho = new Matrix4x4();
            Mpersp2ortho.M11 = zNear;
            Mpersp2ortho.M22 = zNear;
            Mpersp2ortho.M33= zNear + zFar;
            Mpersp2ortho.M34 = -zNear * zFar;
            Mpersp2ortho.M43 = 1.0f;

            Matrix4x4 Mt = Matrix4x4.Identity;
            Mt.M33 = -1.0f;

            Mpersp2ortho = Mpersp2ortho * Mt;

            Matrix4x4 projection = MorthoScale * MorthoPos * Mpersp2ortho;


            return projection;
        }

        public static Vector3 vertex_shader(vertex_shader_payload payload)
        {
            return payload.position;
        }

        public static Vector3 normal_fragment_shader(fragment_shader_payload payload)
        {
            Vector3 head = new Vector3(payload.normal.X, payload.normal.Y, payload.normal.Z);
            Vector3 half1 = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 return_color = head.Normalize() + half1;
            Vector3 result = new Vector3(return_color.X * 255, return_color.Y * 255, return_color.Z * 255);
            return result;
        }

        public static Vector3 reflect(Vector3 vec, Vector3 axis)
        {
            var costheta = Vector3.Dot(vec, axis);
            return (axis * 2 * costheta - vec).Normalize();
        }

        public struct light
        {
            public Vector3 position;
            public Vector3 intensity;

            public light(Vector3 p, Vector3 i)
            {
                position = p;
                intensity = i;
            }
        }

        public static Vector3 texture_fragment_shader(fragment_shader_payload payload, Class1 lightOutside)
        {
            Vector3 return_color = new Vector3();
            
            if (payload.texture != null)
            {
                return_color = payload.texture.GetColor(payload.tex_coords.X, payload.tex_coords.Y);
            }
            Vector3 texture_color = new Vector3(return_color.X, return_color.Y, return_color.Z);
            

            Vector3 ka = new Vector3(0.005f, 0.005f, 0.005f);
            Vector3 kd = texture_color / 255.0f;
            Vector3 ks = new Vector3(0.7937f, 0.7937f, 0.7937f);

            light l1 = new light(new Vector3(lightOutside.x, lightOutside.y, lightOutside.z), new Vector3((float)lightOutside.intensity, (float)lightOutside.intensity, (float)lightOutside.intensity));
            light l2 = new light(new Vector3(-20f, 20f, 0f), new Vector3(500f, 500f, 500f));

            List<light> lights = new List<light>{ l1, l2 };
            Vector3 amb_light_intensity = new Vector3(10, 10, 10);
            Vector3 eye_pos = new Vector3(0, 0, 10);

            float p = 150;

            Vector3 color = texture_color;
            Vector3 point = payload.view_pos;
            Vector3 normal = payload.normal;

            Vector3 result_color = new Vector3();

            foreach(light light in lights)
            {
                Vector3 light_dir = (light.position - point).Normalize();
                Vector3 view_dir = (eye_pos - point).Normalize();
                Vector3 half_dir = (light_dir + view_dir).Normalize();

                Vector3 La = Vector3.cwiseProduct(ka, amb_light_intensity);

                float r2 = Vector3.Dot((light.position - point), (light.position - point));
                Vector3 I_r2 = light.intensity / r2;

                Vector3 Ld = Vector3.cwiseProduct(kd, I_r2);
                Ld *= MathF.Max(0.0f, Vector3.Dot(normal.Normalize(), (light_dir)));

                Vector3 Ls = Vector3.cwiseProduct(ks, I_r2);
                Ls *= (float)Math.Pow(Math.Max(0.0, Vector3.Dot(normal.Normalize(), half_dir)), p);

                result_color += (La + Ld + Ls);
            }

            return result_color * 255.0f;
        }
    }
}
