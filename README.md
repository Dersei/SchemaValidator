# SchemaValidator
This is a very basic XML schema validator. There are three projects - two console apps and one WPF.

First console app is standard .NET Framework app, the second one is .NET Core 3.0. Their code is nearly the same, the main difference is the fact, that Core app was written with C# 8.0 with enabled reference types nullability. It doesn't change much in this case, but it was done to check how this new feature will work.

The WPF app allows to past XML file text and one or more Schema files. Then after pushing the "Validate" button, it will display any errors it can find both in Schemas and XML. Errors in XML should be also visible on the pasted text. Hovering over red fragments will show a tooltip with info about this error. There is a possibility to use ctrl+z and ctr+y in every text box.

Regarding the code, in console apps, it is as simple as possible. All apps were written to help in one of the tasks for a study subject - human-computer communication and there wasn't much need for anything special.

The apps have been tested on a few different files and they worked well, therefore they should work for files with similar features.
