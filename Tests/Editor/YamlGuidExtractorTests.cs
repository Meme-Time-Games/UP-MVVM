using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class YamlGuidExtractorTests
    {
        private const string PlayerDiedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ScriptGuid = "cccccccccccccccccccccccccccccccc";

        [Test]
        public void GetGuidsFromYaml_WhenYamlIsNull_ReturnsEmptyCollection()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(null);

            Assert.IsEmpty(guids);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasNoReferences_ReturnsEmptyCollection()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml("m_Name: Hud\nm_Enabled: 1\n");

            Assert.IsEmpty(guids);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasOneReference_ReturnsThatGuid()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string yaml = "  _eventViewModelSo: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(yaml);

            Assert.AreEqual(new[] { PlayerDiedGuid }, guids.ToArray());
        }

        [Test]
        public void GetGuidsFromYaml_WhenTheSameGuidAppearsTwice_ReturnsItOnce()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string reference = "  _event: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(reference + reference);

            Assert.AreEqual(1, guids.Count);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasScriptAndAssetReferences_ReturnsBoth()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string yaml =
                "  m_Script: {fileID: 11500000, guid: " + ScriptGuid + ", type: 3}\n" +
                "  _eventViewModelSo: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(yaml);

            Assert.AreEqual(2, guids.Count);
        }
    }
}
