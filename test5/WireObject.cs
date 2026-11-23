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

    public static WireObject CreateDigit3(float size = 1.5f, float depth = 0.4f)
    {
        var contour = new (float x, float y)[]
        {
        
        (-0.5f,  1.0f),
        ( 0.3f,  1.0f),
        ( 0.7f,  0.9f),
        ( 0.9f,  0.6f),
        ( 0.9f,  0.3f),
        ( 0.6f,  0.0f),   
        ( 0.9f, -0.3f),
        ( 0.9f, -0.6f),
        ( 0.7f, -0.9f),
        ( 0.3f, -1.0f),
        (-0.5f, -1.0f),
        
        (-0.5f, -0.8f),
        ( 0.2f, -0.8f),
        ( 0.5f, -0.5f),
        ( 0.5f, -0.2f),
        ( 0.3f,  0.0f),  
        ( 0.5f,  0.2f),
        ( 0.5f,  0.5f),
        ( 0.2f,  0.8f),
        (-0.5f,  0.8f),
        };

        float s = size / 2f;      
        float halfD = depth / 2f;
        int n = contour.Length;

        var verts = new List<Vector3>();
        var edges = new List<(int, int)>();

        // Передняя грань 
        for (int i = 0; i < n; i++)
            verts.Add(new Vector3(contour[i].x * s, contour[i].y * s, halfD));
        // Задняя грань 
        for (int i = 0; i < n; i++)
            verts.Add(new Vector3(contour[i].x * s, contour[i].y * s, -halfD));
        // Рёбра передней грани 
        for (int i = 0; i < n; i++)
            edges.Add((i, (i + 1) % n));
        // Рёбра задней грани
        for (int i = 0; i < n; i++)
            edges.Add((n + i, n + (i + 1) % n));
        // Соединительные рёбра 
        for (int i = 0; i < n; i++)
            edges.Add((i, n + i));

        return new WireObject(verts.ToArray(), edges.ToArray());
    }



}
