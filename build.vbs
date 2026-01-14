Option Explicit

If InStr(LCase(WScript.FullName), "cscript.exe") = 0 Then
    CreateObject("WScript.Shell").Run "cscript //nologo """ & WScript.ScriptFullName & """", 1, True
    WScript.Quit
End If



Dim oSh, oFs
Set oSh = WScript.CreateObject("WScript.Shell")
Set oFs = CreateObject("Scripting.FileSystemObject")

Dim sWorkDir, sSrcDir, sDistDir, sTempDir
sWorkDir = oSh.CurrentDirectory
sSrcDir = sWorkDir & "\src"
sDistDir = sWorkDir & "\dist"
sTempDir = sWorkDir & "\temp"

on error resume next

If Not oFs.FolderExists(sDistDir) Then
    oFs.CreateFolder sDistDir
End If
If Not oFs.FolderExists(sTempDir) Then
    oFs.CreateFolder sTempDir
End If

Dim sTempNetDir, sTempWebDir
sTempNetDir = sTempDir & "\NET"
sTempWebDir = sTempDir & "\web"
If Not oFs.FolderExists(sTempNetDir) Then oFs.CreateFolder sTempNetDir
If Not oFs.FolderExists(sTempWebDir) Then oFs.CreateFolder sTempWebDir
oFs.CopyFile sSrcDir & "\*", sTempDir & "\"
oFs.CopyFolder sSrcDir & "\web\*", sTempWebDir & "\"
oFs.CopyFile sSrcDir & "\web\*", sTempWebDir & "\"

' Read manifest.json to get Version
Dim sManifest, sData, sVersion, p1, p2
Set sManifest = oFs.OpenTextFile(sTempDir & "\manifest.json")
sData = sManifest.ReadAll
sManifest.Close

p1 = InStr(sData, """version""")
If p1 > 0 Then
    p1 = InStr(p1, sData, ":") + 1
    p2 = InStr(p1, sData, ",")
    If p2 = 0 Then p2 = InStr(p1, sData, "}")
    sVersion = Trim(Mid(sData, p1, p2 - p1))
    sVersion = Replace(sVersion, """", "")
Else
    sVersion = "Unknown"
End If

Dim sSourceCodeDir, sClientLibName, sSolutionDir, sBuildDir, sReleaseDir, sCmd, sText, nuget

nuget = sWorkDir & "\nuget.exe"
if not (oFs.FileExists(nuget)) then
	Wscript.StdErr.WriteLine chr(34) & nuget & chr(34) & " doesnt exist. Cannot continue."
	wscript.quit(1)
end if

sSourceCodeDir = sSrcDir & "\NET\AlecaFrameClientLib\AlecaFrameClientLib\"
sClientLibName = "AlecaFrameClientLib.dll"
sSolutionDir = sWorkDir & "\AlecaFrame.slnx"
sReleaseDir = sSourceCodeDir & "\bin\Release"

if not (oFs.FileExists(sSolutionDir)) then
	Wscript.StdErr.WriteLine sSolutionDir & " doesnt exist. Cannot continue."
	WScript.Quit(1)
end if

sCmd = nuget & " restore " & sSolutionDir
shellRun sCmd

sCmd = "dotnet build " & chr(34) & sSolutionDir & chr(34) & " -c Release /t:Rebuild"
shellRun sCmd

Dim folder, subfolder
if (oFs.FolderExists(sReleaseDir)) then
	set folder = oFs.GetFolder(sReleaseDir)
	sBuildDir = ""
	For Each subfolder in folder.SubFolders
		sBuildDir = subfolder.Path
	next
	if sBuildDir = "" then sBuildDir = sReleaseDir end if
else
	sBuildDir = ""
end if

if not (oFs.FileExists(sBuildDir & "\" & sClientLibName)) then
	Wscript.StdErr.WriteLine chr(34) & sBuildDir & "\" & sClientLibName & chr(34) & " doesnt exist. Build probably failed. Check the console to see why."
	Wscript.quit(1)
end if

WScript.echo "Build Dir = " & sBuildDir

oFs.CopyFile sBuildDir & "\*", sTempNetDir
oFs.CopyFolder sBuildDir & "\*", sTempNetDir

Dim sVersionedDir
sVersionedDir = sDistDir & "\" & sVersion
if (oFs.FolderExists(sVersionedDir)) then
	oFs.DeleteFolder sVersionedDir
end if

oFs.CreateFolder sVersionedDir
oFs.CopyFolder sTempDir , sVersionedDir
oFs.DeleteFile sVersionedDir & "\NET\*.pdb"
oFs.DeleteFolder sTempDir

WScript.StdOut.WriteLine
WScript.StdOut.Write "Done. Press Enter to continue..."
WScript.StdIn.ReadLine

Sub shellRun(sCmd)
	on error resume next
	Err.Clear
	
	Dim oExec
	set oExec = oSh.Exec(sCmd)
	if Err.Number <> 0 then 
		WScript.StdErr.WriteLine "Failed to execute command : " & chr(34) & sCmd & chr(34)
		WScript.StdOut.WriteLine Err.Description
		WScript.Quit(1)
	end if
	
	while ((oExec.Status) = 0)
		if not (oExec.StdOut.AtEndOfStream) then
			do 
				sText = oExec.StdOut.ReadLine()
				WScript.Echo sText
			loop until (oExec.StdOut.AtEndOfStream)
		end if
		if not (oExec.StdErr.AtEndOfStream) then
			do 
				sText = oExec.StdErr.ReadLine()
				WScript.Echo sText
			loop until (oExec.StdErr.AtEndOfStream)
		end if
		WScript.Sleep(100)
	wend
end sub