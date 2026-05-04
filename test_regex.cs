using System.Text.RegularExpressions;

var line = args[0];
var regex = new Regex("^\\s*<system:String x:Key=\"([^\"]+)\">(.*)</system:String>\\s*$", RegexOptions.Compiled);

var match = regex.Match(line);
if (match.Success)
{
    Console.WriteLine("Key: " + match.Groups[1].Value);
    Console.WriteLine("Text: " + match.Groups[2].Value);
}
else
{
    Console.WriteLine("No match");
}
