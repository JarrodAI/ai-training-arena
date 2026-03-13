---
name: AI Training Arena — Strict Coding Rules
description: >
  Mandatory coding standards for ALL code in the AI Training Arena platform.
  Adapted from C# Modular Monolith + Go Performance Services + WASM patterns.
  Every agent MUST follow these rules. Code that violates these rules is REJECTED.
applyTo: "**/*"
---

# AI TRAINING ARENA — STRICT CODING RULES

> **ENFORCEMENT**: These rules are NON-NEGOTIABLE. Every agent, in every file, in every commit.
> Violations trigger immediate rejection and re-work by the offending agent.

---

## 1. CODE METRICS (HARD LIMITS)

| Metric | Limit | Enforcement |
|--------|-------|-------------|
| Method/function body | ≤50 lines (excluding braces) | HARD LIMIT |
| Cyclomatic complexity | ≤10 per method | HARD LIMIT |
| Parameters per method | ≤5 | HARD LIMIT |
| Nesting depth | ≤4 levels | HARD LIMIT |
| Class/struct/module | ≤300 lines | HARD LIMIT |
| File length | ≤500 lines | HARD LIMIT |
| Line length | ≤120 characters | HARD LIMIT |
| Dependencies per module | ≤7 direct | HARD LIMIT |

### If You Hit a Limit

- **Method too long**: Extract helper methods or use pipeline pattern
- **Too complex**: Break into smaller methods, use early returns / guard clauses
- **Too many params**: Introduce a parameter object / options record
- **Too deeply nested**: Invert conditions (guard clauses), extract inner logic
- **File too long**: Split into partial classes or sub-modules

---

## 2. ARCHITECTURE — HEXAGONAL (PORTS & ADAPTERS)

```
╔══════════════════════════════════════════════╗
║                 SACRED LAYERS                ║
╠══════════════════════════════════════════════╣
║                                              ║
║  Domain (innermost)                          ║
║  ├── Entities                                ║
║  ├── Value Objects                           ║
║  ├── Domain Events                           ║
║  ├── Enums                                   ║
║  └── Interfaces (ports)                      ║
║      NO dependencies on outer layers         ║
║      NO framework references                 ║
║      NO database types                       ║
║                                              ║
║  Application (middle)                        ║
║  ├── Commands (write operations via MediatR) ║
║  ├── Queries (read operations via MediatR)   ║
║  ├── DTOs                                    ║
║  ├── Validators                              ║
║  └── Interfaces (secondary ports)            ║
║      Depends ONLY on Domain                  ║
║      NO infrastructure references            ║
║                                              ║
║  Infrastructure (outermost)                  ║
║  ├── Adapters (MySQL, Azure, gRPC, HTTP)     ║
║  ├── Repositories                            ║
║  ├── External service clients                ║
║  └── Configuration                           ║
║      Depends on Application + Domain         ║
║      Implements interfaces from inner layers ║
║                                              ║
║  Presentation (edge)                         ║
║  ├── API Controllers / gRPC services         ║
║  ├── ViewModels (Blazor MVVM)                ║
║  └── Middleware                              ║
║      Depends on Application (via MediatR)    ║
║      NEVER touches Domain directly           ║
║                                              ║
╚══════════════════════════════════════════════╝
```

### Layer Dependency Rules

```
ALLOWED:
  Presentation → Application → Domain
  Infrastructure → Application → Domain
  Infrastructure → Domain

FORBIDDEN:
  Domain → anything
  Application → Infrastructure
  Application → Presentation
  Domain → Application
  Any circular dependency
```

---

## 3. MODULAR MONOLITH BOUNDARIES

Each domain module is an independent vertical slice:

```
Module: Battles
├── Battles.Domain/
├── Battles.Application/
├── Battles.Infrastructure/
└── Battles.Api/

Module: Agents
├── Agents.Domain/
├── Agents.Application/
├── Agents.Infrastructure/
└── Agents.Api/
```

### Module Communication Rules

