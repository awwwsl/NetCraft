global using System.Diagnostics;
global using NetCraft;
global using NetCraft.Models;
using OpenTK;

using var window = new Window(new() { UpdateFrequency = 1000, }, new() { ClientSize = (960, 540) });
window.Run();
