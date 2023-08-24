// Copyright 2019-2021 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ROS2;
using UnityEngine;

/// <summary>
/// An example class provided for testing of basic ROS2 communication
/// </summary>
public class SubscriptionExample : MonoBehaviour
{
    /// <summary>
    /// Topic to listen on.
    /// </summary>
    public string Topic = "chatter";

    private ISubscription<std_msgs.msg.String> Subscription;

    /// <summary>
    /// Create the subscription.
    /// </summary>
    void Start()
    {
        this.Subscription = GetComponent<NodeComponent>().CreateSubscription<std_msgs.msg.String>(
            this.Topic,
            msg => Debug.Log($"ROS2 Subscription heard: [{msg.Data}]")
        );
    }

    /// <summary>
    /// Dispose the subscription.
    /// </summary>
    void OnDestroy()
    {
        this.Subscription?.Dispose();
    }
}
