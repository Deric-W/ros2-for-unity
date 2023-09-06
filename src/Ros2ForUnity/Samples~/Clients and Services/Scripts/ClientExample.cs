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

using System.Collections;
using System.Threading.Tasks;
using example_interfaces.srv;
using ROS2;
using UnityEngine;

/// <summary>
/// An example class provided for testing of basic ROS2 client
/// </summary>
public class ClientExample : MonoBehaviour
{
    /// <summary>
    /// Service topic.
    /// </summary>
    public string Topic = "add_two_ints";

    /// <summary>
    /// Timeout for requests.
    /// </summary>
    public float Timeout = 1;

    private IClient<AddTwoInts_Request, AddTwoInts_Response> Client;

    /// <summary>
    /// Create the client.
    /// </summary>
    void Start()
    {
        this.Client = this.GetComponent<NodeComponent>().CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
            this.Topic
        );
        this.StartCoroutine(this.RequestAnswers());
    }

    /// <summary>
    /// Dispose the client.
    /// </summary>
    void OnDestroy()
    {
        this.Client?.Dispose();
    }

    /// <summary>
    /// Wait for the service to become available
    /// and send random requests.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RequestAnswers()
    {
        while (!this.Client.IsServiceAvailable())
        {
            Debug.Log("Waiting for Service");
            yield return new WaitForSecondsRealtime(0.25f);
        }

        var request = new AddTwoInts_Request();
        while (true)
        {
            request.A = Random.Range(0, 100);
            request.B = Random.Range(0, 100);

            Debug.Log($"Request answer for {request.A} + {request.B}");
            using (Task<AddTwoInts_Response> task = this.Client.CallAsync(request))
            {
                float deadline = Time.time + this.Timeout;
                yield return new WaitUntil(() => task.IsCompleted || Time.time >= deadline);

                if (task.IsCompleted)
                {
                    Debug.Log($"Received answer {task.Result.Sum}");
                    Debug.Assert(task.Result.Sum == request.A + request.B, "Received invalid answer");
                }
                else
                {
                    Debug.LogError($"Service call timed out");
                    this.Client.Cancel(task);
                }
            }
        }
    }
}
