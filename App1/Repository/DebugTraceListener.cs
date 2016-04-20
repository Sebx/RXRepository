using SQLite.Net;
using System.Diagnostics;

namespace App1.Repository
{
    class DebugTraceListener : ITraceListener
    {
        public void Receive(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
