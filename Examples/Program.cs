// Namespace Declaration
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NationalInstruments.VisaNS;
using WaferProbeTests;

// Namespace Declaration
namespace Examples
// namespace Examples.Example1   // nested namespace
{
    // Program start class
    class MyProgram
    {
        // ``Main`` begins program execution. Entry point.
        // And here's an example of the documentation system. It uses XML. Note that it goes before the function/class def.
        /// <summary>
        /// Main extry point for Solution / Project
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// True, if authentication is successful, otherwise False.
        /// </returns>
        /// <remarks>
        /// For use with local systems
        /// </remarks>
        public static void Main()
        {
            // Write to Console
            Console.WriteLine("Welcome");
            Console.WriteLine("To write a value, use '{0}'");
            Console.WriteLine("Like so:");
            // Inserting items to a string is similar to Python's str.format() function
            Console.WriteLine("There are {0} apples.", 5);

            Console.WriteLine();
            Console.WriteLine("Starting my timer.");
            CodeTimer ct = new CodeTimer();
            ct.start();


            MyProgram mp = new MyProgram();

            Console.WriteLine("The non-static function returns {0}", mp.MyFunc());
            Console.WriteLine("The static function returns {0}", MyStaticFunc());

            // Example of Switch (case) Statement
            switch (1)
            {
                case 1:
                    Console.WriteLine("one chosen!");
                    break;      // I guess I need a break (or other control: break, continue, goto, return, throw) at the end of every case.
                case 2:
                    Console.WriteLine("you fool!");
                    break;
                default:
                    Console.WriteLine("Default");
                    break;
            }

            // for loops take care of initializtion
            // While loops check before execution
            // ForEach loops act like python's for loops. Yay!
            // Do loops check after execution - and have the check after the loop
            int m = 0;
            do
            {
                Console.WriteLine("Do loop");
                m++;
            } while (m < 3);


            // Arrays are made:
            //int[,] array2d;             // Matrix array
            //int[][] jaggedarray;        // Jagged array (array of arrays)

            SecondClass sc = new SecondClass();
            mp.MyOutFunc(out sc);


            // Needed to keep the window up, wait for user enter.
            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();
            ct.stop();
            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();

            // Start the send to the instruments
            Console.WriteLine("Sending cmds to instruments...");

            // Create the Address String and also declare some variables
            string instrAddr = "GPIB0::25::INSTR";
            MessageBasedSession mbSession = null;
            Session mySession = null;
            string responseString = null;

            // open the address
            mySession = ResourceManager.GetLocalManager().Open(instrAddr);

            // Cast to a message-based session      Not quite sure what's going on here.
            mbSession = (MessageBasedSession)mySession;

            // Send the *IDN? Command
            mbSession.Write("*IDN?\n");             // I guess I have to add the EOL token every time?

            // Read the response
            responseString = mbSession.ReadString();

            // Write it to the console
            Console.WriteLine("Response is:");
            Console.WriteLine(responseString);
            
            // Close the session
            mbSession.Dispose();


            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();


            // Here's an example of try-catch code:
            try
            {
                Console.WriteLine("Error-prone code goes in here");
            }
            // Catching exception type 1
            catch (VisaException v_exp)
            {
                Console.WriteLine("Visa caught an error!!");
                Console.WriteLine(v_exp.Message);
            }
            // Catching exception type 2, in this case a general exception
            catch (Exception exp)
            {
                Console.WriteLine("Something didn't work!!");
                Console.WriteLine(exp.Message);
                Console.WriteLine();
            }

            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();

            Console.WriteLine("Now to see if calling the DLL works...");

            // First, declare variable and create a new instance of the class/
            // At this point, it should write "Calling with 1 arg..." to the console.
            SweepVSingleSMU sweep = new SweepVSingleSMU("GPIB0::25::INSTR");

            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();

            // Run the sweep and get the result
            int exitCode;
            //double[,] result = sweep.RunSweep(out exitCode);
            double[,] result = sweep.RunSweep();
            
            //sweep.RunSweep();
            //double[,] result = sweep.GetResults();
            
            // Print the result. This is much more code than python...
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    Console.Write(result[i, j] + "\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Press Enter to Continue");
            Console.ReadLine();


        }

        // non-static methods need to have "MyProgram mp = new MyProgram();" before they're called.
        int MyFunc()
        {
            return 0;
        }

        // Static methods are kind of like Borg or Singleton in Python - there is only ever one instance.
        // OK, technically there never is an instance. According to csharp-station.com, a static member (method)
        // is used when there are no intermediate states required.
        static int MyStaticFunc()
        {
            return 1;
        }

        int MyOtherFunc()
        {
            // the python 'self' is now called 'this'
            return 2;
        }

        void MyOutFunc(out SecondClass yourname)
        {
            // Trying to make sense of the 'out' keyword...
            yourname = new SecondClass();
            yourname.name = "Billy";
            yourname.address = "Nowhere";
        }

    }

    class SecondClass
    {
        public string name;
        public string address;
    }


    // Inheritance is done with the : operator
    public class Parent
    {
        string parentString;
        // This is a constructor - it has the same name as the class. Called upon instancing.
        // Does not have return valus and always has same name as class.
        public Parent()
        {
            Console.WriteLine("Parent Constructor.");
        }
        // This is a constructor - it has the same name as the class
        public Parent(string myString)
        {
            parentString = myString;
            Console.WriteLine(parentString);
        }
        public void print()
        {
            Console.WriteLine("I'm a Parent Class");
        }
        // Destructors are the class name but with ~ in front. No parameters, no return value.
        ~Parent()
        {
            // clean up
        }
    }
    public class Child : Parent
    {
        // This is a constructor - it has the same name as the class
        public Child() : base("From Derived")
        {
            Console.WriteLine("Child Constructor.");
        }
        public new void print()
        {
            base.print();
            Console.WriteLine("I'm a Child Class");
        }
        public static void Main2()          // Rename to Main2 to avoid multiple entry points.
        {
            Child child = new Child();
            child.print();
            ((Parent)child).print();        // this is accessing the instance method (as opposed to static, class method). Not sure of the syntax yet.
            Console.ReadLine();
        }
    }
    // Structures 'structs' are similar to clusters in LabVIEW or mixed-type tuples in Python.
    // They are essentialy a class.
    struct Rectangle
    {
        // Backing Store for Width   - I guess that's like a private attribute in python?
        private int m_width;

        // Width of rectangle
        public int Width
        {
            get             // Oh look, getters and setters
            {
                return m_width;
            }
            set
            {
                m_width = value;
            }
        }

        // Backing store for Height
        private int m_height;

        public int Height
        {
            get
            {
                return m_height;
            }
            set
            {
                m_height = value;
            }
        }
    }

    // This is a reimplemtation of the built-in System.Diagnostics.StopWatch class.
    // In fact, it uses StopWatch...
    public class CodeTimer
    {
        // Class Attributes - all the variables that methods could use.
        string label;
        bool running = false;
        double? startTime = null;               // The ? makes it nullable.
        double stopTime;
        double diff;
        double prevTime;

        Stopwatch stopWatch = new Stopwatch();

        // Constructor
        public CodeTimer()
        {
            // Called upon class instancing
            Console.WriteLine("Instancing with no args");
        }

        // Another Constructor
        public CodeTimer(string lbl)            // Don't forget your types on args!
        {
            // Called upon class instaning with a label arg
            Console.WriteLine("Instancing with a label arg: {0}", lbl);
            label = lbl;
        }

        // A public function
        public void start()
        {
            // Starts the timer. For now, I'll just use a simple counter I think.
            startTime = 0;
            prevTime = (double)startTime;           // Because prevTime is double and startTime is nullable double (double?), I need the
                                                    // the typecast '(TypeToCastTo)VariableName'. I could also use 'VariableName.Value'
            stopWatch.Start();
            running = true;
        }

        public double stop()
        {
            // Stops the timer and prints out the value
            if (!running)
            {
                Console.WriteLine("Error - timer not started");
                return 0;
            }

            running = false;
            stopTime = 2;
            stopWatch.Stop();
            //diff = stopTime - startTime;
            diff = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Exec Time: {0}", diff);
            return diff;
        }
    }

}