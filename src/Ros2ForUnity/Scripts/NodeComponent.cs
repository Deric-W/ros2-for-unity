using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Component which wraps a <see cref="INode"/>.
    /// </summary>
    public sealed class NodeComponent : MonoBehaviour, INode
    {
        /// <summary>
        /// Name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// Context from which the node should be created.
        /// </summary>
        public ContextComponent Context;

        /// <summary>
        /// Executor to which the node should be added, may be <see cref="null"/>.
        /// </summary>
        public ExecutorComponent Executor;

        private INode Node;

        /// <inheritdoc/>
        string INode.Name
        {
            get => this.Node.Name;
        }

        /// <inheritdoc/>
        IContext INode.Context
        {
            get => this.Node.Context;
        }

        /// <inheritdoc/>
        IExecutor INode.Executor
        {
            get => this.Node.Executor;
            set => this.Node.Executor = value;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IPublisherBase> Publishers
        {
            get => this.Node.Publishers;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ISubscriptionBase> Subscriptions
        {
            get => this.Node.Subscriptions;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IClientBase> Clients
        {
            get => this.Node.Clients;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IServiceBase> Services
        {
            get => this.Node.Services;
        }

        /// <inheritdoc/>
        public IPublisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            return this.Node.CreatePublisher<T>(topic, qos);
        }

        /// <inheritdoc/>
        public ISubscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            return this.Node.CreateSubscription<T>(topic, callback, qos);
        }

        /// <inheritdoc/>
        public IClient<I, O> CreateClient<I, O>(string topic, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            return this.Node.CreateClient<I, O>(topic, qos);
        }

        /// <inheritdoc/>
        public IService<I, O> CreateService<I, O>(string topic, Func<I, O> callback, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            return this.Node.CreateService<I, O>(topic, callback, qos);
        }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get => this.Node.IsDisposed;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Node?.Dispose();
        }

        /// <summary>
        /// Create the wrapped node and add it to <see cref="Executor"/> if not <see cref="null"/>.
        /// </summary>
        /// <remarks>
        /// Calling <see cref="INode"/> related methods on this component will fail until this method has finished. 
        /// </remarks>
        /// <exception cref="ArgumentException"> A node with the same name already exists in the context. </exception>
        void Awake()
        {
            if (!this.Context.TryCreateNode(this.Name, out this.Node))
            {
                throw new ArgumentException($"A node with the name {this.Name} already exists in this context", "Name");
            }
            if (!(this.Executor is null))
            {
                this.Executor.Add(this);
            }
        }

        /// <summary>
        /// Dispose the wrapped node.
        /// </summary>
        void OnDestroy()
        {
            this.Dispose();
        }

        /// <summary>
        /// Assert that <see cref="Name"/> and <see cref="Context"/> are not <see cref="null"/>.
        /// </summary>
        void OnValidate()
        {
            Debug.Assert(!(this.Name is null), "Name is null");
            Debug.Assert(!(this.Context is null), "Context is null");
        }
    }
}