using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Internal;

namespace AnyPath.Graphs.PlatformerGraph
{
    /// <summary>
    /// Utility to weld close edge endpoints in a platformer graph together.
    /// Also contains 'continous' weld methods, which make it extremely easy to just draw a graph and the vertices will
    /// be connected together automatically. For managed use of this continious welding, see <see cref="PlatformerGraphDrawer"/>
    /// </summary>
    public class PlatformerGraphWelder
    {
        /// <summary>
        /// Provides an easy way to generate vertices and edges for a platformer graph.
        /// You can just 'draw' lines from point to point and if points are close enough to each other, their shared edges will merge
        /// and thus be connected in the graph.
        /// </summary>
        /// <param name="a">Starting position of the edge</param>
        /// <param name="b">Ending position of the edge</param>
        /// <param name="enterCost">Optional extra cost associated with traversing this edge in a pathfinding query</param>
        /// <param name="flags">Optional flags associated with traversing this edge in a pathfinding query</param>
        /// <param name="vertices">
        /// <para>
        /// List of vertices to append to, keep around for as long as you're continiously welding a graph.
        /// </para>
        /// <para>
        /// Once your welding is finished, you can pass in this list to populate the PlatformerGraph
        /// </para>
        /// </param>
        /// <param name="edges">
        /// <para>
        /// The list of edges to append to, keep around for as long as you're continiously welding a graph.
        /// </para>
        /// <para>
        /// Once your welding is finished, you can pass in this list to populate the PlatformerGraph as either directed or undirected
        /// edges.
        /// </para>
        /// </param>
        /// <param name="buckets">Container that's neccessary for the algorithm to quickly find if there is already a vertex
        /// occupying a certain location.
        /// Allocate and/or make sure to clear this when you begin your first weld and keep it around for as long as you're continuously welding a graph</param>
        /// <param name="thresholdMultiplier">One divided by the maximum (manhattan) distance vertices can have in order to join them.
        /// For example, if you want to weld vertices that are closer than approx 0.1, pass in 1 divided by 0.1.
        /// Note that for performance reasons, this is the manhattan distance</param>
        /// <remarks>
        /// <para>
        /// This method is agnostic about the edges being undirected or directed. This is determined when you populate the PlatformerGraph.
        /// You can continuously weld and use two separate lists for undirected and directed edges if you desire.
        /// As long as the vertex list and buckets remain the same the result will be valid.
        /// </para>
        /// <para>Burst compatible so can run in a job</para>
        /// </remarks>
        public static void ContinuousWeld(
            float2 a, float2 b, float enterCost, int flags, NativeList<float2> vertices, NativeList<PlatformerGraph.Edge> edges, 
            NativeHashMap<int2, int> buckets, float thresholdMultiplier = 1f / 0.01f)
        {
            
            int2 fromBucketPos = (int2)math.round(a * thresholdMultiplier);
            int2 toBucketPos = (int2)math.round(b * thresholdMultiplier);
            int fromIndex, toIndex;

            if (!buckets.TryGetValue(fromBucketPos, out fromIndex))
            {
                fromIndex = vertices.Length;
                buckets.Add(fromBucketPos, fromIndex);
                vertices.Add(a);
            }
            
            if (!buckets.TryGetValue(toBucketPos, out toIndex))
            {
                toIndex = vertices.Length;
                buckets.Add(toBucketPos, toIndex);
                vertices.Add(b);
            }
            
            edges.Add(new PlatformerGraph.Edge(fromIndex, toIndex, enterCost, flags));
        }
        
        [ExcludeFromDocs]
        public static void ContinuousWeld(
            float2 a, float2 b, NativeList<float2> vertices, NativeList<PlatformerGraph.Edge> edges, 
            NativeHashMap<int2, int> buckets, float thresholdMultiplier = 1f / 0.01f)
        {
            
            ContinuousWeld(a, b, 0, 0,  vertices, edges, buckets);
        }
        
        /// <summary>
        /// Provides an easy way to generate vertices and edges for a platformer graph.
        /// You can just 'draw' lines from point to point and if points are close enough to each other, their shared edges will merge
        /// and thus be connected in the graph.
        /// For most easy of use, use the provided <see cref="PlatformerGraphDrawer"/>
        /// </summary>
        public static void ContinuousWeld(
            float2 a, float2 b, float enterCost, int flags, List<float2> vertices, List<PlatformerGraph.Edge> edges, 
            Dictionary<int2, int> buckets, float thresholdMultiplier = 1f / 0.01f)
        {
            
            int2 fromBucketPos = (int2)math.round(a * thresholdMultiplier);
            int2 toBucketPos = (int2)math.round(b * thresholdMultiplier);
            int fromIndex, toIndex;

            if (!buckets.TryGetValue(fromBucketPos, out fromIndex))
            {
                fromIndex = vertices.Count;
                buckets.Add(fromBucketPos, fromIndex);
                vertices.Add(a);
            }
            
            if (!buckets.TryGetValue(toBucketPos, out toIndex))
            {
                toIndex = vertices.Count;
                buckets.Add(toBucketPos, toIndex);
                vertices.Add(b);
            }
            
            edges.Add(new PlatformerGraph.Edge(fromIndex, toIndex, enterCost, flags));
        }
        
