using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using SHDocVw;
using mshtml;
using Microsoft.Win32;
using System.Windows;
using F5spy.NativeWrappers.ComWrappers;
using F5spy.NativeWrappers.PInvokes;


/* This BHO writes debugger traces that can be viewed either via sysintnerals' DebugView tool 
 *  - http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx.  
 * 
 * Notes:
 *  - To eliminate verbosity, please set debugview's filter to "Include = IEspy".
 *  - Please run VS elevated while building this project (To register this BHO you'll need to run regasm elevated. There 
 *    is already a post-build event in this project's setting that invokes regasm).
 * 
 * The trace can also be view via a native debugger (cdb/ntsd/windbg) attached to internet-explorer.
 */

namespace F5spy.IEspy
{
    [ComVisible(true)]
    [Guid("F5C6597E-88A0-40DC-AE67-79E22F9EDAC2")]
    [ClassInterface(ClassInterfaceType.None)]
    public class IESpyBHO : IObjectWithSite
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Private fields and properties


        enum WebBrowserEventEnum
        {
            DocumentComplete,
            DownloadBegin,
            DownloadComplete,
            BeforeNavigate2,
            NavigateComplete2,
            NavigateError,
            OnQuit,
            NewProcess,
            NewWindow3,
            WindowClosing,
            SetSite,
            GetSite
        }

        enum URLSecurityZoneIndexEnum
        {
            Unknown = -1,
            LocalIntranet = 1,
            TrustedSite = 2,
            Internet = 3,
            RestrictedSite = 4
        }

        //
        private InternetExplorer _ieInstance;

        // To register a BHO, a new key should be created under this key.
        private const string _strBHORegKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects";

        //
        private bool _isBHOInitialized = false;

        //
        private IInternetSecurityManager _internetSecurityManager = null;


        #endregion // Private fields and properties

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Com Register/UnRegister Methods
        /// <summary>
        /// When this class is registered to COM, add a new key to the _strBHORegKey 
        /// to make IE use this BHO.
        /// On 64bit machine, if the platform of this assembly and the installer is x86,
        /// 32 bit IE can use this BHO. If the platform of this assembly and the installer
        /// is x64, 64 bit IE can use this BHO.
        /// </summary>
        [ComRegisterFunction]
        public static void RegisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(_strBHORegKey, true);
            if (key == null)
            {
                key = Registry.LocalMachine.CreateSubKey(_strBHORegKey);
            }

            // 32 digits separated by hyphens, enclosed in braces: 
            // {00000000-0000-0000-0000-000000000000}
            string bhoKeyStr = t.GUID.ToString("B");

            RegistryKey bhoKey = key.OpenSubKey(bhoKeyStr, true);

            // Create a new key.
            if (bhoKey == null)
            {
                bhoKey = key.CreateSubKey(bhoKeyStr);
            }

