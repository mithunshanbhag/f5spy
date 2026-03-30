using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace F5spy.NativeWrappers.PInvokes
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region IEFRAME.DLL

    public class IEFrame
    {
        [DllImport("ieframe.dll")]
        public static extern uint IEIsProtectedModeProcess(out bool Result);
    }

    #endregion // IEFRAME.DLL

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
