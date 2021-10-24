
# TODO LIST

A list to track rough idea items to work on without the need to create full GitHub issues for them yet

## General

* Configure GitHub Actions / azure pipelines to perform CI/CD build and test run verification for push and pull request activities
* Update README.md files with build/test status badges showing the results of the above
* Promote extension libraries to the developer community
* Begin signing nuget packages
* Add performance benchmark tests
* EF Core extension library ??
* Selenium extension library ??


## Rhinobyte.Extensions.Reflection

### In a new version

* Make the methodInfo.ParseInstructions() extensions support parsing/returning the instructions from the generated IAsyncStateMachine in lieu of the top level method
* Move the RecursiveInstructionFormatter from the tests project into the class library and export it