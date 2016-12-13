# .NET Core Library Extension

This library contains highly reuseable classes and methods I find to be missing in the cross-platform Core library of .NET. It builds on top of the [.NET Standard Library Extension](https://github.com/Whathecode/NET-Standard-Library-Extension), including functionality which can only be obtained by using features present in .NET Core, but unavailable in .NET Standard (e.g., runtime code compilation).

**Requires**: To open the solution and project files, the latest Visual Studio 2017 RC is required ([released on December 12](https://www.visualstudio.com/en-us/news/releasenotes/vs2017-relnotes) at time of writing).

Namespaces from the original library are followed as much as possible:

- Helper classes are contained within the corresponding namespace: e.g., a helper class for `System.Delegate` will be located in `Whathecode.System.DelegateHelper`.
- Extension methods are placed in the relevant namespace: e.g., general extension are located in `Whathecode.System`, while extension methods when doing reflection are located in `Whathecode.System.Reflection`.
- Unit tests are placed in a Tests namespace in the corresponding namespace: e.g., `Whathecode.Tests.System.Reflection`. xUnit is used. These tests also serve as examples.

Copyright (c) 2016 Steven Jeuris
The library is distributed under the terms of the MIT license (http://opensource.org/licenses/mit-license). More information can be found in "LICENSE"