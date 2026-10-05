using UnityEngine;
using System;
using System.IO.Ports;

public class ArduinoReceiver : MonoBehaviour
{
    [SerializeField] private bool debugEnabled = false;

    private SerialPort port = new SerialPort("COM3", 19200);

    // Start is called before the first frame update
    void Start()
    {
        port.Open();
        /*
        Set the read timeout low so unity doesn't freeze,
        and catch the exception below in update that unity will throw
        when the port isn't open and unity tries to check it
        */
        port.ReadTimeout = 10;
    }
    // Update is called once per frame
    void FixedUpdate()
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
            }
            catch (System.IO.IOException e)
            {
                Debug.LogWarning($"IOException (check COM3 port): {e}");
            }
            catch (TimeoutException)
            {
                if (debugEnabled) Debug.Log("No new data");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception caught: {e}");
            }
        }
    }
}
