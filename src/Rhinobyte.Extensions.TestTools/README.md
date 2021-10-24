
# Rhinobyte.Extensions.TestTools

[![NuGet version (Rhinobyte.Extensions.TestTools)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.TestTools.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.TestTools/)

This library contains general purpose testing helper/extension types and extensions for `MSTest`


## ApartmentState Test Attributes

Windows platform specific `[ApartmentStateTestClass(..)]` and `[ApartmentStateTestMethod(..)]` attributes that will ensure the test execution happens using the
specified thread apartment state

Example Usage:
```cs
[ApartmentStateTestClass(ApartmentState.STA)]
public class YourTestClass
{
    [TestMethod]
    public async Task YourTestMethod1() { /** ... **/ }

    [TestMethod]
    public async Task YourTestMethod2() { /** ... **/ }
}

[TestClass]
public class YourOtherTestClass
{
    [ApartmentStateTestMethod(ApartmentState.STA)]
    public async Task YourTestMethod3() { /** ... **/ }
}
```


## StringComparisonHelper and StringAssertionExtensions

String comparison helper logic and test assertion methods for performing line by line comparison of two strings and returning a description of the per line differences.


```cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.TestTools.Assertions;
using System;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class YourTestClass
{
    [TestMethod]
    public async Task YourTestMethod() 
    {
        var actualResult = ... something ...
        var expectedResult = await File.ReadAllTextAsync(someFilePath, CancellationTokenForTest);
        actualResult.ShouldBeSameAs(expectedResult); 
    }
}
```

## WaitUtility

A test helper wait utility to perform asynchronous delays and wait loops that poll for a conditionl match.
Includes support for cateogry based configuration items (similar to how logging categories are applied) that can be used to supercede the values passed via method parameter.

This utility type is generally intended for functional and end-to-end testing scenarios where waiting for a condition before proceeding is common. 

The configuration object support is intended to allow manual test execution to override delay/wait values, so fast running automated tests are easier to follow by a human, without requiring changes to the actual test code.

Example Usage:
```cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rhinobyte.Extensions.TestTools;
using System;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class YourTestClass
{
    [TestMethod]
    public async Task YourTestMethod() 
    {
        var waitUtility = new WaitUtility();

        if (File.Exists(waitConfigurationFilePath))
        {
            var jsonContent = await File.ReadAllTextAsync(waitConfigurationFilePath, CancellationTokenForTest);
            var waitConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, WaitConfigurationItem>>(jsonConten);
            waitUtility.Configuration = WaitConfiguration.FromDictionary(waitConfigurationDictionary);
        }

        await PrepareSomethingAsync(CancellationTokenForTest);

        var somethingRequest = new SomethingRequest();
        var something = await waitUtility
            .WaitUntilAsync(GetSomethingAsync, somethingRequest, pollingInterval: 1000, timeoutThreshold: 30_000, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);

        // ... more test code ...
    }

    public static async Task<Something?> GetSomethingAsync(SomethingRequest request, CancellationToken cancellationToken)
    {
        if (notReadyYet)
            return null;

        return new Something();
    }
}
```