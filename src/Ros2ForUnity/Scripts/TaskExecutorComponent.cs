using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Executor component which performs spinning within a separate task..
    /// </summary>
    /// <remarks>
    /// Callbacks called by this executor can not interact with most of the Unity API.
    /// </remarks>
    public sealed class TaskExecutorComponent : ExecutorComponent
    {
        /// <summary>
        /// Timeout between spins in seconds.
        /// </summary>
        public double Timeout = 0.25;

        /// <summary>
        /// Task managed by this executor.
        /// </summary>
        /// <remarks>
        /// Available after <see cref="Awake"/> has been called.
        /// </remarks>
        public Task Task { get; private set; }

        private Lazy<Executors.ManualExecutor> LazyExecutor;

        private CancellationTokenSource CancellationSource;

        /// <inheritdoc/>
        protected override IExecutor Executor
        {
            get => this.LazyExecutor.Value;
        }

        public TaskExecutorComponent() : base()
        {
            this.LazyExecutor = new Lazy<Executors.ManualExecutor>(() =>
            {
                return new Executors.ManualExecutor(this.Context.Context);
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Stop <see cref="Task"/> and return after it has stopped.
        /// </summary>
        /// <remarks>
        /// This function returns immediately if <see cref="Task"/>
        /// was not started or has been already stopped.
        /// </remarks>
        private void StopSpinTask()
        {
            try
            {
                this.CancellationSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // task has been canceled before
            }
            try
            {
                this.Task?.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(inner => inner is TaskCanceledException);
            }
            catch (ObjectDisposedException)
            {
                // task has already stopped
            }
        }

        void Awake()
        {
            var executor = this.LazyExecutor.Value;
            this.CancellationSource = new CancellationTokenSource();
            this.Task = executor.CreateSpinTask(TimeSpan.FromSeconds(this.Timeout), this.CancellationSource.Token);
        }

        /// <summary>
        /// Starts <see cref="Task"/>.
        /// </summary>
        /// <remarks>
        /// A callback is registered with <see cref="Context.OnShutdown"/>
        /// to stop <see cref="Task"/>.
        /// </remarks>
        void Start()
        {
            this.Context.OnShutdown += this.StopSpinTask;
            this.Task.Start();
        }

        /// <summary>
        /// Assert that <see cref="ExecutorComponent.Context"/> is not <see cref="null"/>
        /// and <see cref="Timeout"/> is valid.
        /// </summary>
        new void OnValidate()
        {
            base.OnValidate();
            try
            {
                TimeSpan.FromSeconds(this.Timeout);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        /// <summary>
        /// Stops <see cref="Task"/> before calling <see cref="ExecutorComponent.Dispose"/>.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this.StopSpinTask();
            }
            catch (AggregateException)
            {
                // prevent faulted task from preventing disposal
            }
            this.Context.OnShutdown -= this.StopSpinTask;
            this.Task.Dispose();
            base.Dispose();
            this.CancellationSource.Dispose();
        }
    }
}