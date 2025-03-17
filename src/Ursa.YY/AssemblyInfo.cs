using System.Runtime.CompilerServices;
using Avalonia.Metadata;

[assembly:InternalsVisibleTo("HeadlessTest.Ursa")]
[assembly:InternalsVisibleTo("Test.Ursa")]
[assembly:XmlnsPrefix("https://irihi.tech/ursa", "u")]
[assembly:XmlnsDefinition("https://irihi.tech/ursa", "Ursa")]
[assembly:XmlnsDefinition("https://irihi.tech/ursa", "Ursa.Controls")]