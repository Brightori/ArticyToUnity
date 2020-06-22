using Articy.Api;
using Articy.Utils.StaticUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace MyCompany.TestArticy
{
    public enum ClassNameSource
    {
        TechnicalName,
        DisplayName
    }

    public class ExportManager
    {
        private const string SCRIPTABLE_OBJECT = "SriptableObject";
        private const string MONO_BEHAVIOUR = "";
        private const string PATH = "D:/Develop/ExportToUnity/ExportTest/";

        private List<string> mWrittenBaseClasses;
        private List<string> mWrittenTemplateClasses;
        private Dictionary<string, string> mBaseClassBases;
        private ClassNameSource mClassNameSource;
        private int mExportedObjectCount;
        private int mExportedAssetCount;
        private string mResourcesPath;
        private Parser parser = new Parser();

        private readonly ApiSession mSession;
        private DirectoryInfo mClassDir;

        public ExportManager(ApiSession mSession)
        {
            this.mSession = mSession;
        }

        public void Export(ClassNameSource aClassNameSource, string aOutputPath = PATH)
        {
            try
            {
                // prepare export run
                mClassNameSource = aClassNameSource;
                mWrittenBaseClasses = new List<string>();
                mWrittenTemplateClasses = new List<string>();
                mBaseClassBases = new Dictionary<string, string>();

                mExportedObjectCount = 0;
                mExportedAssetCount = 0;
                parser = new Parser();

                var aTopics = new List<string>
                {
                    "Flow",
                    "Entities" ,
                    "Assets",
                    "Locations",
                };

                var flow = mSession.RunQuery("SELECT * FROM Flow WHERE ObjectType=Dialogue");

                foreach (var r in flow.Rows)
                {
                    parser.ProcessDialogues(r);
                }

                // check output dir
                //DirectoryInfo outDir = new DirectoryInfo(aOutputPath);
                //if (!outDir.Exists)
                //{
                //    //mFramework.ShowError("Output directory does not exist! Please specify the asset directory of your Unity project.");
                //    return;
                //}

                //var files = outDir.GetFiles("*.unity");
                //if (files.Length == 0)
                //{
                //    //mFramework.ShowError("The specified output directory does not contain a Unity project file! Please specify the asset directory of your Unity project.");
                //    return;
                //}

                //DirectoryInfo pluginsDir = new DirectoryInfo(outDir.FullName + @"\Plugins");
                //if (!pluginsDir.Exists) pluginsDir.Create();

                //DirectoryInfo articyDir = new DirectoryInfo(pluginsDir.FullName + @"\Articy");
                //if (!articyDir.Exists) articyDir.Create();

                //mClassDir = new DirectoryInfo(articyDir.FullName + @"\Classes");
                //if (!mClassDir.Exists) mClassDir.Create();

                //// export flow);
                //if (aTopics.Contains("Flow"))
                //{
                //    var systemFolderFlow = mSession.GetSystemFolder(SystemFolderNames.Flow);
                //    var flowDir = new DirectoryInfo(articyDir.FullName + @"\Flow");
                //    if (!flowDir.Exists) flowDir.Create();
                //    ExportFlowFolder(systemFolderFlow, flowDir);
                //}

                //// export entities
                //if (aTopics.Contains("Entities"))
                //{
                //    var systemFolderEntities = mSession.GetSystemFolder(SystemFolderNames.Entities);
                //    var entitiesDir = new DirectoryInfo(articyDir.FullName + @"\Entities");
                //    if (!entitiesDir.Exists) entitiesDir.Create();
                //    ExportEntityFolder(systemFolderEntities, entitiesDir);
                //}

                //// export locations
                //if (aTopics.Contains("Locations"))
                //{
                //    var systemFolderLocations = mSession.GetSystemFolder(SystemFolderNames.Locations);
                //    var locationsDir = new DirectoryInfo(articyDir.FullName + @"\Locations");
                //    if (!locationsDir.Exists) locationsDir.Create();
                //    ExportLocationFolder(systemFolderLocations, locationsDir);
                //}

                //// export assets
                //if (aTopics.Contains("Assets"))
                //{
                //    var systemFolderAssets = mSession.GetSystemFolder(SystemFolderNames.Assets);
                //    var assetsDir = new DirectoryInfo(articyDir.FullName + @"\Resources");
                //    mResourcesPath = assetsDir.FullName;
                //    if (!assetsDir.Exists) assetsDir.Create();
                //    ExportAssetsFolder(systemFolderAssets, assetsDir);

                //}


                mSession.ShowMessageBox("Complete");
            }
            catch (Exception ex)
            {
                mSession.ShowMessageBox("Error");
            }

        }

        private void ExportAssetsFolder(ObjectProxy aAssetsFolder, DirectoryInfo aAssetsDir)
        {
            var children = aAssetsFolder.GetChildren();
            foreach (var child in children)
            {
                var featureProperties = GetFeatureProperties(child);

                switch (child.ObjectType)
                {
                    case ObjectType.AssetsUserFolder:
                        DirectoryInfo subFolder = new DirectoryInfo(aAssetsDir.FullName + "\\" + GetClassName(child));
                        if (!subFolder.Exists) subFolder.Create();
                        ExportAssetsFolder(child, subFolder);
                        break;

                    case ObjectType.Asset:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aAssetsDir, child, featureProperties);
                        break;
                }
            }
        }

        private void ExportLocationFolder(ObjectProxy aLocationFolder, DirectoryInfo aLocationDir)
        {
            var children = aLocationFolder.GetChildren();
            foreach (var child in children)
            {
                var featureProperties = GetFeatureProperties(child);

                switch (child.ObjectType)
                {
                    case ObjectType.LocationsUserFolder:
                        DirectoryInfo locationsUserFolder = new DirectoryInfo(aLocationDir.FullName + "\\" + GetClassName(child));
                        if (!locationsUserFolder.Exists) locationsUserFolder.Create();

                        ExportLocationFolder(child, locationsUserFolder);
                        break;

                    case ObjectType.Location:
                        CreateOrUpdateBaseClassScript(mClassDir, child, MONO_BEHAVIOUR);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aLocationDir, child, featureProperties);

                        DirectoryInfo locationsFolder = new DirectoryInfo(aLocationDir.FullName + "\\" + GetClassName(child));
                        if (!locationsFolder.Exists) locationsFolder.Create();

                        ExportLocationFolder(child, locationsFolder);
                        break;

                    case ObjectType.Zone:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aLocationDir, child, featureProperties);
                        break;

                    case ObjectType.Path:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aLocationDir, child, featureProperties);
                        break;

                    case ObjectType.Spot:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aLocationDir, child, featureProperties);
                        break;

                    case ObjectType.Link:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aLocationDir, child, featureProperties);
                        break;
                }
            }
        }

        private void ExportEntityFolder(ObjectProxy aEntityFolder, DirectoryInfo aEntityDir)
        {
            var children = aEntityFolder.GetChildren();
            foreach (var child in children)
            {
                var featureProperties = GetFeatureProperties(child);

                switch (child.ObjectType)
                {
                    case ObjectType.EntitiesUserFolder:
                        DirectoryInfo subFolder = new DirectoryInfo(aEntityDir.FullName + "\\" + GetClassName(child));
                        if (!subFolder.Exists) subFolder.Create();
                        ExportEntityFolder(child, subFolder);
                        break;

                    case ObjectType.Entity:
                        CreateOrUpdateBaseClassScript(mClassDir, child, MONO_BEHAVIOUR);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aEntityDir, child, featureProperties);
                        break;
                }
            }
        }

        private string GetClassName(ObjectProxy aObject)
        {
            switch (mClassNameSource)
            {
                case ClassNameSource.DisplayName:
                    return aObject.GetDisplayName();

                case ClassNameSource.TechnicalName:
                    return aObject.GetTechnicalName();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void CreateOrUpdateScriptFile(DirectoryInfo aFolder, ObjectProxy aObject, Dictionary<string, List<string>> aFeatures)
        {
            var fullFilename = aFolder.FullName + "\\" + GetClassName(aObject) + ".cs";
            var scriptText = AssembleUnityScript(aObject, aFeatures, fullFilename);
            using (StreamWriter outfile = new StreamWriter(fullFilename))
            {
                outfile.Write(scriptText.ToString());
            }

            // copy asset file
            if (aObject.ObjectType == ObjectType.Asset)
            {
                var assetFullFilename = (string)aObject[ObjectPropertyNames.AbsoluteFilePath];
                var assetFile = new FileInfo(assetFullFilename);
                if (assetFile.Exists)
                {
                    var unityFullFilename = FileUtils.GetPath(fullFilename) + aObject[ObjectPropertyNames.AssetFilename];
                    assetFile.CopyTo(unityFullFilename, true);
                    FileInfo unityFile = new FileInfo(unityFullFilename);
                    unityFile.IsReadOnly = false;
                }
            }

            mExportedObjectCount++;
            //mForm.ShowInfo( exists ? "...updated '{0}'" : "...created '{0}'", fullFilename );
        }

        private StringBuilder AssembleUnityScript(ObjectProxy aObject, Dictionary<string, List<string>> aFeatures, string aFullFilename)
        {
            var className = GetClassName(aObject);

            // assemble script header
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine();
            sb.Append("public class ").Append(className).Append(" : ").AppendLine(GetTemplateOrBaseClassName(aObject));
            sb.AppendLine("{");

            // assemble header of initialization
            var initMethodSnippet = mBaseClassBases[GetBaseClassName(aObject)] == MONO_BEHAVIOUR
                                        ? "  void Start()"
                                        : "  " + className + "()";
            sb.AppendLine(initMethodSnippet);
            sb.AppendLine("  {");

            // initialization of properties from base class
            sb.AppendLine("    // properties (base class)");
            foreach (var baseProperty in aObject.GetAvailableProperties(PropertyFilter.Base))
            {
                sb.Append("    ").Append(baseProperty).Append(" = ");
                AddPropertyValue(aObject, baseProperty, sb);
                sb.AppendLine(";");
            }

            // initialization of template features
            var templateName = GetTemplateTechnicalName(aObject);
            if (!String.IsNullOrEmpty(templateName))
            {
                foreach (var featureName in aFeatures.Keys)
                {
                    var propertyNames = aFeatures[featureName];

                    sb.AppendLine();
                    sb.Append("    // properties (").Append(featureName).AppendLine(")");

                    foreach (var propertyName in propertyNames)
                    {
                        sb.Append("    ").Append(GetFeaturePart(propertyName)).Append("_").Append(GetPropertyPart(propertyName)).Append(" = ");
                        AddPropertyValue(aObject, propertyName, sb);
                        sb.AppendLine(";");
                    }
                }
            }

            // input pin successors (children)
            if (aObject.ObjectType == ObjectType.FlowFragment ||
                aObject.ObjectType == ObjectType.Dialogue)
            {
                // collect input pin successors (children)
                var children = new Dictionary<int, List<Successor>>();
                var inputPins = aObject.GetInputPins();
                foreach (var inputPin in inputPins)
                {
                    var pinChildren = new List<Successor>();
                    SearchDirectSuccessors(aObject, pinChildren, null, inputPin, 1);
                    children.Add((int)inputPin[ObjectPropertyNames.PinIndex], pinChildren);
                }

                // write input pin successors to script
                sb.AppendLine();
                sb.AppendLine("    // children");
                sb.AppendLine("    InputPinChildren = new Hashtable();");

                foreach (var pin in children.Keys)
                {
                    var pinChildren = children[pin];
                    if (pinChildren.Count == 0) continue;
                    sb.Append("    InputPinChildren.Add( ").Append(pin).AppendLine(", new Successor[]");
                    sb.Append("    {");
                    //---
                    for (int s = 0; s < pinChildren.Count; s++)
                    {
                        var successor = pinChildren[s];
                        sb.AppendLine(s == 0 ? "" : ",");
                        sb.AppendLine("        new Successor()");
                        sb.AppendLine("        {");
                        sb.Append("          StartPinLabel = \"").Append(successor.StartingPinLabel).AppendLine("\",");
                        sb.AppendLine("          Path = new Connection[]");
                        sb.Append("          {");

                        for (int pp = 0; pp < successor.Path.Count; pp++)
                        {
                            var pathPart = successor.Path[pp];
                            sb.AppendLine(pp == 0 ? "" : ",");
                            sb.Append("            new Connection(){ ConnectionLabel=\"").Append(pathPart.ConnectionLabel);
                            sb.Append("\", ConnectionColor=new Color(").Append(pathPart.ConnectionColor.R).Append(",").Append(
                                pathPart.ConnectionColor.G).
                                Append(",").Append(pathPart.ConnectionColor.B).Append("), PinLabel=\"").Append(pathPart.PinLabel).
                                Append("\", PinIndex=").Append(pathPart.PinIndex).
                                Append(", SubmergeLevel=").Append(pathPart.TargetSubmergeLevel).Append("}");
                        }

                        sb.AppendLine();
                        sb.AppendLine("          },");
                        sb.Append("          Target=\"").Append(successor.Target != null ? successor.Target.GetTechnicalName() : "").AppendLine("\",");
                        sb.Append("          TargetPinIndex=").Append(successor.TargetPinIndex).AppendLine(",");
                        sb.Append("          TargetSubmergeLevel=").Append(successor.TargetSubmergeLevel).AppendLine("");
                        sb.Append("        }");
                    }
                    //---
                    sb.AppendLine();
                    sb.AppendLine("    });");
                }

                sb.AppendLine();
            }

            // add successors
            if (aObject.ObjectType == ObjectType.FlowFragment ||
                aObject.ObjectType == ObjectType.Dialogue ||
                aObject.ObjectType == ObjectType.DialogueFragment ||
                aObject.ObjectType == ObjectType.Hub)
            {
                // collect output pin successors
                var successors = new List<Successor>();
                SearchDirectSuccessors(aObject, successors, null, null, 0);

                // write output pin successors to script
                sb.AppendLine();
                sb.AppendLine("    // successors");
                sb.AppendLine("    Successors = new Successor[]");
                sb.Append("    {");

                for (int s = 0; s < successors.Count; s++)
                {
                    var successor = successors[s];
                    sb.AppendLine(s == 0 ? "" : ",");
                    sb.AppendLine("      new Successor()");
                    sb.AppendLine("      {");
                    sb.Append("        StartPinLabel = \"").Append(successor.StartingPinLabel).AppendLine("\",");
                    sb.AppendLine("        Path = new Connection[]");
                    sb.Append("        {");

                    for (int pp = 0; pp < successor.Path.Count; pp++)
                    {
                        var pathPart = successor.Path[pp];
                        sb.AppendLine(pp == 0 ? "" : ",");
                        sb.Append("          new Connection(){ ConnectionLabel=\"").Append(pathPart.ConnectionLabel);
                        sb.Append("\", ConnectionColor=new Color(").Append(pathPart.ConnectionColor.R).Append(",").Append(
                            pathPart.ConnectionColor.G).
                            Append(",").Append(pathPart.ConnectionColor.B).Append("), PinLabel=\"").Append(pathPart.PinLabel).
                            Append("\", SubmergeLevel=").Append(pathPart.TargetSubmergeLevel).Append("}");
                    }

                    sb.AppendLine();
                    sb.AppendLine("        },");
                    sb.Append("        Target=\"").Append(successor.Target != null ? successor.Target.GetTechnicalName() : "").AppendLine("\",");
                    sb.Append("        TargetSubmergeLevel=").Append(successor.TargetSubmergeLevel).AppendLine("");
                    sb.Append("      }");
                }

                sb.AppendLine();
                sb.AppendLine("    };");
            }

            // add Unity file path for articy assets
            if (aObject.ObjectType == ObjectType.Asset)
            {
                var unityFullFilename = FileUtils.GetPath(aFullFilename) + aObject[ObjectPropertyNames.AssetFilename];
                var unityResourcePath = FileUtils.GetPath(aFullFilename.Substring(mResourcesPath.Length + 1));
                var unityResourceName = unityResourcePath.Replace("\\", "/") + FileUtils.GetBasename(unityFullFilename);

                // write extra path to script
                sb.AppendLine();
                sb.Append("    UnityFullFilename = @\"").Append(unityFullFilename).AppendLine("\";");
                sb.Append("    UnityResourceName = @\"").Append(unityResourceName).AppendLine("\";");
            }

            // assemble end of script
            sb.AppendLine("  }");
            sb.AppendLine("}");

            return sb;
        }

        private static string GetPropertyPart(string aFullPropertyName)
        {
            var segments = aFullPropertyName.Split('.');
            int cutPos = segments[0].Length;
            return aFullPropertyName.Substring(cutPos + 1);
        }

        private static string GetTemplateOrBaseClassName(ObjectProxy aObject)
        {
            var templateName = GetTemplateTechnicalName(aObject);
            return String.IsNullOrEmpty(templateName) ? GetBaseClassName(aObject) : "Template" + templateName;
        }

        private static string GetTemplateTechnicalName(ObjectProxy aObject)
        {
            try
            {
                var tempProps = aObject.GetAvailableProperties(PropertyFilter.Custom);
                return tempProps.Count > 0 ? aObject.GetTemplateTechnicalName() : null;
            }
            catch
            {
                return null;
            }
        }

        private void SearchDirectSuccessors(ObjectProxy aObject, IList<Successor> aSuccessors, Successor aSuccessor, ObjectProxy aInnerPin, int aSubmergeLevel)
        {
            // determine search mode
            IList<ObjectProxy> pins, connections;
            if (aInnerPin == null)
            {
                // find connections emerging from all output pins of the object
                pins = aObject.GetOutputPins();
                connections = SortOutNonConnections(aObject.GetParent().GetChildren());
            }
            else
            {
                // find connections emerging from the inner input pin of the object 
                pins = new List<ObjectProxy>();
                pins.Add(aInnerPin);
                connections = SortOutNonConnections(aObject.GetChildren());
            }

            // prepare cloning successor
            var cloningTemplate = new List<Connection>();
            if (aSuccessor != null)
            {
                cloningTemplate.AddRange(aSuccessor.Path);
            }

            // iterate over pins
            foreach (var pin in pins)
            {
                var cc = 0;
                foreach (var connection in connections)
                {
                    var target = connection[ObjectPropertyNames.Target] as ObjectProxy;
                    var targetPin = connection[ObjectPropertyNames.TargetPin] as ObjectProxy;

                    if (pin.Id == (connection[ObjectPropertyNames.SourcePin] as ObjectProxy).Id)
                    {
                        Successor successor = aSuccessor;
                        if (successor == null)
                        {
                            // create a new successor
                            successor = new Successor
                            {
                                StartingPinLabel = pin.GetDisplayName(),
                                Path = new List<Connection>()
                            };
                            aSuccessors.Add(successor);
                        }
                        else if (cc >= 1)
                        {
                            // clone the exisiting successor for branching
                            successor = new Successor()
                            {
                                StartingPinLabel = aSuccessor.StartingPinLabel,
                                Path = new List<Connection>()
                            };
                            successor.Path.AddRange(cloningTemplate);
                            aSuccessors.Add(successor);
                        }

                        // add new element to the connection path
                        var newConnection = new Connection()
                        {
                            ConnectionColor = (Color)connection[ObjectPropertyNames.Color],
                            ConnectionLabel = connection[ObjectPropertyNames.Label] as string,
                            Target = target,
                            TargetSubmergeLevel = aSubmergeLevel,
                            PinIndex = (int)targetPin[ObjectPropertyNames.PinIndex],
                            PinLabel = targetPin.GetDisplayName()
                        };
                        successor.Path.Add(newConnection);

                        // connection path arrived at an input pin
                        if ((int)targetPin[ObjectPropertyNames.PinType] == 1 /*input*/ )
                        {
                            // a jump node finalizes the connection path
                            if (target.ObjectType == ObjectType.Jump)
                            {
                                var jumpTarget = target[ObjectPropertyNames.Target] as ObjectProxy;
                                var jumpTargetPin = target[ObjectPropertyNames.TargetPin] as ObjectProxy;
                                if (jumpTarget != null && jumpTargetPin != null)
                                {
                                    var jumpConnection = new Connection()
                                    {
                                        Target = jumpTarget,
                                        TargetSubmergeLevel = aSubmergeLevel,
                                        PinIndex = (int)jumpTargetPin[ObjectPropertyNames.PinIndex],
                                        PinLabel = jumpTargetPin.GetDisplayName()
                                    };
                                    successor.Path.Add(jumpConnection);
                                    successor.Target = jumpConnection.Target;
                                    successor.TargetPinIndex = jumpConnection.PinIndex;
                                    successor.TargetSubmergeLevel = aSubmergeLevel;
                                }
                                else
                                {
                                    // jump node has no target
                                    successor.Target = target;
                                    successor.TargetPinIndex = (int)targetPin[ObjectPropertyNames.PinIndex];
                                    successor.TargetSubmergeLevel = aSubmergeLevel;
                                }
                            }

                            // connection submerges into a flow fragment or dialog
                            else if (target.CanHaveChildren && target.GetChildren().Count > 0)
                            {
                                successor.Target = target;
                                successor.TargetPinIndex = (int)targetPin[ObjectPropertyNames.PinIndex];
                                successor.TargetSubmergeLevel = aSubmergeLevel;
                                SearchDirectSuccessors(target, aSuccessors, successor, targetPin, aSubmergeLevel + 1);
                            }

                            // connection path arrived at its final destination
                            else
                            {
                                successor.Target = target;
                                successor.TargetPinIndex = (int)targetPin[ObjectPropertyNames.PinIndex];
                                successor.TargetSubmergeLevel = aSubmergeLevel;
                            }
                        }

                        // connection path arrived at an output pin
                        else
                        {
                            successor.Target = target;
                            successor.TargetPinIndex = (int)targetPin[ObjectPropertyNames.PinIndex];
                            successor.TargetSubmergeLevel = aSubmergeLevel;
                            SearchDirectSuccessors(target, aSuccessors, successor, null, aSubmergeLevel - 1);
                        }

                        // count handled relevant connections
                        cc++;
                    }
                }
            }
        }

        private IList<ObjectProxy> SortOutNonConnections(IList<ObjectProxy> aObjectList)
        {
            var outConns = new List<ObjectProxy>();
            for (int obj = aObjectList.Count - 1; obj >= 0; obj--)
            {
                var artObj = aObjectList[obj];
                if (artObj.ObjectType == ObjectType.Connection)
                {
                    outConns.Add(artObj);
                }
            }
            return outConns;
        }

        private static void AddPropertyValue(ObjectProxy aObject, string aPropertyName, StringBuilder aScript)
        {
            // default data type
            var dataType = aObject.GetPropertyInfo(aPropertyName).DataType;

            // asset exceptions
            if (aObject.ObjectType == ObjectType.Asset)
            {
                if (aPropertyName == ObjectPropertyNames.AbsoluteFilePath ||
                    aPropertyName == ObjectPropertyNames.FilePath ||
                    aPropertyName == ObjectPropertyNames.AssetFilename ||
                    aPropertyName == ObjectPropertyNames.OriginalSource)
                {
                    aScript.Append("@\"").Append((string)(aObject[aPropertyName] ?? "")).Append("\"");
                    return;
                }
            }

            // other exceptions
            // (currently none)

            // handling of data type
            switch (dataType)
            {
                case PropertyDataType.Text:
                case PropertyDataType.DateTime:
                case PropertyDataType.Unknown:
                    aScript.Append("\"").Append(EscapeScript((string)(aObject[aPropertyName] ?? ""))).Append("\"");
                    break;

                case PropertyDataType.Float:
                    double floatNumber = (double)(aObject[aPropertyName] ?? 0.0);
                    aScript.Append(floatNumber.ToString("F", System.Globalization.CultureInfo.InvariantCulture));
                    break;

                case PropertyDataType.Integer:
                case PropertyDataType.Enum:
                    var number = (aObject[aPropertyName] ?? 0);
                    var text = aPropertyName == ObjectPropertyNames.ShortId ? String.Format("0x{0:X8}", number) : number.ToString();
                    aScript.Append(text);
                    break;

                case PropertyDataType.ObjectReference:
                    var refSlot = (ObjectProxy)aObject[aPropertyName];
                    var refName = (refSlot == null || refSlot.GetTechnicalName() == null) ? "" : refSlot.GetTechnicalName();
                    aScript.Append("\"").Append(refName).Append("\"");
                    break;

                case PropertyDataType.ObjectList:
                    var references = (IList<ObjectProxy>)aObject[aPropertyName];
                    aScript.Append("new string[] {");
                    for (int r = 0; r < references.Count; r++)
                    {
                        if (r > 0)
                            aScript.Append(", ");
                        aScript.Append("\"").Append(references[r].GetTechnicalName()).Append("\"");
                    }
                    aScript.Append("}");
                    break;

                case PropertyDataType.Boolean:
                    var boolVal = (bool)(aObject[aPropertyName] ?? false);
                    aScript.Append(boolVal ? "true" : "false");
                    break;

                case PropertyDataType.Color:
                    var color = (Color)aObject[aPropertyName];
                    aScript.Append("new Color(").Append(color.R).Append(",").Append(color.G).Append(",").Append(color.B).Append(")");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string EscapeScript(string aText)
        {
            return aText.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static void AddPropertyType(ObjectProxy aObject, string aPropertyName, StringBuilder aScript)
        {
            // default data type
            var dataType = aObject.GetPropertyInfo(aPropertyName).DataType;

            // exceptions
            // (currently none)

            switch (dataType)
            {
                case PropertyDataType.Text:
                case PropertyDataType.DateTime:
                case PropertyDataType.Unknown:
                    aScript.Append("string");
                    break;

                case PropertyDataType.Integer:
                    aScript.Append("long");
                    break;

                case PropertyDataType.Float:
                    aScript.Append("double");
                    break;

                case PropertyDataType.ObjectReference:
                    aScript.Append("string");
                    break;

                case PropertyDataType.ObjectList:
                    aScript.Append("string[]");
                    break;

                case PropertyDataType.Enum:
                    aScript.Append("int");
                    break;

                case PropertyDataType.Boolean:
                    aScript.Append("bool");
                    break;

                case PropertyDataType.Color:
                    aScript.Append("Color");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateOrUpdateBaseClassScript(DirectoryInfo aFolder, ObjectProxy aTemplateObject, string aUnityBaseClass)
        {
            var baseClassName = GetBaseClassName(aTemplateObject);

            if (mWrittenBaseClasses.Contains(baseClassName))
            {
                return;
            }

            var scriptText = AssembleBaseClassScript(aTemplateObject, aUnityBaseClass);

            var fullFilename = aFolder.FullName + "\\" + baseClassName + ".cs";
            using (StreamWriter outfile = new StreamWriter(fullFilename))
            {
                outfile.Write(scriptText.ToString());
            }

            mWrittenBaseClasses.Add(baseClassName);
            mBaseClassBases.Add(baseClassName, aUnityBaseClass);

            //mForm.ShowInfo( exists ? "...updated '{0}'" : "...created '{0}'", fullFilename );
        }

        private static StringBuilder AssembleBaseClassScript(ObjectProxy aTemplateObject, string aUnityBaseClass)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine();
            sb.Append("public class ").Append(GetBaseClassName(aTemplateObject)).Append(" : ").AppendLine(aUnityBaseClass);
            sb.AppendLine("{");

            // property definition
            foreach (var property in aTemplateObject.GetAvailableProperties(PropertyFilter.Base))
            {
                sb.Append("  public ");
                AddPropertyType(aTemplateObject, property, sb);
                sb.Append(" ").Append(property).AppendLine(";");
            }



            // successors definition
            if (aTemplateObject.ObjectType == ObjectType.FlowFragment ||
                aTemplateObject.ObjectType == ObjectType.Dialogue ||
                aTemplateObject.ObjectType == ObjectType.DialogueFragment ||
                aTemplateObject.ObjectType == ObjectType.Hub)
            {
                sb.AppendLine();
                sb.AppendLine("  public Successor[] Successors;");
            }

            // children definition
            if (aTemplateObject.ObjectType == ObjectType.FlowFragment ||
                aTemplateObject.ObjectType == ObjectType.Dialogue)
            {
                sb.AppendLine();
                sb.AppendLine("  public Hashtable InputPinChildren;");
            }

            // add Unity file path for articy assets
            if (aTemplateObject.ObjectType == ObjectType.Asset)
            {
                sb.AppendLine();
                sb.AppendLine("  public string UnityFullFilename;");
                sb.AppendLine("  public string UnityResourceName;");
            }

            // close class defintion
            sb.AppendLine("}");

            return sb;
        }

        private static string GetBaseClassName(ObjectProxy aObject)
        {
            return "Articy" + aObject.ObjectType;
        }

        private void CreateOrUpdateTemplateClassScripts(DirectoryInfo aFolder, ObjectProxy aTemplateObject, Dictionary<string, List<string>> aFeatures)
        {
            // check if template class script needs to be created at all
            var templateName = GetTemplateTechnicalName(aTemplateObject);
            if (String.IsNullOrEmpty(templateName) ||
                mWrittenTemplateClasses.Contains(templateName))
                return;

            var scriptText = AssembleTemplateClassScript(aTemplateObject, templateName, aFeatures);

            var fullFilename = aFolder.FullName + "\\Template" + templateName + ".cs";
            using (StreamWriter outfile = new StreamWriter(fullFilename))
            {
                outfile.Write(scriptText.ToString());
            }

            mWrittenTemplateClasses.Add(templateName);
            //mForm.ShowInfo( exists ? "...updated '{0}'" : "...created '{0}'", fullFilename );
        }

        private static StringBuilder AssembleTemplateClassScript(ObjectProxy aTemplateObject, string aTemplateName, Dictionary<string, List<string>> aFeatures)
        {
            var sb = new StringBuilder();

            // write script header
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.Append("public class Template").Append(aTemplateName).Append(" : ").AppendLine(GetBaseClassName(aTemplateObject));
            sb.Append("{");

            // write feature property definitions to script
            foreach (var featureName in aFeatures.Keys)
            {
                var featureProperties = aFeatures[featureName];

                sb.AppendLine();
                sb.Append("  // properties from feature '").Append(featureName).AppendLine("'");
                foreach (var propertyName in featureProperties)
                {
                    sb.Append("  public ");
                    AddPropertyType(aTemplateObject, propertyName, sb);
                    sb.Append(" ").Append(GetFeaturePart(propertyName)).Append("_").Append(GetPropertyPart(propertyName)).AppendLine(";");
                }
            }

            // write script footer
            sb.AppendLine("}");

            return sb;
        }

        private void ExportFlowFolder(ObjectProxy aFlowFolder, DirectoryInfo aFlowDir)
        {
            // create fixed flow classes
            CreateOrUpdateConnectionScript(mClassDir);
            CreateOrUpdateSuccessorScript(mClassDir);

            var children = aFlowFolder.GetChildren();
            foreach (var child in children)
            {
                var featureProperties = GetFeatureProperties(child);

                switch (child.ObjectType)
                {
                    case ObjectType.FlowFragment:
                    case ObjectType.Dialogue:
                        parser.ProcessDialogues(child);
                        break;

                    case ObjectType.DialogueFragment:
                    case ObjectType.Hub:
                    case ObjectType.Jump:
                        CreateOrUpdateBaseClassScript(mClassDir, child, SCRIPTABLE_OBJECT);
                        CreateOrUpdateTemplateClassScripts(mClassDir, child, featureProperties);
                        CreateOrUpdateScriptFile(aFlowDir, child, featureProperties);
                        break;
                }
            }
        }

        private Dictionary<string, List<string>> GetFeatureProperties(ObjectProxy aObject)
        {
            // sort properties by feature
            var features = new Dictionary<string, List<string>>();
            foreach (var property in aObject.GetAvailableProperties(PropertyFilter.Custom))
            {
                var featureName = GetFeaturePart(property);
                List<string> featureProperties;
                if (features.ContainsKey(featureName))
                {
                    featureProperties = features[featureName];
                }
                else
                {
                    featureProperties = new List<string>();
                    features.Add(featureName, featureProperties);
                }
                featureProperties.Add(property);
            }

            return features;
        }

        public static string GetFeaturePart(string aFullPropertyName)
        {
            var segments = aFullPropertyName.Split('.');
            int cutPos = segments[0].Length;
            return aFullPropertyName.Substring(0, cutPos);
        }

        private static void CreateOrUpdateConnectionScript(DirectoryInfo aFolder)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.Append("public class Connection : ").AppendLine(SCRIPTABLE_OBJECT);
            sb.AppendLine("{");

            sb.AppendLine("  public string ConnectionLabel;");
            sb.AppendLine("  public Color ConnectionColor;");
            sb.AppendLine("  public string PinLabel;");
            sb.AppendLine("  public int PinIndex;");
            sb.AppendLine("  public int SubmergeLevel;  // '+1' if the connection is one level deeper than the start, '-1' if it is one level higher");

            // close class defintion
            sb.AppendLine("}");

            // write file
            var fullFilename = aFolder.FullName + "\\Connection.cs";
            using (StreamWriter outfile = new StreamWriter(fullFilename))
            {
                outfile.Write(sb.ToString());
            }
        }

        private static void CreateOrUpdateSuccessorScript(DirectoryInfo aFolder)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.Append("public class Successor : ").AppendLine(SCRIPTABLE_OBJECT);
            sb.AppendLine("{");

            sb.AppendLine("  public string StartPinLabel;");
            sb.AppendLine("  public Connection[] Path;");
            sb.AppendLine("  public string Target;");
            sb.AppendLine("  public int TargetPinIndex;");
            sb.AppendLine("  public int TargetSubmergeLevel;  // '+1' if the target is one level deeper than the start element, '-1' if it is one level higher");

            // close class defintion
            sb.AppendLine("}");

            // write file
            var fullFilename = aFolder.FullName + "\\Successor.cs";
            using (StreamWriter outfile = new StreamWriter(fullFilename))
            {
                outfile.Write(sb.ToString());
            }
        }

    }

    public class Successor
    {
        public string StartingPinLabel;
        public List<Connection> Path;
        public ObjectProxy Target;
        public int TargetPinIndex;
        public int TargetSubmergeLevel;
    }

    public class Connection
    {
        public string ConnectionLabel;
        public Color ConnectionColor;
        public string PinLabel;
        public int PinIndex;
        public ObjectProxy Target;
        public int TargetSubmergeLevel;
    }
}