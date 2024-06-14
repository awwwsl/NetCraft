namespace NetCraft.Models.Enums;

[Flags]
public enum BlockFaceVisible
{
    Top = 1 << 0,
    Bottom = 1 << 1,
    XyFront = 1 << 2,
    XyBack = 1 << 3,
    ZyFront = 1 << 4,
    ZyBack = 1 << 5,
}
