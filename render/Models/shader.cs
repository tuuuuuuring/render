using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace render.Models
{
    internal class shader
    {
    }
    public struct fragment_shader_payload
    {
        public Vector3 view_pos;
        public Vector3 color;
        public Vector3 normal;
        public Vector2 tex_coords;
        public Texture? texture { get; set; }

        public fragment_shader_payload(Vector3 col, Vector3 nor, Vector2 tc, Texture tex)
        {
            texture = tex;
            color = col;
            normal = nor;
            tex_coords = tc;
            view_pos = new Vector3();
        }

    };

    public struct vertex_shader_payload
    {
        public Vector3 position;
    };
}
