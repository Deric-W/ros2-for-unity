using System;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Executor component which is spun on every frame if it is enabled.
    /// </summary>
    public sealed class FrameExecutor : ExecutorComponent
    {
        private Lazy<Executors.ManualExecutor> LazyExecutor;

        /// <summary>
        /// Wrapped executor which is constructed using <see cref="Lazy{}"/>
        /// and can be used before <see cref="Awake"/> is called. 
        /// </summary>
        protected override IExecutor Executor
        {
            get => this.LazyExecutor.Value;
        }

        public FrameExecutor() : base()
        {
            this.LazyExecutor = new Lazy<Executors.ManualExecutor>(() =>
            {
                return new Executors.ManualExecutor(this.Context.Context);
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Creates the wrapped executor if it has not been created yet.
        /// </summary>
        void Awake()
        {
            _ = this.LazyExecutor.Value;
        }

        /// <summary>
        /// Spins the executor once with a timeout of <see cref="TimeSpan.Zero"/>
        /// and does a rescan if one is scheduled.
        /// </summary>
        void Update()
        {
            if (!this.LazyExecutor.Value.TrySpin(TimeSpan.Zero))
            {
                this.LazyExecutor.Value.Rescan();
            }
        }
    }
}