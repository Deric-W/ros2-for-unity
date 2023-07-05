using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Base class for Components which act as executors.
    /// </summary>
    public abstract class ExecutorComponent : MonoBehaviour, IExecutor
    {
        /// <summary>
        /// Context used by the executor.
        /// </summary>
        public ContextComponent Context;

        /// <summary>
        /// Wrapped executor.
        /// </summary>
        abstract protected IExecutor Executor { get; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get => this.Executor.IsDisposed;
        }

        /// <inheritdoc/>
        public int Count
        {
            get => this.Executor.Count;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get => this.Executor.IsReadOnly;
        }

        /// <inheritdoc/>
        public void Add(INode node)
        {
            this.Executor.Add(node);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.Executor.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(INode node)
        {
            return this.Executor.Contains(node);
        }

        /// <inheritdoc/>
        public void CopyTo(INode[] array, int arrayIndex)
        {
            this.Executor.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(INode node)
        {
            return this.Executor.Remove(node);
        }

        /// <inheritdoc/>
        public IEnumerator<INode> GetEnumerator()
        {
            return this.Executor.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public void ScheduleRescan()
        {
            this.Executor.ScheduleRescan();
        }

        /// <inheritdoc/>
        public bool TryScheduleRescan(INode node)
        {
            return this.Executor.TryScheduleRescan(node);
        }

        /// <inheritdoc/>
        public void Wait()
        {
            this.Executor.Wait();
        }

        /// <inheritdoc/>
        public bool TryWait(TimeSpan timeout)
        {
            return this.Executor.TryWait(timeout);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.Executor.Dispose();
        }

        /// <summary>
        /// Dispose the executor.
        /// </summary>
        void OnDestroy()
        {
            this.Dispose();
        }

        /// <summary>
        /// Assert that <see cref="Context"/> is not <see cref="null"/>.
        /// </summary>
        void OnValidate()
        {
            Debug.Assert(!(this.Context is null), "Context is null");
        }
    }
}