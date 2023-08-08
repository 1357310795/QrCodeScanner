using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QRCodeMagic.Helpers
{
    public class MutexHelper
    {
        private const string MutexName = "##||QRCodeMagic||##";
        // declare the mutex
        private readonly Mutex _mutex;

        public MutexHelper()
        {
            _mutex = new Mutex(true, MutexName, out var createdNew);
            IsRunning = !createdNew;
        }

        public bool IsRunning { get; set; }
    }
}
