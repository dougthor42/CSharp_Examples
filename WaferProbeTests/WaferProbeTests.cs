using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.VisaNS;

namespace WaferProbeTests
{
    public class WaferProbeTests
    {
    }

    public class SweepVSingleSMU
    {
        // Class attributes and their defaults
        string instrAddr = null;
        double startV = 0;                  // Volts
        double endV = 1;
        double stepV = 0.02;
        int pts = 51;
        double compl = 0.2;                 // Amps

        #region Constructors and Destructor
        public SweepVSingleSMU()
        {
            instrAddr = "GPIB0::25::INSTR";
        }

        public SweepVSingleSMU(string addr)
        {
            Console.WriteLine("Called with 1 arg, using defaults.");
            instrAddr = addr;
            startV = 0;
            endV = 1;
            stepV = 0.02;
            pts = CalcNumPoints();
            compl = 0.02;
        }

        public SweepVSingleSMU(string addr, double start, double stop, double step, double comp)
        {
            Console.WriteLine("Called with all args.");
            instrAddr = addr;
            startV = start;
            endV = stop;
            stepV = step;
            pts = CalcNumPoints();
            compl = comp;
        }
        
        // Destructor
        ~SweepVSingleSMU()
        {
            // Run any clean-up code here.
        }
        #endregion

        private int CalcNumPoints()
        {
            double pts = (endV - startV) / stepV;
            return (int)Math.Floor(pts);
        }

        public double[,] RunSweep()//out int exitCode)
        {
            // Assume that we're exiting OK:
            //exitCode = 0;
            // Create the session and message-based session
            MessageBasedSession mbSession = null;
            Session rm = null;

            string[] general_setup = {
                                         "*RST",
                                         ":TRAC:FEED:CONT NEV",
                                         ":SYST:BEEP:STAT OFF",
                                         "*CLS",
                                         "*ESE 0",
                                         ":STAT:PRES",
                                         ":TRIG:CLE",
                                         ":SYST:AZER ON",
                                         ":ARM:COUN 1",
                                         ":ARM:SOUR IMM",
                                         ":ARM:DIR ACC",
                                         ":ARM:OUTP NONE",
                                         ":TRIG:SOUR IMM",
                                         ":TRIG:DIR ACC",
                                         ":TRIG:DEL 0.02",
                                         ":SOUR:DEL 0.01",
                                         ":STAT:OPER:ENAB 1024"
                                     };

            string[] sweep_smu_setup = {
                                           ":TRIG:OUTP SOUR",
                                           ":TRIG:INP SENS",
                                           ":TRIG:ILIN 1",
                                           ":TRIG:OLIN 2",
                                           //":SYST:RSEN ON",
                                           "*SRE 128"
                                       };

            string[] sweep_setup = {
                                       ":SOUR:FUNC VOLT",
                                       String.Format(":SOUR:VOLT:STAR {0}", startV),
                                       String.Format(":SOUR:VOLT:STOP {0}", endV),
                                       String.Format(":SOUR:VOLT:STEP {0}", stepV),
                                       ":SENS:FUNC 'CURR'",
                                       String.Format(":SENS:CURR:PROT {0}", compl),
                                       ":SENS:CURR:RANG:AUTO ON",
                                       ":SENS:CURR:NPLC 1",
                                       ":SOUR:SWE:CAB LATE",
                                       ":SOUR:SWE:DIR UP",
                                       ":ROUT:TERM FRON",
                                       String.Format(":TRIG:COUN {0}", pts),
                                       String.Format(":TRAC:POIN {0}", pts),
                                       ":TRAC:FEED:CONT NEXT",
                                       ":SOUR:VOLT:MODE SWE"
                                   };

            // open the address
            Console.WriteLine("Sending Commands to Instrument");
            rm = ResourceManager.GetLocalManager().Open(instrAddr);
            
            mbSession = (MessageBasedSession)rm;             // Cast to message-based session

            //mbSession.ServiceRequest += new MessageBasedSessionEventHandler(OnServiceRequest);
            MessageBasedSessionEventType srq = MessageBasedSessionEventType.ServiceRequest;
            mbSession.EnableEvent(srq, EventMechanism.Queue);

            int timeout = 20000;                    // milliseconds

            // Send commands and read response
            // Join my arrays together so that I only use one loop (though the exec time is the same)
            string[] cmds = new string[general_setup.Length + sweep_smu_setup.Length + sweep_setup.Length];
            general_setup.CopyTo(cmds, 0);
            sweep_smu_setup.CopyTo(cmds, general_setup.Length);
            sweep_setup.CopyTo(cmds, general_setup.Length + sweep_smu_setup.Length);

            foreach (string cmd in cmds)
            {
                mbSession.Write(cmd);
            }

            Console.WriteLine("Starting Sweep");
            mbSession.Write(":OUTP ON;:INIT");
            mbSession.WaitOnEvent(srq, timeout);
            
            Console.WriteLine("I hope the sweep is done, cause I'm tired of waiting");
            mbSession.Write(":OUTP OFF;:TRAC:FEED:CONT NEV");

            string data = mbSession.Query(":TRAC:DATA?");
            Console.WriteLine(data);
            
            // Close session
            mbSession.Dispose();

            // Convert the return string to a 2d array of doubles.
            // First, split the string by the commas
            string[] seperators = new string[] {","};
            string[] dataArray = data.Split(seperators, StringSplitOptions.None);

            // Then figure out how long the string is. I know that the Keithley outputs:
            // "Voltage, Current, Resistance, Time, Code" which then repeats.
            int dataLength = dataArray.Length;
            int numCols = 5;
            int numRows = dataLength / numCols;

            // Loop through each item in the array of strings, convert it to a double, and put it in the returnValue array.
            double[,] returnValue = new double[numRows, numCols];
            for (int i = 0; i < dataLength; i++)
            {
                double t = i / numCols;
                int row = (int)Math.Floor(t);
                int col = i % numCols;
                returnValue[row, col] = double.Parse(dataArray[i]);
            }
            

            return returnValue;
        }

    }
}
