namespace AlienRaceTest
{
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using AlienRace;
    using NUnit.Framework;
    using RimWorld;
    using TestSupport;
    using Verse;

    public class BodyAddonXMLTest : BaseUnityTest
    {
        private const string BODY_ADDONS_X_PATH =
            "/Defs/AlienRace.ThingDef_AlienRace/alienRace/generalSettings/alienPartGenerator/bodyAddons";

        private readonly XmlDocument testXmlRaceDef = new XmlDocument();

        [OneTimeSetUp]
        public void SetupXMLTests()
        {
            this.testXmlRaceDef.Load("testData/TestRace.xml");
            PrefsData prefsData = new PrefsData
                                  {
                                      logVerbose = false
                                  };
            InitPrefs(prefsData);
        }

        private XmlNode BodyAddonNodeMatching(string xPathFragment)
        {
            XmlNode testXmlNode = this.testXmlRaceDef.SelectSingleNode($"{BODY_ADDONS_X_PATH}/{xPathFragment}");
            Assert.IsNotNull(testXmlNode, nameof(testXmlNode) + " != null");
            Console.WriteLine($"Parsing:\n{XElement.Parse(testXmlNode.OuterXml)}");
            return testXmlNode;
        }

        [Test]
        public void TestCanParseCustomBackstoryGraphicXML()
        {
            AlienPartGenerator.BodyAddonBackstoryGraphic testBackstoryGraphic =
                new AlienPartGenerator.BodyAddonBackstoryGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar");

            // Attempt to parse XML
            testBackstoryGraphic.LoadDataFromXmlCustom(testXmlNode);

            Assert.AreEqual("Test_Templar", testBackstoryGraphic.backstory);
            Assert.AreEqual("test/B",       testBackstoryGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomAgeGraphicXML()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");

            AlienPartGenerator.BodyAddonAgeGraphic testAgeGraphic =
                new AlienPartGenerator.BodyAddonAgeGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar/ageGraphics/HumanlikeAdult");

            // Attempt to parse XML
            testAgeGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(humanlikeAdultLifeStageDef, testAgeGraphic.age);
            Assert.AreEqual("test/BA", testAgeGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomDamageGraphicXML()
        {
            AlienPartGenerator.BodyAddonDamageGraphic testDamageGraphic =
                new AlienPartGenerator.BodyAddonDamageGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar/ageGraphics/HumanlikeAdult/damageGraphics/a5");

            // Attempt to parse XML
            testDamageGraphic.LoadDataFromXmlCustom(testXmlNode);

            Assert.AreEqual(5f,          testDamageGraphic.damage);
            Assert.AreEqual("test/BAd5", testDamageGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomHediffGraphicXML()
        {
            // Setup XRefs
            HediffDef crackHediffDef = AddHediffWithName("Crack");

            AlienPartGenerator.BodyAddonHediffGraphic bodyAddonHediffGraphic =
                new AlienPartGenerator.BodyAddonHediffGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack");

            // Attempt to parse XML
            bodyAddonHediffGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(crackHediffDef, bodyAddonHediffGraphic.hediff);
            Assert.AreEqual("test/C", bodyAddonHediffGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomHediffSeverityGraphicXML()
        {
            AlienPartGenerator.BodyAddonHediffSeverityGraphic bodyAddonHediffSeverityGraphic =
                new AlienPartGenerator.BodyAddonHediffSeverityGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack/hediffGraphics/Plague/severity/a0.5");

            // Attempt to parse XML
            bodyAddonHediffSeverityGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreEqual(0.5f,        bodyAddonHediffSeverityGraphic.severity);
            Assert.AreEqual("test/CPs5", bodyAddonHediffSeverityGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomBackstorySubtree()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");
            
            // Select test node
            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar");

            // Attempt to parse XML
            AlienPartGenerator.BodyAddonBackstoryGraphic parsedGraphic = DirectXmlToObject.ObjectFromXml<AlienPartGenerator.BodyAddonBackstoryGraphic>(testXmlNode, false);
            
            // Reflectively populate all the XRefs
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
            
            Assert.AreEqual("Test_Templar", parsedGraphic.backstory);
            Assert.AreEqual("test/B",       parsedGraphic.GetPath());
            Assert.IsNotNull(parsedGraphic.ageGraphics);
            Assert.AreEqual(1, parsedGraphic.ageGraphics.Count);

            AlienPartGenerator.BodyAddonAgeGraphic parsedAgeGraphic = parsedGraphic.ageGraphics[0];
            Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
            Assert.AreEqual("test/BA", parsedAgeGraphic.GetPath());
            Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
            Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);

            AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
            Assert.AreEqual("test/BAd1", parsedDamageGraphic1.GetPath());
            
            AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
            Assert.AreEqual(5f,          parsedDamageGraphic5.damage);
            Assert.AreEqual("test/BAd5", parsedDamageGraphic5.GetPath());
        }
    }
}
