using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace F5spy.NativeWrappers.ComWrappers
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region OCIDL.DLL

    /// <summary>
    /// MSDN reference: http://msdn.microsoft.com/en-us/library/aa768220(v=vs.85).aspx
    /// </summary>
    [ComImport]
    [Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectWithSite
    {
        void SetSite(
            [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkSite
            );

        void GetSite(
            ref Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out Object ppvSite
            );
    }

    #endregion // OCIDL.DLL

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region URLMON.DLL

    /// <summary>
    /// MSDN reference: http://msdn.microsoft.com/en-us/library/ms537130(v=vs.85).aspx
    /// </summary>
    [ComImport]
    [Guid("79EAC9EE-BAF9-11CE-8C82-00AA004BA90B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternetSecurityManager
    {
        void SetSecuritySite(
            [In] IntPtr pSite
            );

        void GetSecuritySite(
            [Out] IntPtr pSite
            );

        void MapUrlToZone(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, 
            out UInt32 pdwZone, 
            UInt32 dwFlags
            );

        void GetSecurityId(
            [MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, 
            [MarshalAs(UnmanagedType.LPArray)] byte[] pbSecurityId, 
            ref UInt32 pcbSecurityId, 
            uint dwReserved
            );

        void ProcessUrlAction(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, 
            UInt32 dwAction, 
            out byte pPolicy, 
            UInt32 cbPolicy, 
            byte pContext, 
            UInt32 cbContext, 
            UInt32 dwFlags, 
            UInt32 dwReserved
            );

        void QueryCustomPolicy(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, 
            ref Guid guidKey, 
            ref byte ppPolicy, 
            ref UInt32 pcbPolicy, 
            ref byte pContext, 
            UInt32 cbContext, 
            UInt32 dwReserved
            );

        void SetZoneMapping(
            [MarshalAs(UnmanagedType.U4)]UInt32 dwZone, 
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpszPattern, 
            [MarshalAs(UnmanagedType.U4)]UInt32 dwFlags
            );

        void GetZoneMappings(
            [MarshalAs(UnmanagedType.U4)]UInt32 dwZone, 
            out System.Runtime.InteropServices.ComTypes.IEnumString ppenumString, 
            UInt32 dwFlags);
    }

    #endregion // URLMON.DLL

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}
