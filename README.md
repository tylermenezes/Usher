# Usher
Usher is a "smarthome"/"IoT"/internet of shit platform for making smart devices talk to each other, and do cool things.
It relies less on complicated configuration files and DSLs, and just assumes that if you're the sort of person who
intentionally bought more than one IoT device, you can probably write C# code.

Currently in-progress, but this documentation will be updated once all intended v1 features are working. In the
meantime see the Plugins/ folder for usage.

## Linux

If you're unreasonable enough to want to program your house in C#, you probably want to run this code on Linux! You can
technically compile the code with [.NET Core for Linux](https://www.microsoft.com/net/core#linuxredhat), but it doesn't
actually work on some distributions, and you'll spend several hours trying to debug a bunch of weird error messages
like `The type or namespace name 'System' could not be found` and `Package ... is not compatible with netcoreapp1.0`.

It turns out that:

- The actual dotnet for linux goes by the identifier "netcoreapp", but all packages reference the real "net451" .NET, so you can't install any packages if you use it.
- netcoreapp1.0, the lowest release, is actually based on an unreleased preview of .NET 5.0, because versions don't mean anything.
- netcoreapp1.0 is slightly different than .NET, anyway, so it needs a bunch of NuGet packages released by Microsoft but documented only in StackOverflow threads.
- You can reference net451 (or any other version you like) to fix a lot of problems, EXCEPT...
- net451 and similar are provided by Mono, which comes with .NET Core, but not consistently, resulting in weird missing assemblies.

None of this is documented, so to save you a bunch of time, the solution is to install mono-complete and compile for net451.
