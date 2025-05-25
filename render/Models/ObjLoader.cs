using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace render.Models
{
    public struct Vertex
    {
        public Vector3 Position;

        // Normal Vector
        public Vector3 Normal;

        // Texture Coordinate Vector
        public Vector2 TextureCoordinate;
    }

    public struct Material
    {
        // Material Name
        public string name;
        // Ambient Color
        public Vector3 Ka;
        // Diffuse Color
        public Vector3 Kd;
        // Specular Color
        public Vector3 Ks;
        // Specular Exponent
        public float Ns;
        // Optical Density
        public float Ni;
        // Dissolve
        public float d;
        // Illumination
        public int illum;
        // Ambient Texture Map
        public string map_Ka;
        // Diffuse Texture Map
        public string map_Kd;
        // Specular Texture Map
        public string map_Ks;
        // Specular Highlight Map
        public string map_Ns;
        // Alpha Texture Map
        public string map_d;
        // Bump Map
        public string map_bump;

        // 默认构造函数
        public Material()
        {
            name = string.Empty;
            Ka = new Vector3();  // 假设 Vector3 有默认构造函数
            Kd = new Vector3();
            Ks = new Vector3();
            Ns = 0.0f;
            Ni = 0.0f;
            d = 0.0f;
            illum = 0;
            map_Ka = string.Empty;
            map_Kd = string.Empty;
            map_Ks = string.Empty;
            map_Ns = string.Empty;
            map_d = string.Empty;
            map_bump = string.Empty;
        }
    }
    public struct Mesh
    {
        public string MeshName;
        public List<Vertex> Vertices;
        public List<uint> Indices;
        public Material MeshMaterial;
        public Mesh()
        {
            MeshName = string.Empty;
            Vertices = new List<Vertex>();
            Indices = new List<uint>();
            MeshMaterial = new Material();
        }
        public Mesh(string name, List<Vertex> vertices, List<uint> indices)
        {
            MeshName = name;
            Vertices = vertices;
            Indices = indices;
            MeshMaterial = new Material(); 
        }
    }
    public static class StringExtentions
    {
        public static List<string> spliter(string longstr, string token)
        {
            List<string> shortstrs = new List<string>();
            string temp = string.Empty;
            for (int i = 0; i < longstr.Length; i++)
            {
                string test = longstr.Substring(i, token.Length);
                if (test == token)
                {
                    if (temp != string.Empty)
                    {
                        shortstrs.Add(temp);
                        temp = string.Empty;
                        i += token.Length - 1;
                    }
                    else
                        shortstrs.Add(string.Empty);
                }
                else if (i + token.Length >= longstr.Length)
                {
                    temp += longstr.Substring(i, token.Length);
                    shortstrs.Add(temp);
                    break;
                }
                else
                    temp += longstr[i];
            }
            return shortstrs;
        }
        public static string Tail(string input)
        {
            int tokenStart = -1;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != ' ' && input[i] != '\t')
                {
                    tokenStart = i;
                    break;
                }
            }

            int spaceStart = -1;
            for(int i=tokenStart; i < input.Length; i++)
            {
                if (input[i] == ' ' || input[i] == '\t')
                {
                    spaceStart = i;
                    break;
                }
            }

            int tailStart = -1;
            for(int j=spaceStart; j < input.Length; j++)
            {
                if(input[j] != ' ' && input[j] != '\t')
                {
                    tailStart = j;
                    break;
                }
            }

            int tailEnd = -1;
            for(int i = input.Length - 1; i >= 0; i--)
            {
                if (input[i]!=' ' && input[i] != '\t')
                {
                    tailEnd = i;
                    break;
                }
            }

            if(tailEnd != -1 && tailStart != -1)
            {
                return input.Substring(tailStart, tailEnd - tailStart+1);
            }
            else if (tailStart != -1)
            {
                return input.Substring(tailStart);
            }
            return "";
        }
        public static string FirstToken(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                // 找到第一个不是空格或制表符的字符位置
                int tokenStart = -1;
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] != ' ' && input[i] != '\t')
                    {
                        tokenStart = i;
                        break;
                    }
                }

                // 如果没有找到非空白字符，则返回空字符串
                if (tokenStart == -1)
                {
                    return "";
                }

                // 从 tokenStart 开始，查找第一个空格或制表符
                int tokenEnd = -1;
                for (int i = tokenStart; i < input.Length; i++)
                {
                    if (input[i] == ' ' || input[i] == '\t')
                    {
                        tokenEnd = i;
                        break;
                    }
                }

                // 如果找到 tokenEnd，则返回 tokenStart 到 tokenEnd 之间的子串，否则返回 tokenStart 到末尾的子串
                if (tokenEnd != -1)
                {
                    return input.Substring(tokenStart, tokenEnd - tokenStart);
                }
                else
                {
                    return input.Substring(tokenStart);
                }
            }

            return "";
        }

        public static T GetElement<T>(List<T> elements, string indexStr)
        {
            // 将字符串索引转换为整数
            if (!int.TryParse(indexStr, out int idx))
            {
                throw new ArgumentException("Invalid index format.");
            }

            // 处理负数索引
            if (idx < 0)
            {
                idx = elements.Count + idx;
            }
            else
            {
                idx--; // 将 1-based 索引转换为 0-based 索引
            }

            // 检查索引是否有效
            if (idx < 0 || idx >= elements.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStr), "Index is out of range.");
            }

            // 返回对应元素
            return elements[idx];
        }
    }
    internal class ObjLoader
    {
        public ObjLoader()
        {
            LoadedMeshes = new List<Mesh>();
            LoadedVertices = new List<Vertex>();
            LoadedIndices = new List<uint> { 0 };
            LoadedMaterials = new List<Material>();
        }

        public List<Mesh> LoadedMeshes;
        public List<Vertex> LoadedVertices;
        public List<uint> LoadedIndices;
        public List<Material> LoadedMaterials; 

        public bool LoadFile(string path)
        {
            if (path.Substring(path.Length - 4, 4) != ".obj")
            {
                Debug.WriteLine("not obj");
                return false;
            }

            LoadedMeshes.Clear();
            LoadedVertices.Clear();
            LoadedIndices.Clear();

            List<Vector3> Positions = new List<Vector3>();
            List<Vector2> TCoords = new List<Vector2>();
            List<Vector3> Normals = new List<Vector3>();

            List<Vertex> Vertices = new List<Vertex>();
            List<uint> Indices = new List<uint>();

            List<string> MeshMatNames = new List<string>();

            bool listening = false;
            string meshname = string.Empty;

            Mesh tempMesh;

            using (StreamReader file=new StreamReader(path))
            {
                string curline;

                while ((curline = file.ReadLine()) != null)
                {
                    
                    if(curline.Length == 0)
                        continue;
                    if (StringExtentions.FirstToken(curline) == "o" || StringExtentions.FirstToken(curline) == "g" || curline[0] == 'g')
                    {
                        if (!listening)
                        {
                            
                            listening = true;
                            if (StringExtentions.FirstToken(curline) == "o" || StringExtentions.FirstToken(curline) == "o")
                                meshname = StringExtentions.Tail(curline);
                            else
                                meshname = "unnamed";
                        }
                        else
                        {
                            if (Vertices.Any() && Indices.Any())
                            {
                                tempMesh = new Mesh(meshname, Vertices, Indices);
                                LoadedMeshes.Add(tempMesh);
                                Vertices.Clear();
                                Indices.Clear();
                                meshname=string.Empty;
                                meshname=StringExtentions.Tail(curline);
                            }
                            else
                            {
                                if (StringExtentions.FirstToken(curline) == "o" || StringExtentions.FirstToken(curline) == "o")
                                    meshname = StringExtentions.Tail(curline);
                                else
                                    meshname = "unnamed";
                            }
                        }
                    }

                    if (StringExtentions.FirstToken(curline) == "v")
                    {
                        List<string> spos = StringExtentions.spliter(StringExtentions.Tail(curline), " ");
                        Vector3 vpos;
                        vpos.X=float.Parse(spos[0]);
                        vpos.Y=float.Parse(spos[1]);
                        vpos.Z=float.Parse(spos[2]);

                        Positions.Add(vpos);
                    }

                    if(StringExtentions.FirstToken(curline) == "vt")
                    {
                        List<string> stex = StringExtentions.spliter(StringExtentions.Tail(curline), " ");
                        Vector2 vtex;
                        vtex.X=float.Parse(stex[0]);
                        vtex.Y=float.Parse(stex[1]);

                        TCoords.Add(vtex);
                    }

                    if (StringExtentions.FirstToken(curline) == "vn")
                    {
                        List<string> snor = StringExtentions.spliter(StringExtentions.Tail(curline), " ");
                        Vector3 vnor;
                        vnor.X = float.Parse(snor[0]);
                        vnor.Y = float.Parse(snor[1]);
                        vnor.Z = float.Parse(snor[2]);

                        Normals.Add(vnor);
                    }

                    if (StringExtentions.FirstToken(curline) == "f")
                    {
                        // Generate the vertices
                        List<Vertex> vVerts = new List<Vertex>();
                        GenVerticesFromRawOBJ(ref vVerts, Positions, TCoords, Normals, curline);

                        // Add Vertices
                        for (int i = 0; i < vVerts.Count; i++)
                        {
                            Vertices.Add(vVerts[i]);

                            LoadedVertices.Add(vVerts[i]);
                        }

                        List<uint> iIndices = new List<uint>();
                        VertexTriangluation(ref iIndices, vVerts);

                        for(int i = 0; i < iIndices.Count; i++)
                        {
                            uint indnum = (uint)(Vertices.Count - vVerts.Count + iIndices[i]);
                            Indices.Add(indnum);

                            indnum = (uint)(LoadedVertices.Count - vVerts.Count + iIndices[i]);
                            LoadedIndices.Add(indnum);
                        }
                    }

                    if (StringExtentions.FirstToken(curline) == "usemtl")
                    {
                        MeshMatNames.Add(StringExtentions.Tail(curline));

                        // Create new Mesh, if Material changes within a group
                        if (Indices.Any() && Vertices.Any())
                        {
                            // Create Mesh
                            tempMesh = new Mesh(string.Empty, Vertices, Indices);
                            tempMesh.MeshName = meshname;
                            int i = 2;
                            while (true)
                            {
                                tempMesh.MeshName = meshname + "_" + i.ToString();

                                foreach (Mesh m in LoadedMeshes)
                                    if (m.MeshName == tempMesh.MeshName)
                                        continue;
                                break;
                            }

                            // Insert Mesh
                            LoadedMeshes.Add(tempMesh);

                            // Cleanup
                            Vertices.Clear();
                            Indices.Clear();
                        }
                    }

                    if (StringExtentions.FirstToken(curline) == "mtllib")
                    {
                        // Generate a path to the material file
                        List<string> temp=StringExtentions.spliter(path, "/");

                        string pathtomat = "";

                        if (temp.Count != 1)
                        {
                            for (int i = 0; i < temp.Count - 1; i++)
                            {
                                pathtomat += temp[i] + "/";
                            }
                        }

                        pathtomat += StringExtentions.Tail(curline);

                        // Load Materials
                        LoadMaterials(pathtomat);
                    }

                }

                if (Indices.Any() && Vertices.Any())
                {
                    // Create Mesh
                    tempMesh = new Mesh(meshname, Vertices, Indices);

                    // Insert Mesh
                    LoadedMeshes.Add(tempMesh);
                }

            }

            // Set Materials for each Mesh
            for (int i = 0; i < MeshMatNames.Count; i++)
            {
                string matname = MeshMatNames[i];

                // Find corresponding material name in loaded materials
                // when found copy material variables into mesh material
                for (int j = 0; j < LoadedMaterials.Count; j++)
                {
                    if (LoadedMaterials[j].name == matname)
                    {
                        Mesh tempmesh = LoadedMeshes[i];
                        tempmesh.MeshMaterial = LoadedMaterials[j];
                        LoadedMeshes[i] = tempmesh;
                        break;
                    }
                }
            }

            if (!LoadedMeshes.Any() && !LoadedVertices.Any() && !LoadedIndices.Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void GenVerticesFromRawOBJ(ref List<Vertex> oVerts,
            List<Vector3> iPositions,
            List<Vector2> iTCoords,
            List<Vector3> iNormals,
            string icurline)
        {
            Vertex vVert = new Vertex();
            List<string> sface = StringExtentions.spliter(StringExtentions.Tail(icurline), " ");
            
            bool noNormal = false;

            for(int i = 0; i < sface.Count; i++)
            {
                int vtype = 0;
                List<string> svert = StringExtentions.spliter(sface[i], "/");

                if (svert.Count == 1)
                    vtype = 1;
                if(svert.Count == 2)
                    vtype = 2;
                if (svert.Count == 3)
                {
                    if (svert[1] != "")
                        vtype = 4;
                    else
                        vtype = 3;
                }

                switch (vtype)
                {
                    case 1:
                        vVert.Position = StringExtentions.GetElement(iPositions, svert[0]);
                        vVert.TextureCoordinate = new Vector2(0, 0);
                        noNormal = true;
                        oVerts.Add(vVert);
                        break;
                    case 2:
                        vVert.Position = StringExtentions.GetElement(iPositions, svert[0]);
                        vVert.TextureCoordinate = StringExtentions.GetElement(iTCoords, svert[1]);
                        noNormal = true;
                        oVerts.Add(vVert);
                        break;
                    case 3:
                        vVert.Position = StringExtentions.GetElement(iPositions, svert[0]);
                        vVert.TextureCoordinate = new Vector2(0, 0);
                        vVert.Normal = StringExtentions.GetElement(iNormals, svert[2]);
                        oVerts.Add(vVert);
                        break;
                    case 4:
                        vVert.Position = StringExtentions.GetElement(iPositions, svert[0]);
                        vVert.TextureCoordinate = StringExtentions.GetElement(iTCoords, svert[1]);
                        vVert.Normal = StringExtentions.GetElement(iNormals, svert[2]);
                        oVerts.Add(vVert);
                        break;
                    default:
                        break;
                }
            }

            if (noNormal)
            {
                Vector3 A = oVerts[0].Position - oVerts[1].Position;
                Vector3 B = oVerts[2].Position - oVerts[1].Position;

                Vector3 normal = Vector3.Cross(A, B);

                for (int i = 0; i < oVerts.Count; i++)
                {
                    Vertex vertex = oVerts[i];  // 获取当前顶点的副本
                    vertex.Normal = normal;     // 修改副本
                    oVerts[i] = vertex;
                }
            }
        }
        private void VertexTriangluation(ref List<uint> oIndices, List<Vertex> iVerts)
        {
            if (iVerts.Count < 3)
                return;

            if(iVerts.Count == 3)
            {
                oIndices.Add(0);
                oIndices.Add(1);
                oIndices.Add(2);
                return;
            }

            List<Vertex> tVerts = iVerts;

            while (true)
            {
                for(int i = 0; i < tVerts.Count; i++)
                {
                    Vertex pPre;
                    if (i == 0)
                        pPre = tVerts[tVerts.Count - 1];
                    else
                        pPre = tVerts[i-1];

                    Vertex pCur = tVerts[i];

                    Vertex pNext;
                    if (i == tVerts.Count - 1)
                        pNext = tVerts[tVerts.Count - 1];
                    else
                        pNext = tVerts[i - 1];

                    if (tVerts.Count == 3)
                    {
                        // Create a triangle from pCur, pPrev, pNext
                        for (int j = 0; j < tVerts.Count; j++)
                        {
                            if (iVerts[j].Position == pCur.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == pPre.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == pNext.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                        }

                        tVerts.Clear();
                        break;
                    }

                    if(tVerts.Count == 4)
                    {
                        for (int j = 0; j < iVerts.Count; j++)
                        {
                            if (iVerts[j].Position == pCur.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == pPre.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == pNext.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                        }

                        Vector3 tempVec = new Vector3();
                        for (int j = 0; j < tVerts.Count; j++)
                        {
                            if (tVerts[j].Position != pCur.Position
                                && tVerts[j].Position != pPre.Position
                                && tVerts[j].Position != pNext.Position)
                            {
                                tempVec = tVerts[j].Position;
                                break;
                            }
                        }

                        // Create a triangle from pCur, pPrev, pNext
                        for (int j = 0; j < iVerts.Count; j++)
                        {
                            if (iVerts[j].Position == pPre.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == pNext.Position)
                                oIndices.Add(Convert.ToUInt32(j));
                            if (iVerts[j].Position == tempVec)
                                oIndices.Add(Convert.ToUInt32(j));
                        }

                        tVerts.Clear();
                        break;
                    }

                    float angle = Vector3.AngleBetweenV3(pPre.Position - pCur.Position, pNext.Position - pCur.Position) * (180 / 3.14159265359f);
                    if (angle <= 0 && angle >= 180)
                        continue;

                    bool inTri = false;
                    for (int j = 0; j < iVerts.Count; j++)
                    {
                        if (Vector3.inTriangle(iVerts[j].Position, pPre.Position, pCur.Position, pNext.Position)
                            && iVerts[j].Position != pPre.Position
                            && iVerts[j].Position != pCur.Position
                            && iVerts[j].Position != pNext.Position)
                        {
                            inTri = true;
                            break;
                        }
                    }
                    if (inTri)
                        continue;

                    for (int j = 0; j < iVerts.Count; j++)
                    {
                        if (iVerts[j].Position == pCur.Position)
                            oIndices.Add(Convert.ToUInt32(j));
                        if (iVerts[j].Position == pPre.Position)
                            oIndices.Add(Convert.ToUInt32(j));
                        if (iVerts[j].Position == pNext.Position)
                            oIndices.Add(Convert.ToUInt32(j));
                    }

                    // Delete pCur from the list
                    for (int j = 0; j < tVerts.Count; j++)
                    {
                        if (tVerts[j].Position == pCur.Position)
                        {
                            tVerts.RemoveAt(j);
                            break;
                        }
                    }

                    // reset i to the start
                    // -1 since loop will add 1 to it
                    i = -1;
                }

                if (oIndices.Count == 0)
                    break;

                // if no more vertices
                if (tVerts.Count == 0)
                    break;
            }
        }
        private bool LoadMaterials(string path)
        {
            if (path.Substring(path.Length - 4, 4) != ".mtl") return false;

            Material tempMaterial = new Material();
            bool listening = false;
            
            using(StreamReader file = new StreamReader(path))
            {
                string curline;
                while ((curline = file.ReadLine()) != null)
                {
                    if (StringExtentions.FirstToken(curline) == "newmtl")
                    {
                        if (!listening)
                        {
                            listening = true;

                            if (curline.Count() > 7)
                            {
                                tempMaterial.name = StringExtentions.Tail(curline);
                            }
                            else
                            {
                                tempMaterial.name = "none";
                            }
                        }
                        else
                        {
                            // Generate the material

                            // Push Back loaded Material
                            LoadedMaterials.Add(tempMaterial);

                            // Clear Loaded Material
                            tempMaterial =new Material();

                            if (curline.Count() > 7)
                            {
                                tempMaterial.name = StringExtentions.Tail(curline);
                            }
                            else
                            {
                                tempMaterial.name = "none";
                            }
                        }
                    }

                    if (StringExtentions.FirstToken(curline) == "Ka")
                    {
                        List<string> temp = StringExtentions.spliter(StringExtentions.Tail(curline), " ");

                        if (temp.Count != 3)
                            continue;

                        tempMaterial.Ka.X = float.Parse(temp[0]);
                        tempMaterial.Ka.Y = float.Parse(temp[1]);
                        tempMaterial.Ka.Z = float.Parse(temp[2]);
                    }

                    if (StringExtentions.FirstToken(curline) == "Kd")
                    {
                        List<string> temp = StringExtentions.spliter(StringExtentions.Tail(curline), " ");

                        if (temp.Count != 3)
                            continue;

                        tempMaterial.Kd.X = float.Parse(temp[0]);
                        tempMaterial.Kd.Y = float.Parse(temp[1]);
                        tempMaterial.Kd.Z = float.Parse(temp[2]);
                    }

                    if (StringExtentions.FirstToken(curline) == "Ks")
                    {
                        List<string> temp = StringExtentions.spliter(StringExtentions.Tail(curline), " ");

                        if (temp.Count != 3)
                            continue;

                        tempMaterial.Ks.X = float.Parse(temp[0]);
                        tempMaterial.Ks.Y = float.Parse(temp[1]);
                        tempMaterial.Ks.Z = float.Parse(temp[2]);
                    }

                    if (StringExtentions.FirstToken(curline) == "Ns")
                    {
                        tempMaterial.Ns = float.Parse(StringExtentions.Tail(curline));
                    }

                    // Optical Density
                    if (StringExtentions.FirstToken(curline) == "Ni")
                    {
                        tempMaterial.Ni = float.Parse(StringExtentions.Tail(curline));
                    }
                    // Dissolve
                    if (StringExtentions.FirstToken(curline) == "d")
                    {
                        tempMaterial.d = float.Parse(StringExtentions.Tail(curline));
                    }
                    // Illumination
                    if (StringExtentions.FirstToken(curline) == "illum")
                    {
                        tempMaterial.illum = int.Parse(StringExtentions.Tail(curline));
                    }
                    // Ambient Texture Map
                    if (StringExtentions.FirstToken(curline) == "map_Ka")
                    {
                        tempMaterial.map_Ka = StringExtentions.Tail(curline);
                    }
                    // Diffuse Texture Map
                    if (StringExtentions.FirstToken(curline) == "map_Kd")
                    {
                        tempMaterial.map_Kd = StringExtentions.Tail(curline);
                    }
                    // Specular Texture Map
                    if (StringExtentions.FirstToken(curline) == "map_Ks")
                    {
                        tempMaterial.map_Ks = StringExtentions.Tail(curline);
                    }
                    // Specular Hightlight Map
                    if (StringExtentions.FirstToken(curline) == "map_Ns")
                    {
                        tempMaterial.map_Ns = StringExtentions.Tail(curline);
                    }
                    // Alpha Texture Map
                    if (StringExtentions.FirstToken(curline) == "map_d")
                    {
                        tempMaterial.map_d = StringExtentions.Tail(curline);
                    }
                    // Bump Map
                    if (StringExtentions.FirstToken(curline) == "map_Bump" || StringExtentions.FirstToken(curline) == "map_bump" || StringExtentions.FirstToken(curline) == "bump")
                    {
                        tempMaterial.map_bump = StringExtentions.Tail(curline);
                    }
                }

            }

            LoadedMaterials.Add(tempMaterial);

            if (LoadedMaterials.Count > 0)
                return true;
            else
                return false;
        }
    }
}
