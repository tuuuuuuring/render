using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace render.Models
{
    internal class Triangle
    {
        public Vector4[] v = new Vector4[3];
        public Vector3[] color = new Vector3[3];
        public Vector2[] tex_coords = new Vector2[3];
        public Vector3[] normal = new Vector3[3];

        public Texture? texture;

        public Triangle()
        {
            v[0]=new Vector4(0,0,0,1);
            v[1]=new Vector4(0,0,0,1);
            v[2] = new Vector4(0, 0, 0, 1);

            color[0] = new Vector3();
            color[1] = new Vector3();
            color[2] = new Vector3();

            tex_coords[0] = new Vector2();
            tex_coords[1] = new Vector2();
            tex_coords[2] = new Vector2();
        }

        public Vector4 a() { return v[0]; }
        public Vector4 b() { return v[1]; }
        public Vector4 c() { return v[2]; }
        public void setVertex(int ind, Vector4 ver)
        {
            v[(int)ind] = ver;
        }
        public void setNormal(int ind, Vector3 n)
        {
            normal[(int)ind] = n;
        }
        public void setColor(int ind, float r, float g, float b)
        {
            if(r<0.0 || r>255.0 || g < 0.0 || g > 255.0 || b < 0.0 || b > 255.0)
            {
                throw new ArgumentOutOfRangeException(nameof(r), r, "Pixel value must be between 0 and 255.");
            }
            color[ind] =new Vector3((float)(r / 255.0), (float)(g / 255.0), (float)(b / 255.0));
            
        }

        public void setNormals(Vector3[] normals)
        {
            if (normals.Length != 3)
            {
                throw new ArgumentException("normals must be 3");
            }
            normal[0] = normals[0];
            normal[1] = normals[1];
            normal[2] = normals[2];
        }
        public void setColors(Vector3[] colors)
        {
            if (colors.Length != 3)
            {
                throw new ArgumentException("normals must be 3");
            }
            colors[0] = colors[0];
            colors[1] = colors[1];
            colors[2] = colors[2];
        }
        public void setTexCoord(int ind, Vector2 uv)
        {
            tex_coords[ind] = uv;
        }
        public Vector4[] toVector4()
        {
            Vector4[] result = new Vector4[3];
            for(int i = 0; i < v.Length; i++)
            {
                result[i] = new Vector4(v[i].X, v[i].Y, v[i].Z, 1);
            }
            return result;
        }
    }
}
