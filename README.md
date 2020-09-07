# Chatta

A backend project to demonstrate clean Software Architectural principles in .Net Core as discussed in Clean Architecture by Robert C. Martin.

The solution is segmented into three projects:

- `Chatta.Core`
- `Chatta.Infrastructure`
- `Chatta.Web`

The most 'stable' project, `Chatta.Core` is the layer that all other components depend on. It contains the business rules, entities and interfaces such that it can be deployed independently of the UI and the database. This is what makes it 'stable'.

`Chatta.Infrastructure` on the other hand, contains infrastructure or setup code. This includes codes concerning data migrations, repositories, services and extension classes (Caching).

`Chatta.Web` is the web application project. It contains componenents suc as authorization, controllers and UI views.

During development care was taken to avoid dependence on concretion. Interfaces were used to invert dependencies and ensure dependencies pointed in one direction.
