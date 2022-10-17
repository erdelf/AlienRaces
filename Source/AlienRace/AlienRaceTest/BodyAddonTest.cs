namespace AlienRaceTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AlienRace;
    using AlienRace.ExtendedGraphics;
    using Moq;
    using NUnit.Framework;
    using RimWorld;
    using TestSupport;
    using Verse;

    [TestFixture]
    public class BodyAddonTest : BaseUnityTest
    {
        private readonly Mock<LifeStageDef> mockHumanlikeAdultLifestageDef = new Mock<LifeStageDef>();
        private readonly Mock<LifeStageDef> mockOtherAdultLifestageDef     = new Mock<LifeStageDef>();
        private readonly Mock<HediffDef>    mockCutHediff                  = new Mock<HediffDef>();
        private readonly Mock<HediffDef>    mockBurnHediff                 = new Mock<HediffDef>();

        private AlienPartGenerator.BodyAddon GetTestBodyAddon()
        {
            AlienPartGenerator.ExtendedHediffGraphic cut = new AlienPartGenerator.ExtendedHediffGraphic
                                                            {
                                                                hediff = mockCutHediff.Object,
                                                                path   = "/hediffGraphics/cut",
                                                                ageGraphics =
                                                                    new List<AlienPartGenerator.ExtendedAgeGraphic>
                                                                    {
                                                                        new AlienPartGenerator.ExtendedAgeGraphic
                                                                        {
                                                                            age = mockHumanlikeAdultLifestageDef.Object,
                                                                            path = "/hediffGraphics/cut/age"
                                                                        }
                                                                    }
                                                            };

            AlienPartGenerator.ExtendedHediffGraphic burn = new AlienPartGenerator.ExtendedHediffGraphic
                                                             {
                                                                 hediff = mockBurnHediff.Object,
                                                                 path   = "/hediffGraphics/burn",
                                                                 ageGraphics =
                                                                     new List<AlienPartGenerator.ExtendedAgeGraphic>
                                                                     {
                                                                         new AlienPartGenerator.ExtendedAgeGraphic
                                                                         {
                                                                             age = mockHumanlikeAdultLifestageDef
                                                                             .Object,
                                                                             path =
                                                                                 "/hediffGraphics/burn/ageGraphics/humanlikeAdult"
                                                                         }
                                                                     },
                                                                 severity =
                                                                     new List<AlienPartGenerator.
                                                                         ExtendedHediffSeverityGraphic>
                                                                     {
                                                                         new AlienPartGenerator.
                                                                         ExtendedHediffSeverityGraphic
                                                                         {
                                                                             path = "/hediffGraphics/burn/severity/a0",
                                                                             severity = 0f,
                                                                             ageGraphics =
                                                                                 new List<AlienPartGenerator.
                                                                                     ExtendedAgeGraphic>
                                                                                 {
                                                                                     new AlienPartGenerator.
                                                                                     ExtendedAgeGraphic
                                                                                     {
                                                                                         age =
                                                                                             mockHumanlikeAdultLifestageDef
                                                                                             .Object,
                                                                                         path =
                                                                                             "/hediffGraphics/burn/severity/a0/ageGraphics/humanlikeAdult"
                                                                                     }
                                                                                 }
                                                                         }
                                                                     }
                                                             };

            AlienPartGenerator.ExtendedBackstoryGraphic backstory =
                new AlienPartGenerator.ExtendedBackstoryGraphic
                {
                    backstory = new BackstoryDef() { defName = "specificBackstory", identifier = "specificBackstory" },
                    path      = "/backstoryGraphics/specificBackstory",
                    ageGraphics = new List<AlienPartGenerator.ExtendedAgeGraphic>
                                  {
                                      new AlienPartGenerator.ExtendedAgeGraphic
                                      {
                                          age  = mockOtherAdultLifestageDef.Object,
                                          path = "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                                          damageGraphics = new List<AlienPartGenerator.ExtendedDamageGraphic>
                                                           {
                                                               new AlienPartGenerator.ExtendedDamageGraphic
                                                               {
                                                                   damage = 1f,
                                                                   path =
                                                                       "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a1"
                                                               },
                                                               new AlienPartGenerator.ExtendedDamageGraphic
                                                               {
                                                                   damage = 5f,
                                                                   path =
                                                                       "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5"
                                                               }
                                                           }
                                      }
                                  },
                    damageGraphics = new List<AlienPartGenerator.ExtendedDamageGraphic>
                                     {
                                         new AlienPartGenerator.ExtendedDamageGraphic
                                         {
                                             damage = 1f,
                                             path =
                                                 "/backstoryGraphics/specificBackstory/damageGraphics/a1"
                                         },
                                         new AlienPartGenerator.ExtendedDamageGraphic
                                         {
                                             damage = 5f,
                                             path =
                                                 "/backstoryGraphics/specificBackstory/damageGraphics/a5"
                                         }
                                     }
                };
            AlienPartGenerator.ExtendedAgeGraphic age = new AlienPartGenerator.ExtendedAgeGraphic
                                                         {
                                                             age  = mockOtherAdultLifestageDef.Object,
                                                             path = "/ageGraphics/otherAdultLifestage",
                                                             damageGraphics =
                                                                 new List<AlienPartGenerator.ExtendedDamageGraphic>
                                                                 {
                                                                     new AlienPartGenerator.ExtendedDamageGraphic
                                                                     {
                                                                         damage = 1f,
                                                                         path =
                                                                             "/ageGraphics/otherAdultLifestage/damageGraphics/a1"
                                                                     },
                                                                     new AlienPartGenerator.ExtendedDamageGraphic
                                                                     {
                                                                         damage = 5f,
                                                                         path =
                                                                             "/ageGraphics/otherAdultLifestage/damageGraphics/a5"
                                                                     }
                                                                 }
                                                         };

            AlienPartGenerator.ExtendedDamageGraphic damageA1 = new AlienPartGenerator.ExtendedDamageGraphic
                                                                 {
                                                                     damage = 1f,
                                                                     path =
                                                                         "/damageGraphics/a1"
                                                                 };
            AlienPartGenerator.ExtendedDamageGraphic damageA5 = new AlienPartGenerator.ExtendedDamageGraphic
                                                                 {
                                                                     damage = 5f,
                                                                     path =
                                                                         "/damageGraphics/a5"
                                                                 };

            AlienPartGenerator.BodyAddon addonUnderTest = new AlienPartGenerator.BodyAddon
                                                          {
                                                              bodyPart      = "nose",
                                                              inFrontOfBody = true,
                                                              alignWithHead = true,
                                                              ColorChannel  = "base",
                                                              path          = "/",
                                                              hediffGraphics =
                                                                  new List<AlienPartGenerator.ExtendedHediffGraphic>
                                                                  {
                                                                      cut, burn
                                                                  },
                                                              backstoryGraphics =
                                                                  new List<AlienPartGenerator.ExtendedBackstoryGraphic>
                                                                  {
                                                                      backstory
                                                                  },
                                                              ageGraphics =
                                                                  new List<AlienPartGenerator.ExtendedAgeGraphic>
                                                                  {
                                                                      age
                                                                  },
                                                              damageGraphics =
                                                                  new List<AlienPartGenerator.ExtendedDamageGraphic>
                                                                  {
                                                                      damageA1, damageA5
                                                                  }
                                                          };
            return addonUnderTest;
        }

        [Test]
        public void TestReturnsDeepestLeafFromBackstory()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<ExtendedGraphicsPawnWrapper>   mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(true);
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object)).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5",
                            bestGraphic.GetPath());
        }

        [Test]
        public void TestDoesNotDescendToDamageWhenInGoodHealth()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<ExtendedGraphicsPawnWrapper>   mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object)).Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                            bestGraphic.GetPath());
        }

        [Test]
        public void TestPicksDamageAtTopLevel()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<ExtendedGraphicsPawnWrapper>   mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object)).Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/damageGraphics/a5", bestGraphic.GetPath());
        }

        [Test]
        public void TestPicksDefaultWhenNothingMatches()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<ExtendedGraphicsPawnWrapper>   mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object)).Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/", bestGraphic.GetPath());
        }

        [Test]
        public void TestHandlesTopLevelNullPath()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.path = null;
            Mock<ExtendedGraphicsPawnWrapper> mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/damageGraphics/a5", bestGraphic.GetPath());
        }

        [Test]
        public void TestFallsBackToParentWhenFindingNull()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.backstoryGraphics[0].ageGraphics[0].damageGraphics
                       .Find(d => Math.Abs(d.damage - 5f) < 0.0001).path = null;
            Mock<ExtendedGraphicsPawnWrapper> mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                            bestGraphic.GetPath());
        }

        [Test]
        public void TestFallsBackToDeeperSiblingMatchOnParentWhenFindingNull()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.backstoryGraphics[0].ageGraphics[0].path = null;
            addonUnderTest.backstoryGraphics[0].ageGraphics[0].damageGraphics
                       .Find(d => Math.Abs(d.damage - 5f) < 0.0001).path = null;
            Mock<ExtendedGraphicsPawnWrapper> mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/damageGraphics/a5",
                            bestGraphic.GetPath());
            // Verify that we got to the damage child but then backtracked hence needing to call this again on the top level backstory->damage branch
            mockPawnWrapper.Verify(p => p.IsPartBelowHealthThreshold("nose", 5f), Times.Exactly(2));
        }

        [Test]
        public void TestFallsBackToDeeperSiblingMatchOnParentWhenFindingNullAndParentPathIsNotNull()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.backstoryGraphics[0].ageGraphics[0].ageGraphics =
                new List<AlienPartGenerator.ExtendedAgeGraphic>
                {
                    new AlienPartGenerator.ExtendedAgeGraphic
                    {
                        age = mockOtherAdultLifestageDef.Object,
                        path = "" //if not null would have been ./ageGraphics/otherAdultLifestage/ageGraphics/otherAdultLifestage
                    }
                };
            Mock<ExtendedGraphicsPawnWrapper> mockPawnWrapper = new Mock<ExtendedGraphicsPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object)).Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStory(addonUnderTest.backstoryGraphics.First().backstory)).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IExtendedGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5",
                            bestGraphic.GetPath());

            // Verify that we hit the extra age branch added above by confirming we descended into 2 age/otherAdultLifestage branches and then picked the sibling
            mockPawnWrapper.Verify(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object),
                                   Times.Exactly(2));
        }
    }
}