            // NoExplorer:dword = 1 prevents the BHO to be loaded by Explorer
            string name = "NoExplorer";
            object value = (object)1;
            bhoKey.SetValue(name, value);
            key.Close();
            bhoKey.Close();
        }

        /// <summary>
        /// When this class is unregistered from COM, delete the key.
        /// </summary>
        [ComUnregisterFunction]
        public static void UnregisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(_strBHORegKey, true);
            string guidString = t.GUID.ToString("B");
            if (key != null)
            {
                key.DeleteSubKey(guidString, false);
            }
        }
        #endregion // Com Register/UnRegister Methods

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public IESpyBHO()
        {
            if (null == _internetSecurityManager)
            {
                Type t = Type.GetTypeFromCLSID(new Guid("7b8a2d94-0ac9-11d1-896c-00c04fb6bfc4"));
                _internetSecurityManager = (IInternetSecurityManager)Activator.CreateInstance(t);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Utility methods

        private void WriteLog(string url, WebBrowserEventEnum evt)
        {
            //Debug.Assert(!string.IsNullOrEmpty(url));

            lock (this)
            {
                uint zone;

                _internetSecurityManager.MapUrlToZone(url, out zone, 0);
                Debug.Assert(zone > 0);

                string strZone = ((URLSecurityZoneIndexEnum)Enum.ToObject(typeof(URLSecurityZoneIndexEnum), zone)).ToString();

                bool isProtectedMode = false;

                uint ret = F5spy.NativeWrappers.PInvokes.IEFrame.IEIsProtectedModeProcess(out isProtectedMode);
                Debug.Assert(0 == ret);

                //string output = string.Format("[IEspy PM={0}, IL={1}] event={2}, zone={3}, url={4}",
                string output = string.Format("[IEspy PM={0}] event={1}, zone={2}, url={3}",
                                                isProtectedMode.ToString(),
                                                //"???",
                                                evt.ToString(),
                                                strZone,
                                                url);

                Debug.WriteLine(output);
            }
        }

        #endregion //Utility methods

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Browser-related callbacks

        /// <summary>
        /// This method is called when the BHO is instantiated and when it is destroyed. This method receives the site's
        /// IUnknown pointer. A typical implementation will simply stash away the pointer for further use.
        /// Reference: http://msdn.microsoft.com/en-us/library/aa768221(v=vs.85).aspx
        /// </summary>
        /// <param name="pUnkSite"></param>
        public void SetSite(Object site)
        {
            Debug.Assert(null != site);
            if (null != site)
            {
                _ieInstance = (InternetExplorer) site;
                Debug.Assert(null != _ieInstance);

                _ieInstance.DocumentComplete += _ieInstance_DocumentComplete;
                _ieInstance.DownloadBegin += _ieInstance_DownloadBegin;
                _ieInstance.DownloadComplete += _ieInstance_DownloadComplete;
                _ieInstance.BeforeNavigate2 += _ieInstance_BeforeNavigate2;
                _ieInstance.NavigateComplete2 += _ieInstance_NavigateComplete2;
                _ieInstance.NavigateError += _ieInstance_NavigateError;
                _ieInstance.OnQuit += _ieInstance_OnQuit;
                _ieInstance.NewProcess += _ieInstance_NewProcess;
                _ieInstance.NewWindow3 += _ieInstance_NewWindow3;
                _ieInstance.WindowClosing += _ieInstance_WindowClosing;

                _isBHOInitialized = true;
            }
        }

        /// <summary>
        /// This method is responsible for retrieving and returning the specified interface from the last site set 
        /// via the SetSite() method. A typical implementation will query the previously stored IUnknown pointer for the
        /// specified interface.
        /// Reference: http://msdn.microsoft.com/en-us/library/aa768219(v=vs.85).aspx
        /// </summary>
        /// <param name="riid"></param>
        /// <param name="ppvSite"></param>
        public void GetSite(ref Guid riid, out Object ppvSite)
        {
            Debug.Assert(null != riid);

            IntPtr punk = Marshal.GetIUnknownForObject(_ieInstance);
            ppvSite = new object();
            IntPtr ppvSiteIntPtr = Marshal.GetIUnknownForObject(ppvSite);

            int hr = Marshal.QueryInterface(punk, ref riid, out ppvSiteIntPtr);
            Marshal.ThrowExceptionForHR(hr);
            Marshal.Release(punk);
            Marshal.Release(ppvSiteIntPtr);
        }


        void _ieInstance_WindowClosing(bool IsChildWindow, ref bool Cancel)
        {
        }

        void _ieInstance_NewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
        }

        void _ieInstance_NewProcess(int lCauseFlag, object pWB2, ref bool Cancel)
        {
        }

        void _ieInstance_OnQuit()
        {
        }


        void _ieInstance_NavigateError(object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel)
        {
        }

        void _ieInstance_NavigateComplete2(object pDisp, ref object URL)
        {
            WriteLog(URL.ToString(), WebBrowserEventEnum.NavigateComplete2);
        }

        void _ieInstance_DownloadComplete()
        {
        }

        void _ieInstance_DownloadBegin()
        {
        }

        void _ieInstance_BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
        }

        void _ieInstance_DocumentComplete(object pDisp, ref object URL)
        {
        }


        #endregion // Browser-related callbacks

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

}