```
╔══════════════════════════════════════════════════════════════════╗
║  CROSS-MODULE COMMUNICATION — STRICT RULES                       ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ✅ Integration Events via NATS JetStream (async)                ║
║  ✅ MediatR notifications (in-process, fire-and-forget)          ║
║  ✅ Read-through API calls for queries                           ║
║  ✅ Shared kernel types (IDs, enums) from SharedKernel project   ║
║                                                                  ║
║  ❌ Direct class references across modules                       ║
║  ❌ Shared database tables across modules                        ║
║  ❌ Foreign keys across module schemas                           ║
║  ❌ Shared mutable state                                         ║
║  ❌ Circular module dependencies                                 ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 4. NAMING CONVENTIONS (MANDATORY)

### C# (.NET 9 — Codebase B, Desktop App)

```csharp
// Classes, records, interfaces — PascalCase
public class BattleEngine { }
public record BattleResult { }
public interface IBattleRepository { }

// Methods, properties — PascalCase
public async Task<Result<BattleOutcome>> ExecuteBattleAsync(BattleRequest request)

// Local variables, parameters — camelCase
var battleResult = await _engine.ExecuteAsync(request);

// Private fields — _camelCase with underscore prefix
private readonly IBattleRepository _battleRepo;

// Constants — PascalCase
public const int MaxRoundsPerBattle = 100;

// Async methods — MUST end in Async
public Task<Result<T>> GetByIdAsync(Guid id);

// Boolean properties/vars — MUST start with is/has/can/should
public bool IsActive { get; }
private bool _hasStarted;
```

### Solidity (Codebase A)

```solidity
// Contracts — PascalCase
contract AITrainingArena { }

// Functions — camelCase
function registerAgent(uint256 tokenId) external

// Events — PascalCase
event BattleCompleted(uint256 indexed battleId, address winner);

// Errors — PascalCase with module prefix
error Arena__BattleNotFound(uint256 battleId);
error Arena__InsufficientStake(uint256 required, uint256 provided);

// State variables (private) — s_ prefix
mapping(uint256 => Battle) private s_battles;

// Constants — UPPER_SNAKE_CASE
uint256 public constant MAX_AGENTS_PER_BATTLE = 8;

// Immutables — i_ prefix
address private immutable i_tokenContract;
```

### Rust (Codebase C — Frontend WASM)

```rust
// Structs/enums/traits — PascalCase
pub struct BattleState { }
pub enum AgentClass { }
pub trait BattleValidator { }

// Functions, methods, variables — snake_case
fn validate_battle_proof(proof: &BattleProof) -> Result<(), ValidationError>

// Constants — UPPER_SNAKE_CASE
const MAX_AGENTS: usize = 8;

// Modules — snake_case
mod battle_engine;

// Type parameters — single uppercase letter or PascalCase
fn process<T: Serialize>(item: T) -> Result<T>
```

### Svelte 5 / JavaScript (Codebase D)

```javascript
// Components — PascalCase (file and import)
import NavBar from './NavBar.svelte';

// Variables, functions — camelCase
let battleCount = $state(0);
function handleBattleStart() { }

// Constants (module-level) — UPPER_SNAKE_CASE
export const MAX_AGENTS = 8;

// Stores — camelCase with $ prefix for reactive
const count = $derived(data.length);

// CSS classes — kebab-case
class="battle-card agent-display"

// Files — PascalCase for Svelte, camelCase for JS
// NavBar.svelte, utils.js, constants.js
```

---

## 5. ERROR HANDLING — RESULT PATTERN (NO EXCEPTIONS FOR FLOW)

### C# — Result<T> Pattern

```csharp
// MANDATORY: Use Result<T> for business operations
public sealed record Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new() { Value = value };
    public static Result<T> Failure(Error error) => new() { Error = error };
}

