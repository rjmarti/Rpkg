Get-ChildItem "$AppPath\_Temp" -Recurse | Remove-Item
Copy-Item  "$AppPath\_Site\" -Destination "$AppPath\_Temp\Site" -Recurse -Force 