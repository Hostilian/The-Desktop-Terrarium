# Desktop Terrarium - Technology Stack

## Core Technologies

### **Primary Language & Framework**
- **C# 12.0** - Modern C# with nullable reference types enabled
- **.NET 8.0** - Latest LTS version with Windows-specific features
- **Windows Presentation Foundation (WPF)** - Desktop UI framework

### **Project Structure**
- **Terrarium.Logic** - Core business logic (net8.0, cross-platform)
- **Terrarium.Desktop** - WPF UI application (net8.0-windows)
- **Terrarium.Tests** - Unit tests (net8.0-windows, MSTest framework)

### **Testing & Quality**
- **MSTest Framework** - Microsoft testing framework
- **Coverlet** - Code coverage analysis
- **152 Unit Tests** - Comprehensive test suite

### **Key Libraries & Dependencies**
- **System.Windows** - WPF UI components
- **System.Windows.Media** - Graphics and rendering
- **System.Windows.Controls** - UI controls
- **System.Windows.Threading** - UI threading
- **System.IO** - File operations
- **System.Linq** - LINQ queries
- **System.Diagnostics** - Performance monitoring

### **Development Tools**
- **Visual Studio 2022** - Primary IDE
- **.NET SDK 8.0** - Build and runtime
- **MSBuild** - Build system
- **NuGet** - Package management

### **Architecture Patterns**
- **MVVM** - Model-View-ViewModel (UI separation)
- **Observer Pattern** - Event system
- **Factory Pattern** - Entity creation
- **Strategy Pattern** - God power behaviors
- **Singleton Pattern** - Manager classes

### **Performance Considerations**
- **60 FPS Rendering** - Smooth animation target
- **DispatcherTimer** - UI thread synchronization
- **Stopwatch** - Frame rate monitoring
- **Efficient Collections** - List<T>, Dictionary<TKey,TValue>

### **Cross-Platform Compatibility**
- **Logic Layer**: Fully cross-platform (net8.0)
- **UI Layer**: Windows-specific (net8.0-windows)
- **Test Layer**: Windows-specific for WPF testing

### **Build Configuration**
- **Debug/Release** - Standard configurations
- **Any CPU** - Platform target
- **Implicit Usings** - Modern C# features
- **Nullable References** - Type safety

### **File Organization**
- **.cs** - C# source files
- **.xaml** - WPF UI markup
- **.csproj** - Project files
- **.sln** - Solution file
- **.md** - Documentation
- **.json** - Configuration files</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\STACK.md
