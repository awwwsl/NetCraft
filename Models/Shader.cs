using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetCraft.Models;

// A simple class meant to help create shaders.
public class Shader
{
    public readonly int Handle;
    private string name;
    private static readonly Dictionary<string, Shader> shaders = new();
    private readonly Dictionary<string, int> uniformLocations;

    private static int ubo;

    public static Shader GetShaderFromId(string id)
    {
        if (shaders.TryGetValue(id, out var early))
        {
            return early;
        }
        else
        {
            throw new InvalidOperationException("Shader not found.");
        }
    }

    private Shader(string fragmentShaderPath, string vertexShaderPath, string name)
    {
        this.name = name;
        var vertSource = File.ReadAllText(vertexShaderPath);
        var vertShader = GL.CreateShader(ShaderType.VertexShader);

        CompileShader(vertShader, vertSource, name);

        var fragSource = File.ReadAllText(fragmentShaderPath);
        var fragShader = GL.CreateShader(ShaderType.FragmentShader);
        CompileShader(fragShader, fragSource, name);

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertShader);
        GL.AttachShader(Handle, fragShader);

        // And then link them together.
        LinkProgram(Handle);

        // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
        // Detach them, and then delete them.
        GL.DetachShader(Handle, vertShader);
        GL.DetachShader(Handle, fragShader);
        GL.DeleteShader(fragShader);
        GL.DeleteShader(vertShader);

        // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
        // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
        // later.

        // First, we have to get the number of active uniforms in the shader.
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        // Next, allocate the dictionary to hold the locations.
        uniformLocations = new Dictionary<string, int>();

        // Loop over all the uniforms,
        for (var i = 0; i < numberOfUniforms; i++)
        {
            // get the name of this uniform,
            var key = GL.GetActiveUniform(Handle, i, out _, out _);

            // get the location,
            var location = GL.GetUniformLocation(Handle, key);

            // and then add it to the dictionary.
            uniformLocations.Add(key, location);
        }
    }

    private static void CompileShader(int shader, string source, string name)
    {
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code != (int)All.True)
        {
            // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred whilst compiling Shader({name}).\n\n{infoLog}");
        }
    }

    private static void LinkProgram(int program)
    {
        // We link the program
        GL.LinkProgram(program);

        // Check for linking errors
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
        if (code != (int)All.True)
        {
            // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
            throw new Exception($"Error occurred whilst linking Program({program})");
        }
    }

    // A wrapper function that enables the shader program.
    private static Shader? _lastUsed = null;

    public void Use()
    {
        if (_lastUsed == this)
            return;
        GL.UseProgram(Handle);
        _lastUsed = this;
    }

    // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
    // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }

    // Uniform setters
    // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
    // You use VBOs for vertex-related data, and uniforms for almost everything else.

    // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
    //     1. Bind the program you want to set the uniform on
    //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
    //     3. Use the appropriate GL.Uniform* function to set the uniform.

    /// <summary>
    /// Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        if (uniformLocations.TryGetValue(name, out var location))
            GL.Uniform1(uniformLocations[name], data);
        else
            throw new KeyNotFoundException($"Shader {this.name} doesn't have prpoerty {name}.");
    }

    /// <summary>
    /// Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetFloat(string name, float data)
    {
        GL.UseProgram(Handle);
        if (uniformLocations.TryGetValue(name, out var location))
            GL.Uniform1(uniformLocations[name], data);
        else
            throw new KeyNotFoundException($"Shader {this.name} doesn't have prpoerty {name}.");
    }

    /// <summary>
    /// Set a uniform Matrix4 on this shader
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    /// <remarks>
    ///   <para>
    ///   The matrix is transposed before being sent to the shader.
    ///   </para>
    /// </remarks>
    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        if (uniformLocations.TryGetValue(name, out var location))
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        else
            throw new KeyNotFoundException($"Shader {this.name} doesn't have prpoerty {name}.");
    }

    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        if (uniformLocations.TryGetValue(name, out var location))
            GL.Uniform3(uniformLocations[name], data);
        else
            throw new KeyNotFoundException($"Shader {this.name} doesn't have prpoerty {name}.");
    }

    ///
    /// <summary>
    /// Set a uniform Vector4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetVector4(string name, Vector4 data)
    {
        GL.UseProgram(Handle);
        if (uniformLocations.TryGetValue(name, out var location))
            GL.Uniform4(uniformLocations[name], data);
        else
            throw new KeyNotFoundException($"Shader {this.name} doesn't have prpoerty {name}.");
    }

    private bool uboFirst = false;

    public void SetTransformation(ref Transformation transformation)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ubo);
        if (uboFirst)
        {
            GL.BufferData(
                BufferTarget.UniformBuffer,
                Marshal.SizeOf(typeof(Transformation)),
                ref transformation,
                BufferUsageHint.StaticDraw
            );
        }
        else
        {
            GL.BufferSubData(
                BufferTarget.UniformBuffer,
                0,
                Marshal.SizeOf(typeof(Transformation)),
                ref transformation
            );
        }
    }

    public static void InitializeShader()
    {
        List<(string Frag, string Vert, string Name)> pairs =
            new()
            {
                ("Shaders/blockLamp.frag", "Shaders/blockLamp.vert", "lightedSimpleVoxel"),
                ("Shaders/blockNormal.frag", "Shaders/blockNormal.vert", "simpleVoxel"),
            };

        foreach (var pair in pairs)
        {
            var shader = new Shader(pair.Frag, pair.Vert, pair.Name);
            shaders.Add(pair.Name, shader);
        }
    }
}
