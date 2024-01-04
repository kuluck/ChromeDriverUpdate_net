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
