Ros2 For Unity
===============

ROS2 For Unity is a high-performance communication solution to connect Unity3D and ROS2 ecosystem in a ROS2 "native" way. Communication is not bridged as in several other solutions, but instead it uses ROS2 middleware stack (rcl layer and below), which means you can have ROS2 nodes in your simulation.
Advantages of this module include:
- High performance - higher throughput and considerably lower latencies comparing to bridging solutions.
- Your simulation entities are real ROS2 nodes / publishers / subscribers. They will behave correctly with e.g. command line tools such as `ros2 topic`. They will respect QoS settings and can use ROS2 native time.
- The module supplies abstractions and tools to use in your Unity project, including transformations, sensor interface, a clock, spinning loop wrapped in a MonoBehavior, handling initialization and shutdown.
- Supports all standard ROS2 messages.
- Custom messages are generated automatically with build, using standard ROS2 way. It is straightforward to generate and use them without having to define `.cs` equivalents by hand.
- The module is wrapped as a Unity package.

## Platforms

Supported OSes:
- Ubuntu 22.04 (bash)
- Ubuntu 20.04 (bash)
- Windows 10 (powershell)
- Windows 11* (powershel)

> \* ROS2 Galactic and Humble support only Windows 10 ([ROS 2 Windows system requirements](https://docs.ros.org/en/humble/Installation/Windows-Install-Binary.html#system-requirements)), but it is proven that it also works fine on Windows 11.


Supported ROS2 distributions:
- Galactic
- Humble

Supported Unity3d:
- 2020+

Older versions of Unity3d may work, but the editor executable most probably won't be detected properly by deployment script. This would require user confirmation for using unsupported version.

This asset can be prepared in two flavours:

- standalone mode, where no ROS2 installation is required on target machine, e.g., your Unity3D simulation server. All required dependencies are installed and can be used e.g. as a complete set of Unity3D plugins.
- overlay mode, where the ROS2 installation is required on target machine. Only asset libraries and generated messages are installed therefore ROS2 instance must be sourced.

## Releases

> **Note:** The releases are built in overlay mode.

The best way to start quickly is to use our releases.

You can download pre-built [releases](https://github.com/RobotecAI/ros2-for-unity/releases) of the Asset that support both platforms and specific ros2 and Unity3D versions.

## Building

> **Note:** The project will pull `ros2cs` into the workspace, which also functions independently as it is a more general project aimed at any `C# / .Net` environment.
It has its own README and scripting, but for building the Unity package, please use instructions and scripting in this document instead, unless you also wish to run tests or examples for `ros2cs`.

Please see OS-specific instructions:
- [Instructions for Ubuntu](README-UBUNTU.md)
- [Instructions for Windows](README-WINDOWS.md)

## Custom messages

Custom messages can be included in the build by either:
* listing them in `ros2_for_unity_custom_messages.repos` file, or
* manually inserting them in `src/ros2cs` directory. If the folder doesn't exist, you must pull repositories first (see building steps for each OS).

## Installation

1. Perform building steps described in the OS-specific readme or download pre-built Unity package. Do not source `ros2-for-unity` nor `ros2cs` project into ROS2 workspace.
2. Open or create Unity project.
3. Import `install/package/Ros2ForUnity` into your project with the package manager

## Usage

**Prerequisites**

* If your build was prepared with `--standalone` flag then you are fine, and all you have to do is run the editor

otherwise

* source ROS2 which matches the `Ros2ForUnity` version, then run the editor from within the very same terminal/console.

**Initializing Ros2ForUnity**

1. Initialize ROS by creating a context. You have two options:
    1. `Context` which is a standard class that can be created anywhere:
        ```cs
        using ROS2;

        IContext context = new Context();
        ```
    2. `ContextComponent` which is a `MonoBehavior` and attached to a `GameObject` somewhere in the scene:
        ```cs
        using ROS2;

        // asuming that the component is attached to the same GameObject
        IContext context = GetComponent<ContextComponent>();
        ```

    When running in edit mode call `Setup.SetupPath()` before to prevent the native libraries from failing to load.

2. Create a node. You have (again) two options:
    1. Creating nodes directly using the context:
        ```cs
        using ROS2;

        if (!context.TryCreateNode("ROS2UnityListenerNode", out INode node))
        {
            throw new InvalidOperationException("node already exists");
        }
        ```

    2. Using a `NodeComponent` which is a `MonoBehavior` and attached to a `GameObject` somewhere in the scene:

        - the component allows an executor to be configured which allows you to skip parts of the next step

        ```cs
        using ROS2;

        // asuming that the component is attached to the same GameObject
        INode node = GetComponent<NodeComponent>();
        ```

3. Create an executor handling events of your nodes. You have multiple options:
    1. use `ManualExecutor` if you want to have control over where and when events are handled:
        ```cs
        using ROS2.Executors;

        ManualExecutor executor = new ManualExecutor(context);
        executor.Add(node);

        executor.SpinWhile(() => true);
        ```
    2. use `TaskExecutor` to perform the handling of events in a separate task:
        ```cs
        using ROS2.Executors;

        TaskExecutor executor = new TaskExecutor(context, TimeSpan.FromSeconds(0.25));
        executor.Add(node);
        ```
    3. use `TaskExecutorComponent` which is a `MonoBehavior` behaving like `TaskExecutor`

        - dont forget that most of the Unity API is not available when executing in a different task or thread

    4. use `FrameExecutor` which is a `MonoBehavior` and handles events every time `Update` is called

**Publishing messages:**

1. Create a publisher
    ```cs
    using ROS;

    IPublisher<std_msgs.msg.String> chatter_pub = node.CreatePublisher("chatter");
    ```
2. Send messages
    ```cs
    std_msgs.msg.String msg = new std_msgs.msg.String { Data = "Hello Ros2ForUnity!" };
    chatter_pub.Publish(msg);
    ```

**Subscribing to a topic**

1. Create a subscription:
    ```cs
    using ROS;

    ISubscription<std_msgs.msg.String> chatter_sub = node.CreateSubscription(
        "chatter",
        msg => Debug.Log($"Unity listener heard: [{msg.Data}]")
    );
    ```

**Creating a service**

1. Create a service:
    ```cs
    using ROS;

    IService<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response> service =
        node.CreateService(
            "add_two_ints"
            request => new example_interfaces.srv.AddTwoInts_Response { Sum = request.A + request.B };
        )
    ```

**Calling a service**

1. Create a client:
    ```cs
    using ROS;

    IClient<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response> addTwoIntsClient =
        node.CreateClient("add_two_ints");
    ```

2. Create a request and call a service:
    ```cs
    using ROS;

    var request = new example_interfaces.srv.AddTwoInts_Request { A = 1, B = 2 };
    Debug.Log($"Received answer {addTwoIntsClient.Call(request)}");
    ```

3. You can also make an async call to prevent blocking Unity and other scripts:
    ```cs
    using ROS2;

    using (Task<example_interfaces.srv.AddTwoInts_Response> asyncTask = addTwoIntsClient.CallAsync(request))
    {
        // has to be done inside a coroutine
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log($"Received answer {task.Result.Sum}");
        }
        else
        {
            Debug.Log("Call failed");
        }
    }

    ```

Dont forget to dispose the instances after you finished using them.

### Examples

More complete examples are included in the Unity package.

## Acknowledgements 

Open-source release of ROS2 For Unity was made possible through cooperation with [Tier IV](https://tier4.jp). Thanks to encouragement, support and requirements driven by Tier IV the project was significantly improved in terms of portability, stability, core structure and user-friendliness.
