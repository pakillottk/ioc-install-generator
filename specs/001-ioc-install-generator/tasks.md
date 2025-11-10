# Tasks: IoC Install Generator

**Input**: Design documents from `/specs/001-ioc-install-generator/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included as they are critical for Source Generator validation per constitution requirements (Microsoft.CodeAnalysis.Testing for generator tests, integration tests for end-to-end validation).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths follow the structure defined in plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create solution file IoC.InstallGenerator.sln in repository root
- [x] T002 Create project structure per implementation plan (src/ and tests/ directories)
- [x] T003 [P] Create IoC.InstallGenerator.Abstractions project in src/IoC.InstallGenerator.Abstractions/
- [x] T004 [P] Create IoC.InstallGenerator project in src/IoC.InstallGenerator/
- [x] T005 [P] Create IoC.InstallGenerator.Tests project in src/IoC.InstallGenerator.Tests/
- [x] T006 [P] Create Demo.Host project in src/Demo.Host/
- [x] T007 [P] Create Demo.Modules directory structure in src/Demo.Modules/
- [x] T008 [P] Configure .NET SDK version (.NET 6.0+) in all project files
- [x] T009 [P] Add Microsoft.CodeAnalysis package references to IoC.InstallGenerator project
- [x] T010 [P] Add Microsoft.CodeAnalysis.Testing package references to IoC.InstallGenerator.Tests project
- [x] T011 [P] Add xUnit or NUnit package references to test projects
- [x] T012 [P] Configure Source Generator project as analyzer library in IoC.InstallGenerator.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T013 Create IIoCInstaller interface in src/IoC.InstallGenerator.Abstractions/IIoCInstaller.cs
- [x] T014 Create IIoCContainer interface in src/IoC.InstallGenerator.Abstractions/IIoCContainer.cs
- [x] T015 Create InstallOrderAttribute class in src/IoC.InstallGenerator.Abstractions/InstallOrderAttribute.cs
- [x] T016 Create InstallerException class in src/IoC.InstallGenerator.Abstractions/InstallerException.cs
- [x] T017 [P] Setup base project structure for Source Generator (ISourceGenerator implementation skeleton)
- [x] T018 [P] Setup test infrastructure for Source Generator tests in src/IoC.InstallGenerator.Tests/
- [x] T019 Configure Source Generator to be discoverable by consuming projects

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Descubrimiento Automático de Instaladores (Priority: P1) 🎯 MVP

**Goal**: El sistema descubre automáticamente todos los instaladores de IoC de los ensamblados referenciados mediante análisis estático en tiempo de compilación.

**Independent Test**: Crear una aplicación host con referencias a múltiples ensamblados que implementan instaladores, ejecutar el source generator, y verificar que el código generado contiene referencias a todos los instaladores encontrados.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T020 [P] [US1] Create test for discovering installers in single assembly in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T021 [P] [US1] Create test for discovering installers in multiple assemblies in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T022 [P] [US1] Create test for handling assemblies with no installers in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T023 [P] [US1] Create test for discovering multiple installers in same assembly in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T024 [P] [US1] Create test for transitive references discovery in src/IoC.InstallGenerator.Tests/GeneratorTests.cs

### Implementation for User Story 1

- [x] T025 [US1] Implement ISyntaxReceiver to collect syntax nodes in src/IoC.InstallGenerator/SyntaxReceiver.cs
- [x] T026 [US1] Implement reference analysis logic using Compilation.References in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T027 [US1] Implement installer discovery logic to find classes implementing IIoCInstaller in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T028 [US1] Implement transitive reference analysis with 3-level depth limit in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T029 [US1] Implement handling for assemblies with no installers (skip without error) in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T030 [US1] Implement error handling and clear error messages for analysis failures in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T031 [US1] Add validation for public, instanciable installer classes in src/IoC.InstallGenerator/IoCInstallGenerator.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently - the generator can discover all installers from referenced assemblies.

---

## Phase 4: User Story 2 - Carga de Instaladores en Contenedor IoC (Priority: P2)

**Goal**: El sistema genera código estático que carga automáticamente todos los instaladores descubiertos en el contenedor IoC durante la inicialización, con manejo de errores resiliente.

**Independent Test**: Crear un contenedor IoC, llamar al código generado que carga los instaladores, y verificar que todas las dependencias esperadas están registradas en el contenedor. Verificar que errores en un instalador no detienen la carga de otros.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T032 [P] [US2] Create test for generated code structure and LoadAll method signature in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T033 [P] [US2] Create test for installer execution order in generated code in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T034 [P] [US2] Create integration test for loading installers into container in src/IoC.InstallGenerator.Tests/InstallerLoadingTests.cs
- [x] T035 [P] [US2] Create test for error handling when installer fails in src/IoC.InstallGenerator.Tests/InstallerLoadingTests.cs
- [x] T036 [P] [US2] Create test for AggregateException when multiple installers fail in src/IoC.InstallGenerator.Tests/InstallerLoadingTests.cs

### Implementation for User Story 2

- [x] T037 [US2] Implement code generation logic for IoCInstallerLoader class in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T038 [US2] Implement LoadAll method generation with container parameter in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T039 [US2] Implement installer instantiation and Install() call generation in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T040 [US2] Implement try-catch error handling for each installer in generated code in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T041 [US2] Implement AggregateException generation when errors occur in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T042 [US2] Implement InstallerException wrapping for failed installers in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T043 [US2] Implement code formatting for readability and debuggability in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T044 [US2] Implement InstallOrderAttribute reading and installer ordering logic in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T045 [US2] Integrate code generation with installer discovery in src/IoC.InstallGenerator/IoCInstallGenerator.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - the generator discovers installers and generates code that loads them into a container.

---

## Phase 5: User Story 3 - Mejora de Rendimiento sobre Reflexión (Priority: P3)

**Goal**: Validar que el sistema es más eficiente que soluciones basadas en reflexión, cumpliendo objetivos de rendimiento (<100ms para 50 ensamblados, 50% más rápido que reflexión).

**Independent Test**: Comparar el tiempo de inicialización del sistema con source generators versus una implementación equivalente usando reflexión, ejecutando benchmarks con múltiples ensamblados. Verificar que el código generado no contiene llamadas a reflexión.

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T046 [P] [US3] Create benchmark test comparing source generator vs reflection approach in src/IoC.InstallGenerator.Tests/PerformanceTests.cs
- [x] T047 [P] [US3] Create test to verify generated code contains no reflection calls in src/IoC.InstallGenerator.Tests/GeneratorTests.cs
- [x] T048 [P] [US3] Create test for initialization time with 50 assemblies in src/IoC.InstallGenerator.Tests/PerformanceTests.cs
- [x] T049 [P] [US3] Create test for 3-level transitive reference performance in src/IoC.InstallGenerator.Tests/PerformanceTests.cs

### Implementation for User Story 3

- [x] T050 [US3] Implement performance optimization in code generation (direct method calls) in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T051 [US3] Optimize reference analysis to avoid duplicate processing in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T052 [US3] Implement caching for discovered installers during generation in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T053 [US3] Verify generated code is AOT and trimming compatible in src/IoC.InstallGenerator/CodeGenerator.cs
- [x] T054 [US3] Create reflection-based implementation for benchmark comparison in src/IoC.InstallGenerator.Tests/PerformanceTests.cs

**Checkpoint**: All user stories should now be independently functional and performance validated.

---

## Phase 6: Demo & Integration

**Purpose**: Create demonstration projects to validate the complete system

- [x] T055 [P] Create Demo.Modules.ModuleA project with ModuleAInstaller in src/Demo.Modules/ModuleA/
- [x] T056 [P] Create Demo.Modules.ModuleB project with ModuleBInstaller in src/Demo.Modules/ModuleB/
- [x] T057 [P] Create Demo.Modules.ModuleC project with ModuleCInstaller in src/Demo.Modules/ModuleC/
- [x] T058 Create Demo.Host project with references to demo modules in src/Demo.Host/
- [x] T059 Implement container adapter example (e.g., AutofacAdapter) in src/Demo.Host/
- [x] T060 Implement Program.cs with IoCInstallerLoader usage in src/Demo.Host/Program.cs
- [x] T061 Add InstallOrderAttribute example to Demo.Host in src/Demo.Host/Program.cs
- [x] T062 Create integration test with demo modules in src/IoC.InstallGenerator.Tests/InstallerLoadingTests.cs

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T063 [P] Add XML documentation comments to all public APIs in src/IoC.InstallGenerator.Abstractions/
- [x] T064 [P] Add XML documentation comments to Source Generator classes in src/IoC.InstallGenerator/
- [x] T065 [P] Code cleanup and refactoring across all projects
- [x] T066 [P] Add comprehensive error messages with diagnostic information in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T067 [P] Validate quickstart.md examples work correctly
- [x] T074 Validate 10-minute integration time criterion (SC-006) by creating test project from scratch and measuring integration time
- [x] T068 [P] Add edge case handling for corrupted assemblies in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T069 [P] Add edge case handling for multiple interface versions in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T070 [P] Add edge case handling for circular dependencies in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T073 [P] Add edge case handling for assemblies unavailable at compile-time in src/IoC.InstallGenerator/IoCInstallGenerator.cs
- [x] T071 Create README.md with usage examples in repository root
- [x] T072 Validate all acceptance scenarios from spec.md work correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Demo & Integration (Phase 6)**: Depends on User Stories 1 and 2 completion
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on User Story 1 completion (needs installer discovery to generate code)
- **User Story 3 (P3)**: Can start after User Story 2 completion (needs working system to benchmark)

### Within Each User Story

- Tests (included) MUST be written and FAIL before implementation
- Core discovery logic before code generation
- Code generation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- All tests for a user story marked [P] can run in parallel
- Demo module creation tasks can run in parallel
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Create test for discovering installers in single assembly in src/IoC.InstallGenerator.Tests/GeneratorTests.cs"
Task: "Create test for discovering installers in multiple assemblies in src/IoC.InstallGenerator.Tests/GeneratorTests.cs"
Task: "Create test for handling assemblies with no installers in src/IoC.InstallGenerator.Tests/GeneratorTests.cs"
Task: "Create test for discovering multiple installers in same assembly in src/IoC.InstallGenerator.Tests/GeneratorTests.cs"
Task: "Create test for transitive references discovery in src/IoC.InstallGenerator.Tests/GeneratorTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently - verify installer discovery works
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP - installer discovery!)
3. Add User Story 2 → Test independently → Deploy/Demo (Complete system!)
4. Add User Story 3 → Test independently → Deploy/Demo (Performance validated!)
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (discovery)
   - Developer B: Prepares demo modules
3. Once User Story 1 is done:
   - Developer A: User Story 2 (code generation)
   - Developer B: User Story 3 (performance tests)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Source Generator tests use Microsoft.CodeAnalysis.Testing framework
- Integration tests validate end-to-end functionality with real projects
- Performance tests compare against reflection-based implementation
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

