@echo off  
for /r %%f in (*.cs) do (  
powershell -Command "(Get-Content '%%f') -replace '\[Fact\]', '[TestMethod]' | Set-Content '%%f'"  
) 
