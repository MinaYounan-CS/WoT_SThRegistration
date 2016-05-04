using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Arduino_Control
{
    class Op_Var
    {
        private static SerialPort S_Arduino;
        public  void set_port(string com,int baudrate)
        {
            S_Arduino.PortName = com;
            S_Arduino.BaudRate = baudrate;
        }
        public void recieved()
        {
            
        }
    }
}
