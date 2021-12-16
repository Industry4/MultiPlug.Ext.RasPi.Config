# Creating a new .nupkg file

* Update AssemblyInfo.cs with new version numbers
* Build Release in Visual Studio
* Update MultiPlug.Ext.RasPi.Config.nuspec with new version numbers
* Run pack.bat (nuget.exe is required in the parent/parent directory)
* Upload it to https://www.nuget.org/ manually