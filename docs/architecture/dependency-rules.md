# Dependency rules

1. Domain depends only on SharedKernel and the BCL.
2. Application depends on its own Domain, Contracts when needed, and SharedKernel.
3. Infrastructure implements abstractions from its own module.
4. Hosts are composition roots.
5. Commercial and Licensing never reference each other's internals; integration uses versioned contracts and asynchronous messaging.
6. SharedKernel never references business modules.
7. No project references a Host.
8. Never commit production secrets or private signing keys.
