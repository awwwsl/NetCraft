using System.Collections;
using NetCraft.Models.Enums;
using NetCraft.Models.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetCraft.Models;

public class Chunk
{
    public WorldBlock?[,,] Blocks { get; set; } = new WorldBlock[SizeX, SizeY, SizeZ];

    public Vector2i Location { get; init; }

    private PointLightRenderable[] _pLights = Array.Empty<PointLightRenderable>();

    private static Stopwatch _watch = new();

    public const int SizeX = 16;
    public const int GenerateSizeX = 16;
    public const int SizeY = 256;
    public const int GenerateSizeY = 2;
    public const int SizeZ = 16;
    public const int GenerateSizeZ = 16;

    private Stopwatch watch = new();

    public Chunk(Vector2i location)
    {
        Location = location;

        watch.Start();
        for (int x = 0; x < GenerateSizeX; x++)
        for (int y = 0; y < GenerateSizeY; y++)
        for (int z = 0; z < GenerateSizeZ; z++)
        {
            Blocks[x, y, z] = new WorldBlock("container2") { Location = new(x + Location.X * SizeX, y, z + Location.Y * SizeZ) };
        }
        Console.WriteLine("Construct time(ms): " + watch.Elapsed.TotalMilliseconds);
        watch.Reset();
    }

    public void Load()
    {
        List<Shader> loadedShader = new();

        _watch.Start();
        var lights = new List<PointLight>();
        for (int x = 0; x < Chunk.SizeX; x++)
        for (int y = 0; y < Chunk.SizeY; y++)
        for (int z = 0; z < Chunk.SizeZ; z++)
        {
            var block = Blocks[x, y, z];
            if (block is null)
                continue;
            if (block.PointLight is not null)
            {
                lights.Add(block.PointLight.Value);
                Console.WriteLine($"Added light {block.Location}");
            }
            if (!loadedShader.Contains(block.Shader))
            {
                loadedShader.Add(block.Shader);
            }
        }
        _pLights = lights.Select(e => e.GetAligned()).ToArray();
        Console.WriteLine($"Number of point lights: {_pLights.Length}");
        Console.WriteLine("Size of PointLightRenderable: " + System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLightRenderable)));

        Console.WriteLine("Load time(ms): " + watch.Elapsed.TotalMilliseconds);
        watch.Restart();

        // Calculate facecull
        for (int x = 0; x < GenerateSizeX; x++)
        for (int y = 0; y < GenerateSizeY; y++)
        for (int z = 0; z < GenerateSizeZ; z++)
        {
            WorldBlock? block = Blocks[x, y, z];
            if (block is null)
                continue;
            if (y < SizeY - 1 && Blocks[x, y + 1, z] is not null)
                block.FaceCulling &= ~BlockFaceCulling.Top;
            if (y > 0 && Blocks[x, y - 1, z] is not null)
                block.FaceCulling &= ~BlockFaceCulling.Bottom;
            if (x < SizeX - 1 && Blocks[x + 1, y, z] is not null)
                block.FaceCulling &= ~BlockFaceCulling.ZyFront;
            if (x > 0 && Blocks[x - 1, y, z] is not null)
                block.FaceCulling &= ~BlockFaceCulling.ZyBack;
            if (z < SizeZ - 1 && Blocks[x, y, z + 1] is not null)
                block.FaceCulling &= ~BlockFaceCulling.XyFront;
            if (z > 0 && Blocks[x, y, z - 1] is not null)
                block.FaceCulling &= ~BlockFaceCulling.XyBack;
        }
        Console.WriteLine("Facecull time(ms): " + watch.Elapsed.TotalMilliseconds);
        watch.Reset();
    }

    public PointLightRenderable[] GetPointLights()
    {
        return _pLights;
    }

    public IEnumerable<WorldBlock> GetBlocks()
    {
        for (int x = 0; x < SizeX; x++)
        for (int y = 0; y < SizeY; y++)
        for (int z = 0; z < SizeZ; z++)
        {
            var block = Blocks[x, y, z];
            if (block is not null)
            {
                yield return block;
            }
        }
    }
}
