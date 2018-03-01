
## Problem

HttpWebRequest in Core does not reuse connections.


## Test Core

Execute `dotnet run` and observe output; it will show new connection being created each time.

## Test .NET Framework

Execute `csc Program.cs` then `Program`; it will show connection being re-used.