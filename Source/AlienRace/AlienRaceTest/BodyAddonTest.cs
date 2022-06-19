namespace AlienRaceTest
{
    using System;
    using System.Collections.Generic;
    using AlienRace;
    using AlienRace.BodyAddonSupport;
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
            AlienPartGenerator.BodyAddonHediffGraphic cut = new AlienPartGenerator.BodyAddonHediffGraphic
                                                            {
                                                                hediff = mockCutHediff.Object,
                                                                path   = "/hediffGraphics/cut",
                                                                ageGraphics =
                                                                    new List<AlienPartGenerator.BodyAddonAgeGraphic>
                                                                    {
                                                                        new AlienPartGenerator.BodyAddonAgeGraphic
                                                                        {
                                                                            age = mockHumanlikeAdultLifestageDef.Object,
                                                                            path = "/hediffGraphics/cut/age"
                                                                        }
                                                                    }
                                                            };

            AlienPartGenerator.BodyAddonHediffGraphic burn = new AlienPartGenerator.BodyAddonHediffGraphic
                                                             {
                                                                 hediff = mockBurnHediff.Object,
                                                                 path   = "/hediffGraphics/burn",
                                                                 ageGraphics =
                                                                     new List<AlienPartGenerator.BodyAddonAgeGraphic>
                                                                     {
                                                                         new AlienPartGenerator.BodyAddonAgeGraphic
                                                                         {
                                                                             age = mockHumanlikeAdultLifestageDef
                                                                             .Object,
                                                                             path =
                                                                                 "/hediffGraphics/burn/ageGraphics/humanlikeAdult"
                                                                         }
                                                                     },
                                                                 severity =
                                                                     new List<AlienPartGenerator.
                                                                         BodyAddonHediffSeverityGraphic>
                                                                     {
                                                                         new AlienPartGenerator.
                                                                         BodyAddonHediffSeverityGraphic
                                                                         {
                                                                             path = "/hediffGraphics/burn/severity/a0",
                                                                             severity = 0f,
                                                                             ageGraphics =
                                                                                 new List<AlienPartGenerator.
                                                                                     BodyAddonAgeGraphic>
                                                                                 {
                                                                                     new AlienPartGenerator.
                                                                                     BodyAddonAgeGraphic
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

            AlienPartGenerator.BodyAddonBackstoryGraphic backstory =
                new AlienPartGenerator.BodyAddonBackstoryGraphic
                {
                    backstory = "specificBackstory",
                    path      = "/backstoryGraphics/specificBackstory",
                    ageGraphics = new List<AlienPartGenerator.BodyAddonAgeGraphic>
                                  {
                                      new AlienPartGenerator.BodyAddonAgeGraphic
                                      {
                                          age  = mockOtherAdultLifestageDef.Object,
                                          path = "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                                          damageGraphics = new List<AlienPartGenerator.BodyAddonDamageGraphic>
                                                           {
                                                               new AlienPartGenerator.BodyAddonDamageGraphic
                                                               {
                                                                   damage = 1f,
                                                                   path =
                                                                       "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a1"
                                                               },
                                                               new AlienPartGenerator.BodyAddonDamageGraphic
                                                               {
                                                                   damage = 5f,
                                                                   path =
                                                                       "/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5"
                                                               }
                                                           }
                                      }
                                  },
                    damageGraphics = new List<AlienPartGenerator.BodyAddonDamageGraphic>
                                     {
                                         new AlienPartGenerator.BodyAddonDamageGraphic
                                         {
                                             damage = 1f,
                                             path =
                                                 "/backstoryGraphics/specificBackstory/damageGraphics/a1"
                                         },
                                         new AlienPartGenerator.BodyAddonDamageGraphic
                                         {
                                             damage = 5f,
                                             path =
                                                 "/backstoryGraphics/specificBackstory/damageGraphics/a5"
                                         }
                                     }
                };
            AlienPartGenerator.BodyAddonAgeGraphic age = new AlienPartGenerator.BodyAddonAgeGraphic
                                                         {
                                                             age  = mockOtherAdultLifestageDef.Object,
                                                             path = "/ageGraphics/otherAdultLifestage",
                                                             damageGraphics =
                                                                 new List<AlienPartGenerator.BodyAddonDamageGraphic>
                                                                 {
                                                                     new AlienPartGenerator.BodyAddonDamageGraphic
                                                                     {
                                                                         damage = 1f,
                                                                         path =
                                                                             "/ageGraphics/otherAdultLifestage/damageGraphics/a1"
                                                                     },
                                                                     new AlienPartGenerator.BodyAddonDamageGraphic
                                                                     {
                                                                         damage = 5f,
                                                                         path =
                                                                             "/ageGraphics/otherAdultLifestage/damageGraphics/a5"
                                                                     }
                                                                 }
                                                         };

            AlienPartGenerator.BodyAddonDamageGraphic damageA1 = new AlienPartGenerator.BodyAddonDamageGraphic
                                                                 {
                                                                     damage = 1f,
                                                                     path =
                                                                         "/damageGraphics/a1"
                                                                 };
            AlienPartGenerator.BodyAddonDamageGraphic damageA5 = new AlienPartGenerator.BodyAddonDamageGraphic
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
                                                                  new List<AlienPartGenerator.BodyAddonHediffGraphic>
                                                                  {
                                                                      cut, burn
                                                                  },
                                                              backstoryGraphics =
                                                                  new List<AlienPartGenerator.BodyAddonBackstoryGraphic>
                                                                  {
                                                                      backstory
                                                                  },
                                                              ageGraphics =
                                                                  new List<AlienPartGenerator.BodyAddonAgeGraphic>
                                                                  {
                                                                      age
                                                                  },
                                                              damageGraphics =
                                                                  new List<AlienPartGenerator.BodyAddonDamageGraphic>
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
            Mock<BodyAddonPawnWrapper>   mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5",
                            bestGraphic.GetPath());
        }

        [Test]
        public void TestDoesNotDescendToDamageWhenInGoodHealth()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<BodyAddonPawnWrapper>   mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                            bestGraphic.GetPath());
        }

        [Test]
        public void TestPicksDamageAtTopLevel()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<BodyAddonPawnWrapper>   mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/damageGraphics/a5", bestGraphic.GetPath());
        }

        [Test]
        public void TestPicksDefaultWhenNothingMatches()
        {
            AlienPartGenerator.BodyAddon addonUnderTest  = this.GetTestBodyAddon();
            Mock<BodyAddonPawnWrapper>   mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/", bestGraphic.GetPath());
        }

        [Test]
        public void TestHandlesTopLevelNullPath()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.path = null;
            Mock<BodyAddonPawnWrapper> mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockHumanlikeAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object,  "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/damageGraphics/a5", bestGraphic.GetPath());
        }

        [Test]
        public void TestFallsBackToParentWhenFindingNull()
        {
            AlienPartGenerator.BodyAddon addonUnderTest = this.GetTestBodyAddon();
            addonUnderTest.backstoryGraphics[0].ageGraphics[0].damageGraphics
                       .Find(d => Math.Abs(d.damage - 5f) < 0.0001).path = null;
            Mock<BodyAddonPawnWrapper> mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

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
            Mock<BodyAddonPawnWrapper> mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

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
                new List<AlienPartGenerator.BodyAddonAgeGraphic>
                {
                    new AlienPartGenerator.BodyAddonAgeGraphic
                    {
                        age = mockOtherAdultLifestageDef.Object,
                        path = "" //if not null would have been ./ageGraphics/otherAdultLifestage/ageGraphics/otherAdultLifestage
                    }
                };
            Mock<BodyAddonPawnWrapper> mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                        .Returns(true);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.IsPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5",
                            bestGraphic.GetPath());

            // Verify that we hit the extra age branch added above by confirming we descended into 2 age/otherAdultLifestage branches and then picked the sibling
            mockPawnWrapper.Verify(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object),
                                   Times.Exactly(2));
        }
    }
}