        /// <summary>
        /// Provides an easy way to generate vertices and edges for a platformer graph.
        /// You can just 'draw' lines from point to point and if points are close enough to each other, their shared edges will merge
        /// and thus be connected in the graph.
        /// For most easy of use, use the provided <see cref="PlatformerGraphDrawer"/>
        /// </summary>
        [ExcludeFromDocs]
        public static void ContinuousWeld(
            float2 a, float2 b, List<float2> vertices, List<PlatformerGraph.Edge> edges, 
            Dictionary<int2, int> buckets, float thresholdMultiplier = 1f / 0.01f)
        {
            ContinuousWeld(a, b, 0, 0,  vertices, edges, buckets);
        }

        
        /// <summary>
        /// Finds vertices that are close together in a graph and welds them together.
        /// Depending on your mesh this may or may not be neccessary for correct pathfinding, as neighbouring triangles need to share
        /// the same vertices.
        /// </summary>
        /// <param name="inOutVertices">The vertices to weld together. This list contains the modified vertices afterwards.</param>
        /// <param name="dependsOn">Optional job dependency for the scheduled job</param>
        /// <param name="inOutUndirectedEdges">The undirected edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no undirected edges.</param>
        /// <param name="inOutDirectedEdges">The directed edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no directed edges</param>
        /// <param name="weldThreshold">Distance below which two vertices will be welded together</param>
        /// <remarks>This method schedules a burst compiled job doing the work and can be run on another thread.</remarks>
        public static JobHandle ScheduleWeld(
            NativeList<float2> inOutVertices, 
            NativeArray<PlatformerGraph.Edge> inOutUndirectedEdges, 
            NativeArray<PlatformerGraph.Edge> inOutDirectedEdges = default, float weldThreshold = .001f, JobHandle dependsOn = default)
        {
            bool disposeUndirected = !inOutUndirectedEdges.IsCreated;
            bool disposeDirected = !inOutDirectedEdges.IsCreated;
            
            if (!inOutUndirectedEdges.IsCreated)
                inOutUndirectedEdges = new NativeArray<PlatformerGraph.Edge>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (!inOutDirectedEdges.IsCreated)
                inOutDirectedEdges = new NativeArray<PlatformerGraph.Edge>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            var job = new WeldJobInPlace()
            {
                weldThreshold = weldThreshold,
                inOutVertices = inOutVertices,
                inOutUndirectedEdges = inOutUndirectedEdges,
                inOutDirectedEdges = inOutDirectedEdges
            };

            var handle = job.Schedule(dependsOn);
            if (disposeUndirected)
                inOutUndirectedEdges.Dispose(handle);
            if (disposeDirected)
                inOutDirectedEdges.Dispose(handle);

            return handle;
        }
        
        /// <summary>
        /// Finds vertices that are close together in a mesh and welds them together.
        /// Depending on your graph this may or may not be neccessary for correct pathfinding, as neighbouring edges need to share
        /// the same vertices.
        /// </summary>
        /// <param name="inVertices">The vertices to weld together.</param>
        /// <param name="dependsOn">Optional job dependency for the scheduled job</param>
        /// <param name="inOutUndirectedEdges">The undirected edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no undirected edges.</param>
        /// <param name="inOutDirectedEdges">The directed edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no directed edges</param>
        /// <param name="outVertices">List to store the output vertices in, this list should be cleared beforehand</param>
        /// <param name="weldThreshold">Distance below which two vertices will be welded together</param>
        /// <remarks>This method schedules a burst compiled job doing the work and can be run on another thread.</remarks>
        public static JobHandle ScheduleWeld(
            NativeList<float2> inVertices, 
            NativeArray<PlatformerGraph.Edge> inOutUndirectedEdges, 
            NativeArray<PlatformerGraph.Edge> inOutDirectedEdges, NativeList<float2> outVertices, float weldThreshold = .001f, JobHandle dependsOn = default)
        {
            bool disposeUndirected = !inOutUndirectedEdges.IsCreated;
            bool disposeDirected = !inOutDirectedEdges.IsCreated;
            
            if (!inOutUndirectedEdges.IsCreated)
                inOutUndirectedEdges = new NativeArray<PlatformerGraph.Edge>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (!inOutDirectedEdges.IsCreated)
                inOutDirectedEdges = new NativeArray<PlatformerGraph.Edge>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            var job = new WeldJob()
            {
                weldThreshold = weldThreshold,
                inVertices = inVertices,
                outVertices = outVertices,
                inOutUndirectedEdges = inOutUndirectedEdges,
                inOutDirectedEdges = inOutDirectedEdges
            };

            var handle = job.Schedule(dependsOn);
            if (disposeUndirected)
                inOutUndirectedEdges.Dispose(handle);
            if (disposeDirected)
                inOutDirectedEdges.Dispose(handle);

            return handle;
        }

