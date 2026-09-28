using System.Numerics;

namespace test5;

public class WireObject
{
    public Vector3[] Vertices;
    public (int, int)[] Edges;

    public WireObject(Vector3[] vertices, (int, int)[] edges )
    {
        Vertices =  vertices; Edges = edges;
    }


    public static WireObject CreateCube(float size = 1.5f)
    {
        float h = size / 2f;
        var v = new Vector3[]
            {
            new(-h, -h, -h), new( h, -h, -h), new( h,  h, -h), new(-h,  h, -h),
            new(-h, -h,  h), new( h, -h,  h), new( h,  h,  h), new(-h,  h,  h),
            };
        var e = new (int, int)[]
        {
            (0,1),(1,2),(2,3),(3,0),
            (4,5),(5,6),(6,7),(7,4),
            (0,4),(1,5),(2,6),(3,7),
        };

        return new WireObject(v, e);
    }
}
