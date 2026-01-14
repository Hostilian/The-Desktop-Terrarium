# Desktop Terrarium - Coding Conventions

## Language & Framework Standards

### **C# Version & Features**
- **Target**: C# 12.0 with .NET 8.0
- **Nullable References**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled for modern syntax
- **File-Scoped Namespaces**: Preferred for brevity

### **Naming Conventions**

#### **Classes & Types**
- **PascalCase**: `SimulationEngine`, `WorldEntity`, `FactionManager`
- **Descriptive Names**: `MovementCalculator`, `CollisionDetector`
- **Suffix Patterns**:
  - Managers: `XXXManager` (e.g., `FoodManager`)
  - Systems: `XXXSystem` (e.g., `EventSystem`)
  - Calculators: `XXXCalculator` (e.g., `MovementCalculator`)
  - Detectors: `XXXDetector` (e.g., `CollisionDetector`)

#### **Methods**
- **PascalCase**: `Update()`, `Initialize()`, `SpawnEntity()`
- **Verb-Noun Pattern**: `CalculateMovement()`, `DetectCollisions()`
- **Boolean Methods**: `IsValid()`, `CanMove()`, `ShouldReproduce()`

#### **Properties**
- **PascalCase**: `Speed`, `Hunger`, `FactionId`
- **Auto-Implemented**: Preferred when no custom logic
- **Backing Fields**: `_speed`, `_hunger` (camelCase with underscore)

#### **Fields**
- **camelCase with underscore**: `_simulationEngine`, `_renderTimer`
- **Private**: All fields are private by default
- **Readonly**: Use `readonly` for immutable references

#### **Constants**
- **PascalCase**: `MaxHunger`, `RenderFps`, `LogicTickRate`
- **Descriptive**: Include units when applicable

#### **Variables**
- **camelCase**: `entity`, `position`, `deltaTime`
- **Descriptive**: `currentFaction`, `targetPosition`
- **Single Letter**: Only for loop counters (`i`, `j`, `k`)

### **File Organization**

#### **Directory Structure**
```
Terrarium.Logic/
├── Entities/          # Game objects
├── Interfaces/        # Contracts
├── Persistence/       # Save/load system
└── Simulation/        # Core logic

Terrarium.Desktop/
├── Rendering/         # Graphics & UI
└── [MainWindow files] # UI logic

Terrarium.Tests/
├── Entities/          # Entity tests
├── Persistence/       # Persistence tests
├── Rendering/         # UI tests
└── Simulation/        # Logic tests
```

#### **File Naming**
- **Class Files**: `ClassName.cs` (e.g., `SimulationEngine.cs`)
- **Partial Classes**: `MainWindow.xaml.cs`, `MainWindow.Interaction.cs`
- **Test Files**: `ClassNameTests.cs` (e.g., `SaveManagerTests.cs`)

### **Code Style**

#### **Braces & Formatting**
- **Allman Style**: Opening brace on new line
- **Consistent Indentation**: 4 spaces (or IDE default)
- **Line Length**: Aim for 100-120 characters
- **Empty Lines**: Separate logical blocks

#### **Access Modifiers**
- **Explicit**: Always specify `public`, `private`, `protected`
- **Private by Default**: Fields and methods start private
- **Interface Members**: Implicitly public

#### **Method Structure**
```csharp
/// <summary>
/// Descriptive summary of what the method does.
/// </summary>
public void MethodName(ParameterType parameterName)
{
    // Input validation
    if (parameterName == null)
        throw new ArgumentNullException(nameof(parameterName));

    // Early returns for edge cases
    if (condition)
        return;

    // Main logic
    // ...

    // Return result or void
}
```

#### **Property Structure**
```csharp
/// <summary>
/// Description of the property.
/// </summary>
public PropertyType PropertyName
{
    get => _propertyName;
    set => _propertyName = ValidateValue(value);
}
```

### **Documentation Standards**

#### **XML Comments**
- **All Public Members**: Classes, methods, properties
- **Summary Tags**: `<summary>` for descriptions
- **Parameter Tags**: `<param name="name">description</param>`
- **Return Tags**: `<returns>description</returns>`
- **Exception Tags**: `<exception cref="ExceptionType">description</exception>`

#### **Comment Style**
- **Single Line**: `// Explanation of complex logic`
- **Multi-Line**: For longer explanations
- **TODO Comments**: `// TODO: Implement feature X`
- **HACK/WARNING**: For temporary solutions

### **Error Handling**

#### **Exception Patterns**
- **Argument Validation**: `ArgumentNullException`, `ArgumentOutOfRangeException`
- **Custom Exceptions**: For domain-specific errors
- **Try-Catch**: Only where recovery is possible
- **Logging**: Use `Console.WriteLine()` for debugging

#### **Validation**
- **Guard Clauses**: Early returns for invalid inputs
- **Null Checks**: Before dereferencing
- **Range Checks**: For numeric parameters
- **State Validation**: Ensure object is in valid state

### **Testing Conventions**

#### **Test Structure**
- **Arrange-Act-Assert**: Standard testing pattern
- **Descriptive Names**: `MethodName_Scenario_ExpectedResult`
- **Test Categories**: Factories, Integration, Performance

#### **Test Data**
- **Constants**: For test values
- **Builder Pattern**: For complex object creation
- **Randomization**: For fuzz testing

### **Performance Guidelines**

#### **Collections**
- **List<T>**: For dynamic arrays
- **Dictionary<TKey,TValue>**: For key-value lookups
- **HashSet<T>**: For set operations
- **Array**: For fixed-size collections

#### **Memory Management**
- **Object Reuse**: Pool common objects
- **Disposable Pattern**: For unmanaged resources
- **Weak References**: For cache implementations

#### **Threading**
- **UI Thread**: All WPF operations
- **Background Tasks**: For heavy computations
- **Synchronization**: `Dispatcher.Invoke()` for thread safety

### **Design Principles**

#### **SOLID Principles**
- **Single Responsibility**: Each class has one purpose
- **Open/Closed**: Extensible without modification
- **Liskov Substitution**: Subtypes are substitutable
- **Interface Segregation**: Client-specific interfaces
- **Dependency Inversion**: Depend on abstractions

#### **DRY (Don't Repeat Yourself)**
- **Common Code**: Extract to methods/utilities
- **Constants**: Define once, reference everywhere
- **Base Classes**: For shared functionality

#### **Clean Code**
- **Descriptive Names**: Self-documenting code
- **Small Methods**: Single responsibility
- **Few Parameters**: Maximum 3-4 parameters
- **Early Returns**: Simplify conditional logic

### **Git & Version Control**

#### **Commit Messages**
- **Format**: `Type: Description` (e.g., `feat: Add faction system`)
- **Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

#### **Branching**
- **Feature Branches**: `feature/xxx-description`
- **Bug Fixes**: `fix/xxx-description`
- **Main Branch**: Protected, only merges

### **Build & Deployment**

#### **Configuration**
- **Debug**: Development builds
- **Release**: Production builds
- **Conditional Compilation**: Platform-specific code

#### **Dependencies**
- **NuGet Packages**: Version pinning
- **Project References**: Internal dependencies
- **Target Frameworks**: Explicit specification</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\CONVENTIONS.md