        /// <summary>
        /// Finds vertices that are close together in a graph and welds them together.
        /// Depending on your graph this may or may not be neccessary for correct pathfinding, as neighbouring edges need to share
        /// the same vertices.
        /// </summary>
        /// <param name="inVertices">Original vertices of the graph</param>
        /// <param name="inOutUndirectedEdges">The undirected edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no undirected edges.</param>
        /// <param name="inOutDirectedEdges">The directed edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no directed edges</param>
        /// <param name="outVertices">The new unique vertices of the welded mesh. This list should be cleraed beforehand</param>
        /// <param name="buckets">A temporary container used by the algorithm, this list should be cleraed beforehand</param>
        /// <param name="shiftedIndices">A temporary container used by the algorithm</param>
        /// <param name="weldThreshold">Distance below which two vertices will be welded together</param>
        /// <remarks>This method can be used inside a burst compiled job</remarks>
        public static void Weld( 
            NativeArray<float2> inVertices, 
            NativeArray<PlatformerGraph.Edge> inOutUndirectedEdges, 
            NativeArray<PlatformerGraph.Edge> inOutDirectedEdges,
            NativeList<float2> outVertices, 
            NativeHashMap<int2, int> buckets,
            NativeHashMap<int, int> shiftedIndices, float weldThreshold = .001f)
        {
            // assumes buckets are cleared
            // no need to clear shiftedIndices, as we're certain to overwrite all values we use

            weldThreshold = 1 / weldThreshold;
            
            for (int i = 0; i < inVertices.Length; i++)
            {
                float2 vert = inVertices[i];
                int2 bucketPos = (int2)math.round(vert * weldThreshold);

                if (buckets.TryGetValue(bucketPos, out int shiftedIndex))
                {
                    shiftedIndices[i] = shiftedIndex;
                }
                else
                {
                    shiftedIndex = outVertices.Length;
                    buckets.Add(bucketPos, shiftedIndex);
                    shiftedIndices[i] = shiftedIndex;
                    outVertices.Add(vert);
                }
            }

            if (inOutUndirectedEdges.IsCreated)
            {
                for (int i = 0; i < inOutUndirectedEdges.Length; i++)
                {
                    var edge = inOutUndirectedEdges[i];
                    edge.vertexIndexA = shiftedIndices[edge.vertexIndexA];
                    edge.vertexIndexB = shiftedIndices[edge.vertexIndexB];
                    inOutUndirectedEdges[i] = edge;
                }
            }
            
            if (inOutDirectedEdges.IsCreated)
            {
                for (int i = 0; i < inOutDirectedEdges.Length; i++)
                {
                    var edge = inOutDirectedEdges[i];
                    edge.vertexIndexA = shiftedIndices[edge.vertexIndexA];
                    edge.vertexIndexB = shiftedIndices[edge.vertexIndexB];
                    inOutDirectedEdges[i] = edge;
                }
            }
        }

        /// <summary>
        /// Finds vertices that are close together in a graph and welds them together in place.
        /// Depending on your hraph this may or may not be neccessary for correct pathfinding, as neighbouring edges need to share
        /// the same vertices.
        /// </summary>
        /// <param name="inOutVertices">The vertices to weld together. This list contains the modified vertices afterwards.</param>
        /// <param name="inOutUndirectedEdges">The undirected edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no undirected edges.</param>
        /// <param name="inOutDirectedEdges">The directed edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no directed edges</param>
        /// <param name="weldThreshold">Distance below which two vertices will be welded together</param>
        /// <remarks>For best performance, use the native overloads as they can utilize Unity's burst compiler for significant speed gains</remarks>
        public static void Weld(
            List<float2> inOutVertices,
            List<PlatformerGraph.Edge> inOutUndirectedEdges,
            List<PlatformerGraph.Edge> inOutDirectedEdges,
            float weldThreshold = .001f)
        {
            List<float2> temp = new List<float2>();
            Weld(inOutVertices, inOutUndirectedEdges, inOutDirectedEdges, temp, weldThreshold);
            inOutVertices.Clear();
            inOutVertices.AddRange(temp);
        }

