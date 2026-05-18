# Harmonize - Claude Project memory

## Project
- Unity 6.4, URP
- Target Platform: PC
- Namespace: Harmonize.{Module}

## Code conventions
- No coroutines - use UniTask everywhere
- All MonoBehaviours must be in namespace
- XML doc comments on all public APIs
- No magic numbers - use named constants or ScriptableObjects

## Architecture
- ScriptableObjects for all data/config
- Events via UnityEvent or a custom EventBus - no direct component references across systems
- One responsibility per MonoBehaviour
- SOLID principles must be respected

## What to avoid
- Don't use FindObjectOfType at runtime
- Don't use GameObject.Find
- Don't put game logic in UI scripts