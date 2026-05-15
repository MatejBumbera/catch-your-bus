# Catch your bus!

MAUI application for commuters to find their ideal route to their destination :).

## How to Run

### Prerequisites
* Android Emulator or a physical Android device.

### Config
you have to create Config.cs class in Catch-your-bus project with your own Google Api Key
 - the Google Api Key should have access to Routes API and Places (NEW) API

Example:
```csharp
namespace Catch_your_bus;

public static class Config
{
    public const string GoogleApiKey = "exampleApiKey";
}
```