// EXAMPLE: Service method
public async Task<Result<BattleOutcome>> ExecuteBattleAsync(BattleRequest request)
{
    var validation = _validator.Validate(request);
    if (!validation.IsValid)
        return Result<BattleOutcome>.Failure(new ValidationError(validation.Errors));

    var agents = await _agentRepo.GetByIdsAsync(request.AgentIds);
    if (agents.Count != request.AgentIds.Count)
        return Result<BattleOutcome>.Failure(new NotFoundError("One or more agents not found"));

    var outcome = _engine.Run(agents, request.Rules);
    return Result<BattleOutcome>.Success(outcome);
}
```

### When Exceptions ARE Allowed

```
╔══════════════════════════════════════════════════════════════════╗
║  EXCEPTIONS ONLY FOR:                                            ║
║  • Infrastructure failures (DB down, network timeout)            ║
║  • Programming errors (null ref, index out of range)             ║
║  • Framework-provided exception flows (ASP.NET middleware)        ║
║                                                                  ║
║  NEVER throw exceptions for:                                     ║
║  • Validation failures → Result.Failure                          ║
║  • Not found → Result.Failure(NotFoundError)                     ║
║  • Business rule violations → Result.Failure(DomainError)        ║
║  • Expected error conditions → Result.Failure                    ║
╚══════════════════════════════════════════════════════════════════╝
```

### Solidity — Custom Errors (Gas Efficient)

```solidity
// MANDATORY: Use custom errors, NOT require strings
// GOOD
error Arena__InsufficientStake(uint256 required, uint256 provided);
if (stake < required) revert Arena__InsufficientStake(required, stake);

// FORBIDDEN
require(stake >= required, "Insufficient stake"); // wastes gas
```

### Rust — Result<T, E> (idiomatic)

```rust
// MANDATORY: Use Result<T, E> — never panic in production code
pub fn validate_proof(proof: &BattleProof) -> Result<(), ProofError> {
    if proof.rounds.is_empty() {
        return Err(ProofError::EmptyRounds);
    }
    Ok(())
}
```

---

## 6. DATABASE RULES — MYSQL ENTERPRISE EDITION

### Connection Pooling

```csharp
// MANDATORY: Use connection pooling via MySQL Connector/NET
services.AddDbContextPool<BattlesDbContext>(options =>
    options.UseMySQL(connectionString, mysql =>
    {
        mysql.MigrationsAssembly("Battles.Infrastructure");
        mysql.EnableRetryOnFailure(maxRetryCount: 3);
        mysql.CommandTimeout(30);
    })
);
```

### Query Rules

```
╔══════════════════════════════════════════════════════════════════╗
║  MYSQL QUERY RULES                                               ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ✅ ALWAYS use parameterized queries (prevent SQL injection)     ║
║  ✅ ALWAYS use async database operations (Async/Await)           ║
║  ✅ ALWAYS add indexes for columns in WHERE / JOIN / ORDER BY    ║
║  ✅ Use EF Core for CRUD, raw SQL only for reports/analytics     ║
║  ✅ Use database migrations — never modify schema manually       ║
║  ✅ Use optimistic concurrency (RowVersion / Timestamp)          ║
║  ✅ Connection strings from Azure Key Vault — NEVER hardcoded    ║
║                                                                  ║
║  ❌ NEVER use string concatenation in queries                    ║
║  ❌ NEVER use SELECT * — always specify columns                  ║
║  ❌ NEVER use N+1 queries — use Include() or explicit joins      ║
║  ❌ NEVER store secrets in MySQL — use Azure Key Vault            ║
║  ❌ NEVER disable SSL for MySQL connections                       ║
║  ❌ NEVER use root account — use least-privilege service accounts ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

### Schema Migrations

```bash
# EF Core migrations only — one migration per feature
dotnet ef migrations add AddBattleTelemetry -p Battles.Infrastructure -s Battles.Api
dotnet ef database update -p Battles.Infrastructure -s Battles.Api
```

---

## 7. SECURITY RULES

### Authentication & Authorization

