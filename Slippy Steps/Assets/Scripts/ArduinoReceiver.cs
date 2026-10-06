using UnityEngine;
using System;
using System.IO.Ports;

/// <summary>
/// Helper class to poll the arduino controller
/// </summary>
public class ArduinoReceiver 
{
    private bool debugEnabled = false;
    private int center = 512;
    private short mult = 64;
    private SerialPort port = new("COM3", 19200);

    public short XMovement { get; private set; }
    public short YMovement { get; private set; }
    public bool JumpHeld { get; private set; }
    //
    // Start is called before the first frame update
    public ArduinoReceiver()
    {
        port.Open();
        port.ReadTimeout = 10;
    }

    // polls the serial port
    public void PollInput()
    {
        if (port.IsOpen)
        {
            try
            {
                string Arudine = port.ReadLine();
                //Debug.Log(Arudine);
                string[] ArduinoInputs = Arudine.Split(';');
                string X_Input = ArduinoInputs[0];
                string Y_Input = ArduinoInputs[1];
                string B_Input = ArduinoInputs[2];
                if (debugEnabled) Debug.Log("X Input: " + X_Input + " ... Y Input: " + Y_Input + "... B Input: " + B_Input);

                XMovement = (short) ((short.Parse(X_Input) - center) * mult);
                YMovement = (short) ((short.Parse(Y_Input) - center) * mult);
                JumpHeld = short.Parse(B_Input) > 1;
            }
            // the immense catch block count
            catch (System.IO.IOException e)
            {
                Debug.LogWarning($"IOException (check COM3 port): {e}");
            }
            catch (TimeoutException)
            {
                if (debugEnabled) Debug.Log("No new data");
                // this case can do nothing cause the input values shouldnt change regardless
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception caught: {e}");
            }
        }
    }
}
