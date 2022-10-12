namespace AlienRaceTest.TestSupport
{
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using RimWorld;
    using UnityEngine;
    using Verse;

    /**
     * Test base class to keep unity specific handling out of the way and available to all tests.
     * Also provides helper methods to assist with working with the Rimworld content and auto-cleaning it up.
     */
    [TestFixture]
    public abstract class BaseUnityTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            // Prevent Unity builtin Log Handler to avoid: ECall methods must be packaged into a system module.
            Logger newLogger = new Logger(new UnityLogHandler());
            FieldInfo fieldInfo =
                typeof(Debug).GetField("s_Logger",
                                       BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic |
                                       BindingFlags.Static);
            fieldInfo?.SetValue(null, newLogger);
        }

        protected static void InitPrefs(PrefsData prefsData)
        {
            // Have to initialise via reflection to avoid Prefs trying to load save data folders.
            FieldInfo fieldInfo =
                typeof(Prefs).GetField("data",
                                       BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic |
                                       BindingFlags.Static);
            fieldInfo?.SetValue(null, prefsData);
        }

        [TearDown]
        public void TearDownUnityBase()
        {
            ClearAllDefDbs();
        }
        
        [SetUp]
        public void SetupUnityBase()
        {
            ClearAllDefDbs();
        }

        private static void ClearAllDefDbs()
        {
            // Clear down the Def databases which may have been populated with custom objects by various tests to avoid cross-contamination.
            foreach (Type type in GenDefDatabase.AllDefTypesWithDatabases())
                GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, "Clear");
        }
        
        protected static HediffDef AddHediffWithName(string hediffName)
        {
            HediffDef fakeHediffDef = new HediffDef()
                                      {
                                          defName = hediffName
                                      };
            DefDatabase<HediffDef>.Add(fakeHediffDef);
            return fakeHediffDef;
        }
        protected static LifeStageDef AddLifestageWithName(string lifestage)
        {
            LifeStageDef fakeLifeStageDef = new LifeStageDef()
                                            {
                                                defName = lifestage
                                            };
            DefDatabase<LifeStageDef>.Add(fakeLifeStageDef);
            return fakeLifeStageDef;
        }
    }
}