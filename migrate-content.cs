// this script is used to "patch" a folder that was copied from the core specification

/*
* Copy folder into resources/resourcename
* delete *-spreadsheet.*
* delete *-header.xml (report if any existed - and had any significant content)
* delete *-mapping-exceptions.xml
* delete *.gen.svg (*keep the plain svg file)
* delete *-packs.xml
* rename resourcename.svg to ResourceName.svg (fixing case sensitivity)
* rename structuredefinition-ResourceName.xml to StructureDefinition-ResourceName.xml
* move `*-introduction.xml` and `*-notes.xml` over to the pagecontent folder
    - and rename to StructureDefinition-ResourceName-intro.xml
    - and rename to StructureDefinition-ResourceName-notes.xml
    - update their content to tweak the header levels h2 -> h3 etc
*/

string sourceFolderPath = args[0];
string targetFolderPath = args[1];

var resourceName = GetResourceName(sourceFolderPath);
if (resourceName == null)
{
    Console.WriteLine($"ERROR: Could not determine resource name for folder: {sourceFolderPath}");
    return;
}

targetFolderPath = Path.Combine(targetFolderPath, resourceName.ToLower());
if (!System.IO.Directory.Exists(targetFolderPath))
{
    System.IO.Directory.CreateDirectory(targetFolderPath);
}
Console.WriteLine($"Migrating folder: {sourceFolderPath} to {targetFolderPath}");


Console.WriteLine($"  Resource Name: {resourceName}");

string[] files = Directory.GetFiles(sourceFolderPath);
foreach (string file in files)
{
    string fileName = Path.GetFileName(file);
    string? outputName = OutputFileName(file, resourceName ?? "");
    Console.WriteLine($"  Processing file: {fileName} => {outputName ?? "SKIPPED"}");
    if (outputName != null)
    {
        System.IO.File.Copy(file, outputName, true);
        if (outputName.EndsWith("-intro.xml") || outputName.EndsWith("-notes.xml"))
        {
            PatchNarrative(outputName);
        }
    }
}


// -------------------------------------------------------
// Helper functions
// -------------------------------------------------------
string? GetResourceName(string path)
{
    // get the list of files in the folder, then check for a file called "structuredefinition-XXX.xml" and return the XXX
    string[] files = Directory.GetFiles(path);
    foreach (string file in files)
    {
        string fileName = Path.GetFileName(file);
        if (fileName.StartsWith("structuredefinition-") && fileName.EndsWith(".xml"))
        {
            return fileName.Substring("structuredefinition-".Length, fileName.Length - "structuredefinition-".Length - ".xml".Length);
        }
    }
    return null;
}

string? OutputFileName(string path, string resourceName)
{
    string fileName = Path.GetFileName(path);
    if (fileName.EndsWith(".gen.svg"))
    {
        return null; // skip
    }
    if (fileName.Contains("-spreadsheet.")
        || fileName.EndsWith("-header.xml")
        || fileName.EndsWith("-mapping-exceptions.xml")
        || fileName.EndsWith(".gen.svg")
        || fileName.EndsWith("-packs.xml"))
    {
        return null; // skip
    }
    var fi = new FileInfo(path);
    if (fi.Extension.Contains(" ") || fi.Extension == "tmp" || fi.Extension == "bak")
    {
        // skip temp files or files with spaces in the extension (weird)
        return null;
    }
    if (fileName.ToLower().StartsWith($"{resourceName.ToLower()}.svg"))
    {
        return Path.Combine("input", "resources", resourceName.ToLower(), $"{resourceName}.svg");
    }
    if (fileName.ToLower().StartsWith($"structuredefinition-{resourceName.ToLower()}.xml"))
    {
        return Path.Combine("input","resources", resourceName.ToLower(), $"StructureDefinition-{resourceName}.xml");
    }
    if (fileName.ToLower().EndsWith("-introduction.xml"))
    {
        return Path.Combine("input", "pagecontent", $"StructureDefinition-{resourceName}-intro.xml");
    }
    if (fileName.ToLower().EndsWith("-notes.xml"))
    {
        return Path.Combine("input", "pagecontent", $"StructureDefinition-{resourceName}-notes.xml");
    }
    return Path.Combine("input", "resources", resourceName.ToLower(), fileName);
}

void PatchNarrative(string filePath)
{
    string content = File.ReadAllText(filePath);
    content = content.Replace("<h6>", "<h7>");
    content = content.Replace("<h5>", "<h6>");
    content = content.Replace("<h4>", "<h5>");
    content = content.Replace("<h3>", "<h4>");
    content = content.Replace("<h2>", "<h3>");
    content = content.Replace("<h1>", "<h2>");

    content = content.Replace("</h6>", "</h7>");
    content = content.Replace("</h5>", "</h6>");
    content = content.Replace("</h4>", "</h5>");
    content = content.Replace("</h3>", "</h4>");
    content = content.Replace("</h2>", "</h3>");
    content = content.Replace("</h1>", "</h2>");

    File.WriteAllText(filePath, content);
    Console.WriteLine("  Patched narrative in: " + filePath);
}
