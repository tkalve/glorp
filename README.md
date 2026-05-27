# Glorp

A small playground exploring a custom mediator pattern (`IGlorpRequest<TResponse>` / `IGlorpResponse<T>`) end-to-end:

- **Backend** — ASP.NET Core 10 minimal API with a `Glorpiator` mediator dispatched through a single `/glorp` endpoint.
- **Polymorphic JSON** — every request/response carries a `$type` discriminator; the endpoint reads it and routes to the correct handler.
- **Code generation** — a build step reflects over the API assembly and emits a typed TypeScript `GlorpClient` (plus types and JSDoc from XML comments) directly into the frontend.
- **Frontend** — Vite + React 19, consumes the generated client.
- **Docker** — compose stack runs the API privately and serves the SPA through Caddy, which reverse-proxies `/glorp` to the API.

## Repository layout

```
.
├── api/
│   ├── Glorp.slnx
│   ├── Dockerfile
│   └── src/Glorp.Api/
│       ├── Entities/             # Bar, Foo (domain types reused as response data)
│       ├── Handlers/             # Get{Bars,Foos}Handler — request + response nested inside
│       ├── Glorpiatr/            # Glorpiator + IGlorpiator
│       ├── Json/                 # $type discriminator wiring
│       ├── Generator/            # C# → TS code generator (+ XmlDocs reader)
│       ├── XmlDocs.cs
│       └── Program.cs
├── frontend/
│   ├── Dockerfile
│   ├── Caddyfile
│   ├── vite.config.ts
│   └── src/
│       ├── glorp/                # AUTO-GENERATED — do not edit
│       │   ├── types.ts
│       │   └── client.ts
│       ├── App.tsx
│       └── App.css
└── compose.yaml
```

## Running locally

### Prerequisites
- .NET 10 SDK
- Node.js 22+
- Docker (optional, for the compose stack)

### Backend

```bash
cd api/src/Glorp.Api
dotnet run
```

- API: <http://localhost:5151>
- Scalar OpenAPI docs (dev only): <http://localhost:5151/scalar/v1>

The build automatically regenerates `frontend/src/glorp/{types,client}.ts` via an MSBuild `AfterTargets="Build"` target. The generator skips writes when content (excluding the auto-generated timestamp header) is unchanged, so mtimes stay stable. Disable the target entirely with `-p:GenerateTypeScript=false`. Regenerate manually with `dotnet run -- generate-client`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

- UI: <http://localhost:5173>
- Vite proxies `/glorp` → `http://localhost:5151` so the API can run on its own port.

### Docker compose

```bash
docker compose up --build
```

- App: <http://localhost:8080>
- Caddy serves the built SPA and reverse-proxies `/glorp*` to the `api` service over the internal network. The API is not exposed to the host.

## How it works

### `$type` discriminator

`Json/TypeInfoResolver.cs` injects a `$type` property on every concrete `IGlorpRequest<>` / `IGlorpResponse<>` type. `InterfaceConverterFactory` reads `$type` when deserializing interface-typed variables back to the concrete type.

### `/glorp` endpoint

```http
POST /glorp
Content-Type: application/json

{ "$type": "BarsRequest", "minHeight": 0 }
```

The endpoint reads `$type` from the JSON document, looks up the concrete CLR type, deserializes, then calls `Glorpiator.SendAsync(object)` which uses reflection to dispatch to the matching `IGlorpHandler<TReq,TResp>`.

### TypeScript client

Generated `GlorpClient.send(request)` is fully typed — the response type is inferred from the request's `$type`:

```ts
import { GlorpClient } from "./glorp/client";

const client = new GlorpClient();
const res = await client.send({ $type: "BarsRequest", minHeight: 60 });
//    ^? BarsResponse
```

XML doc comments on C# types and record parameters surface as JSDoc on the generated TS interfaces.

## Adding a new request

1. Create a new handler in `Handlers/` with the request/response declared as **nested types** inside the handler class:
   ```csharp
   public class GetThingsHandler : IRequestHandler<GetThingsHandler.ThingsRequest, GetThingsHandler.ThingsResponse>
   {
       /// <summary>Returns things matching the query.</summary>
       /// <param name="Query"></param>
       public record ThingsRequest(string Query) : IGlorpRequest<ThingsResponse>;

       public class ThingsResponse : IGlorpResponse<IEnumerable<Thing>>
       {
           public IEnumerable<Thing>? Data { get; set; }
           public bool Success { get; set; }
           public string? Message { get; set; }
       }

       public async Task<ThingsResponse> HandleAsync(ThingsRequest request, CancellationToken ct) { ... }
   }
   ```
2. Add XML doc comments — they propagate to the generated TS as JSDoc.
3. Rebuild — `AddGlorp()` auto-discovers the handler by reflection, and the TypeScript client / types / unions are regenerated.
