This project is an attempt to build a .NET desktop application with HTML/JS as layer for user interface.
It is an unfinished attempt for now.

### Supported Operating Systems

* Windows - implementation based on WinForms and [CefSharp](https://github.com/cefsharp/CefSharp)
* Linux - implementation based on Gtk# and webkit-sharp

See more details on [Browser Components](https://github.com/yu-kopylov/html-csharp-desktop-ui/wiki/Browser-Components) page.

### Project Structure

* **HCDU.API** - common components which can be reused in other projects.
* **HCDU.Content** - main content of the application. It will be used in both Windows and Linux versions of application.
* **HCDU.Linux.Gtk** - Linux application. It is a thin wrapper around Gtk#, webkit-sharp and HCDU.Content.
* **HCDU.Windows** - Windows application. It is a thin wrapper around WinForms, [CefSharp](https://github.com/cefsharp/CefSharp) and HCDU.Content.
