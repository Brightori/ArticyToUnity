param($installPath, $toolsPath, $package, $project)

####################################
#####  Start of tool functions #####
####################################

#####
# Add a <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>None</ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
# entry in the project file
#####
function DisableArchWarning
{
  Param( $project )
  
	$project.Save()
	
	#Load the csproj file into an xml object
	$xml = [XML] (gc $project.FullName)
	
	#grab the namespace from the project element so your xpath works.
	$nsmgr = New-Object System.Xml.XmlNamespaceManager -ArgumentList $xml.NameTable
	$nsmgr.AddNamespace('a',$xml.Project.GetAttribute("xmlns"))
	
	$existingNode = $xml.Project.SelectSingleNode("//a:ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch", $nsmgr)
	
	if ( $existingNode -eq $null )
	{
		write-host "Creating 'ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch' project property"
		
	  $ns = $xml.Project.GetAttribute("xmlns")
		$propGroup = $xml.CreateElement("PropertyGroup", $ns)
		$node = $xml.Project.PrependChild($propGroup)
		$archNode = $xml.CreateElement("ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch", $ns)
		$archNode.InnerXml = "None"
		$node = $propGroup.AppendChild($archNode)
		
		#save the changes.
		$xml.Save($project.FullName)
	}
}

# Check if the given item exits in the given collection
# return null if not
function GetProjectItem
{
  Param( $Item, [string]$Name )
	
	try
	{
		return $Item.ProjectItems.Item($Name);
	}
	catch
	{
		return $null;
	}
}

function ContainsProjectItem 
{
  Param( $Item, [string]$Name )
  
	return (GetProjectItem -Item $Item -Name $Name) -ne $null;
}

# Check if the given Filename exists in the project, if not create it from its template file
# and replace the rootnamespace token
function AddProjectItem 
{
	Param($ToolsPath, $Project, [string]$Filename, [string]$Namespace )

	$item = GetProjectItem -Item $Project -Name $Filename
	if ( $item -eq $null )
	{
		$srcFn = Join-Path $toolsPath "../data/$filename.tmpl";
		$prjPath = Split-Path -Path $project.FullName -Parent
		$dstFn = Join-Path $prjPath $filename;
		
		ForEach ($line in (Get-Content $srcFn)) {
			$line -replace "\`$RootNamespace\`$", $Namespace | Out-File -Encoding "UTF8" $dstFn -Append
		}
			
		$item = $project.ProjectItems.AddFromFile( $dstFn );
	}
	return $item;
}

############################
#####  Start of script #####
############################

write-host "Setting CopyLocal of articy:draft assembly references to 'false'";

$asms = $package.AssemblyReferences | %{$_.Name} 
foreach ($reference in $project.Object.References) 
{
    if ($asms -contains $reference.Name + ".dll") 
    {
        $reference.CopyLocal = $false;
        write-host $reference.Name;
    }
}

# first time installation
$rootNamespace = $project.Properties.Item("RootNamespace").Value

write-host "Using Rootnamespace '$rootNamespace' for plugin name and files";

$isFirstInstall = (GetProjectItem -Item $project -Name "PluginManifest.xml") -eq $null

write-host "Checking/Adding plugin manifest";
$item = AddProjectItem -ToolsPath $toolsPath -Project $project -Filename "PluginManifest.xml" -Namespace $rootNamespace
$item.Properties.Item("CopyToOutputDirectory").Value = 2;

write-host "Checking/Adding plugin texts package";
$item = AddProjectItem -ToolsPath $toolsPath -Project $project -Filename "PluginTexts.tpk" -Namespace $rootNamespace
$item.Properties.Item("CopyToOutputDirectory").Value = 2;
$item.Properties.Item("CustomTool").Value = "BuildTextConsts"

write-host "Checking/Adding plugin main class";
$item = AddProjectItem -ToolsPath $toolsPath -Project $project -Filename "Plugin.cs" -Namespace $rootNamespace

write-host "Checking/Adding plugin icon";
$resFolder = GetProjectItem -Item $project -Name "Resources"
if ( $resFolder -eq $null )
{
  write-host "creating 'Resources' folder";
	$resFolder = $project.ProjectItems.AddFolder("Resources")
}
$icon = GetProjectItem -Item $resFolder -Name "Icon.png"
if ( $icon -eq $null )
{	
  write-host "Adding 'Icon.png' to 'Resources'";
	$iconSrc = Join-Path $toolsPath "../data/Icon.png";
	$iconDst = Join-Path (Split-Path -Path $project.FullName -Parent) "Resources/Icon.png";
	[System.IO.File]::Copy($iconSrc, $iconDst, $true)
	$icon = $resFolder.ProjectItems.AddFromFile( $iconDst )
  $icon.Properties.Item("CopyToOutputDirectory").Value = 2;
}

DisableArchWarning( $project );
