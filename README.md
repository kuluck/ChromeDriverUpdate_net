# ChromeDriverUpdater

[NuGet Package](https://www.nuget.org/packages/ChromeDriverUpdate_net/)

# Originaly Source
https://github.com/Hyo-Seong/ChromeDriverUpdater
''' This is code modified by adapting the contents of this source to the latest site. by-kuluck

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
