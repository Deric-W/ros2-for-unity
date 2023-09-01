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

using System;
using example_interfaces.srv;
using ROS2;
using UnityEngine;

/// <summary>
/// An example class provided for testing of basic ROS2 service
/// </summary>
public class ROS2ServiceExample : MonoBehaviour
{
    /// <summary>
    /// Topic of the service.
    /// </summary>
    public string Topic = "add_two_ints";

    private IService<AddTwoInts_Request, AddTwoInts_Response> Service;

    /// <summary>
    /// Create the service.
    /// </summary>
    void Start()
    {
        this.Service = this.GetComponent<NodeComponent>().CreateService<AddTwoInts_Request, AddTwoInts_Response>(
            this.Topic,
            this.AddTwoInts
        );
    }

    /// <summary>
    /// Dispose the service.
    /// </summary>
    void OnDestroy()
    {
        this.Service?.Dispose();
    }

    private AddTwoInts_Response AddTwoInts(AddTwoInts_Request msg)
    {
        Debug.Log($"Incoming Service Request A={msg.A} B={msg.B}");
        AddTwoInts_Response response = new AddTwoInts_Response();
        response.Sum = msg.A + msg.B;
        return response;
    }
}
