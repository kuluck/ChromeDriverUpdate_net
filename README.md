# ChromeDriverUpdater

[NuGet Package](https://www.nuget.org/packages/ChromeDriverUpdate_net/)

## Usage

```csharp
try
{
    new ChromeDriverUpdater().Update(@"c:\path\to\chromedriver.exe");
}
catch (Exception exc)
{
    // ...
}
