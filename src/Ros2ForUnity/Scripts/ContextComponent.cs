using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Component which wraps a <see cref="ROS2.Context"/>
    /// and disposes it when it is destroyed.
    /// </summary>
    public sealed class ContextComponent : MonoBehaviour, IContext
    {
        private readonly Lazy<Context> LazyContext = new Lazy<Context>(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Wrapped <see cref="ROS2.Context"/> 
        /// </summary>
        /// <remarks>
        /// The context is created using <see cref="Lazy{Context}"/>
        /// which allows it to be used before <see cref="Awake"/> is called.
        /// </remarks>
        public Context Context
        {
            get => this.LazyContext.Value;
        }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get => this.Context.IsDisposed;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, INode> Nodes
        {
            get => this.Context.Nodes;
        }

        /// <inheritdoc/>
        public event Action OnShutdown
        {
            add { this.Context.OnShutdown += value; }
            remove { this.Context.OnShutdown -= value; }
        }

        /// <inheritdoc/>
        public bool Ok()
        {
            return this.Context.Ok();
        }

        /// <inheritdoc/>
        public bool TryCreateNode(string name, out INode node)
        {
            return this.Context.TryCreateNode(name, out node);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Context.Dispose();
        }

        /// <summary>
        /// Creates the wrapped context if it was not created yet.
        /// </summary>
        void Awake()
        {
            _ = this.Context;
        }

        /// <summary>
        /// Disposes the wrapped context.
        /// </summary>
        void OnDestroy()
        {
            this.Dispose();
        }
    }
}