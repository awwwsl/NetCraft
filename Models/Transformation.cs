using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetCraft.Models;

[StructLayout(LayoutKind.Sequential)]
public struct Transformation
{
    public Matrix4 View;

    public Matrix4 Projection;
}
