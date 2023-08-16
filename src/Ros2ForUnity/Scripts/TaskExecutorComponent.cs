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
        /// Available after <see cref="OnEnable"/> has been called the first time.
        /// The task is stored until the next call to <see cref="OnEnable"/>.
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
        /// This method returns immediately if <see cref="Task"/>
        /// was not started or has been already stopped.
        /// It ignores whether <see cref="Task"/> faulted.
        /// </remarks>
        private void AssertStopped()
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
            catch (AggregateException)
            {
                // ignore whether the task faulted
            }
            catch (ObjectDisposedException)
            {
                // task has already stopped
            }
        }

        /// <summary>
        /// Registers a callback with <see cref="Context.OnShutdown"/>
        /// to stop <see cref="Task"/>.
        /// </summary>
        void Awake()
        {
            _ = this.LazyExecutor.Value;
            this.Context.OnShutdown += this.AssertStopped;
        }

        /// <summary>
        /// Starts a new <see cref="Task"/>.
        /// </summary>
        void OnEnable()
        {
            Debug.Assert(this.Task is null || this.Task.IsCompleted, "Task was not stopped before enabling executor", this);
            Debug.Assert(this.CancellationSource is null, "CancellationSource was not cleaned up when enabling executor", this);

            this.CancellationSource = new CancellationTokenSource();
            this.Task = this.LazyExecutor.Value.CreateSpinTask(TimeSpan.FromSeconds(this.Timeout), this.CancellationSource.Token);
            this.Task.Start();
        }

        /// <summary>
        /// Stops the current <see cref="Task"/>.
        /// </summary>
        /// <remarks>
        /// Inspect <see cref="Task.IsFaulted"/> to see if it faulted.
        /// </remarks>
        void OnDisable()
        {
            Debug.Assert(this.Task.Status != TaskStatus.Created, "Task was not started before disabling executor", this);

            this.CancellationSource.Cancel();
            try
            {
                this.Task.Wait();
            }
            catch (AggregateException)
            {
                // ignore since we only need to make sure that the task is not running
            }
            this.Task.Dispose();
            this.CancellationSource.Dispose();
            this.CancellationSource = null;
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
            this.AssertStopped();
            this.Context.OnShutdown -= this.AssertStopped;
            this.Task?.Dispose();
            base.Dispose();
            this.CancellationSource?.Dispose();
        }
    }
}