```
╔══════════════════════════════════════════════════════════════════╗
║  SECURITY NON-NEGOTIABLES                                        ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  1. ALL API endpoints MUST require authentication                ║
║     Exception: health checks, public marketing pages             ║
║                                                                  ║
║  2. Use Azure AD / Entra ID for identity                         ║
║     → JWT bearer tokens for API auth                             ║
║     → MSAL for desktop/mobile app auth                           ║
║                                                                  ║
║  3. ALL secrets in Azure Key Vault                               ║
║     → Private keys, API keys, connection strings                 ║
║     → Use DefaultAzureCredential — never store in code/config    ║
║                                                                  ║
║  4. Input Validation at EVERY system boundary                    ║
║     → FluentValidation for C# DTOs                               ║
║     → Zod or manual for TypeScript                               ║
║     → Custom validators for Solidity calldata                    ║
║                                                                  ║
║  5. OWASP Top 10 compliance (checked by POLECAT-7)              ║
║     → SQL injection: parameterized queries only                  ║
║     → XSS: output encoding, CSP headers                          ║
║     → CSRF: anti-forgery tokens                                  ║
║     → Broken access control: attribute-based authorization        ║
║                                                                  ║
║  6. Smart Contract Security                                      ║
║     → ReentrancyGuard on ALL external calls                      ║
║     → Checks-Effects-Interactions pattern                        ║
║     → OpenZeppelin audited base contracts                        ║
║     → ALL contracts MUST have 100% test coverage                 ║
║     → Slither + Mythril static analysis before deploy            ║
║                                                                  ║
║  7. WASM Security                                                ║
║     → All crypto in WASM boundary — keys never in JS             ║
║     → Code-sign WASM modules                                     ║
║     → Hash-verify WASM modules on load                           ║
║     → Per-instance resource limits (CPU, memory, time)           ║
║                                                                  ║
║  8. Network Security                                             ║
║     → TLS 1.3 for ALL connections                                ║
║     → mTLS for P2P node communication                            ║
║     → Rate limiting on all public endpoints                      ║
║     → Azure NSG / Firewall rules for MySQL access                ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 8. TESTING REQUIREMENTS

### Coverage Targets

| Codebase | Unit Tests | Integration Tests | E2E |
|----------|-----------|-------------------|-----|
| Smart Contracts (A) | 100% | 100% (fork tests) | N/A |
| P2P Node (B) | ≥80% | ≥70% | ≥50% |
| Frontend WASM (C) | ≥80% | ≥60% | ≥50% |
| Marketing Site (D) | N/A | Build pass | Visual |
| Desktop/Mobile | ≥75% | ≥60% | ≥40% |

### Test Naming Convention

```csharp
// C#: [Method]_[Scenario]_[ExpectedResult]
public async Task ExecuteBattle_WithValidAgents_ReturnsBattleOutcome()
public async Task ExecuteBattle_WithInsufficientStake_ReturnsFailure()
```

```rust
// Rust
#[test]
fn validate_proof_with_empty_rounds_returns_error() { }
```

```javascript
// Solidity (Hardhat)
it("should revert when stake is insufficient", async () => { });
```

```python
# Python integration tests
def test_battle_api_returns_outcome_for_valid_agents():
def test_mysql_connection_with_ssl_succeeds():
```

### Test Rules

```
╔══════════════════════════════════════════════════════════════════╗
║  TESTING COMMANDMENTS                                            ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  1. NEVER skip, disable, or delete a failing test                ║
║  2. EVERY public method/function gets at least 1 test            ║
║  3. Test behavior, NOT implementation                            ║
║  4. One assertion per test (prefer)                              ║
║  5. Tests MUST be deterministic — no flaky tests                 ║
║  6. Use factories/builders for test data — no magic strings      ║
║  7. Integration tests use TEST database schema                   ║
║  8. Python tests always run in .venv                             ║
║  9. All tests pass in CI before merge — no exceptions            ║
║  10. Smart contract tests MUST test revert conditions            ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 9. GIT RULES

