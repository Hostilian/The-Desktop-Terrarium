# The Desktop Terrarium - Architecture Diagram

## 📐 System Architecture Overview

```mermaid
graph TD
    subgraph User Desktop
        Desktop[User Desktop Screen]
        OtherApps[Other Applications]
        Terrarium[🌿 The Desktop Terrarium]
    end

    Desktop --> OtherApps
    Desktop --> Terrarium
    
    style Terrarium fill:#2ECC71,stroke:#27AE60,stroke-width:2px,color:white
```

---

## 🏛️ Layered Architecture

```mermaid
graph TD
    subgraph Presentation Layer [Terrarium.Desktop - WPF]
        MainWindow[MainWindow]
        Renderer[Renderer]
        SysMon[SystemMonitor]
        
        MainWindow -->|Delegates Rendering| Renderer
        MainWindow -->|Reads Stats| SysMon
    end

    subgraph Logic Layer [Terrarium.Logic - .NET Standard]
        SimEngine[SimulationEngine]
        World[World]
        Entities[Entities]
        Managers[Sub-Managers]
        
        SimEngine -->|Orchestrates| World
        SimEngine -->|Updates| Entities
        SimEngine -->|Uses| Managers
        
        World -->|Contains| Entities
    end

    MainWindow -->|Calls Update()| SimEngine
    Renderer -->|Reads State| World

    style MainWindow fill:#3498DB,stroke:#2980B9,color:white
    style SimEngine fill:#E67E22,stroke:#D35400,color:white
```

---

## 🌳 Entity Inheritance Tree

```mermaid
classDiagram
    class WorldEntity {
        +double X
        +double Y
        +int Id
        +DistanceTo()
    }
    
    class LivingEntity {
        +double Health
        +double Age
        +bool IsAlive
        +TakeDamage()
    }
    
    class Plant {
        +double Size
        +double GrowthRate
        +Grow()
    }
    
    class Creature {
        +double Speed
        +double Hunger
        +Move()
        +Eat()
    }
    
    class Herbivore {
        +Flee()
    }
    
    class Carnivore {
        +Hunt()
    }

    WorldEntity <|-- LivingEntity
    LivingEntity <|-- Plant
    LivingEntity <|-- Creature
    Creature <|-- Herbivore
    Creature <|-- Carnivore
    
    style WorldEntity fill:#f9f9f9,stroke:#333
    style LivingEntity fill:#e1f5fe,stroke:#0277bd
    style Plant fill:#e8f5e9,stroke:#2e7d32
    style Creature fill:#fff3e0,stroke:#ef6c00
```

---

## 🔄 Simulation Loop (Sequence)

```mermaid
sequenceDiagram
    participant UI as MainWindow (WPF)
    participant Engine as SimulationEngine
    participant World as World
    participant System as Sub-Systems

    loop Every 200ms (Logic Tick)
        UI->>Engine: Update(deltaTime)
        Engine->>System: DayNightCycle.Update()
        Engine->>System: DiseaseManager.Update()
        Engine->>World: Get All Entities
        
        par Parallel Updates
            Engine->>World: Update Plants
            Engine->>World: Update Creatures
        end
        
        Engine-->>UI: Simulation State Updated
    end

    loop Every 16ms (Render Tick)
        UI->>Engine: World State Access
        UI->>UI: Draw Entities (Canvas)
    end
```
