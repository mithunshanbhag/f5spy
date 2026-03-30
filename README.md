# F5spy

> 🔍 A legacy Internet Explorer Browser Helper Object (BHO) for tracing URL security zones and Protected Mode state while browsing or debugging.

## 🚨 Status

This repository contains an old, diagnostic-only Internet Explorer extension.

- 🧭 It is meant for **Windows + Internet Explorer** environments.
- 🧪 The original note says it is **not built in nightly test builds**.
- 🏚️ It is mainly useful for investigating legacy IE behavior on older systems.

## ✨ What it does

`F5spy` loads into Internet Explorer as a Browser Helper Object and writes debug trace lines that show:

- 🛡️ Whether IE is running in **Protected Mode**
- 🌐 Which **security zone** a URL maps to
- 🧾 The browser event that triggered the log entry

At the moment, the implementation subscribes to several IE browser events, but the actual logging happens on **`NavigateComplete2`**.

Example output:

```text
[IEspy PM=True] event=NavigateComplete2, zone=Internet, url=http://example.com/
```

## 🧱 Repository layout

### `IEspy/`

The main BHO implementation lives here.

- 🧩 `IEspy.cs` implements the COM-visible BHO class
- 🪟 Registers the add-on under IE's **Browser Helper Objects** registry key
- 🔔 Hooks IE events through `SHDocVw`
- 📝 Emits debug output with `System.Diagnostics.Debug.WriteLine`

### `F5spy.NativeWrappers/`

Thin interop helpers used by the BHO.

- 🔌 `ComWrappers.cs` defines COM interfaces such as `IObjectWithSite` and `IInternetSecurityManager`
- ⚙️ `PInvokes.cs` imports `IEIsProtectedModeProcess` from `ieframe.dll`

### `F5spy.sln`

- 🛠️ Visual Studio solution containing both C# projects
- 📦 Solution format targets **Visual Studio 2013**, while the projects target **.NET Framework 4.8**

## 🧰 Requirements

To build and use this project, you will need:

- 🪟 Windows
- 🌐 Internet Explorer installed and available
- 🧑‍💻 Visual Studio / MSBuild with **.NET Framework 4.8**
- 🔐 Administrator rights for COM registration
- 🔎 A debugger or **Sysinternals DebugView** to read trace output

## 🚀 Build and register

1. 🔓 Start Visual Studio **as Administrator**.
2. 📂 Open `F5spy.sln`.
3. 🏗️ Build the solution.
4. 🧾 The `IEspy` project runs this post-build step:

   ```text
   regasm.exe <assembly> /codebase /register
   ```

5. ✅ After registration, enable the extension in Internet Explorer:

   `Tools -> Manage add-ons`

## 👀 Viewing the logs

You can inspect trace output in either of these ways:

- 🧪 Attach a native debugger such as **NTSD**, **CDB**, or **WinDbg** to IE
- 📺 Use **Sysinternals DebugView**

If you use DebugView, set the filter to:

```text
Include = IEspy
```

## 🔍 How it works

The BHO combines two IE/Windows APIs:

- 🌐 `IInternetSecurityManager.MapUrlToZone(...)` to resolve the URL security zone
- 🛡️ `IEIsProtectedModeProcess(...)` from `ieframe.dll` to read Protected Mode status

Those values are combined into a single debug trace message for each logged navigation completion event.

## ⚠️ Notes and limitations

- 🕰️ This is a **legacy IE-only** diagnostic tool, not a modern browser extension.
- 🪦 It will not be useful on systems where Internet Explorer is unavailable or disabled.
- 🧱 It has **no UI**; all output is written to the debugger stream.
- 🔐 Registration writes under `HKLM`, so elevated privileges are required.

## 🧹 Unregistering

To unregister the BHO manually, run:

```text
regasm.exe F5spy.IEspy.dll /unregister
```

## 📜 License

Licensed under the MIT License. See `LICENSE`.
