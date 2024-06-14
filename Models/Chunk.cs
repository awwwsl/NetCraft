using System.Collections;
using NetCraft.Models.Enums;
using NetCraft.Models.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetCraft;

public class Chunk
{
    public WorldBlock?[,,] Blocks { get; set; } = new WorldBlock[SizeX, SizeY, SizeZ];

    public Vector2i Location { get; init; }

    private int _vertexBufferObject;
    private int _vertexArrayObject;

    private int _shaderStorageBufferObject;

    private PointLightAligned[] _pLights = Array.Empty<PointLightAligned>();

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
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, LocalBlock.Vertices.Length * sizeof(float), LocalBlock.Vertices, BufferUsageHint.StaticDraw);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        List<Shader> loadedShader = [];

        // load blocks & lights & initialize shader
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

                var positionLocation = block.Shader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                var normalLocation = block.Shader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                if (block.DiffuseMap is not null)
                {
                    var texCoordLocation = block.Shader.GetAttribLocation("aTexCoords");
                    GL.EnableVertexAttribArray(texCoordLocation);
                    GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                }
            }
        }
        _pLights = lights.Select(e => e.GetAligned()).ToArray();
        Console.WriteLine($"Number of point lights: {_pLights.Length}");
        Console.WriteLine("Size of PointLightAligned: " + System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLightAligned)));

        _shaderStorageBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _shaderStorageBufferObject);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, _pLights.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLightAligned)), _pLights, BufferUsageHint.StaticDraw);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _shaderStorageBufferObject); // match binding in shader.frag

        loadedShader.ForEach(e =>
        {
            if (!e.LightShader)
                e.SetInt("pLightNum", _pLights.Length);
        });

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

    public void Render(Camera camera, Vector3 light)
    {
        Shader? shader = null;
        int count = 0;
        for (int x = 0; x < SizeX; x++)
        for (int y = 0; y < SizeY; y++)
        for (int z = 0; z < SizeZ; z++)
        {
            var block = Blocks[x, y, z];
            if (block is null)
                continue;
            count++;
            block.DiffuseMap?.Use(TextureUnit.Texture0);
            block.SpecularMap?.Use(TextureUnit.Texture1);

            if (block.Shader != shader)
            {
                shader = block.Shader;
                shader.Use();
                shader.SetMatrix4("view", camera.GetViewMatrix());
                shader.SetMatrix4("projection", camera.GetProjectionMatrix());

                if (block.DiffuseMap is not null)
                {
                    shader.SetVector3("viewPos", camera.Position);
                    shader.SetInt("material.diffuse", 0);
                }
                if (block.SpecularMap is not null)
                {
                    shader.SetInt("material.specular", 1);
                    shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
                    shader.SetFloat("material.shininess", 32.0f);
                }
            }
            if (block.PointLight != null)
                shader.SetVector3("fragColor", block.PointLight.Value.Diffuse);
            block.Shader.SetMatrix4("model", Matrix4.Identity * Matrix4.CreateTranslation(block.Location));

            if (block.FaceCulling.HasFlag(BlockFaceCulling.Top))
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            if (block.FaceCulling.HasFlag(BlockFaceCulling.Bottom))
                GL.DrawArrays(PrimitiveType.Triangles, 6, 6);
            if (block.FaceCulling.HasFlag(BlockFaceCulling.XyFront))
                GL.DrawArrays(PrimitiveType.Triangles, 12, 6);
            if (block.FaceCulling.HasFlag(BlockFaceCulling.XyBack))
                GL.DrawArrays(PrimitiveType.Triangles, 18, 6);
            if (block.FaceCulling.HasFlag(BlockFaceCulling.ZyFront))
                GL.DrawArrays(PrimitiveType.Triangles, 24, 6);
            if (block.FaceCulling.HasFlag(BlockFaceCulling.ZyBack))
                GL.DrawArrays(PrimitiveType.Triangles, 30, 6);
        }
        Console.WriteLine($"Rendered {count} blocks");
    }
}
