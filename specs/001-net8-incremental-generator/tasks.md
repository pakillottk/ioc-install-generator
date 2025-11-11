# Tasks: Actualización a .NET 8 e Incremental Source Generator

**Input**: Design documents from `/specs/001-net8-incremental-generator/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included as they are critical for Source Generator validation per constitution requirements (Microsoft.CodeAnalysis.Testing for generator tests, integration tests for end-to-end validation).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single solution**: `src/` at repository root
- Paths follow the structure defined in plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify prerequisites and prepare for migration

- [X] T001 Verify .NET 8 SDK is installed and accessible via `dotnet --version`
- [X] T002 [P] Verify solution structure exists: `IoC.InstallGenerator.sln`
- [X] T003 [P] Create backup branch from current master/main branch
- [X] T004 Verify all existing projects compile successfully before migration

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Update solution file to ensure all projects are included in `IoC.InstallGenerator.sln`
- [X] T006 [P] Verify project references are correct between projects
- [X] T007 [P] Document current state: list all .csproj files and their current TargetFramework values

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Actualizar todos los proyectos a .NET 8 (Priority: P1) 🎯 MVP

**Goal**: Todos los proyectos del repositorio actualizados a .NET 8 y compilando exitosamente

**Independent Test**: Ejecutar `dotnet build` en todos los proyectos y confirmar que compilan exitosamente con .NET 8. Los tests unitarios deben ejecutarse sin errores.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T008 [P] [US1] Create test to verify TargetFramework is net8.0 in `src/IoC.InstallGenerator.Tests/ProjectVersionTests.cs`
- [ ] T009 [P] [US1] Create integration test to verify all projects compile in `src/IoC.InstallGenerator.Tests/CompilationTests.cs`

### Implementation for User Story 1

- [X] T010 [P] [US1] Update TargetFramework to net8.0 in `src/IoC.InstallGenerator/IoC.InstallGenerator.csproj`
- [X] T011 [P] [US1] Update TargetFramework to net8.0 in `src/IoC.InstallGenerator.Abstractions/IoC.InstallGenerator.Abstractions.csproj`
- [X] T012 [P] [US1] Update TargetFramework to net8.0 in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T013 [P] [US1] Update TargetFramework to net8.0 in `src/Demo.Host/Demo.Host.csproj`
- [X] T014 [P] [US1] Update TargetFramework to net8.0 in `src/Demo.Modules/ModuleA/ModuleA.csproj`
- [X] T015 [P] [US1] Update TargetFramework to net8.0 in `src/Demo.Modules/ModuleB/ModuleB.csproj`
- [X] T016 [P] [US1] Update TargetFramework to net8.0 in `src/Demo.Modules/ModuleC/ModuleC.csproj`
- [X] T017 [US1] Verify all projects compile: run `dotnet build` from solution root
- [X] T018 [US1] Verify all tests pass: run `dotnet test` from solution root

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently - all projects compile with .NET 8

---

## Phase 4: User Story 2 - Convertir Source Generator a Incremental (Priority: P2)

**Goal**: El source generator usa IIncrementalGenerator y genera código idéntico al anterior

**Independent Test**: Verificar que el generador funciona correctamente generando el mismo código que antes, pero usando la API incremental. Los tiempos de compilación deben mantenerse o mejorar.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T019 [P] [US2] Create test to verify incremental generator generates identical code in `src/IoC.InstallGenerator.Tests/IncrementalGeneratorTests.cs`
- [ ] T020 [P] [US2] Create test to verify caching works in incremental generator in `src/IoC.InstallGenerator.Tests/IncrementalCachingTests.cs`
- [ ] T021 [P] [US2] Create integration test to verify all installers discovered correctly in `src/IoC.InstallGenerator.Tests/InstallerDiscoveryTests.cs`

### Implementation for User Story 2

- [X] T022 [US2] Update Microsoft.CodeAnalysis.CSharp to version 4.9.0+ in `src/IoC.InstallGenerator/IoC.InstallGenerator.csproj`
- [X] T023 [US2] Update Microsoft.CodeAnalysis to version 4.9.0+ in `src/IoC.InstallGenerator/IoC.InstallGenerator.csproj`
- [X] T024 [US2] Update Microsoft.CodeAnalysis.Analyzers to compatible version in `src/IoC.InstallGenerator/IoC.InstallGenerator.csproj`
- [X] T025 [US2] Replace ISourceGenerator with IIncrementalGenerator interface in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T026 [US2] Implement Initialize method with IncrementalGeneratorInitializationContext in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T027 [US2] Create SyntaxProvider to find classes with [IoCInstallerLoader] attribute in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T028 [US2] Create CompilationProvider to analyze types implementing IIoCInstaller in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T029 [US2] Combine SyntaxProvider and CompilationProvider using Combine() in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T030 [US2] Implement RegisterSourceOutput with incremental pipeline in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T031 [US2] Update code generation logic to work with incremental context in `src/IoC.InstallGenerator/CodeGenerator.cs`
- [X] T032 [US2] Remove SyntaxReceiver.cs file (no longer needed) from `src/IoC.InstallGenerator/SyntaxReceiver.cs`
- [X] T033 [US2] Update IoCInstallGenerator.cs to remove ISyntaxReceiver registration in `src/IoC.InstallGenerator/IoCInstallGenerator.cs`
- [X] T034 [US2] Verify generated code is identical to previous version by comparing output
- [X] T035 [US2] Verify all existing tests pass with incremental generator

**Checkpoint**: At this point, User Story 2 should be fully functional - incremental generator works and generates identical code

---

## Phase 5: User Story 3 - Actualizar dependencias y paquetes NuGet (Priority: P3)

**Goal**: Todas las dependencias de paquetes NuGet actualizadas a versiones compatibles con .NET 8

**Independent Test**: Verificar que todas las referencias de paquetes se resuelven correctamente y que no hay conflictos de versiones. Los tests deben ejecutarse sin problemas.

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T036 [P] [US3] Create test to verify package versions are compatible with .NET 8 in `src/IoC.InstallGenerator.Tests/PackageVersionTests.cs`
- [ ] T037 [P] [US3] Create test to verify no package conflicts in `src/IoC.InstallGenerator.Tests/PackageConflictTests.cs`

### Implementation for User Story 3

- [X] T038 [P] [US3] Update Microsoft.NET.Test.Sdk to .NET 8 compatible version in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T039 [P] [US3] Update Microsoft.CodeAnalysis.Testing to .NET 8 compatible version in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T040 [P] [US3] Update xUnit to .NET 8 compatible version in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T041 [P] [US3] Update xunit.runner.visualstudio to .NET 8 compatible version in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T042 [P] [US3] Update coverlet.collector to .NET 8 compatible version in `src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj`
- [X] T043 [P] [US3] Update BenchmarkDotNet to .NET 8 compatible version in `src/Demo.Host/Demo.Host.csproj`
- [X] T044 [US3] Run `dotnet restore` to verify all packages resolve correctly
- [X] T045 [US3] Run `dotnet build` to verify no package conflicts
- [X] T046 [US3] Run `dotnet test` to verify all tests pass with updated packages

**Checkpoint**: At this point, User Story 3 should be complete - all packages updated and resolving correctly

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [X] T047 [P] Update README.md to reflect .NET 8 requirements
- [X] T048 [P] Update quickstart.md if needed (already updated in Phase 1 design)
- [X] T049 [P] Verify all benchmarks work with .NET 8 in `src/Demo.Host/Benchmark.cs`
- [X] T050 Run full solution build and verify compilation time is under 30 seconds
- [X] T051 Verify all existing integration tests pass
- [X] T052 Verify generated code output matches previous version exactly
- [X] T053 [P] Code cleanup: remove any deprecated code or comments
- [X] T054 [P] Update any documentation references to .NET 5.0 to .NET 8.0
- [X] T055 Verify incremental generator caching works by making small changes and rebuilding
- [X] T056 Run performance benchmarks to verify compilation times maintained or improved

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on User Story 1 completion (needs .NET 8 projects to work with)
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May be done in parallel with US2 but should verify after US1

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Update project files before updating code
- Update dependencies before implementing new features
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, User Story 1 tasks marked [P] can run in parallel
- All project file updates in US1 marked [P] can run in parallel (different .csproj files)
- All package updates in US3 marked [P] can run in parallel (different .csproj files)
- Tests for a user story marked [P] can run in parallel
- User Story 3 can potentially start in parallel with User Story 2 (after US1 completes)

---

## Parallel Example: User Story 1

```bash
# Launch all project file updates together:
Task: "Update TargetFramework to net8.0 in src/IoC.InstallGenerator/IoC.InstallGenerator.csproj"
Task: "Update TargetFramework to net8.0 in src/IoC.InstallGenerator.Abstractions/IoC.InstallGenerator.Abstractions.csproj"
Task: "Update TargetFramework to net8.0 in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update TargetFramework to net8.0 in src/Demo.Host/Demo.Host.csproj"
Task: "Update TargetFramework to net8.0 in src/Demo.Modules/ModuleA/ModuleA.csproj"
Task: "Update TargetFramework to net8.0 in src/Demo.Modules/ModuleB/ModuleB.csproj"
Task: "Update TargetFramework to net8.0 in src/Demo.Modules/ModuleC/ModuleC.csproj"
```

---

## Parallel Example: User Story 3

```bash
# Launch all package updates together:
Task: "Update Microsoft.NET.Test.Sdk to .NET 8 compatible version in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update Microsoft.CodeAnalysis.Testing to .NET 8 compatible version in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update xUnit to .NET 8 compatible version in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update xunit.runner.visualstudio to .NET 8 compatible version in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update coverlet.collector to .NET 8 compatible version in src/IoC.InstallGenerator.Tests/IoC.InstallGenerator.Tests.csproj"
Task: "Update BenchmarkDotNet to .NET 8 compatible version in src/Demo.Host/Demo.Host.csproj"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently - all projects compile with .NET 8
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP! - All projects on .NET 8)
3. Add User Story 2 → Test independently → Deploy/Demo (Incremental Generator working)
4. Add User Story 3 → Test independently → Deploy/Demo (All packages updated)
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (update all projects to .NET 8)
   - Developer B: Prepare for User Story 2 (research IIncrementalGenerator API)
3. Once User Story 1 is done:
   - Developer A: User Story 2 (convert to incremental generator)
   - Developer B: User Story 3 (update packages)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- User Story 2 depends on User Story 1 (needs .NET 8 projects)
- User Story 3 can be done in parallel with User Story 2 after US1 completes
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

