# General rules

1. Make sure that there is a corresponding issue for your change first. If there is none, create one.
2. Create a fork in GitHub.
3. Create a branch of the master branch. Name it something that makes sense (for example: DatabaseConnectionIssue). This makes it easy for everyone to figure out what the branch is used for.
4. Log the changes you made in the CHANGELOG file, following the specified rules.
5. Commit your changes and push them to GitHub.
6. Create a pull request against the origin's master branch.

# C# Coding Style

The general rules we follow are "Visual Studio defaults".

* We use Allman style braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and it must not be nested in other statement blocks that use. For example the following is allowed:
```C#
foreach(var item in collection)
    Console.WriteLine(item.ToString());
```
But this is not: 
```C#
foreach(var item in collection) Console.WriteLine(item.ToString());
```
* We use four spaces of indentation (no tabs).
* We use `camelCase` private members. We DO NOT use prefixes for instance fields.
* We always specify the visibility, even if it's the default (i.e. `private string foo` not `string foo`).
* Namespace imports should be specified at the top of the file, outside of namespace declarations and should be sorted alphabetically. Use Remove and Sort function of Visual Studio.
* Avoid more than one empty line at any time. For example, do not have two blank lines between members of a type.
* We only use `var `when it's obvious what the variable type is (i.e. `var stream = new MemoryStream()` not `var stream = GetStream()`). But we generally prefer using strongly typed variables.
* We use language keywords instead of BCL types (i.e. `int, string, float` instead of `Int32, String, Single`, etc) for type references. For method calls we use the BCL types (i.e. `Int32.Parse instead of int.Parse`). 
* We use `PascalCasing` to name all our constant local variables and fields. 
* When writing comments, we follow a simple rule: every comment must begin with a capital letter and finish with a dot. Also, there is one interval between the `//` and the begining of the comment. Example:
`// Insert comment here.`
* If a file differs in style from these guidelines we will ask you to correct it.

# Logging changes

To make it easy tracking what has changed in every version of the application, we have included the CHANGELOG file. WE follow a small set of rules when logging the changes to the file.

## Keywords
There are several keywords that we use to identify the type of the change that has been made:

* ADDED - something has been added(a functionality, a new database implementation and etc.). Do not use this keyword just to indicate that a new file has been added.
* CHANGED - something important has been changed (an implementation, an interface and etc.).
* FIXED - an issue has been fixed.
* IMPROVED - something has been improved (an implementation, a class architecture and etc.).
* REMOVED - something has been removed.
* UPDATED - something has been updated(usually a database binary).

The keywords are in an alphabetical order and we use them in the CHANGELOG in the same way.

## File structure

The changelog for a specific version beggins with the following header:

`ver. X.X.X (unreleased/released)`

After that follow the keywords indicating the changes (in alphabetical order).
```
ADDED
CHANGED 
FIXED
IMPROVED
REMOVED 
UPDATED 
```

It is not nessasary for every keyword to persist. Sometimes the changes do not include all of the specified types.