```
╔══════════════════════════════════════════════════════════════════╗
║  GIT RULES (ALL AGENTS)                                          ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  Commit format: conventional commits                             ║
║  type(scope): description                                        ║
║                                                                  ║
║  Types: feat, fix, refactor, test, docs, build, ci, chore        ║
║  Scopes: contracts, node, frontend, marketing, infra, security   ║
║                                                                  ║
║  Examples:                                                       ║
║  feat(contracts): add ATAToken ERC-20 with staking hooks         ║
║  fix(node): resolve race condition in battle matchmaking          ║
║  test(contracts): add 100% coverage for AgentNFT minting         ║
║  build(infra): add Azure MySQL Bicep template                    ║
║                                                                  ║
║  RULES:                                                          ║
║  • One logical change per commit                                 ║
║  • Never commit .env, secrets, or private keys                   ║
║  • Never force-push main/master                                  ║
║  • Always check git status before committing                     ║
║  • Stage specific files — never blindly git add .                ║
║  • Branch naming: <type>/<scope>/<short-desc>                    ║
║    e.g., feat/contracts/ata-token                                ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 10. CODE REVIEW CHECKLIST (MANAGER VERIFIES)

Before any phase is marked COMPLETE, the Manager checks:

- [ ] All methods ≤50 lines
- [ ] All complexity ≤10
- [ ] All files ≤500 lines
- [ ] Hexagonal architecture boundaries respected
- [ ] No cross-module direct dependencies
- [ ] Result pattern used (no thrown exceptions for business logic)
- [ ] All database queries parameterized
- [ ] All inputs validated at boundaries
- [ ] All secrets from Azure Key Vault
- [ ] Tests pass with required coverage
- [ ] Conventional commit messages
- [ ] No TODO/FIXME/HACK comments left behind
- [ ] WASM modules code-signed (where applicable)
- [ ] Naming conventions followed per language
- [ ] No hardcoded URLs, IPs, or credentials

---

## 11. TECHNOLOGY STACK REFERENCE

| Layer | Technology | Version |
|-------|-----------|---------|
| Smart Contracts | Solidity + Hardhat | ^0.8.20 / 2.x |
| Contract Libraries | OpenZeppelin | 5.x |
| Contract Network | Mantle L2 | Latest |
| P2P Node Runtime | .NET 9 | 9.0 |
| P2P Framework | ASP.NET Core + libp2p | 9.0 |
| CQRS / Mediator | MediatR | 12.x |
| Event Bus | NATS JetStream | Latest |
| Frontend Runtime | Dioxus + Rust WASM | 0.6+ |
| WASM Toolchain | wasm-pack + wasm-bindgen | Latest |
| Marketing Site | Svelte 5 + Vite 6 + Tailwind 4 | Latest |
| Desktop / Mobile | .NET MAUI + Blazor WASM | 9.0 |
| Database | MySQL Enterprise Edition | 8.0 |
| Cloud | Azure (AKS, Container Apps, Static Web Apps) | Latest |
| Secrets | Azure Key Vault | Latest |
| Identity | Microsoft Entra ID | Latest |
| CI/CD | GitHub Actions + Azure DevOps | Latest |
| Monitoring | Azure Monitor + Application Insights | Latest |
| Agent Orchestration | Gas Town (gt.exe) | Latest |
| Issue Tracking | Beads (bd) | 0.55.4+ |

---

## 12. PERFORMANCE RULES

### C# / .NET

```
• Use async/await for ALL I/O operations
• Use IAsyncEnumerable for streaming data
• Use ArrayPool/MemoryPool for hot paths
• Use Span<T> / ReadOnlySpan<T> for parsing
• Profile before optimizing — no premature optimization
• connection pool min=10, max=100 for MySQL
```

### Solidity

```
• Use custom errors (not require strings) — saves gas
• Use immutable for constructor-set values
• Pack storage variables (uint128 + uint128 in one slot)
• Prefer calldata over memory for external function params
• Cache storage reads in local variables
• Use unchecked { } for safe arithmetic in tight loops
```

### Rust / WASM

```
• Minimize WASM binary size — use wasm-opt
• Avoid unnecessary allocations in hot paths
• Use #[inline] for small frequently-called functions
• Profile with wasm-profiler before optimizing
• Set hard memory limits per WASM instance
```
