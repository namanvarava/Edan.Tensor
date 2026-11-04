# SharedKernel

Shared primitives used across EDAN Tensor bounded contexts: identity and value semantics, aggregate event collection, operation results, domain/integration event contracts, UTC clock abstraction, and domain exceptions.

Keep this project independent of business modules, persistence providers, UI frameworks, and host projects. Add types here only when they are stable and genuinely shared.
