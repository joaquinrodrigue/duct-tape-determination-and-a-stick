using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Arduino input device memory structure layout
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 5)]
public struct ArduinoDeviceState : IInputStateTypeInfo
{
    // i have no clue if this is necessary
    public FourCC format { get => new('a', 'r', 'd', 'o'); }

    [InputControl(name = "xAxis", layout = "Axis")]
    [FieldOffset(0)]
    public short xAxis;

    [InputControl(name = "yAxis", layout = "Axis")]
    [FieldOffset(2)]
    public short yAxis;

    [InputControl(name = "button", layout = "Button", bit = 0)]
    [FieldOffset(4)]
    public byte button;
}

/// <summary>
/// Arduino input device
/// </summary>
[InputControlLayout(stateType = typeof(ArduinoDeviceState))]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ArduinoControlDevice : InputDevice, IInputUpdateCallbackReceiver
{
    private readonly static string InterfaceName = "ArduinoInputDevice";
    private readonly static string ProductName = "ArduinoInputDevice";

    private static ArduinoReceiver api;
    public AxisControl xAxis { get; private set; }
    public AxisControl yAxis { get; private set; }
    public ButtonControl button { get; private set; }

    /// <summary>
    /// Constructor for editor purposes
    /// </summary>
    static ArduinoControlDevice()
    {
        InitializeInPlayer();
    }

    /// <summary>
    /// Actual initializer
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void InitializeInPlayer()
    {
        if (api == null) api = new ArduinoReceiver();

        InputSystem.RegisterLayout<ArduinoControlDevice>(
            matches: new InputDeviceMatcher()
                .WithInterface(InterfaceName)
                .WithProduct(ProductName)
        );

        bool exists = InputSystem.devices.Any(d => 
            d.description.interfaceName.Equals(InterfaceName) 
            && d.description.product.Equals(ProductName));
        if (!exists) InputSystem.AddDevice(new InputDeviceDescription { interfaceName = InterfaceName, product = ProductName });
    }

    /// <summary>
    /// Finish setting up the controller
    /// </summary>
    protected override void FinishSetup()
    {
        base.FinishSetup();
        xAxis = GetChildControl<AxisControl>("xAxis");
        yAxis = GetChildControl<AxisControl>("yAxis");
        button = GetChildControl<ButtonControl>("button");
    }
    //
    /// <summary>
    /// Update for when to poll device input
    /// </summary>
    public void OnUpdate()
    {
        ArduinoDeviceState state = new();
        api.PollInput();
        state.xAxis = api.XMovement;
        state.yAxis = api.YMovement;
        state.button = (byte) (api.JumpHeld ? 1 : 0);
        InputSystem.QueueStateEvent(this, state);
    }
}