using BarRaider.SdTools;
using System;

namespace WebFlow
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            SDWrapper.Run(args);
        }
    }
}
