using Unity.VisualScripting;
using UnityEngine;

namespace CHM.VisualScriptingKai.Editor
{

    /// <summary>
    /// Data structure that can be used to locate a node's location.
    /// </summary>
    public struct NodeTrace : IGraphElementTrace
    {
        public readonly IGraphElement GraphElement => unit;
        public IUnit unit;
        public GraphReference Reference { get; set; }
        public GraphSource Source { get; set; }
        public long Score { get; set; }
        public readonly Vector2 GraphPosition => unit.position;
        public readonly int CompareTo(IGraphElementTrace other)
        => this.DefaultCompareTo(other);
        public readonly string GetInfo()
        {
            return $"<b><size=14>{unit.Name()}</size></b>" 
            + $"\n<b>Default values:</b> {unit.defaultValues.ToCommaSeparatedString()}"
            + $"\n{Source.Info}";
        }

        public readonly Texture2D GetIcon(int resolution)
        {
            // Cursed operator overload. Gets texture with resolution.
            return unit.Icon()[resolution];
        }
    }
}
