using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetCraft.Models;

// In this tutorial we take a look at how we can use textures to make the light settings we set up in the last episode
// different per fragment instead of making them per object.
// Remember to check out the shaders for how we converted to using textures there.
public class Window : GameWindow
{
    private Shader _lampShader;
    private Shader _lightingShader;

    private Chunk _chunk;

    private Camera _camera;

    private Stopwatch _watch = new();

    private bool _firstMove = true;

    private Vector2 _lastPos;

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

        _chunk.Load();

        CursorState = CursorState.Grabbed;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        _watch.Restart();
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _chunk.Render(_camera, (8, 18, 8));

        SwapBuffers();

        Console.WriteLine($"FPS: {Math.Round(1f / e.Time, 2)}({_watch.Elapsed.TotalMilliseconds}ms)");
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
