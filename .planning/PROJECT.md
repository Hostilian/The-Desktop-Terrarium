# Desktop Terrarium - God Simulator

## Project Vision

Transform the simple Desktop Terrarium into a deep God Simulator inspired by WorldBox and Caves of Qud, featuring:

- **WorldBox-inspired visual clarity**: Pixel-art top-down view with clean UI
- **Caves of Qud-style narrative depth**: Procedural lore and atmospheric descriptions
- **Cellular automata mechanics**: Faction warfare and territory conquest
- **Aggressive reproduction/expansion**: Entities spread like viruses across terrain
- **Opposing faction teams**: Elemental counters and strategic positioning
- **God painting tools**: Direct terrain manipulation with pixel-level control
- **Performance optimization**: Handle thousands of entities efficiently

## Technical Foundation

- **C# WPF Desktop Application**: Transparent window with pixel-art rendering
- **Cellular Automata Terrain System**: 9 terrain types with conquest mechanics
- **Faction-Based Simulation**: 6 opposing teams with relationship matrices
- **Procedural Lore Generation**: Markov chain-based history and naming
- **God Power System**: Creation/destruction/blessing/curse/environment/painting categories
- **Grid-Based Collision Detection**: Efficient spatial partitioning
- **Entity Reproduction Mechanics**: Swarm behavior and expansion patterns

## Success Criteria

- [ ] Clean, polished UI without "cursed" artifacts
- [ ] God powers only visible in God Simulator mode
- [ ] Proper theme-based backgrounds (no text bleeding)
- [ ] Stable WPF application (no transparency crashes)
- [ ] 152+ unit tests passing
- [ ] GSD integration for structured development
- [ ] Complete Phase 1-10 implementation roadmap

## Development Phases

1. **Phase 1-5**: Core God Simulator (Complete)
   - Visual overhaul, faction system, lore generation, god powers, terrain grid

2. **Phase 6**: Emergent Complexity
   - Advanced AI behaviors, terrain-aware entity movement

3. **Phase 7**: Polish & UI Improvements
   - Clean interface, theme separation, visual refinements

4. **Phase 8**: Technical Optimization
   - Performance improvements, spatial partitioning, entity culling

5. **Phase 9**: Content Expansion
   - Additional god powers, terrain types, faction abilities

6. **Phase 10**: QA & Finalization
   - Comprehensive testing, bug fixes, documentation

## Quality Standards

- **Code Quality**: Clean, maintainable C# with proper separation of concerns
- **Testing**: 152+ unit tests with high coverage
- **Performance**: Handle 1000+ entities at 60 FPS
- **UI/UX**: WorldBox-inspired clarity and polish
- **Lore**: Caves of Qud-style procedural depth

## Risk Mitigation

- **UI Stability**: Prevent transparency-related crashes
- **Performance**: Implement efficient algorithms for large simulations
- **Code Quality**: Regular refactoring and testing
- **Scope Control**: Use GSD for structured development planning