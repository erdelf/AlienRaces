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
            AlienPartGenerator.ExtendedBackstoryGraphic testBackstoryGraphic =
                new AlienPartGenerator.ExtendedBackstoryGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_backstory");

            // Attempt to parse XML
            testBackstoryGraphic.LoadDataFromXmlCustom(testXmlNode);

            //Assert.AreEqual("Test_backstory", testBackstoryGraphic.backstory); // name loaded via crossref
            Assert.AreEqual("test/B",       testBackstoryGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomAgeGraphicXML()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");

            AlienPartGenerator.ExtendedAgeGraphic testAgeGraphic =
                new AlienPartGenerator.ExtendedAgeGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_backstory/ageGraphics/HumanlikeAdult");

            // Attempt to parse XML
            testAgeGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(humanlikeAdultLifeStageDef, testAgeGraphic.age);
            Assert.AreEqual("test/BA", testAgeGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomDamageGraphicXML()
        {
            AlienPartGenerator.ExtendedDamageGraphic testDamageGraphic =
                new AlienPartGenerator.ExtendedDamageGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_backstory/ageGraphics/HumanlikeAdult/damageGraphics/a5");

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
            AddHediffWithName("Plague");
            AddLifestageWithName("HumanlikeAdult");

            AlienPartGenerator.ExtendedHediffGraphic extendedHediffGraphic =
                new AlienPartGenerator.ExtendedHediffGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack");

            // Attempt to parse XML
            extendedHediffGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(crackHediffDef, extendedHediffGraphic.hediff);
            Assert.AreEqual("test/C", extendedHediffGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomHediffSeverityGraphicXML()
        {
            AddLifestageWithName("HumanlikeAdult");
            AlienPartGenerator.ExtendedHediffSeverityGraphic extendedHediffSeverityGraphic =
                new AlienPartGenerator.ExtendedHediffSeverityGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack/hediffGraphics/Plague/severity/a0.5");

            // Attempt to parse XML
            extendedHediffSeverityGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreEqual(0.5f,        extendedHediffSeverityGraphic.severity);
            Assert.AreEqual("test/CPs5", extendedHediffSeverityGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomGenderGraphicXML()
        {
            AlienPartGenerator.ExtendedGenderGraphic testGenderGraphic =
                new AlienPartGenerator.ExtendedGenderGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/genderGraphics/Male");

            // Attempt to parse XML
            testGenderGraphic.LoadDataFromXmlCustom(testXmlNode);
         
            Assert.AreEqual(Gender.Male, testGenderGraphic.GetGender);
            Assert.AreEqual("test/M", testGenderGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomGenderGraphicXMLWithWrongCase()
        {
            AlienPartGenerator.ExtendedGenderGraphic testGenderGraphic =
                new AlienPartGenerator.ExtendedGenderGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/genderGraphics/feMale");

            // Attempt to parse XML
            testGenderGraphic.LoadDataFromXmlCustom(testXmlNode);
         
            Assert.AreEqual(Gender.Female, testGenderGraphic.GetGender);
            Assert.AreEqual("test/f",    testGenderGraphic.GetPath());
        }

        [Test]
        public void TestInvalidGenderIsNeverApplicable()
        {
            AlienPartGenerator.ExtendedGenderGraphic testGenderGraphic =
                new AlienPartGenerator.ExtendedGenderGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/genderGraphics/NotARealGender");

            // Attempt to parse XML
            testGenderGraphic.LoadDataFromXmlCustom(testXmlNode);
         
            // Gender is an enum so is always non-null and defaults to "None" if an invalid gender is given
            Assert.AreEqual(Gender.None, testGenderGraphic.gender);
            // GetGender provides a safer access, explicitly allowing nulls 
            Assert.IsNull(testGenderGraphic.GetGender);
            Assert.AreEqual("test/Narg", testGenderGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomTraitGraphicXML()
        {
            AlienPartGenerator.ExtendedTraitGraphic testTraitGraphic =
                new AlienPartGenerator.ExtendedTraitGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/genderGraphics/Male/traitGraphics/Brawler");

            // Attempt to parse XML
            testTraitGraphic.LoadDataFromXmlCustom(testXmlNode);
         
            Assert.AreEqual("Brawler", testTraitGraphic.trait);
            Assert.AreEqual("test/MB",    testTraitGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomBodytypeGraphicXML()
        {
            AlienPartGenerator.ExtendedBodytypeGraphic testBodytypeGraphic =
                new AlienPartGenerator.ExtendedBodytypeGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/genderGraphics/Male/bodytypeGraphics/Thin");

            // Attempt to parse XML
            testBodytypeGraphic.LoadDataFromXmlCustom(testXmlNode);

            //Assert.AreEqual("Thin", testBodytypeGraphic.bodytype); // loaded via crossref
            Assert.AreEqual("test/MT",    testBodytypeGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomBackstorySubtree()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");
            
            // Select test node
            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_backstory");

            // Attempt to parse XML
            AlienPartGenerator.ExtendedBackstoryGraphic parsedGraphic = DirectXmlToObject.ObjectFromXml<AlienPartGenerator.ExtendedBackstoryGraphic>(testXmlNode, false);
            
            // Reflectively populate all the XRefs
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            //Assert.AreEqual("Test_backstory", parsedGraphic.backstory); // name loaded via crossref
            Assert.AreEqual("test/B",       parsedGraphic.GetPath());
            Assert.IsNotNull(parsedGraphic.ageGraphics);
            Assert.AreEqual(1, parsedGraphic.ageGraphics.Count);

            AlienPartGenerator.ExtendedAgeGraphic parsedAgeGraphic = parsedGraphic.ageGraphics[0];
            Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
            Assert.AreEqual("test/BA", parsedAgeGraphic.GetPath());
            Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
            Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);

            AlienPartGenerator.ExtendedDamageGraphic parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
            Assert.AreEqual("test/BAd1", parsedDamageGraphic1.GetPath());
            
            AlienPartGenerator.ExtendedDamageGraphic parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
            Assert.AreEqual(5f,          parsedDamageGraphic5.damage);
            Assert.AreEqual("test/BAd5", parsedDamageGraphic5.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomWholeAddon()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");
            HediffDef crackHediffDef = AddHediffWithName("Crack");
            HediffDef plagueHediffDef = AddHediffWithName("Plague");
            
            // Select test node
            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]");

            // Attempt to parse XML
            AlienPartGenerator.BodyAddon parsedGraphic = DirectXmlToObject.ObjectFromXml<AlienPartGenerator.BodyAddon>(testXmlNode, false);
            
            // Reflectively populate all the XRefs
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
            
            Assert.AreEqual("Nose", parsedGraphic.bodyPart);
            Assert.IsTrue(parsedGraphic.inFrontOfBody);
            Assert.IsTrue(parsedGraphic.alignWithHead);
            Assert.AreEqual("base", parsedGraphic.ColorChannel);
            Assert.AreEqual("test/default", parsedGraphic.path);
            
            // Hediff Graphics
            Assert.IsNotNull(parsedGraphic.hediffGraphics);
            Assert.AreEqual(1, parsedGraphic.hediffGraphics.Count);
            
                // Crack
                AlienPartGenerator.ExtendedHediffGraphic parsedCrackGraphic = parsedGraphic.hediffGraphics[0];
                Assert.AreEqual("test/C", parsedCrackGraphic.GetPath());
                Assert.AreSame(crackHediffDef, parsedCrackGraphic.hediff);
                
                // Crack Hediff Graphics
                Assert.IsNotNull(parsedCrackGraphic.hediffGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.hediffGraphics.Count);
                
                    // Crack->Plague
                    AlienPartGenerator.ExtendedHediffGraphic parsedPlagueGraphic = parsedCrackGraphic.hediffGraphics[0];
                    Assert.AreEqual("test/CP", parsedPlagueGraphic.GetPath());
                    Assert.AreSame(plagueHediffDef, parsedPlagueGraphic.hediff);
                    
                    // Crack->Plague->Severity Graphics
                    Assert.IsNotNull(parsedPlagueGraphic.severity);
                    Assert.AreEqual(1, parsedPlagueGraphic.severity.Count);
                    
                        AlienPartGenerator.ExtendedHediffSeverityGraphic parsedSeverityGraphic = parsedPlagueGraphic.severity[0];
                        Assert.AreEqual("test/CPs5", parsedSeverityGraphic.GetPath());
                        Assert.AreEqual(0.5f, parsedSeverityGraphic.severity);
                        
                        // Crack->Plague->Severity->Backstory Graphics
                        Assert.IsNotNull(parsedSeverityGraphic.backstoryGraphics);
                        Assert.AreEqual(1, parsedSeverityGraphic.backstoryGraphics.Count);
                        
                            AlienPartGenerator.ExtendedBackstoryGraphic parsedBackstoryGraphic = parsedSeverityGraphic.backstoryGraphics[0];
                            //Assert.AreEqual("Test_backstory", parsedBackstoryGraphic.backstory); //linked via def
                            Assert.AreEqual("test/CPs5B", parsedBackstoryGraphic.GetPath());
                            
                            // Crack->Plague->Severity->Backstory->Age Graphics
                            Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                            Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                            
                                AlienPartGenerator.ExtendedAgeGraphic parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                                Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                                Assert.AreEqual("test/CPs5BA", parsedAgeGraphic.GetPath());
                                
                                // Crack->Plague->Severity->Backstory->Age->Damage Graphics
                                Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                                Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                                
                                    AlienPartGenerator.ExtendedDamageGraphic parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                                    Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                                    Assert.AreEqual("test/CPs5BAd1", parsedDamageGraphic1.GetPath());
                                    
                                    AlienPartGenerator.ExtendedDamageGraphic parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                                    Assert.AreEqual(5f,          parsedDamageGraphic5.damage);
                                    Assert.AreEqual("test/CPs5BAd5", parsedDamageGraphic5.GetPath());

                // Crack Backstory Graphics
                Assert.IsNotNull(parsedCrackGraphic.backstoryGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.backstoryGraphics.Count);
                
                    parsedBackstoryGraphic = parsedCrackGraphic.backstoryGraphics[0];
                    //Assert.AreEqual("Test_backstoryr", parsedBackstoryGraphic.backstory); //read via def
                    Assert.AreEqual("test/CB", parsedBackstoryGraphic.GetPath());
                            
                    // Crack->Backstory->Age Graphics
                    Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                    Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                    
                        parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                        Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                        Assert.AreEqual("test/CBA", parsedAgeGraphic.GetPath());
                        
                        // Crack->Backstory->Age->Damage Graphics
                        Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                        Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                        
                            parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                            Assert.AreEqual("test/CBAd1", parsedDamageGraphic1.GetPath());
                            
                            parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                            Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                            Assert.AreEqual("test/CBAd5", parsedDamageGraphic5.GetPath());
                            
                // Crack Age Graphics
                Assert.IsNotNull(parsedCrackGraphic.ageGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.ageGraphics.Count);
        
                    parsedAgeGraphic = parsedCrackGraphic.ageGraphics[0];
                    Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                    Assert.AreEqual("test/CA", parsedAgeGraphic.GetPath());
            
                    // Crack->Age->Damage Graphics
                    Assert.IsNull(parsedAgeGraphic.damageGraphics);
                        
                // Backstory Graphics
                Assert.IsNotNull(parsedGraphic.backstoryGraphics);
                Assert.AreEqual(1, parsedGraphic.backstoryGraphics.Count);
                
                    parsedBackstoryGraphic = parsedGraphic.backstoryGraphics[0];
                    //Assert.AreEqual("Test_backstory", parsedBackstoryGraphic.backstory); linked vi df
                    Assert.AreEqual("test/B", parsedBackstoryGraphic.GetPath());
                            
                    // Backstory->Age Graphics
                    Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                    Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                    
                        parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                        Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                        Assert.AreEqual("test/BA", parsedAgeGraphic.GetPath());
                        
                        // Backstory->Age->Damage Graphics
                        Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                        Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                        
                            parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                            Assert.AreEqual("test/BAd1", parsedDamageGraphic1.GetPath());
                            
                            parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                            Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                            Assert.AreEqual("test/BAd5", parsedDamageGraphic5.GetPath());
                            
                            
                // Age Graphics
                Assert.IsNotNull(parsedGraphic.ageGraphics);
                Assert.AreEqual(1, parsedGraphic.ageGraphics.Count);
                
                    parsedAgeGraphic = parsedGraphic.ageGraphics[0];
                    Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                    Assert.AreEqual("test/A", parsedAgeGraphic.GetPath());
                    
                    // Age->Damage Graphics
                    Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                    Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                    
                        parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                        Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                        Assert.AreEqual("test/Ad1", parsedDamageGraphic1.GetPath());
                        
                        parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                        Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                        Assert.AreEqual("test/Ad5", parsedDamageGraphic5.GetPath());
                        
                        
                // Age->Damage Graphics
                Assert.IsNotNull(parsedGraphic.damageGraphics);
                Assert.AreEqual(2, parsedGraphic.damageGraphics.Count);
                
                    parsedDamageGraphic1 = parsedGraphic.damageGraphics[0];
                    Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                    Assert.AreEqual("test/d1", parsedDamageGraphic1.GetPath());
                    
                    parsedDamageGraphic5 = parsedGraphic.damageGraphics[1];
                    Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                    Assert.AreEqual("test/d5", parsedDamageGraphic5.GetPath());
            
                // Gender Graphics
                Assert.IsNotNull(parsedGraphic.genderGraphics);
                Assert.AreEqual(3, parsedGraphic.genderGraphics.Count);
                
                    AlienPartGenerator.ExtendedGenderGraphic parsedGenderGraphic1 = parsedGraphic.genderGraphics[0];
                    // As it is represented by a primitive byte gender can't be null so it defaults to 0 which represents None 
                    Assert.AreEqual(Gender.None, parsedGenderGraphic1.gender);
                    // GetGender provides a safer access, explicitly allowing nulls 
                    Assert.IsNull(parsedGenderGraphic1.GetGender);
                    Assert.AreEqual("test/Narg", parsedGenderGraphic1.GetPath());
                    
                    AlienPartGenerator.ExtendedGenderGraphic parsedGenderGraphic2 = parsedGraphic.genderGraphics[1];
                    Assert.AreEqual(Gender.Male, parsedGenderGraphic2.GetGender);
                    Assert.AreEqual("test/M", parsedGenderGraphic2.GetPath());
                    
                    AlienPartGenerator.ExtendedGenderGraphic parsedGenderGraphic3 = parsedGraphic.genderGraphics[2];
                    Assert.AreEqual(Gender.Female, parsedGenderGraphic3.GetGender);
                    Assert.AreEqual("test/f", parsedGenderGraphic3.GetPath());
                    
                    // Gender->Trait Graphics
                    Assert.IsNotNull(parsedGenderGraphic2.traitGraphics);
                    Assert.AreEqual(2, parsedGenderGraphic2.traitGraphics.Count);

                        AlienPartGenerator.ExtendedTraitGraphic parsedTraitGraphic1 = parsedGenderGraphic2.traitGraphics[0];
                        Assert.AreEqual("Brawler", parsedTraitGraphic1.trait);
                        Assert.AreEqual("test/MB",    parsedTraitGraphic1.GetPath());
                        
                        AlienPartGenerator.ExtendedTraitGraphic parsedTraitGraphic2 = parsedGenderGraphic2.traitGraphics[1];
                        Assert.AreEqual("staggeringly ugly", parsedTraitGraphic2.trait);
                        Assert.AreEqual("test/MSu",    parsedTraitGraphic2.GetPath());

                    // Gender->Bodytype Graphics
                    Assert.IsNotNull(parsedGenderGraphic2.bodytypeGraphics);
                    Assert.AreEqual(1, parsedGenderGraphic2.bodytypeGraphics.Count);
                        
                        AlienPartGenerator.ExtendedBodytypeGraphic parsedBodytypeGraphic = parsedGenderGraphic2.bodytypeGraphics[0];
                        //Assert.AreEqual("Thin", parsedBodytypeGraphic.bodytype); //linked via def
                        Assert.AreEqual("test/MT",    parsedBodytypeGraphic.GetPath());

        }
    }
}
