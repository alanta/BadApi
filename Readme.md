# Bad API

This is a simple API used to demonstrate common API security vulnerabilities in ASP.NET Core.

Vulnerabilities include:
* SQL Injection
* Bad XML Deserialization (XXE)
* Over-posting
* Broken authorization
* Path traversal
* DDoS (Rate limiting)

It also includes some common mitigations for these vulnerabilities.

# Running this project

There are three recommended ways to run the project, depending on what tooling you already have installed.

## 1. Directly from the command line

This is the simplest option if you already have the .NET 10 SDK installed locally.

```sh
git clone <repository-url>
cd vulnerable_net_core

dotnet --version
# should resolve to 10.0.302 via global.json

dotnet restore BadApi.slnx
dotnet build BadApi.slnx
dotnet test BadApi.slnx

dotnet run --project BadApi/BadApi.csproj --urls http://localhost:5062
```

This starts the vulnerable API directly on `http://localhost:5062`.

The project also includes a small load test app under [DDoS](DDoS) and an Aspire host under [AppHost](AppHost), but the plain API is the fastest way to exercise the endpoints manually.

## 2. Using Aspire for richer request insights

Aspire is the best choice when you want to inspect the running services and the request flow in a richer dashboard.

```sh
dotnet restore BadApi.slnx
dotnet build BadApi.slnx
dotnet run --project AppHost/AppHost.csproj
```

This starts the Aspire AppHost and opens the dashboard in the browser. You can then inspect the distributed application, service health, logs, and requests while exercising the API.

This mode is ideal for seeing request traces and logs together with the application behavior.

## 3. Using a dev container

If you want a consistent, preconfigured environment, use the repo's dev container.

Open the repository in VS Code and choose:

- `Dev Containers: Reopen in Container`

The container is configured for the .NET 10 toolchain and exposes the app on the sample port used by the labs.

## Working through the labs

The repository is structured as a set of scenario folders. Each scenario contains:

- an `.http` file with example requests for the student
- an `Endpoints.cs` file with the route registrations and implementations
- often supporting request/response models and related code

Use the labs in this order:

1. Start the app using one of the run modes above.
2. Open the matching `.http` file for the scenario you are studying.
3. Read the endpoint code in the relevant folder to understand what the route is doing.
4. Execute the requests in the `.http` file using the REST Client extension or by calling the endpoints with `curl`.
5. Watch the console logs from the running API and compare them with what the code is doing.
6. Review the difference between the insecure version and the safer alternative in the same scenario folder.

### Scenario folders

- [BadApi/Account](BadApi/Account) - authentication and account actions
- [BadApi/BrokenAccessControl](BadApi/BrokenAccessControl) - authorization flaws and filtered access
- [BadApi/DDoS](BadApi/DDoS) - rate limiting and availability scenarios
- [BadApi/OverPosting](BadApi/OverPosting) - mass assignment and validation issues
- [BadApi/SqlInjection](BadApi/SqlInjection) - SQL injection examples
- [BadApi/XXE](BadApi/XXE) - XML external entity deserialization examples

## Using the HTTP files

The `.http` files are the student-facing lab instructions.

- They contain the example requests to send to the running API.
- They show the expected request payloads, request headers, and authentication patterns.
- They are meant to be used alongside the code in the matching endpoint folder.
- They help you verify the observable behavior before and after a fix.

Important: to learn the material properly, you should not only send the requests. You should also:

- open the route definitions in the folder's `Endpoints.cs` file
- inspect the associated models and validation code
- read the API logs in the terminal or Aspire dashboard
- connect the behavior you see in the HTTP response to the code path that produced it

## Notes

- This repo is intentionally insecure by design; the exercises are meant to be studied, not “fixed” unless the task explicitly calls for remediation.
- The endpoints are intentionally vulnerable and educational; they should be treated as training scenarios rather than production-ready code.
- The easiest path for a class or workshop is usually to open the repo in a dev container or run the API directly from the command line, then work through the `.http` files one scenario at a time.