        /// <summary>
        /// Finds vertices that are close together in a graph and welds them together.
        /// Depending on your hraph this may or may not be neccessary for correct pathfinding, as neighbouring edges need to share
        /// the same vertices.
        /// </summary>
        /// <param name="inVertices">Original vertices of the graph</param>
        /// <param name="inOutUndirectedEdges">The undirected edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no undirected edges.</param>
        /// <param name="inOutDirectedEdges">The directed edges. This array is modified in place and will contain the new edges afterwards.
        /// Use default if you have no directed edges</param>
        /// <param name="outVertices">The new unique vertices of the welded mesh, use in conjunction with inOutIndices</param>
        /// <param name="weldThreshold">Distance below which two vertices will be welded together</param>
        /// <remarks>For best performance, use the native overloads as they can utilize Unity's burst compiler for significant speed gains</remarks>
        public static void Weld(
            List<float2> inVertices,
            List<PlatformerGraph.Edge> inOutUndirectedEdges,
            List<PlatformerGraph.Edge> inOutDirectedEdges,
            List<float2> outVertices,
            float weldThreshold = .001f)
        {

            Dictionary<int2, int> buckets = new Dictionary<int2, int>(inVertices.Count);
            Dictionary<int, int> shiftedIndices = new Dictionary<int, int>(inVertices.Count);

            weldThreshold = 1 / weldThreshold;
            
            for (int i = 0; i < inVertices.Count; i++)
            {
                float2 vert = inVertices[i];
                int2 bucketPos = (int2)math.round(vert * weldThreshold);

                if (buckets.TryGetValue(bucketPos, out int shiftedIndex))
                {
                    shiftedIndices[i] = shiftedIndex;
                }
                else
                {
                    shiftedIndex = outVertices.Count;
                    buckets.Add(bucketPos, shiftedIndex);
                    shiftedIndices[i] = shiftedIndex;
                    outVertices.Add(vert);
                }
            }

            if (inOutUndirectedEdges != null)
            {
                for (int i = 0; i < inOutUndirectedEdges.Count; i++)
                {
                    var edge = inOutUndirectedEdges[i];
                    edge.vertexIndexA = shiftedIndices[edge.vertexIndexA];
                    edge.vertexIndexB = shiftedIndices[edge.vertexIndexB];
                    inOutUndirectedEdges[i] = edge;
                }
            }
            
            if (inOutDirectedEdges != null)
            {
                for (int i = 0; i < inOutDirectedEdges.Count; i++)
                {
                    var edge = inOutDirectedEdges[i];
                    edge.vertexIndexA = shiftedIndices[edge.vertexIndexA];
                    edge.vertexIndexB = shiftedIndices[edge.vertexIndexB];
                    inOutDirectedEdges[i] = edge;
                }
            }
        }
        
        [BurstCompile]
        private struct WeldJob : IJob
        {
            public float weldThreshold;
            [ReadOnly] public NativeList<float2> inVertices;
            public NativeList<float2> outVertices;
            public NativeArray<PlatformerGraph.Edge> inOutUndirectedEdges;
            public NativeArray<PlatformerGraph.Edge> inOutDirectedEdges;

            public void Execute()
            {
                var buckets = new NativeHashMap<int2, int>(inVertices.Length, Allocator.Temp);
                var shiftedIndices = new NativeHashMap<int, int>(inOutUndirectedEdges.Length + inOutDirectedEdges.Length, Allocator.Temp);
                Weld(inVertices, inOutUndirectedEdges, inOutDirectedEdges, outVertices, buckets, shiftedIndices, weldThreshold);
            }
        }

        [BurstCompile]
        private struct WeldJobInPlace : IJob
        {
            public float weldThreshold;
            public NativeList<float2> inOutVertices;
            public NativeArray<PlatformerGraph.Edge> inOutUndirectedEdges;
            public NativeArray<PlatformerGraph.Edge> inOutDirectedEdges;

            public void Execute()
            {
                var buckets = new NativeHashMap<int2, int>(inOutVertices.Length, Allocator.Temp);
                var shiftedIndices = new NativeHashMap<int, int>(inOutUndirectedEdges.Length + inOutDirectedEdges.Length, Allocator.Temp);
                var tempVertices = new NativeList<float2>(inOutVertices.Length, Allocator.Temp);
                
                Weld(inOutVertices, inOutUndirectedEdges, inOutDirectedEdges, tempVertices, buckets, shiftedIndices, weldThreshold);
                
                inOutVertices.Clear();
                inOutVertices.CopyFrom(tempVertices);
            }
        }
    }
}