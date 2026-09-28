using System;
using System.Numerics;


namespace test5;


public class Mat4
{
    public float[,] M = new float[4, 4];

    public static Mat4 Identity()
    {
        var m = new Mat4();
        m.M[0, 0] = m.M[1, 1] = m.M[2, 2] = m.M[3, 3] = 1f;
        return m;
    }

    public static Mat4 RotationZ(float angle)
    {

        var m = Identity();
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        m.M[0, 0] = c; m.M[0, 1] = -s;
        m.M[1, 0] =s; m.M[1, 1] = c;
        return m;

    }

    public static Mat4 RotationX(float angle)
    {

        var m = Identity();
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        m.M[1, 1] = c; m.M[1, 2] = -s;
        m.M[2, 1] = s; m.M[2, 2] = c;
        return m;

    }

    public static Mat4 RotationY(float angle)
    {

        var m = Identity();
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        m.M[0, 0] = c; m.M[0, 2] = s;
        m.M[2, 0] = -s; m.M[2, 2] = c;
        return m;

    }


    public static Mat4 Orthographic(float left, float right, float bottom, float top,
                                    float near, float far)
    {
        var m = Identity();
        m.M[0, 0] = 2f / (right - left);
        m.M[1, 1] = 2f / (top - bottom);
        m.M[2, 2] = -2f / (far - near);
        m.M[0, 3] = -(right + left) / (right - left);
        m.M[1, 3] = -(top + bottom) / (top - bottom);
        m.M[2, 3] = -(far + near) / (far - near);
        return m;


    }

    public static Mat4 operator *(Mat4 a, Mat4 b)
    {
        var r = new Mat4();
        for (int i=0; i<4; i++)
            for(int j=0; j<4; j++)
            {
                float sum = 0;
                for (int k = 0; k < 4; k++)
                    sum += a.M[i, k] * b.M[k, j];
                r.M[i,j]=sum;
            }
        return r;
    }

    public Vector4 Transform(Vector4 v) => new Vector4(
        M[0, 0]* v.X + M[0, 1]* v.Y + M[0, 2]* v.Z + M[0, 3]* v.W,
        M[1, 0]* v.X + M[1, 1]* v.Y + M[1, 2]* v.Z + M[1, 3]* v.W,
        M[2, 0]* v.X + M[2, 1]* v.Y + M[2, 2]* v.Z + M[2, 3]* v.W,
        M[3, 0]* v.X + M[3, 1]* v.Y + M[3, 2]* v.Z + M[3, 3]* v.W
        );


}


