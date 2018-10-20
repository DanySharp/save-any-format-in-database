using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace Me_save_file_DB
{
   public class ThreadClass
    {
        private static Thread MyThread;

        public void startprogress()
        {
            MyThread = new Thread(new ThreadStart(showfrm));
            MyThread.Start();
        }

        public void showfrm()
        {
            new FrmWaitin().ShowDialog();
        }

        public void threadinEnd()
        {
            MyThread.Abort();
            MyThread= null;
        }
    }
}
