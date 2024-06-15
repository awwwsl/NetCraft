using NetCraft.Models.Enums;
using NetCraft.Models.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetCraft.Models;

public class Window : GameWindow
{
    private Shader _lampShader;
    private Shader _lightingShader;

    private Chunk _chunk;

    private Camera _camera;

    private Stopwatch _watch = new();

    private bool _firstMove = true;

    private Vector2 _lastPos;

    private int _allvertsVBO;
    private int _allvertsVAO;
    private int _pLightsSSBO;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        _lampShader = Shader.GetShaderFromId("blockLamp");
        _lightingShader = Shader.GetShaderFromId("blockNormal");

        _camera = new Camera(new Vector3(4f, 2f, 4f), Size.X / (float)Size.Y);

        _chunk = new((0, 0));

        _chunk.Blocks[5, 2, 5] = new WorldBlock("blockLamp")
        {
            Location = (5, 2, 5),
            PointLight = new()
            {
                Position = (5, 2, 5),
                Constant = 1f,
                Linear = 0.09f,
                Quadratic = 0.032f,
                Ambient = (0.8f, 0f, 0f),
                Diffuse = (1f, 0f, 0f),
                Specular = (0.4f, 0f, 0f),
            }
        };

        _chunk.Blocks[9, 2, 5] = new WorldBlock("blockLamp")
        {
            Location = (9, 2, 5),
            PointLight = new()
            {
                Position = (9, 2, 5),
                Constant = 1f,
                Linear = 0.09f,
                Quadratic = 0.032f,
                Ambient = (0f, 0.8f, 0f),
                Diffuse = (0f, 1f, 0f),
                Specular = (0.4f, 0f, 0f),
            }
        };

        _chunk.Blocks[9, 2, 9] = new WorldBlock("blockLamp")
        {
            Location = (9, 2, 9),
            PointLight = new()
            {
                Position = (9, 2, 9),
                Constant = 1f,
                Linear = 0.09f,
                Quadratic = 0.032f,
                Ambient = (0f, 0, 0.8f),
                Diffuse = (0f, 0f, 1f),
                Specular = (0f, 0f, 0.4f),
            }
        };
        _chunk.Blocks[5, 2, 9] = new BlockPointLight("blockLamp")
        {
            Location = (5, 2, 9),
            PointLight = new()
            {
                Position = (5, 2, 9),
                Constant = 1f,
                Linear = 0.09f,
                Quadratic = 0.032f,
                Ambient = (1.0f, 1.0f, 1.0f),
                Diffuse = (1f, 1f, 1f),
                Specular = (0.5f, 0f, 0f),
            }
        };

        _chunk.Load();

        CursorState = CursorState.Grabbed;

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        _allvertsVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _allvertsVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, LocalBlock.Vertices.Length * sizeof(float), LocalBlock.Vertices, BufferUsageHint.StaticDraw);

        _allvertsVAO = GL.GenVertexArray();
        GL.BindVertexArray(_allvertsVAO);

        var positionLocation = _lightingShader.GetAttribLocation("aPos");
        GL.EnableVertexAttribArray(positionLocation);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

        var normalLocation = _lightingShader.GetAttribLocation("aNormal");
        GL.EnableVertexAttribArray(normalLocation);
        GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

        var texCoordLocation = _lightingShader.GetAttribLocation("aTexCoords");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

        var pointLights = _chunk.GetPointLights();
        _pLightsSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _pLightsSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, pointLights.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLightRenderable)), pointLights, BufferUsageHint.StaticDraw);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _pLightsSSBO); // match binding in shader.frag

        _lightingShader.Use();
        _lightingShader.SetInt("pLightNum", pointLights.Length);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        _watch.Restart();
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        RenderChunk();

        SwapBuffers();

        Console.WriteLine($"FPS: {Math.Round(1f / e.Time, 2)}({_watch.Elapsed.TotalMilliseconds}ms)");
    }

    private void RenderChunk()
    {
        foreach (var block in _chunk.GetBlocks())
        {
            block.DiffuseMap.Use(TextureUnit.Texture0);
            block.SpecularMap.Use(TextureUnit.Texture1);

            var shader = block.Shader;

            shader.Use();
            shader.SetMatrix4("view", _camera.GetViewMatrix());
            shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            if (block.DiffuseMap != NullTexture.Instance)
            {
                shader.SetVector3("viewPos", _camera.Position);
                shader.SetInt("material.diffuse", 0);
            }
            if (block.SpecularMap != NullTexture.Instance)
            {
                shader.SetInt("material.specular", 1);
                shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
                shader.SetFloat("material.shininess", 32.0f);
            }

            if (block.PointLight != null)
                shader.SetVector3("fragColor", block.PointLight.Value.Diffuse);

            shader.SetMatrix4("model", Matrix4.Identity * Matrix4.CreateTranslation(block.Location));

            if (block.FaceVisible.HasFlag(BlockFaceVisible.Top))
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            if (block.FaceVisible.HasFlag(BlockFaceVisible.Bottom))
                GL.DrawArrays(PrimitiveType.Triangles, 6, 6);
            if (block.FaceVisible.HasFlag(BlockFaceVisible.XyFront))
                GL.DrawArrays(PrimitiveType.Triangles, 12, 6);
            if (block.FaceVisible.HasFlag(BlockFaceVisible.XyBack))
                GL.DrawArrays(PrimitiveType.Triangles, 18, 6);
            if (block.FaceVisible.HasFlag(BlockFaceVisible.ZyFront))
                GL.DrawArrays(PrimitiveType.Triangles, 24, 6);
            if (block.FaceVisible.HasFlag(BlockFaceVisible.ZyBack))
                GL.DrawArrays(PrimitiveType.Triangles, 30, 6);
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        DumpDebugInfo();

        UpdateUserAction(e);
    }

    private void UpdateUserAction(FrameEventArgs e)
    {
        if (!IsFocused)
        {
            return;
        }

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        float cameraSpeed = input.IsKeyDown(Keys.LeftControl) ? 5f : 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += new Vector3(_camera.Front.X, 0f, _camera.Front.Z).Normalized() * cameraSpeed * (float)e.Time; // Forward
        }
        if (input.IsKeyDown(Keys.S))
        {
            _camera.Position -= new Vector3(_camera.Front.X, 0f, _camera.Front.Z).Normalized() * cameraSpeed * (float)e.Time; // Backward
        }
        if (input.IsKeyDown(Keys.A))
        {
            _camera.Position -= new Vector3(_camera.Right.X, 0f, _camera.Right.Z).Normalized() * cameraSpeed * (float)e.Time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            _camera.Position += new Vector3(_camera.Right.X, 0f, _camera.Right.Z).Normalized() * cameraSpeed * (float)e.Time; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            _camera.Position += Vector3.UnitY * cameraSpeed * (float)e.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            _camera.Position -= Vector3.UnitY * cameraSpeed * (float)e.Time; // Down
        }

        var mouse = MouseState;

        if (_firstMove)
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity;
        }
    }

    private void DumpDebugInfo()
    {
        Console.WriteLine("Camera: " + _camera.Position);
        Console.WriteLine("CameraFacing: " + _camera.Front);
        try
        {
            var cap = (DebugCapability)_chunk.Blocks[(int)_camera.Position.X, (int)_camera.Position.Y, (int)_camera.Position.Z]?.Capabilities.FirstOrDefault(e => e is DebugCapability)!;
            cap.Dump();
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Out of chunk");
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("No Block");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _camera.Fov -= e.OffsetY;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _camera.AspectRatio = Size.X / (float)Size.Y;
    }
}
