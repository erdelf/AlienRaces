namespace AlienRaceTest
{
    using System.Collections.Generic;
    using AlienRace;
    using AlienRace.BodyAddonSupport;
    using Moq;
    using NUnit.Framework;
    using RimWorld;
    using Verse;

    [TestFixture]
    public class BodyAddonTest
    {
        [Test]
        public void TestReturnsDeepestLeafFromBackstory()
        {
            Mock<LifeStageDef> mockHumanlikeAdultLifestageDef = new Mock<LifeStageDef>();
            Mock<LifeStageDef> mockOtherAdultLifestageDef     = new Mock<LifeStageDef>();
            Mock<HediffDef>    mockCutHediff                  = new Mock<HediffDef>();
            Mock<HediffDef>    mockBurnHediff                 = new Mock<HediffDef>();


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

            Mock<BodyAddonPawnWrapper> mockPawnWrapper =
                new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.SetupGet(p => p.CurrentLifeStageDef).Returns(mockOtherAdultLifestageDef.Object);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage/damageGraphics/a5",
                            bestGraphic.GetPath());
        }
        
        [Test]
        public void TestDoesNotDescendToDamageWhenInGoodHealth()
        {
            Mock<LifeStageDef> mockHumanlikeAdultLifestageDef = new Mock<LifeStageDef>();
            Mock<LifeStageDef> mockOtherAdultLifestageDef     = new Mock<LifeStageDef>();
            Mock<HediffDef>    mockCutHediff                  = new Mock<HediffDef>();
            Mock<HediffDef>    mockBurnHediff                 = new Mock<HediffDef>();


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

            Mock<BodyAddonPawnWrapper> mockPawnWrapper =
                new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.SetupGet(p => p.CurrentLifeStageDef).Returns(mockOtherAdultLifestageDef.Object);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(true);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/backstoryGraphics/specificBackstory/ageGraphics/otherAdultLifestage",
                            bestGraphic.GetPath());
        }
        
        [Test]
        public void TestPicksDamageAtTopLevel()
        {
            Mock<LifeStageDef> mockHumanlikeAdultLifestageDef = new Mock<LifeStageDef>();
            Mock<LifeStageDef> mockOtherAdultLifestageDef     = new Mock<LifeStageDef>();
            Mock<HediffDef>    mockCutHediff                  = new Mock<HediffDef>();
            Mock<HediffDef>    mockBurnHediff                 = new Mock<HediffDef>();


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

            Mock<BodyAddonPawnWrapper> mockPawnWrapper =
                new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.SetupGet(p => p.CurrentLifeStageDef).Returns(mockHumanlikeAdultLifestageDef.Object);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 5f)).Returns(true);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/damageGraphics/a5",
                            bestGraphic.GetPath());
        }
        [Test]
        public void TestPicksDefaultWhenNothingMatches()
        {
            Mock<LifeStageDef> mockHumanlikeAdultLifestageDef = new Mock<LifeStageDef>();
            Mock<LifeStageDef> mockOtherAdultLifestageDef     = new Mock<LifeStageDef>();
            Mock<HediffDef>    mockCutHediff                  = new Mock<HediffDef>();
            Mock<HediffDef>    mockBurnHediff                 = new Mock<HediffDef>();


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

            Mock<BodyAddonPawnWrapper> mockPawnWrapper =
                new Mock<BodyAddonPawnWrapper>();
            mockPawnWrapper.SetupGet(p => p.CurrentLifeStageDef).Returns(mockHumanlikeAdultLifestageDef.Object);
            mockPawnWrapper.Setup(p => p.HasBackStoryWithIdentifier("specificBackstory")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockBurnHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOfDefAndPart(mockCutHediff.Object, "nose")).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 1f)).Returns(false);
            mockPawnWrapper.Setup(p => p.HasHediffOnPartBelowHealthThreshold("nose", 5f)).Returns(false);

            // Resolve
            IBodyAddonGraphic bestGraphic = addonUnderTest.GetBestGraphic(mockPawnWrapper.Object, "nose");

            Assert.AreEqual("/",
                            bestGraphic.GetPath());
        }
    }
}