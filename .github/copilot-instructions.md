# Copilot Instructions for This Workspace

## General Guidance
- This solution uses **Blazor with Server-Side Rendering (SSR) only**.
- Do **not** suggest or generate code for Blazor client-side (WebAssembly) or traditional server-side (Blazor Server) rendering modes.
- All recommendations and code samples must be compatible with Blazor SSR.

## Best Practices for Blazor SSR
- Use [Blazor SSR patterns](https://learn.microsoft.com/en-us/aspnet/core/blazor/ssr/) for component rendering and data fetching.
- Prefer asynchronous data loading using dependency injection and services.
- Forbid JavaScript interop.
- Ensure all components and pages are optimized for SSR (e.g., forbid direct DOM manipulation in C#).
- Use [Blazor's built-in navigation and routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing) for enhanced page navigation.

## JavaScript Interactivity with Datastar
- Use **[Datastar](https://data-star.dev/)** for client-side interactivity (e.g., dynamic UI updates, widgets, or data manipulation).
- Datastar always uses **server-sent events (SSE)** when responding to requests.
- Datastar uses **signals on the client** for interactivity.
- Do **not** use Datastar for enhanced page navigation; always use Blazor's built-in navigation mechanisms for this purpose.
- When suggesting JS interop, provide concise and secure examples.
- When suggesting integration patterns or best practices, consider Datastar's use of SSE and client signals.

## Example Scenarios
- For dynamic tables or charts, always use Datastar.
- For navigation between pages, always use Blazor's `<NavLink>` or `NavigationManager`.
- For API responses, always include hypermedia links and demonstrate their use in Blazor components.

## Additional Notes
- All code should target **.NET 9**.
- Prioritize Blazor SSR documentation and patterns over Razor Pages or ASP.NET Core MVC.

## Resources
- Datastar documentation: https://data-star.dev/
- HYPERMEDIA documentation: https://learn.microsoft.com/en-us/aspnet/core/web-api/hateoas, https://htmx.org/essays/hypermedia-driven-applications