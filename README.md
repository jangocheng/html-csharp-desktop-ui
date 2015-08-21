This project is an attempt to build a cross-platform .NET desktop application with HTML/JS as a layer for the user interface.

It is an unfinished attempt for now.


### Supported Operating Systems

* Windows - implementation based on WinForms and [CefSharp](https://github.com/cefsharp/CefSharp)
* Linux - implementation based on Gtk# and webkit-sharp

See more details on [Browser Components](https://github.com/yu-kopylov/html-csharp-desktop-ui/wiki/Browser-Components) page.

### Project Structure

* **HCDU.API** - common components for creation of HTML/C# applications which can be reused in other projects.
* **HCDU.Content** - main content of the application. It will be used in both Windows and Linux versions of application.
* **HCDU.Linux.Gtk** - Linux application. It is a thin wrapper around Gtk#, webkit-sharp and HCDU.Content.
* **HCDU.Web.Api** - general purpose classes and interfaces for processing of HTTP and WebSocket requests.
* **HCDU.Web.Server** - a generic HTTP web-server implementation with WebSocket support.
* **HCDU.Windows** - Windows application. It is a thin wrapper around WinForms, [CefSharp](https://github.com/cefsharp/CefSharp) and HCDU.Content.
