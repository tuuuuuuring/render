using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace render.Models
{
    internal class Eigen
    {
    }
    public struct Vector2 : IEquatable<Vector2>
    {
        // 字段
        public float X;
        public float Y;

        // 默认构造函数（C# 10+）
        public Vector2()
        {
            X = 0.0f;
            Y = 0.0f;
        }

        // 带参构造函数
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // 加法运算符重载
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        // 减法运算符重载
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        // 标量乘法
        public static Vector2 operator *(Vector2 v, float scalar)
        {
            return new Vector2(v.X * scalar, v.Y * scalar);
        }

        // 标量除法
        public static Vector2 operator /(Vector2 v, float scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Scalar cannot be zero.");
            return new Vector2(v.X / scalar, v.Y / scalar);
        }

        // 点积
        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        // 向量长度
        public float Magnitude()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }

        // 归一化
        public Vector2 Normalized()
        {
            float magnitude = Magnitude();
            if (magnitude == 0)
                throw new InvalidOperationException("Cannot normalize a zero vector.");
            return this / magnitude;
        }

        // 相等运算符重载
        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        // 不等运算符重载
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }

        // 重写 Equals
        public override bool Equals(object obj)
        {
            return obj is Vector2 other && this == other;
        }

        // 实现 IEquatable<Vector2>
        public bool Equals(Vector2 other)
        {
            return this == other;
        }

        // 重写 GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        // 重写 ToString
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        // 构造函数
        public Vector3()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // 加法运算符
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        // 减法运算符
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        // 标量乘法
        public static Vector3 operator *(Vector3 v, float scalar)
        {
            return new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        // 标量除法
        public static Vector3 operator /(Vector3 v, float scalar)
        {
            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        // 点积
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 cwiseProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        // 叉积
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static float MagnitudeV3(Vector3 a)
        {
            return MathF.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public static float AngleBetweenV3(Vector3 a, Vector3 b)
        {
            float angle = Dot(a, b);
            angle /= (MagnitudeV3(a) * MagnitudeV3(b));
            return MathF.Acos(angle);
        }

        public static Vector3 ProjV3(Vector3 a, Vector3 b)
        {
            Vector3 bn = b / MagnitudeV3(b);
            return bn * Dot(a, bn);
        }

        public Vector3 Normalize()
        {
            float magnitude = MagnitudeV3(this);

            // 检查模长是否为零
            if (magnitude == 0)
            {
                return new Vector3(0, 0, 0); // 返回零向量
                                             // 或者抛出异常：
                                             // throw new InvalidOperationException("Cannot normalize a zero vector.");
            }

            return this / magnitude;
        }

        public static bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
        {
            Vector3 cp1 = Cross(b - a, p1 - a);
            Vector3 cp2 = Cross(b - a, p2 - a);
            if (Dot(cp1, cp2) >= 0) return true;
            else return false;
        }

        public static Vector3 GenTriNormal(Vector3 t1, Vector3 t2, Vector3 t3)
        {
            Vector3 u = t2 - t1;
            Vector3 v = t3 - t1;

            Vector3 normal = Cross(u, v);

            return normal;
        }

        public static bool inTriangle(Vector3 point, Vector3 tri1, Vector3 tri2, Vector3 tri3)
        {
            // Test to see if it is within an infinite prism that the triangle outlines.
            bool within_tri_prisim = SameSide(point, tri1, tri2, tri3) && SameSide(point, tri2, tri1, tri3)
                                     && SameSide(point, tri3, tri1, tri2);

            // If it isn't it will never be on the triangle
            if (!within_tri_prisim)
                return false;

            // Calulate Triangle's Normal
            Vector3 n = GenTriNormal(tri1, tri2, tri3);

            // Project the point onto this normal
            Vector3 proj = ProjV3(point, n);

            // If the distance from the triangle to the point is 0
            //	it lies on the triangle
            if (MagnitudeV3(proj) == 0)
                return true;
            else
                return false;
        }

        // 相等运算符
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        // 不等运算符
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        // 重写 Equals
        public override bool Equals(object obj)
        {
            if (obj is Vector3 other)
            {
                return this == other;
            }
            return false;
        }

        // 重写 GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        // 重写 ToString
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
    
    /*public struct Vector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        public Vector4()
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 0;
        }
        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }*/
}
