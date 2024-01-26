using System;
using UnityEngine.Internal;

namespace AnyPath.Native
{
    /// <summary>
    /// Modifier that only allows nodes in a path when any bit flag matches (bitwise OR produces a non zero result).
    /// This could for example represent different kinds of surfaces that an agent can walk on. 
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public struct FlagBitmask<TNode> : IEdgeMod<TNode> where TNode : unmanaged, IEquatable<TNode>, INodeFlags
    {
        /// <summary>
        /// If true, compares node flags with the set bitmask. If false, all nodes may pass.
        /// </summary>
        public bool useBitmask;
        
        /// <summary>
        /// The bitmask to perform a bitwise OR with on every node
        /// </summary>
        public int bitmask;
        
        [ExcludeFromDocs]
        public bool ModifyCost(in TNode @from, in TNode to, ref float cost)
        {
            return !useBitmask || (to.Flags & bitmask) != 0;
        }

        /// <summary>
        /// Create a flag bitmask. In order for a node to be traversable, any of the flags must match.
        /// </summary>
        /// <param name="bitmask">The bitmask to use</param>
        public FlagBitmask(int bitmask)
        {
            this.useBitmask = true;
            this.bitmask = bitmask;
        }
    }
}